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

namespace Lite
{
  /// <summary>
  /// The view for displaying feature details; needs to be bound to a LiteFeatureDetailsViewModel,
  /// </summary>
    public partial class LiteProspectosView
  {
    /// <summary>
    /// Constructs the view for displaying feature details
    /// </summary>
    public LiteProspectosView()
    {
      InitializeComponent();
      cboTipo.Items.Add("");
      cboTipo.Items.Add("RESIDENCIAL");
      cboTipo.Items.Add("COMERCIAL");
      cboTipo.SelectedItem = "";
      Indicador.Visibility = Visibility.Collapsed;

    }

    private void cmdAlmacena_Click(object sender, RoutedEventArgs e)
    {
        Indicador.InProgress = true;
        Indicador.Visibility = Visibility.Visible;
        ServiceAlmacenaProspecto.WsAlmacenaUbicacionProspectoFVPortTypeClient proxy = new ServiceAlmacenaProspecto.WsAlmacenaUbicacionProspectoFVPortTypeClient();
        ServiceAlmacenaProspecto.wsAlmacenaUbicacionProspectoFVRQType peticionType = new ServiceAlmacenaProspecto.wsAlmacenaUbicacionProspectoFVRQType();
            
        try
        {
            ServiceAlmacenaProspecto.almacenaUbicacionProspectoFVRequest peticion = new ServiceAlmacenaProspecto.almacenaUbicacionProspectoFVRequest();
            peticion.wsAlmacenaUbicacionProspectoFVRQ = peticionType;
            peticionType.nombre = txtNombre.Text;
            peticionType.direccion = txtDireccion.Text;
            peticionType.telefono = txtTelefono.Text;
            peticionType.tipo = cboTipo.SelectedValue.ToString();
            peticionType.paquete = txtPaquete.Text;
            peticionType.fecha = dpFecha.Text;
            peticionType.latitud = txtLatitud.Text.ToString();
            peticionType.longitud = txtLongitud.Text.ToString();
            proxy.almacenaUbicacionProspectoFVCompleted += new EventHandler<ServiceAlmacenaProspecto.almacenaUbicacionProspectoFVCompletedEventArgs>(almacenar_prospecto_completado);
            proxy.almacenaUbicacionProspectoFVAsync(peticion);
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

    void almacenar_prospecto_completado(object sender, ServiceAlmacenaProspecto.almacenaUbicacionProspectoFVCompletedEventArgs e)
    {
        if (e.Result == null)
        {
            MessageBox.Show("Error de Conectividad con el Servidor");
            //lblErrores.Text = "Error de Conectividad con el Servidor";
        }
        else
        {
            MessageBox.Show(e.Result.wsAlmacenaUbicacionProspectoFVRS.estado + ":    " + e.Result.wsAlmacenaUbicacionProspectoFVRS.idSw);
            //lblErrores.Text = e.Result.wsAlmacenaUbicacionProspectoFVRS.estado + "  " + e.Result.wsAlmacenaUbicacionProspectoFVRS.idSw;
            txtIdSw.Text = e.Result.wsAlmacenaUbicacionProspectoFVRS.idSw;
        }
        Indicador.InProgress = false;
        Indicador.Visibility = Visibility.Collapsed;
    }

    private void cmdElimina_Click(object sender, RoutedEventArgs e)
    {
        Indicador.InProgress = true;
        Indicador.Visibility = Visibility.Visible;
        ServiceEliminaProspecto.WsEliminaUbicacionProspectoFVPortTypeClient proxy = new ServiceEliminaProspecto.WsEliminaUbicacionProspectoFVPortTypeClient();
        ServiceEliminaProspecto.wsEliminaUbicacionProspectoFVRQType peticionType = new ServiceEliminaProspecto.wsEliminaUbicacionProspectoFVRQType();
            
        try
        {
            ServiceEliminaProspecto.eliminaUbicacionProspectoFVRequest peticion = new ServiceEliminaProspecto.eliminaUbicacionProspectoFVRequest();
            peticion.wsEliminaUbicacionProspectoFVRQ = peticionType;
            peticionType.idSw = txtIdSw.Text;
            proxy.eliminaUbicacionProspectoFVCompleted += new EventHandler<ServiceEliminaProspecto.eliminaUbicacionProspectoFVCompletedEventArgs>(eliminar_prospecto_completado);
            proxy.eliminaUbicacionProspectoFVAsync(peticion);
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

    void eliminar_prospecto_completado(object sender, ServiceEliminaProspecto.eliminaUbicacionProspectoFVCompletedEventArgs e)
    {
        if (e.Result == null)
        {
            MessageBox.Show("Error de Conectividad con el Servidor");
            //lblErrores.Text = "Error de Conectividad con el Servidor";
        }
        else
        {
            MessageBox.Show(e.Result.wsEliminaUbicacionProspectoFVRS.estado);
            //lblErrores.Text = e.Result.wsEliminaUbicacionProspectoFVRS.estado;
        }
        Indicador.InProgress = false;
        Indicador.Visibility = Visibility.Collapsed;
        
    }

    private void cmdLimpiar_Click(object sender, RoutedEventArgs e)
    {
        txtDireccion.Text = "";
        txtIdSw.Text = "";
        txtLatitud.Text = "";
        txtLongitud.Text = "";
        txtNombre.Text = "";
        txtPaquete.Text = "";
        txtTelefono.Text = "";
        cboTipo.SelectedValue = "";
        dpFecha.Text="";
        lblErrores.Text = "";
    }
    
    

  }
}
