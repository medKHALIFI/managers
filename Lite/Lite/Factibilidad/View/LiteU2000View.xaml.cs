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
using System.Collections;

namespace Lite
{
    /// <summary>
    /// The view for displaying feature details; needs to be bound to a LiteFeatureDetailsViewModel,
    /// </summary>
    public partial class LiteU2000View : UserControl
    {

        /// <summary>
        /// Constructs the view for displaying feature details
        /// </summary>
        public LiteU2000View()
        {
            InitializeComponent();
            Indicador.Visibility = Visibility.Collapsed;
            LayoutRoot.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void cmdInfoLogica_Click(object sender, RoutedEventArgs e)
        {

            Indicador.InProgress = true;
            Indicador.Visibility = Visibility.Visible;

            ServiceU2000.WsOpticalInformationPortTypeClient proxy = new ServiceU2000.WsOpticalInformationPortTypeClient();
            ServiceU2000.process peticionType = new ServiceU2000.process();

            try
            {
                ServiceU2000.opticalInformationRequest peticion = new ServiceU2000.opticalInformationRequest();
                peticion.WsOpticalInformationRQ = peticionType;
                peticionType.No_Account = lblCuenta.Text;
                proxy.opticalInformationCompleted += new EventHandler<ServiceU2000.opticalInformationCompletedEventArgs>(consulta_informacion);
                proxy.opticalInformationAsync(peticion);

            }
            catch (CommunicationException ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK);
                Indicador.InProgress = false;
                Indicador.Visibility = Visibility.Collapsed;
            }

        }

        void consulta_informacion(object sender, ServiceU2000.opticalInformationCompletedEventArgs e)
        {
            if (e.Result.WsOpticalInformationRS.ONTS.Length == 0)
            {
                MessageBox.Show("NO ES POSIBLE ENCONTRAR INFORMACIÓN");
            }
            else
            {
                for (int i = 0; i < e.Result.WsOpticalInformationRS.ONTS.Length; i++)
                {
                    txtInfoU2000.Text += "-----------------------------------------------------\r\n";
                    txtInfoU2000.Text += "-SN: " + e.Result.WsOpticalInformationRS.ONTS[i].SN + "\r\n";
                    txtInfoU2000.Text += "-TXPower: " + e.Result.WsOpticalInformationRS.ONTS[i].TXPower + "\r\n";
                    txtInfoU2000.Text += "-RXPower: " + e.Result.WsOpticalInformationRS.ONTS[i].RXPower + "\r\n";
                    txtInfoU2000.Text += "-TXTemperature:" + e.Result.WsOpticalInformationRS.ONTS[i].TXTEMPERATURE + "\r\n";
                    txtInfoU2000.Text += "-TXVol: " + e.Result.WsOpticalInformationRS.ONTS[i].TXVOL + "\r\n";
                    txtInfoU2000.Text += "-BIASCURRENT: " + e.Result.WsOpticalInformationRS.ONTS[i].BIASCURRENT + "\r\n";
                    txtInfoU2000.Text += "-OLT: " + e.Result.WsOpticalInformationRS.ONTS[i].OLT + "\r\n";
                    txtInfoU2000.Text += "-FRAME: " + e.Result.WsOpticalInformationRS.ONTS[i].FRAME + "\r\n";
                    txtInfoU2000.Text += "-SLOT: " + e.Result.WsOpticalInformationRS.ONTS[i].SLOT + "\r\n";
                    txtInfoU2000.Text += "-PORT: " + e.Result.WsOpticalInformationRS.ONTS[i].PORT + "\r\n";
                }

            }
            Indicador.InProgress = false;
            Indicador.Visibility = Visibility.Collapsed;

        }


        private void lblCuenta_TextChanged(object sender, EventArgs e)
        {

            txtInfoU2000.Text = "";
            Indicador.Visibility = Visibility.Collapsed;


        }
    }
}
