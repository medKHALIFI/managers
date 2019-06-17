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
    public partial class LiteCreateSensor : UserControl
    {
        public LiteCreateSensor()
        {
            InitializeComponent();
            // Instantiate random number generator using system-supplied value as seed.
            Random rand = new Random();
            // Generate and display 5 random byte (integer) values.
            byte[] bytes = new byte[5];
            rand.NextBytes(bytes);
            label_id_view.Content =  rand.Next();
            comboBox_type.Items.Add("select item ...");
            comboBox_type.Items.Add("Physical");
            comboBox_type.Items.Add("Chimical");
            comboBox_type.Items.Add("Biological");

            comboBox_type.SelectedIndex = 0;
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate random number generator using system-supplied value as seed.
            Random rand = new Random();
            // Generate and display 5 random byte (integer) values.
            byte[] bytes = new byte[5];
            rand.NextBytes(bytes);
           
            MessageBox.Show("Sensor created successfully");
            label_id_view.Content = " ";
            comboBox_type.SelectedIndex = 0;
            textBox_name.Text = " ";
            label_id_view.Content = rand.Next();

        }
    }
}
