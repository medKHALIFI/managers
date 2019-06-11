using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.ServiceModel;
using System.Text.RegularExpressions;

namespace Lite
{
  /// <summary>
  /// The view for displaying feature details; needs to be bound to a LiteFeatureDetailsViewModel,
  /// </summary>
    public partial class LiteInventarioCajaView 
  {
        String mensajeError = "";
        String patronCuenta1 = "^([0-9]{10})$";
        String patronCuenta2 = "^([0-9])([.])([0-9]{7})$";
        String patronPuertos1 = "^([0-9])$";
        String patronPuertos2 = "^([0-9][0-9])$";
        String patronPuertos3 = "^([0-9][0-9][0-9])$";
       
        /// <summary>
    /// Constructs the view for displaying feature details
    /// </summary>
    public LiteInventarioCajaView()
    {
        
        InitializeComponent();
        Indicador.Visibility = Visibility.Collapsed;
        LayoutRoot.Visibility = System.Windows.Visibility.Collapsed;
    }

    private void lblCaja_TextChanged(object sender, EventArgs e)
    {
        LiteMessageBoxView msg = new LiteMessageBoxView();
        msg.InitializeComponent();

        txtCuenta.Text = "";
        //txtNombreCaja.Text = "";
        txtPuertosOcupados.Text = "";
        txtPuertosTotales.Text = "";
    }

    private void limpiar()
    {
        txtCuenta.Text = "";
        txtNombreCaja.Text = "";
        txtPuertosOcupados.Text = "";
        txtPuertosTotales.Text = "";
        txtNumeroPuerto.Text = "";
    }

    private void cmdActualizaPuertos_Click(object sender, RoutedEventArgs e)
    {
        
        mensajeError = "";
        if (!datosValidos())
        {
            MessageBox.Show(mensajeError,"",MessageBoxButton.OK);
        }
        else
        {
            Indicador.InProgress = true;
            Indicador.Visibility = Visibility.Visible;
            ServiceGestionPuertos.WsGestionaInfoPuertosPortTypeClient proxy = new ServiceGestionPuertos.WsGestionaInfoPuertosPortTypeClient();
            ServiceGestionPuertos.WsGestionaInfoPuertosRQType peticionType = new ServiceGestionPuertos.WsGestionaInfoPuertosRQType();

            try
            {
                ServiceGestionPuertos.actualizaPuertosRequest peticion = new ServiceGestionPuertos.actualizaPuertosRequest();
                peticion.WsGestionaInfoPuertosRQ = peticionType;
                peticionType.id_caja = lblCaja.Text.ToString();
                peticionType.nombre = txtNombreCaja.Text.ToString();
                peticionType.puertos_totales = txtPuertosTotales.Text.ToString();
                peticionType.puertos_ocupados = txtPuertosOcupados.Text.ToString();
                proxy.actualizaPuertosCompleted += new EventHandler<ServiceGestionPuertos.actualizaPuertosCompletedEventArgs>(actualizar_puertos_completado);
                proxy.actualizaPuertosAsync(peticion);

            }
            catch (CommunicationException ex)
            {
                proxy.Abort();
                MessageBox.Show(ex.Message,"",MessageBoxButton.OK);
                //lblErrores.Text = ex.Message;
                Indicador.InProgress = false;
                Indicador.Visibility = Visibility.Collapsed;
            }
            limpiar();
        }
    }

    void actualizar_puertos_completado(object sender, ServiceGestionPuertos.actualizaPuertosCompletedEventArgs e)
    {
        if (e.Result == null)
        {
            MessageBox.Show("Error de Conectividad con el Servidor");
        }
        else
        {
              string msg  = e.Result.WsGestionaInfoPuertosRS.estado +
                    e.Result.WsGestionaInfoPuertosRS.Detalle_Respuesta.CodigoError + "\n" +
                                 e.Result.WsGestionaInfoPuertosRS.Detalle_Respuesta.DescripcionError + "\n" +
                                 e.Result.WsGestionaInfoPuertosRS.Detalle_Respuesta.MensajeError;
            MessageBox.Show(msg);
        }
        Indicador.InProgress = false;
        Indicador.Visibility = Visibility.Collapsed;

    }


