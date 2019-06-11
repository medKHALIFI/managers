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
    public partial class LiteClientesView
  {
        String mensajeError = "";
        /// <summary>
    /// Constructs the view for displaying feature details
    /// </summary>
    public LiteClientesView()
    {
      InitializeComponent();
      cboTipo.Items.Add("ENLACE");
      cboTipo.Items.Add("TOTALPLAY");
      cboEstado.Items.Add("COMPLETO");
      cboEstado.Items.Add("COMPLETO_PENDIENTE");
      cboEstado.Items.Add("PENDIENTE");
      cboEstado.Items.Add("CANCELADO");

      Indicador.Visibility = Visibility.Collapsed;
    }


    private void cmdAlmacena_Click(object sender, RoutedEventArgs e)
    {
        mensajeError = "";
        if (!datosValidos("almacena"))
        {
            MessageBox.Show(mensajeError);

        }
        else
        {

            Indicador.InProgress = true;
            Indicador.Visibility = Visibility.Visible;
            ServiceAlmacenaCliente.WsAlmacenaUbicacionClienteFVPortTypeClient proxy = new ServiceAlmacenaCliente.WsAlmacenaUbicacionClienteFVPortTypeClient();
            ServiceAlmacenaCliente.wsAlmacenaUbicacionClienteFVRQType peticionType = new ServiceAlmacenaCliente.wsAlmacenaUbicacionClienteFVRQType();

            try
            {
                ServiceAlmacenaCliente.almacenaUbicacionClienteFVRequest peticion = new ServiceAlmacenaCliente.almacenaUbicacionClienteFVRequest();
                peticion.wsAlmacenaUbicacionClienteFVRQ = peticionType;
                peticionType.numero_cuenta = txtNumeroCuenta.Text;
                peticionType.etiqueta = txtEtiqueta.Text;
                peticionType.tipo = cboTipo.SelectedValue.ToString();
                peticionType.estado = cboEstado.SelectedValue.ToString();
                peticionType.originador = txtOriginador.Text.ToString();
                peticionType.latitud = txtLatitud.Text.ToString();
                peticionType.longitud = txtLongitud.Text.ToString();
                proxy.almacenaUbicacionClienteFVCompleted += new EventHandler<ServiceAlmacenaCliente.almacenaUbicacionClienteFVCompletedEventArgs>(almacenar_cliente_completado);
                proxy.almacenaUbicacionClienteFVAsync(peticion);

            }
            catch (CommunicationException ex)
            {
                proxy.Abort();
                MessageBox.Show(ex.Message);
                //lblErrores.Text = ex.Message;
                Indicador.InProgress = false;
                Indicador.Visibility = Visibility.Collapsed;
            }
            limpiar();
        }
    }

    void almacenar_cliente_completado(object sender, ServiceAlmacenaCliente.almacenaUbicacionClienteFVCompletedEventArgs e)
    {
        if (e.Result == null)
        {
            MessageBox.Show("Error de Conectividad con el Servidor");
            //lblErrores.Text = "Error de Conectividad con el Servidor";
        }
        else
        {
            //lblErrores.Text
              string msg  = e.Result.wsAlmacenaUbicacionClienteFVRS.estado +
                    e.Result.wsAlmacenaUbicacionClienteFVRS.Detalle_Respuesta.CodigoError + "\n" +
                                 e.Result.wsAlmacenaUbicacionClienteFVRS.Detalle_Respuesta.DescripcionError + "\n" +
                                 e.Result.wsAlmacenaUbicacionClienteFVRS.Detalle_Respuesta.MensajeError;
            MessageBox.Show(msg);
        }
        Indicador.InProgress = false;
        Indicador.Visibility = Visibility.Collapsed;
       


    }


    private void cmdModificar_Click(object sender, RoutedEventArgs e)
    {
        mensajeError = "";
        if (!datosValidos("modifica"))
        {
            MessageBox.Show(mensajeError);

        }
        else
        {
            Indicador.InProgress = true;
            Indicador.Visibility = Visibility.Visible;
            ServiceModificaCliente.WsModificaUbicacionClienteFVPortTypeClient proxy = new ServiceModificaCliente.WsModificaUbicacionClienteFVPortTypeClient();
            ServiceModificaCliente.wsModificaUbicacionClienteFVRQType peticionType = new ServiceModificaCliente.wsModificaUbicacionClienteFVRQType();

            try
            {
                ServiceModificaCliente.modificaUbicacionClienteFVRequest peticion = new ServiceModificaCliente.modificaUbicacionClienteFVRequest();
                peticion.wsModificaUbicacionClienteFVRQ = peticionType;
                peticionType.numero_cuenta = txtNumeroCuenta.Text;
                peticionType.etiqueta = txtEtiqueta.Text;
                peticionType.tipo = cboTipo.SelectedValue.ToString();
                peticionType.estado = cboEstado.SelectedValue.ToString();
                peticionType.originador = txtOriginador.Text.ToString();
                peticionType.latitud = txtLatitud.Text.ToString();
                peticionType.longitud = txtLongitud.Text.ToString();
                proxy.modificaUbicacionClienteFVCompleted += new EventHandler<ServiceModificaCliente.modificaUbicacionClienteFVCompletedEventArgs>(modificar_cliente_completado);
                proxy.modificaUbicacionClienteFVAsync(peticion);
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

    void modificar_cliente_completado(object sender, ServiceModificaCliente.modificaUbicacionClienteFVCompletedEventArgs e)
    {
        if (e.Result == null)
        {
            MessageBox.Show("Error de Conectividad con el Servidor");
            //lblErrores.Text = "Error de Conectividad con el Servidor";
        }
        else
        {
            //lblErrores.Text
                string msg = e.Result.wsModificaUbicacionClienteFVRS.estado +
                                 e.Result.wsModificaUbicacionClienteFVRS.Detalle_Respuesta.CodigoError +
                                 e.Result.wsModificaUbicacionClienteFVRS.Detalle_Respuesta.DescripcionError +
                                 e.Result.wsModificaUbicacionClienteFVRS.Detalle_Respuesta.MensajeError;
                MessageBox.Show(msg);
        }
        
        Indicador.InProgress = false;
        Indicador.Visibility = Visibility.Collapsed;


    }


    private void cmdEliminar_Click(object sender, RoutedEventArgs e)
    {
        mensajeError = "";
        if (!datosValidos("elimina"))
        {
            MessageBox.Show(mensajeError);

        }
        else
        {
            Indicador.InProgress = true;
            Indicador.Visibility = Visibility.Visible;
            ServiceEliminaCliente.WsEliminaUbicacionClienteFVPortTypeClient proxy = new ServiceEliminaCliente.WsEliminaUbicacionClienteFVPortTypeClient();
            ServiceEliminaCliente.wsEliminaUbicacionClienteFVRQType peticionType = new ServiceEliminaCliente.wsEliminaUbicacionClienteFVRQType();

            try
            {
                ServiceEliminaCliente.eliminaUbicacionClienteFVRequest peticion = new ServiceEliminaCliente.eliminaUbicacionClienteFVRequest();
                peticion.wsEliminaUbicacionClienteFVRQ = peticionType;
                peticionType.numero_cuenta = txtNumeroCuenta.Text;
                proxy.eliminaUbicacionClienteFVCompleted += new EventHandler<ServiceEliminaCliente.eliminaUbicacionClienteFVCompletedEventArgs>(eliminar_cliente_completado);
                proxy.eliminaUbicacionClienteFVAsync(peticion);
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

    void eliminar_cliente_completado(object sender, ServiceEliminaCliente.eliminaUbicacionClienteFVCompletedEventArgs e)
    {
        if (e.Result == null)
        {
            MessageBox.Show("Error de Conectividad con el Servidor");
            //lblErrores.Text = "Error de Conectividad con el Servidor";
        }
        else
        {
            //lblErrores.Text 
            string msg = e.Result.wsEliminaUbicacionClienteFVRS.estado +
                                e.Result.wsEliminaUbicacionClienteFVRS.Detalle_Respuesta.CodigoError +
                                 e.Result.wsEliminaUbicacionClienteFVRS.Detalle_Respuesta.DescripcionError +
                                 e.Result.wsEliminaUbicacionClienteFVRS.Detalle_Respuesta.MensajeError;
            MessageBox.Show(msg);
        }
      
        Indicador.InProgress = false;
        Indicador.Visibility = Visibility.Collapsed;

    }

    private void cmdLimpiar_Click(object sender, RoutedEventArgs e)
    {
        limpiar();
    }
    private void limpiar()
    {
        txtLatitud.Text = "";
        txtLongitud.Text = "";
        cboTipo.SelectedValue = "";
        cboEstado.SelectedValue = "";
        txtNumeroCuenta.Text = "";
        txtEtiqueta.Text = "";
        lblErrores.Text = "";
    }
   public bool datosValidos(String accion)
    {

        bool datosOK = true;
        string patronCuenta1 = "^([0-9]{10})$";
        string patronCuenta2 = "^([0-9])([.])([0-9]{7})$";
        bool m1, m2;

        m1 = Regex.IsMatch(txtNumeroCuenta.Text, patronCuenta1);
        m2 = Regex.IsMatch(txtNumeroCuenta.Text, patronCuenta2);
 
       if (!m1 &&  !m2)
        {
            datosOK = false;
            mensajeError = "Error en número de cuenta.";
        }

       if ((accion!="elimina")&&((cboEstado.SelectedValue==null)||( cboTipo.SelectedValue==null)))
       {
           datosOK = false;
           mensajeError = mensajeError + " Error: Debe seleccionar un valor de Línea de Negocio y Estado";
       }

        return datosOK;
    }
  }
}
