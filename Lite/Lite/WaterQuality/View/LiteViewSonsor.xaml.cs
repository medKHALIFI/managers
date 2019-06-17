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
    public partial class LiteViewSonsor : UserControl
    {
        public LiteViewSonsor()
        {
            InitializeComponent();
            // add listener to list box sensor
            listBox_sensor.SelectionChanged += OnSelectionChanged;
        }

        // test listener 
        //listbox changed
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // MessageBox.Show(" test");

            //Do something with the selected item

            // todo add here laboratory from database
            /* label_stat_view.Content = "ready";
             label_stat_view.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 250, 0));
             label_laboratory_view.Content = "laboratory " + listBox_mission.SelectedIndex;
             label_labassistant_view.Content = "labassistant " + listBox_mission.SelectedIndex;
             */
            // Instantiate random number generator using system-supplied value as seed.
            Random rand = new Random();
            // Generate and display 5 random byte (integer) values.
            byte[] bytes = new byte[5];
            rand.NextBytes(bytes);

            label_id_view.Content = rand.Next();
            label_stat_view.Content = "ready";
            label_stat_view.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 250, 0));
            label_type_view.Content = "temperature";
            label_name_view.Content = "sensor " + listBox_sensor.SelectedIndex;



        }
    }
}