    private void cmdAgregarCuenta_Click(object sender, RoutedEventArgs e)
    {
        //mensajeError = "";
        bool okPatron = Regex.IsMatch(txtCuenta.Text, patronCuenta1) || Regex.IsMatch(txtCuenta.Text, patronCuenta2);
        if (txtCuenta.Text.Length==0||!okPatron)
        {
            MessageBox.Show("Error: Debe proporcionar una cuenta válida");

        }
        else if (String.IsNullOrWhiteSpace(txtNumeroPuerto.Text.ToString()) || Convert.ToInt32(txtNumeroPuerto.Text)<=0)
        {
            MessageBox.Show("Error: Debe proporcionar un número de puerto");
        }
        else
        {
            Indicador.InProgress = true;
            Indicador.Visibility = Visibility.Visible;
            ServiceGestionCuentas1.WsGestionaInfoCuentasPortTypeClient proxy = new ServiceGestionCuentas1.WsGestionaInfoCuentasPortTypeClient();
            ServiceGestionCuentas1.WsGestionaInfoCuentasRQType peticionType = new ServiceGestionCuentas1.WsGestionaInfoCuentasRQType();

            try
            {
                //ServiceGestionCuentas1.gestionaCuentaRequest peticion = new ServiceGestionCuentas1.gestionaCuentaRequest();
                //ServiceGestionCuentas1.WsGestionaInfoCuentasRQType peticion = new ServiceGestionCuentas1.WsGestionaInfoCuentasRQType();

                //peticion.WsGestionaInfoCuentasRQ = peticionType;
                peticionType.id_caja = lblCaja.Text.ToString();
                peticionType.cuenta = txtCuenta.Text.ToString();
                peticionType.numero_puerto = txtNumeroPuerto.Text.ToString();
                peticionType.operacion = "asociar";
                proxy.gestionaCuentaCompleted += new EventHandler<ServiceGestionCuentas1.gestionaCuentaCompletedEventArgs>(agregar_cuenta_completado);
                proxy.gestionaCuentaAsync(peticionType);

            }
            catch (CommunicationException ex)
            {
                MessageBox.Show(ex.Message);
                LiteMessageBoxView ms = new LiteMessageBoxView();
                ms.InitializeComponent();
                ms.Focus();
                ms.IsEnabled = true;
                ms.IsHitTestVisible = true;
                ms.LayoutRoot = this.LayoutRoot;


                //lblErrores.Text = ex.Message;
                proxy.Abort();
                Indicador.InProgress = false;
                Indicador.Visibility = Visibility.Collapsed;
            }

            limpiar();
        }
    }

    void agregar_cuenta_completado(object sender, ServiceGestionCuentas1.gestionaCuentaCompletedEventArgs e)
    {
        if (e.Result == null)
        {
            MessageBox.Show("Error de Conectividad con el Servidor");
            //lblErrores.Text = "Error de Conectividad con el Servidor";
        }
        else
        {
            //lblErrores.Text
            
                string msg = e.Result.estado +
                                 e.Result.Detalle_Respuesta.CodigoError +
                                 e.Result.Detalle_Respuesta.DescripcionError +
                                 e.Result.Detalle_Respuesta.MensajeError;
                MessageBox.Show(msg);
        }
        Indicador.InProgress = false;
        Indicador.Visibility = Visibility.Collapsed;


    }


    private void cmdDesagregarCuenta_Click(object sender, RoutedEventArgs e)
    {
        mensajeError = "";
        bool okPatron = Regex.IsMatch(txtCuenta.Text, patronCuenta1) || Regex.IsMatch(txtCuenta.Text, patronCuenta2);
        if (txtCuenta.Text.Length == 0 || !okPatron)
        {
            MessageBox.Show("Error: Debe proporcionar una cuenta válida");

        }
        else
        {
            Indicador.InProgress = true;
            Indicador.Visibility = Visibility.Visible;
            ServiceGestionCuentas1.WsGestionaInfoCuentasPortTypeClient proxy = new ServiceGestionCuentas1.WsGestionaInfoCuentasPortTypeClient();
            ServiceGestionCuentas1.WsGestionaInfoCuentasRQType peticionType = new ServiceGestionCuentas1.WsGestionaInfoCuentasRQType();

            try
            {

                //ServiceGestionCuentas1.gestionaCuentaRequest peticion = new ServiceGestionCuentas1.gestionaCuentaRequest();
                

                
                //peticion.WsGestionaInfoCuentasRQ = peticionType;
                peticionType.id_caja = lblCaja.Text.ToString();
                peticionType.cuenta = txtCuenta.Text.ToString();
                peticionType.numero_puerto = txtNumeroPuerto.Text.ToString();
                peticionType.operacion = "desasociar";
                proxy.gestionaCuentaCompleted += new EventHandler<ServiceGestionCuentas1.gestionaCuentaCompletedEventArgs>(agregar_cuenta_completado);
                proxy.gestionaCuentaAsync(peticionType);
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show(ex.Message);
                //lblErrores.Text = ex.Message;
                proxy.Abort();
                Indicador.InProgress = false;
                Indicador.Visibility = Visibility.Collapsed;
            }
            limpiar();
        }
    }

