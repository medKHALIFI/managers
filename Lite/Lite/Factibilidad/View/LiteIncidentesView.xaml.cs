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
    public partial class LiteIncidentesView
  {
    /// <summary>
    /// Constructs the view for displaying feature details
    /// </summary>
    public LiteIncidentesView()
    {
      InitializeComponent();
      Indicador.Visibility = Visibility.Collapsed;
    }


    private void cmdAlmacena_Click(object sender, RoutedEventArgs e)
    {
        if (!datosValidos())
        {
            MessageBox.Show("Error: Fólio y/o Nombre Invalidos");

        }
        else
        {

            Indicador.InProgress = true;
            Indicador.Visibility = Visibility.Visible;
            ServiceAlmacenaIncidente.WsAlmacenaUbicacionIncidentePortTypeClient proxy = new ServiceAlmacenaIncidente.WsAlmacenaUbicacionIncidentePortTypeClient();
            ServiceAlmacenaIncidente.wsAlmacenaUbicacionIncidenteRQType peticionType = new ServiceAlmacenaIncidente.wsAlmacenaUbicacionIncidenteRQType();

            try
            {
                ServiceAlmacenaIncidente.almacenaUbicacionIncidenteRequest peticion = new ServiceAlmacenaIncidente.almacenaUbicacionIncidenteRequest();
                peticion.wsAlmacenaUbicacionIncidenteRQ = peticionType;
                peticionType.folio = txtFolio.Text;
                peticionType.nombre = txtNombre.Text;
                peticionType.latitud = txtLatitud.Text.ToString();
                peticionType.longitud = txtLongitud.Text.ToString();
                proxy.almacenaUbicacionIncidenteCompleted += new EventHandler<ServiceAlmacenaIncidente.almacenaUbicacionIncidenteCompletedEventArgs>(almacenar_incidente_completado);
                proxy.almacenaUbicacionIncidenteAsync(peticion);

            }
            catch (CommunicationException ex)
            {
                proxy.Abort();
                MessageBox.Show(ex.Message);
                //lblErrores.Text = ex.Message;
                Indicador.InProgress = false;
                Indicador.Visibility = Visibility.Collapsed;
            }
        }
    }

    void almacenar_incidente_completado(object sender, ServiceAlmacenaIncidente.almacenaUbicacionIncidenteCompletedEventArgs e)
    {
        if (e.Result == null)
        {
            MessageBox.Show("Error de Conectividad con el Servidor");
            //lblErrores.Text = "Error de Conectividad con el Servidor";
        }
        else
        {
            //lblErrores.Text
              string msg  = e.Result.wsAlmacenaUbicacionIncidenteRS.estado +
                    e.Result.wsAlmacenaUbicacionIncidenteRS.Detalle_Respuesta.CodigoError + "\n" +
                                 e.Result.wsAlmacenaUbicacionIncidenteRS.Detalle_Respuesta.DescripcionError + "\n" +
                                 e.Result.wsAlmacenaUbicacionIncidenteRS.Detalle_Respuesta.MensajeError;
            MessageBox.Show(msg);
        }
        Indicador.InProgress = false;
        Indicador.Visibility = Visibility.Collapsed;
       


    }


    private void cmdModificar_Click(object sender, RoutedEventArgs e)
    {
        if (!datosValidos())
        {
            MessageBox.Show("Error: Fólio Invalido");

        }
        else
        {
            Indicador.InProgress = true;
            Indicador.Visibility = Visibility.Visible;
            ServiceModificaIncidente.WsModificaUbicacionIncidentePortTypeClient proxy = new ServiceModificaIncidente.WsModificaUbicacionIncidentePortTypeClient();
            ServiceModificaIncidente.wsModificaUbicacionIncidenteRQType peticionType = new ServiceModificaIncidente.wsModificaUbicacionIncidenteRQType();

            try
            {
                ServiceModificaIncidente.modificaUbicacionIncidenteRequest peticion = new ServiceModificaIncidente.modificaUbicacionIncidenteRequest();
                peticion.wsModificaUbicacionIncidenteRQ = peticionType;
                peticionType.folio = txtFolio.Text;
                peticionType.nombre = txtNombre.Text;
                peticionType.latitud = txtLatitud.Text.ToString();
                peticionType.longitud = txtLongitud.Text.ToString();
                proxy.modificaUbicacionIncidenteCompleted += new EventHandler<ServiceModificaIncidente.modificaUbicacionIncidenteCompletedEventArgs>(modificar_incidente_completado);
                proxy.modificaUbicacionIncidenteAsync(peticion);
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show(ex.Message);
                //lblErrores.Text = ex.Message;
                proxy.Abort();
                Indicador.InProgress = false;
                Indicador.Visibility = Visibility.Collapsed;
            }
        }
    }

    void modificar_incidente_completado(object sender, ServiceModificaIncidente.modificaUbicacionIncidenteCompletedEventArgs e)
    {
        if (e.Result == null)
        {
            MessageBox.Show("Error de Conectividad con el Servidor");
            //lblErrores.Text = "Error de Conectividad con el Servidor";
        }
        else
        {
            //lblErrores.Text
                string msg = e.Result.wsModificaUbicacionIncidenteRS.estado +
                                 e.Result.wsModificaUbicacionIncidenteRS.Detalle_Respuesta.CodigoError +
                                 e.Result.wsModificaUbicacionIncidenteRS.Detalle_Respuesta.DescripcionError +
                                 e.Result.wsModificaUbicacionIncidenteRS.Detalle_Respuesta.MensajeError;
                MessageBox.Show(msg);
        }
        Indicador.InProgress = false;
        Indicador.Visibility = Visibility.Collapsed;


    }


    private void cmdEliminar_Click(object sender, RoutedEventArgs e)
    {
        if (!datosValidos())
        {
            MessageBox.Show("Error: Fólio Invalido debe ser numerico");

        }
        else
        {
            Indicador.InProgress = true;
            Indicador.Visibility = Visibility.Visible;
            ServiceEliminaIncidente.WsEliminaUbicacionIncidentePortTypeClient proxy = new ServiceEliminaIncidente.WsEliminaUbicacionIncidentePortTypeClient();
            ServiceEliminaIncidente.wsEliminaUbicacionIncidenteRQType peticionType = new ServiceEliminaIncidente.wsEliminaUbicacionIncidenteRQType();

            try
            {
                ServiceEliminaIncidente.eliminaUbicacionIncidenteRequest peticion = new ServiceEliminaIncidente.eliminaUbicacionIncidenteRequest();
                peticion.wsEliminaUbicacionIncidenteRQ = peticionType;
                peticionType.folio = txtFolio.Text;
                proxy.eliminaUbicacionIncidenteCompleted += new EventHandler<ServiceEliminaIncidente.eliminaUbicacionIncidenteCompletedEventArgs>(eliminar_incidente_completado);
                proxy.eliminaUbicacionIncidenteAsync(peticion);
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show(ex.Message);
                //lblErrores.Text = ex.Message;
                proxy.Abort();
                Indicador.InProgress = false;
                Indicador.Visibility = Visibility.Collapsed;
            }
        }
    }

    void eliminar_incidente_completado(object sender, ServiceEliminaIncidente.eliminaUbicacionIncidenteCompletedEventArgs e)
    {
        if (e.Result == null)
        {
            MessageBox.Show("Error de Conectividad con el Servidor");
            //lblErrores.Text = "Error de Conectividad con el Servidor";
        }
        else
        {
            //lblErrores.Text 
            string msg = e.Result.wsEliminaUbicacionIncidenteRS.estado +
                                e.Result.wsEliminaUbicacionIncidenteRS.Detalle_Respuesta.CodigoError +
                                 e.Result.wsEliminaUbicacionIncidenteRS.Detalle_Respuesta.DescripcionError +
                                 e.Result.wsEliminaUbicacionIncidenteRS.Detalle_Respuesta.MensajeError;
            MessageBox.Show(msg);
        }
        Indicador.InProgress = false;
        Indicador.Visibility = Visibility.Collapsed;

    }

    private void cmdLimpiar_Click(object sender, RoutedEventArgs e)
    {
        txtLatitud.Text = "";
        txtLongitud.Text = "";
        txtFolio.Text = "";
        txtNombre.Text = "";
        lblErrores.Text = "";
    }

   public bool datosValidos()
    {

        bool datosOK = true;
        string patronCuenta1 = "^([0-9]{6})$";
        string patronCuenta2 = "^([0-9]{7})$";
        string patronCuenta3 = "^([0-9]{8})$";
        string patronCuenta4 = "^([0-9]{9})$";

        bool m1, m2, m3, m4;

        m1 = Regex.IsMatch(txtFolio.Text, patronCuenta1) || Regex.IsMatch(txtFolio.Text, patronCuenta2) ||
             Regex.IsMatch(txtFolio.Text, patronCuenta3) || Regex.IsMatch(txtFolio.Text, patronCuenta4);
        //m1 = txtFolio.Text.Length<=50;//Regex.IsMatch(txtFolio.Text, patronCuenta1);
        //m2 = txtFolio.Text.Length > 0;
        m3 = txtNombre.Text.Length <= 100;//Regex.IsMatch(txtFolio.Text, patronCuenta2);
        m4 = txtNombre.Text.Length > 0;
 
       if (!(m1)||!(m3&&m4))
        {
            datosOK = false;
        }
        return datosOK;
    }

  }
}
