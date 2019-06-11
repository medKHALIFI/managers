using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

//Clase para extender las propiedades del control textbox, se le agrego la propiedad "mandatory" para que se pueda indicar si el 
//campo a llenar es "Necesario"
namespace EdicionSwExProperty
{
    public class TextBoxEx : TextBox
    {
        private static DependencyProperty ExtraPropProperty = DependencyProperty.Register
        ("mandatory", typeof(string), typeof(TextBoxEx), 
        new PropertyMetadata("No extra prop."));
    
        public string mandatory
            {
                get { return (string)GetValue(ExtraPropProperty); }
                set { SetValue(ExtraPropProperty, value); }
            }

        private static DependencyProperty ExtraPropProperty2 = DependencyProperty.Register
       ("external_name", typeof(string), typeof(TextBoxEx),
       new PropertyMetadata("No extra prop."));

        public string external_name
        {
            get { return (string)GetValue(ExtraPropProperty2); }
            set { SetValue(ExtraPropProperty2, value); }
        }

        private static DependencyProperty ExtraPropProperty3 = DependencyProperty.Register
       ("numero", typeof(string), typeof(TextBoxEx),
       new PropertyMetadata("No extra prop."));

        public string numero
        {
            get { return (string)GetValue(ExtraPropProperty3); }
            set { SetValue(ExtraPropProperty3, value); }
        }


        public TextBoxEx() : base() { }


    }

    public class ComboBoxExt : ComboBox
    {
        private static DependencyProperty ExtraPropProperty = DependencyProperty.Register
        ("mandatory", typeof(string), typeof(ComboBoxExt),
        new PropertyMetadata("No extra prop."));

        public string mandatory
        {
            get { return (string)GetValue(ExtraPropProperty); }
            set { SetValue(ExtraPropProperty, value); }
        }

        private static DependencyProperty ExtraPropProperty2 = DependencyProperty.Register
      ("external_name", typeof(string), typeof(ComboBoxExt),
      new PropertyMetadata("No extra prop."));

        public string external_name
        {
            get { return (string)GetValue(ExtraPropProperty2); }
            set { SetValue(ExtraPropProperty2, value); }
        }

        public ComboBoxExt() : base() { }


    }

    public class DatePickerExt : DatePicker
    {
        private static DependencyProperty ExtraPropProperty = DependencyProperty.Register
        ("mandatory", typeof(string), typeof(DatePickerExt),
        new PropertyMetadata("No extra prop."));

        public string mandatory
        {
            get { return (string)GetValue(ExtraPropProperty); }
            set { SetValue(ExtraPropProperty, value); }
        }

        private static DependencyProperty ExtraPropProperty2 = DependencyProperty.Register
   ("external_name", typeof(string), typeof(DatePickerExt),
   new PropertyMetadata("No extra prop."));

        public string external_name
        {
            get { return (string)GetValue(ExtraPropProperty2); }
            set { SetValue(ExtraPropProperty2, value); }
        }

        public DatePickerExt() : base() { }


    }

}
