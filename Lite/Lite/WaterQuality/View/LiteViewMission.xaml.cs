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
    public partial class LiteViewMission : UserControl
    {
        public LiteViewMission()
        {
            InitializeComponent();
            // label_labassistant.Visibility( collapsed);
            // add listener to the combobox
            listBox_mission.SelectionChanged += OnSelectionChanged;

        }

        // test listener 
        //listbox changed
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // MessageBox.Show(" test");

            //Do something with the selected item

            // todo add here laboratory from database
            label_stat_view.Content = "ready" ;
            label_stat_view.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 250, 0));
            label_laboratory_view.Content = "laboratory " + listBox_mission.SelectedIndex;
            label_labassistant_view.Content = "labassistant " + listBox_mission.SelectedIndex;

        }

        private void button_delete_Click(object sender, RoutedEventArgs e)
        {

           // MessageBox.Show("MessageBox for Silverlight", "AlertMessageBox", MessageBoxButton.OKCancel);

            MessageBoxResult isConfirmed = MessageBox.Show("are you sure you want delete this mission", "Alert delete", MessageBoxButton.OKCancel);

            if (isConfirmed == MessageBoxResult.OK)

            {

                //Perfrom some Action;
                listBox_mission.Items.Remove(listBox_mission.SelectedItem);
                label_stat_view.Content = " ";

                label_laboratory_view.Content = "  ";
                label_labassistant_view.Content = "  ";
            }
            
        }
    }

}
