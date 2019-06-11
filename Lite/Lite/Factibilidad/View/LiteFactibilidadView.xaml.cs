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
using SpatialEye.Framework.Client;
using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// The view for displaying feature details; needs to be bound to a LiteFeatureDetailsViewModel,
  /// </summary>
    public partial class LiteFactibilidadView : UserControl
  {
    /// <summary>
    /// Constructs the view for displaying feature details
    /// </summary>
    public LiteFactibilidadView()
    {
      InitializeComponent();
      cboTipo.Items.Add("RESIDENCIAL");
      cboTipo.Items.Add("EMPRESARIAL");
      cboTipo.SelectedValue="RESIDENCIAL";
      Indicador.Visibility = Visibility.Collapsed;
    }

    private void btnCalcularFactibilidad_Click(object sender, RoutedEventArgs e)
    {
        txtResultados.Text = "";
       

        if (cboTipo.SelectedValue.ToString() == "RESIDENCIAL")
        {
            
            this.ProcesarPeticionResidencia(sender, e);
            
        }
        else
        {
            
            this.ProcesarPeticionEmpresarial(sender, e);

        }
    }


    private void ProcesarPeticionResidencia(object sender, RoutedEventArgs e)
    {
        
        Indicador.InProgress = true;
        Indicador.Visibility = Visibility.Visible;

        try
        {
            ServiceFactibilidadResidencial.WsFactibilidadResidencialPortTypeClient proxy = new ServiceFactibilidadResidencial.WsFactibilidadResidencialPortTypeClient();
            ServiceFactibilidadResidencial.wsFactibilidadResidencialRQType peticionType = new ServiceFactibilidadResidencial.wsFactibilidadResidencialRQType();
            lblResultados.Content = "FACTIBILIDAD RESIDENCIAL:";
            
            peticionType.latitud = txtLatitud.Text.ToString();
            peticionType.longitud = txtLongitud.Text.ToString();
            peticionType.colonia = "";
            peticionType.cp = "";
            
            proxy.getFactibilidadResidencialCompleted += new EventHandler<ServiceFactibilidadResidencial.getFactibilidadResidencialCompletedEventArgs>(calcula_factibilidad_residencial_completado);
            proxy.getFactibilidadResidencialAsync(peticionType);

           
        }
        catch (CommunicationException ex)
        {
            MessageBox.Show(ex.Message);
            //lblErrores.Text=ex.Message;
            Indicador.InProgress = false;
            Indicador.Visibility = Visibility.Collapsed;
        }
    }
    private void ProcesarPeticionEmpresarial(object sender, RoutedEventArgs e)
    {
        Indicador.InProgress = true;
        Indicador.Visibility = Visibility.Visible;

        try
        {
            ServiceFactibilidadEmpresarial.WsFactibilidadEmpresarialPortTypeClient proxy = new ServiceFactibilidadEmpresarial.WsFactibilidadEmpresarialPortTypeClient();
            ServiceFactibilidadEmpresarial.wsFactibilidadEmpresarialRQType peticionType = new ServiceFactibilidadEmpresarial.wsFactibilidadEmpresarialRQType();

            //ServiceFactibilidadEmpresarial.getFactibilidadEmpresarialRequest peticion = new ServiceFactibilidadEmpresarial.getFactibilidadEmpresarialRequest();

            lblResultados.Content = "FACTIBILIDAD EMPRESARIAL:";

            
            //peticion.wsFactibilidadEmpresarialRQ = peticionType;
            peticionType.latitud = txtLatitud.Text.ToString();
            peticionType.longitud = txtLongitud.Text.ToString();
            peticionType.colonia = "";
            peticionType.cp = "";
            proxy.getFactibilidadEmpresarialCompleted += new EventHandler<ServiceFactibilidadEmpresarial.getFactibilidadEmpresarialCompletedEventArgs>(calcula_factibilidad_empresarial_completado);
            proxy.getFactibilidadEmpresarialAsync(peticionType);


        }
        catch (CommunicationException ex)
        {
            MessageBox.Show(ex.Message);
            //lblErrores.Text=ex.Message;
            Indicador.InProgress = false;
            Indicador.Visibility = Visibility.Collapsed;
        }
    }


    void calcula_factibilidad_empresarial_completado(object sender, ServiceFactibilidadEmpresarial.getFactibilidadEmpresarialCompletedEventArgs e)
    {
        if (e.Result==null)
        {
            MessageBox.Show("Error de Conectividad con el Servidor");
            //lblErrores.Text = "Error de Conectividad con el Servidor"; 
        }
        else {
            lblResultados.Content = "FACTIBILIDAD EMPRESARIAL:";
            //txtResultados.Text =  "Código de Factibilidad: " + e.Result.factibilidad + Environment.NewLine;
            txtResultados.Text += "Factbilidad:            " + e.Result.factibilidad + Environment.NewLine;
            txtResultados.Text += "Dirección:              " + e.Result.domicilio + Environment.NewLine;
            txtResultados.Text += "Region:                 " + e.Result.region_enlace + Environment.NewLine;
            txtResultados.Text += "Ciudad:                 " + e.Result.ciudad_enlace + Environment.NewLine;
            txtResultados.Text += "Zona:                   " + e.Result.buffer_enlace + Environment.NewLine;
            txtResultados.Text += "Distrito:               " + e.Result.distrito_enlace + Environment.NewLine;
            txtResultados.Text += "Cluster:              " + e.Result.cluster_totalplay + Environment.NewLine;
            txtResultados.Text += "Comentarios:            " + e.Result.comentario + Environment.NewLine;
           

            
            //lblErrores.Text 
            string msg = e.Result.detalleRespuesta.MensajeTransaccion +
                                e.Result.detalleRespuesta.CodigoError +
                                 e.Result.detalleRespuesta.DescripcionError +
                                 e.Result.detalleRespuesta.MensajeError;
            MessageBox.Show(msg);

            }
        Indicador.InProgress = false;
        Indicador.Visibility = Visibility.Collapsed;
        
    }
    void calcula_factibilidad_residencial_completado(object sender, ServiceFactibilidadResidencial.getFactibilidadResidencialCompletedEventArgs e)
    {
        if (e.Result == null)
        {
            MessageBox.Show("Error de Conectividad con el Servidor");
            //lblErrores.Text = "Error de Conectividad con el Servidor"; 
        }
        else
        {
            //txtResultados.Text = "Código de Factibilidad: " + e.Result.factibilidad + Environment.NewLine;
            lblResultados.Content = "FACTIBILIDAD RESIDENCIAL:";
            txtResultados.Text += "Factibilidad:            " + e.Result.factibilidad + Environment.NewLine;
            txtResultados.Text += "Dirección:              " + e.Result.domicilio + Environment.NewLine;
            txtResultados.Text += "Region:                 " + e.Result.region_totalplay + Environment.NewLine;
            txtResultados.Text += "Ciudad:                 " + e.Result.ciudad_totalplay + Environment.NewLine;
            txtResultados.Text += "Zona:                   " + e.Result.zona_totalplay + Environment.NewLine;
            txtResultados.Text += "Distrito:               " + e.Result.distrito_totalplay + Environment.NewLine;
            txtResultados.Text += "Cluster:              " + e.Result.cluster_totalplay + Environment.NewLine;
            txtResultados.Text += "Comentarios:            " + e.Result.comentario + Environment.NewLine;


            //lblErrores.Text 
            string msg = e.Result.detalleRespuesta.MensajeTransaccion +
                                e.Result.detalleRespuesta.CodigoError +
                                 e.Result.detalleRespuesta.DescripcionError +
                                 e.Result.detalleRespuesta.MensajeError;
            MessageBox.Show(msg);

        }
        Indicador.InProgress = false;
        Indicador.Visibility = Visibility.Collapsed;

    }

    private void cmdLimpiar_Click(object sender, RoutedEventArgs e)
    {
        limpiar();
        
    }

    void limpiar()
    {
        txtLatitud.Text = "";
        txtLongitud.Text = "";
        txtResultados.Text = "";
        lblErrores.Text = "";
        lblResultados.Tag = "RESULTADOS DE FACTIBILIDAD:";
    }
    

  }
}
