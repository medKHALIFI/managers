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



using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;

using SpatialEye.Framework.Maps;
using SpatialEye.Framework.Authentication;
using SpatialEye.Framework.Authentication.Services;
using SpatialEye.Framework.Export.Services;
using SpatialEye.Framework.Features.Services;
using SpatialEye.Framework.Maps.Services;
using SpatialEye.Framework.Queries;
using SpatialEye.Framework.Queries.Services;
using SpatialEye.Framework.Reports.Services;
using SpatialEye.Framework.ServiceProviders;
using SpatialEye.Framework.ServiceProviders.XY;
using SpatialEye.Framework.Services;

namespace Lite
{
    public partial class LiteWaterQualityView : UserControl
    {
        public LiteWaterQualityView()
        {
            InitializeComponent();
            comboBox_laboratory.Items.Add("Laboratory 1");
            comboBox_laboratory.Items.Add("Laboratory 2");
            comboBox_laboratory.Items.Add("Laboratory 3");
            comboBox_laboratory.Items.Add("Laboratory 4");
            comboBox_type_parametre.Items.Add("Biological");
            comboBox_type_parametre.Items.Add("Chimical");
            comboBox_type_parametre.Items.Add("Physical");
            comboBox_type_parametre.Items.Add("Microbiological");
        }

        private  void button_Click(object sender, RoutedEventArgs e)
        {
          
        }
    }
}