    void desagregar_cuenta_completado(object sender, ServiceGestionCuentas1.gestionaCuentaCompletedEventArgs e)
    {
        if (e.Result == null)
        {
            MessageBox.Show("Error de Conectividad con el Servidor");
            //lblErrores.Text = "Error de Conectividad con el Servidor";
        }
        else
        {
            //lblErrores.Text 
            string msg = e.Result.estado +
                                e.Result.Detalle_Respuesta.CodigoError +
                                 e.Result.Detalle_Respuesta.DescripcionError +
                                 e.Result.Detalle_Respuesta.MensajeError;
            MessageBox.Show(msg);
        }
        Indicador.InProgress = false;
        Indicador.Visibility = Visibility.Collapsed;

    }

    private void txtPuertosTotales_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (bolValidaNumeros(e.Key))
            e.Handled = false;
        else
            e.Handled = true;
    }

    private void txtPuertosOcupados_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (bolValidaNumeros(e.Key))
            e.Handled = false;
        else
            e.Handled = true;
    }


    public bool bolValidaLetras(System.Windows.Input.Key e)
    {
        if (e == Key.A || e == Key.B || e == Key.C || e == Key.D || e == Key.E || e == Key.F || e == Key.G || e == Key.H || e == Key.I || e == Key.J || e == Key.K
            || e == Key.L || e == Key.M || e == Key.N || e == Key.O || e == Key.P || e == Key.Q || e == Key.R || e == Key.S || e == Key.T || e == Key.U || e == Key.V
            || e == Key.W || e == Key.W || e == Key.X || e == Key.Y || e == Key.Z)
            return true;
        else
            return false;
    }

    public bool bolValidaNumeros(System.Windows.Input.Key e)
    {
        if (e == Key.D0 || e == Key.D1 || e == Key.D2 || e == Key.D3 || e == Key.D4 || e == Key.D5 || e == Key.D6 || e == Key.D7 || e == Key.D8 || e == Key.D9
            || e == Key.NumPad0 || e == Key.NumPad1 || e == Key.NumPad2 || e == Key.NumPad3 || e == Key.NumPad4 || e == Key.NumPad5 || e == Key.NumPad6
            || e == Key.NumPad7 || e == Key.NumPad8 || e == Key.NumPad9)
            return true;
        else
            return false;
    }

    public bool bolValidaLetrasyNumeros(System.Windows.Input.Key e)
    {
        if (bolValidaLetras(e) || bolValidaNumeros(e))
            return true;
        else
            return false;
    }

   public bool datosValidos()
    {
       
        bool datosOK = true;
        bool m1, m2, m3, m4, m5, m6;

        m1 = Regex.IsMatch(txtPuertosTotales.Text, patronPuertos1);
        m2 = Regex.IsMatch(txtPuertosTotales.Text, patronPuertos2);
        m3 = Regex.IsMatch(txtPuertosTotales.Text, patronPuertos3);
        m4 = Regex.IsMatch(txtPuertosOcupados.Text, patronPuertos1);
        m5 = Regex.IsMatch(txtPuertosOcupados.Text, patronPuertos2);
        m6 = Regex.IsMatch(txtPuertosOcupados.Text, patronPuertos3);


        if ((!m1 && !m2 && !m3 && !m4 && !m5 && !m6)||txtPuertosOcupados.Text.Length==0||txtPuertosTotales.Text.Length==0)
        {
            datosOK = false;
            mensajeError = "Error: Puertos totales y ocupados deben ser números.";
        }
        else
        {
            if (int.Parse(txtPuertosOcupados.Text) > int.Parse(txtPuertosTotales.Text))
            {
                datosOK = false;
                mensajeError = "Error: Puertos Ocupados no puede ser mayor a Puertos Totales";
            }
        }

        return datosOK;
    }

  }
}
