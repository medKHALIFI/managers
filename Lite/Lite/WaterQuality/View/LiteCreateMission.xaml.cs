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

namespace Lite
{
    public partial class LiteCreateMission : UserControl
    {
        public LiteCreateMission()
        {
            InitializeComponent();
            comboBox_laboratory.Items.Add("Laboratory 1");
            comboBox_laboratory.Items.Add("Laboratory 2");
            comboBox_laboratory.Items.Add("Laboratory 3");
            comboBox_laboratory.Items.Add("Laboratory 4");
            comboBox_Labasstant.Items.Add("Labassistant 1");
            comboBox_Labasstant.Items.Add("Labassistant 2");
            comboBox_Labasstant.Items.Add("Labassistant 3");
            comboBox_Labasstant.Items.Add("Labassistant 4");
            comboBox_param_type.Items.Add("Biological");
            comboBox_param_type.Items.Add("Chimical");
            comboBox_param_type.Items.Add("Physical");
            comboBox_param_type.Items.Add("Microbiological");
        }

        private void button__Click(object sender, RoutedEventArgs e)
        {
            // todo
            MessageBox.Show("Mission created sussfuly");
            comboBox_laboratory.SelectedItem = null;
            comboBox_Labasstant.SelectedItem = null;
            comboBox_param_type.SelectedItem = null; 
            textBox_note.Text = string.Empty;
            //TextBox_note.Text = string.Empty;
        }
    }
}
