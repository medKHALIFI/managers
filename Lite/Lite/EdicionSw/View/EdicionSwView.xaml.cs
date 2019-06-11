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

using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Windows.Data;
using EdicionSwExProperty;
using Lite.Resources.Localization;
using System.Windows.Navigation;
using System.ServiceModel;
using SpatialEye.Framework.Client;
using System.Runtime.Serialization;
using System.ServiceModel.Web;
using System.Windows.Browser;
using System.Runtime.Serialization.Json;
using System.Text;
using SpatialEye.Framework.ComponentModel.Design;


using SpatialEye.Framework.Features;
namespace Lite
{
    public partial class EdicionSwView : UserControl 
    {
        XElement doc = new XElement("Tabla");
        string strgeometry = string.Empty;
       
            

        public EdicionSwView()
        {
            InitializeComponent();
            Indicador.Visibility = Visibility.Collapsed;
            LeerXml();

           
        }
       
       #region Eventos
        //Evento del combo que inicia seleccionando una tabla y generar los controles
        public void cboTablas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Indicador.InProgress = true;
                Indicador.Visibility = Visibility.Visible;

                if (cboTablas.SelectedIndex >= 0)
                {
                    if ((string)cboTablas.SelectedValue != "Seleccione")
                    {
                        GetTableName((string)cboTablas.SelectedValue);
                    }
                    else
                    {
                        stkDatos.Children.Clear();
                    }
                }

                Indicador.InProgress = false;
                Indicador.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                Indicador.InProgress = false;
                Indicador.Visibility = Visibility.Collapsed;
            }
        }

        //Evento para realizar la inserción de datos
        private void btnInserta_Click(object sender, RoutedEventArgs e)
        {
            string type_accion = "ALTA";

                  
            try
            {

                MessageBoxResult result = MessageBox.Show("Desea insertar la información.?", "Edición", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {


                    if (validaStatusNuevo() == false)
                    {
                        MessageBox.Show("Estatus no válido para insertar");
                    }
                    else
                    {
                       
                        if (strgeometry == "punto")
                        {
                            if (txtLatitudP.Text == string.Empty)
                            {
                                MessageBox.Show("Latitud no válida");
                                return;
                            }

                            if (txtLongitudP.Text == string.Empty)
                            {
                                MessageBox.Show("Longitud no válida");
                                return;
                            }

                        }
                        else if (strgeometry == "linea")
                        {
                            if (txtLatitudT.Text == string.Empty)
                            {
                                MessageBox.Show("Latitud no válida");
                                return;
                            }

                            if (txtLongitudT.Text == string.Empty)
                            {
                                MessageBox.Show("Longitud no válida");
                                return;
                            }
                        }
                        else if (strgeometry == "area")
                        {
                            if (txtLatitudTC.Text == string.Empty)
                            {
                                MessageBox.Show("Latitud no válida");
                                return;
                            }

                            if (txtLongitudTC.Text == string.Empty)
                            {
                                MessageBox.Show("Longitud no válida");
                                return;
                            }
                        }

                        post_webservice(type_accion);

                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
              
        }


        //Evento para la modificación de datos
        private void btnModifica_Click(object sender, RoutedEventArgs e)
        {
            string type_accion = "MODIFICAR";
            try
            {
                MessageBoxResult result = MessageBox.Show("Desea modificar la información.?", "Edición", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    if (validaStatusNuevo() == false)
                    {
                        MessageBox.Show("Estatus no válido para modificar");
                        return;
                    }

                    //Validar si es para la Tabla Optical Splitter
                    //Validar que no se reduzca el número de ports de 8 a 16 unicamente 
                    //
                    if ((string)cboTablas.SelectedValue == "Optical Splitter")
                    {

                        //Recorrer Controles tipo ComboBox                        
                        foreach (var child in stkDatos.Children.OfType<ComboBoxExt>())
                        {
                            var splitcambio = from splt in doc.Descendants("split")
                                              where (string)splt.Attribute("name") == (string)child.SelectedValue.ToString()
                                              select splt.Attribute("value").Value; 
                            
                            
                            var split = from splt in doc.Descendants("split")
                                        where (string)splt.Attribute("name") != (string)child.SelectedValue.ToString()
                                        select splt.Attribute("value").Value;


                            if(splitcambio.Count() > 0)
                            {
                                if(split.Count()>0)
                                {
                                    for(int s =0; s< split.Count(); s++)
                                    {
                                        if(Convert.ToInt16(splitcambio.First()) < Convert.ToInt16(split.ElementAt(s)))
                                        {
                                            MessageBox.Show("Valor no válido para modificar.");
                                            return;
                                        }

                                    }

                                    //Si no cambio el valor no enviar nada
                                    int max = 0;
                                    foreach (string cant in lblNames.Content.ToString().Split('|'))
                                    {
                                        max++;
                                    }
                                    string[,] lsNames = new string[max, 2];
                                    int i = 0;
                                    foreach (string name in lblNames.Content.ToString().Split('|'))
                                    {
                                        lsNames[i, 0] = name;
                                        i++;
                                    }

                                    int j = 0;
                                    foreach (string value in lblValues.Content.ToString().Split('|'))
                                    {
                                        lsNames[j, 1] = value;
                                        j++;
                                    }

                                    bool bolID = false;
                                    bool boolVal = false;
                                    foreach (var childtxt in stkDatos.Children.OfType<TextBoxEx>())
                                    {
                                        for (int x = 0; x < max; x++)
                                        {
                                            //Si el nombre del control y valor del textbox son iguales poner bandera en true
                                            if((string)childtxt.Name == lsNames[x,0].ToString() &&  (string)childtxt.Text == lsNames[x, 1].ToString() && bolID== false)
                                            {
                                                bolID = true;
                                            }
                                            //Si el nombre del control y el valor del combobox son iguales poner bandera en true
                                            if ( (string)child.Name.ToString() == lsNames[x, 0].ToString() && (string)child.SelectedValue.ToString() == lsNames[x, 1].ToString() && boolVal == false)
                                            {
                                                boolVal =  true;
                                            }

                                            //Si las 2 banderas son true terminar 
                                            if(bolID == true && boolVal== true)
                                            {
                                                return;
                                            }
                                        }
                                    }
                                }

                            }
                            else
                            {
                                MessageBox.Show("Valor no válido para modificar.");
                                return;
                            }
                        }                        

                    }



                    post_webservice(type_accion);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        //Evento para la baja de datos
        private void btnElimina_Click(object sender, RoutedEventArgs e)
        {
            string type_accion = "BAJA";
            try
            {
                MessageBoxResult result = MessageBox.Show("Desea eliminar la información.?", "Edición", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {

                    if (validaStatusNuevo() == false)
                    {
                        MessageBox.Show("Estatus no válido para eliminar");
                        return;
                    }


                    post_webservice(type_accion);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        //Evento para limpiar los datos de los controles
        private void btnLimpia_Click(object sender, RoutedEventArgs e)
        {
            try
            {              

                this.Limpia();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        //Evento para obtener los valores del elemento seleccionado del mapa
        private void btnObtener_Click(object sender, RoutedEventArgs e)
        {

            string tabla = (string)this.lblTabla.Content;

            if (tabla != string.Empty)
            {
                this.cboTablas.SelectedValue = tabla;
                GetTableName((string)tabla);
            }

            //Limpiar 
            this.Limpia();

            if (lblNames.Content != null && lblValues.Content != null)
            {
                //Crear Lista
                int max = 0;
                foreach (string cant in lblNames.Content.ToString().Split('|'))
                {
                    max++;
                }


                string[,] lsNames = new string[max, 2];
                int i = 0;
                foreach (string name in lblNames.Content.ToString().Split('|'))
                {
                    lsNames[i, 0] = name;
                    i++;
                }

                int j = 0;
                foreach (string value in lblValues.Content.ToString().Split('|'))
                {
                    lsNames[j, 1] = value;
                    j++;
                }

                //Recorrer los controles y validar coincidencias e insertar valor en el control

                if (lsNames.Length > 0)
                {
                    //Recorrer Controles de tipo TextBox

                    foreach (var child in stkDatos.Children.OfType<TextBoxEx>())
                    {
                        //Recorrer Lista
                        for (int x = 0; x < max; x++)
                        {
                            if (child.Name == lsNames[x, 0])
                            {
                                child.Text = lsNames[x, 1];
                            }

                        }
                    }

                    //Recorrer Controles tipo ComboBox
                    foreach (var child in stkDatos.Children.OfType<ComboBoxExt>())
                    {
                        //Recorrer Lista
                        for (int x = 0; x < max; x++)
                        {

                            /*SI EL COMBO ES Visible? activarlo*/
                            if(child.Name == "visible?")
                            {
                                child.IsEnabled = true;
                            }

                            if (child.Name == lsNames[x, 0])
                            {
                                
                                //Validación de combo visible?, convertir los valores booleanos en string para asignarlos al combo
                                if (child.Name == "visible?")
                                {
                                    if (Convert.ToBoolean(lsNames[x, 1]) == true)
                                    {
                                        child.SelectedValue = "true";

                                    }
                                    else
                                    {
                                        child.SelectedValue = "false";
                                    }
                                }
                                else
                                {
                                    child.SelectedValue = lsNames[x, 1];
                                }                                
                            }

                        }                    
                    }

                    //Recorrer Controles tipo Date
                    foreach(var child in stkDatos.Children.OfType<DatePickerExt>())
                    {
                        //Recorrer Lista
                        for (int x = 0; x < max; x++)
                        {
                            if (child.Name == lsNames[x, 0])
                            {
                                child.Text = lsNames[x, 1];                              
                            }
                        } 
                    }
                }



            }
        }


        #endregion


        #region Metodos

        //Metódo para leer el archivo xml con la información de los datos
        void LeerXml()
            {
                try
                {

                    //string source = Application.Current.Host.Source.ToString();
                    //int lastSlash = source.LastIndexOf(@"/") + 1;
                    //string xmlLocation = source.Substring(0, lastSlash) + "Datos.xml";
                    //Uri url = new Uri(xmlLocation);


                    WebClient wc = new WebClient();
                    wc.OpenReadCompleted += wc_OpenReadCompleted;
                    Uri uri = new Uri("Datos.xml", UriKind.RelativeOrAbsolute);
                    wc.OpenReadAsync(uri);
                    
                    
                    //wc.OpenReadAsync(url);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message);
                }
            }

        //Metódo para cargar los datos del XML al Combo 
        private void wc_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    //MessageBox.Show("Error: " + e.Error.Message);
                    return;
                }
                using (Stream s = e.Result)
                {
                    doc = XElement.Load(s);
                    //lblInventario.Content = doc.ToString(SaveOptions.OmitDuplicateNamespaces);

                    var tables = (
                                     from tab in doc.Elements("table")
                                     select tab.Attribute("name_external").Value
                                 ).ToList();
                    

                    tables.Add("Seleccione");
                    cboTablas.ItemsSource = tables;
                    cboTablas.SelectedItem = "Seleccione";

                    

                }
            }
            catch { }
        }

        //Método para obtener el nombre de la tabla seleccionada en el combo 
        public void GetTableName(string tableName)
        {
            bool ingrupo = false;

            this.txtLatitudP.Text = string.Empty;
            this.txtLongitudP.Text = string.Empty;
            this.txtLatitudT.Text = string.Empty;
            this.txtLongitudT.Text = string.Empty;
            this.txtLatitudTC.Text = string.Empty;
            this.txtLongitudTC.Text = string.Empty;

            //Validaciones



            List<LiteEdicionSWDefinition> lista = new List<LiteEdicionSWDefinition>();


            var filtro = from tabla in doc.Descendants("table")
                         where (string)tabla.Attribute("name_external") == (string)tableName
                         select tabla;


            lista = (
                          from field in filtro.Elements("field")
                          select new LiteEdicionSWDefinition
                          {
                              Internal_Name = field.Attribute("internal_name") != null ? field.Attribute("internal_name").Value : "false",
                              External_Name = field.Attribute("external_name") != null ? field.Attribute("external_name").Value : "false",
                              Type = field.Attribute("type") != null ? field.Attribute("type").Value : "false",
                              Visible = field.Attribute("visible") != null ? field.Attribute("visible").Value : "false",
                              Datalist = field.Attribute("datalist") != null ? field.Attribute("datalist").Value : "false",
                              Size = field.Attribute("size") != null ? field.Attribute("size").Value : "false",
                              Mandatory = field.Attribute("mandatory") != null ? field.Attribute("mandatory").Value : "false",
                              Enabled = field.Attribute("enabled") != null ? field.Attribute("enabled").Value : "false"
                          }
                    ).ToList();

            subGeneraControles(lista);

            //Activar si es Punto o Trazo
            List<LiteEdicionSWDefinition> lstGeometry = new List<LiteEdicionSWDefinition>();

            lstGeometry = (from tabla in doc.Descendants("table")
                           where (string)tabla.Attribute("name_external") == (string)tableName
                           select new LiteEdicionSWDefinition
                           {
                               Geometry = tabla.Attribute("geometry").Value,
                           }
                         ).ToList();
            if (lstGeometry.Count > 0)
            {
                strgeometry = lstGeometry[0].Geometry;

                if (strgeometry == "punto")
                {
                    this.txtLatitudP.Visibility = Visibility.Visible;
                    this.txtLongitudP.Visibility = Visibility.Visible;

                    this.txtLatitudT.Visibility = Visibility.Collapsed;
                    this.txtLongitudT.Visibility = Visibility.Collapsed;

                    this.txtLatitudTC.Visibility = Visibility.Collapsed;
                    this.txtLongitudTC.Visibility = Visibility.Collapsed;
                }
                else if (strgeometry == "linea")
                {
                    this.txtLatitudP.Visibility = Visibility.Collapsed;
                    this.txtLongitudP.Visibility = Visibility.Collapsed;

                    this.txtLatitudT.Visibility = Visibility.Visible;
                    this.txtLongitudT.Visibility = Visibility.Visible;

                    this.txtLatitudTC.Visibility = Visibility.Collapsed;
                    this.txtLongitudTC.Visibility = Visibility.Collapsed;
                }
                else if (strgeometry == "area")
                {
                    this.txtLatitudP.Visibility = Visibility.Collapsed;
                    this.txtLongitudP.Visibility = Visibility.Collapsed;

                    this.txtLatitudT.Visibility = Visibility.Collapsed;
                    this.txtLongitudT.Visibility = Visibility.Collapsed;

                    this.txtLatitudTC.Visibility = Visibility.Visible;
                    this.txtLongitudTC.Visibility = Visibility.Visible;
                }
            }


            //grupos seguridad
            var grupos = (
              from tab in doc.Elements("group")
              select tab.Attribute("name").Value
              ).ToList();

            if (grupos.Count > 0 && lblGrupos.Content !=null )
            {
                for (int i = 0; i < grupos.Count; i++)
                {
                    foreach (string value in lblGrupos.Content.ToString().Split('|'))
                    {
                       //Si esta en el grupo salir
                        if (grupos[i].ToString() == value)
                        {
                            ingrupo = true;
                            break;
                        } 
                    }              
                }
            }

           



            //Validar si se encuentra en el grupo
            if (ingrupo == false)
            {
                this.btnInserta.IsEnabled = false;
                this.btnModifica.IsEnabled = false;
                this.btnElimina.IsEnabled = false;
                this.btnLimpia.IsEnabled = false;
                this.btnObtener.IsEnabled = false;
            }
            else
            {

                //Validar insert
                var insert = from tabla in doc.Descendants("table")
                             where (string)tabla.Attribute("name_external") == (string)tableName
                             select tabla.Attribute("insert").Value;

                var delete = from tabla in doc.Descendants("table")
                             where (string)tabla.Attribute("name_external") == (string)tableName
                             select tabla.Attribute("delete").Value;

                var modify = from tabla in doc.Descendants("table")
                             where (string)tabla.Attribute("name_external") == (string)tableName
                             select tabla.Attribute("modify").Value;



                if (insert.Count() >0 && insert.First().ToString() == "true" )
                {
                    this.btnInserta.IsEnabled = true;
                }
                else
                {
                    this.btnInserta.IsEnabled = false;
                }
                
                if(delete.Count()> 0 && delete.First().ToString()== "true")
                {
                    this.btnElimina.IsEnabled = true;
                }
                else
                {
                    this.btnElimina.IsEnabled = false;
                }

                if(modify.Count()>0 &&  modify.First().ToString() == "true")
                {
                    this.btnModifica.IsEnabled = true;
                }
                else
                {
                    this.btnModifica.IsEnabled = false;
                }
                
                
                this.btnLimpia.IsEnabled = true;
                this.btnObtener.IsEnabled = true;
            }
        
        }

        //Método para generar los controles dínamicos a partir de una list de elementos del XML
        private void subGeneraControles(List<LiteEdicionSWDefinition> lista)
        {

            stkDatos.Children.Clear();

            LiteEdicionSWDefinition Objetos = new LiteEdicionSWDefinition();

            Binding binding_numero = new Binding()
            {
                Source = Objetos,
                Path = new PropertyPath("Number"),
                Mode = BindingMode.TwoWay,
                NotifyOnValidationError = true,
                ValidatesOnExceptions = true,
                UpdateSourceTrigger= UpdateSourceTrigger.Default
            };


            for (int i = 0; i < lista.Count; i++)
            {
                //Si es text crear un textbox
                if (lista[i].Type == "text" && lista[i].Visible == "true")
                {

                    Label lbl = new Label();
                    lbl.Width = 200;
                    lbl.Margin = new Thickness(10, 0, 0, 0);
                    lbl.Content = lista[i].External_Name;
                    lbl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    lbl.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    stkDatos.Children.Add(lbl);

                    TextBoxEx txt = new TextBoxEx();
                    txt.Width = 300;
                    txt.Margin = new Thickness(10, 0, 0, 0);
                    txt.Name = lista[i].Internal_Name;
                    txt.external_name = lista[i].External_Name;

                    if (txt.Name == "user_gsa")
                    {
                        txt.Text = Convert.ToString( UserLabel.Content);
                    
                    }

                    txt.mandatory = lista[i].Mandatory;
                    txt.IsEnabled = Convert.ToBoolean(lista[i].Enabled);
                    txt.numero = "false";

                    if (!string.IsNullOrEmpty(lista[i].Size) == true)
                    {
                        txt.MaxLength = Convert.ToInt16(lista[i].Size);
                    }

                    txt.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    txt.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    stkDatos.Children.Add(txt);
                }
                //Si es number crear textbox
                if (lista[i].Type == "number" && lista[i].Visible == "true")
                {
                    Label lbl = new Label();
                    lbl.Width = 300;
                    lbl.Margin = new Thickness(10, 0, 0, 0);
                    lbl.Content = lista[i].External_Name;
                    lbl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    stkDatos.Children.Add(lbl);

                    TextBoxEx txt = new TextBoxEx();
                    txt.Width = 300;
                    txt.Margin = new Thickness(10, 0, 0, 0);
                    txt.Name = lista[i].Internal_Name;
                    txt.external_name = lista[i].External_Name;
                    txt.SetBinding(TextBoxEx.TextProperty, binding_numero);
                    txt.mandatory = lista[i].Mandatory;
                    txt.IsEnabled = Convert.ToBoolean(lista[i].Enabled);
                    txt.numero = "true";

                    if (!string.IsNullOrEmpty(lista[i].Size) == true)
                    {
                        txt.MaxLength = Convert.ToInt16(lista[i].Size);
                    }

                    txt.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    stkDatos.Children.Add(txt);
                }
                //si es multiple crear combobox
                if (lista[i].Type == "multiple" && lista[i].Visible == "true")
                {
                    Label lbl = new Label();
                    lbl.Width = 300;
                    lbl.Margin = new Thickness(10, 0, 0, 0);
                    lbl.Content = lista[i].External_Name;
                    lbl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    stkDatos.Children.Add(lbl);

                    ComboBoxExt cbo = new ComboBoxExt();
                    cbo.Width = 300;
                    var array = lista[i].Datalist.Split(',');
                    cbo.ItemsSource = array;
                    cbo.Name = lista[i].Internal_Name;
                    cbo.external_name = lista[i].External_Name;
                    cbo.Margin = new Thickness(10, 0, 0, 0);
                    cbo.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    cbo.mandatory = lista[i].Mandatory;
                    cbo.IsEnabled = Convert.ToBoolean(lista[i].Enabled);
                    cbo.SelectedIndex = 0;
                    stkDatos.Children.Add(cbo);
                }
                //si es date agregar un datepicker
                if (lista[i].Type == "date" && lista[i].Visible == "true")
                {
                    Label lbl = new Label();
                    lbl.Width = 300;
                    lbl.Margin = new Thickness(10, 0, 0, 0);
                    lbl.Content = lista[i].External_Name;
                    lbl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    stkDatos.Children.Add(lbl);

                    DatePickerExt dtp = new DatePickerExt();
                    dtp.Width = 120;
                    dtp.Margin = new Thickness(10, 0, 0, 0);
                    dtp.Name = lista[i].Internal_Name;
                    dtp.external_name = lista[i].External_Name;
                    dtp.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    dtp.mandatory = lista[i].Mandatory;
                    dtp.IsEnabled = Convert.ToBoolean(lista[i].Enabled);
                    dtp.SelectedDateFormat = DatePickerFormat.Short;
                    dtp.SelectedDate = Convert.ToDateTime( DateTime.Today.ToShortDateString());

                    stkDatos.Children.Add(dtp);
                }
            }

        }

        //Método para validar si existen campos mandatory que estan sin llenar
        private bool ValidaMandatory(string type_accion)
        {
            bool band;
            band = true;

            if (stkDatos.Children.Count <= 0)
            {

                band = false;
                MessageBox.Show("No existe Información.");
            }


            foreach (var child in stkDatos.Children.OfType<TextBoxEx>())
            {
                var txt = child.mandatory;

                if (type_accion == "ALTA" && child.Name == "id")
                {
                }
                else
                {
                    if (!string.IsNullOrEmpty(txt) == true)
                    {

                        if (child.mandatory == "true" && string.IsNullOrWhiteSpace(child.Text) == true)
                        {
                            MessageBox.Show("El Campo " + (string)child.external_name + ", es necesario.");
                            band = false;
                        }

                    }
                }
            }
            
            foreach (var child in stkDatos.Children.OfType<ComboBoxExt>())
            {
                var txt = child.mandatory;
                if (!string.IsNullOrEmpty(txt) == true)
                {
                    if (child.mandatory == "true" && string.IsNullOrWhiteSpace((string)child.SelectedValue) == true)
                    {
                        MessageBox.Show("El Campo " + (string)child.external_name + ", es necesario.");
                        band = false;
                    }

                }
            }

            foreach (var child in stkDatos.Children.OfType<DatePickerExt>())
            {
                var txt = child.mandatory;
                if (!string.IsNullOrEmpty(txt) == true)
                {
                    if (child.mandatory == "true" && string.IsNullOrWhiteSpace(child.Text) == true)
                    {
                        MessageBox.Show("El Campo " + (string)child.external_name + ", es necesario.");
                        band = false;
                    }

                }
            }

            return band;
        }

        //Metódo para validar textbox como numero
        bool ValidaNumber()
        {
            bool band = true;

            foreach(var child in stkDatos.Children.OfType<TextBoxEx>())
            {
                //Si el textbox debe llevar texto
                if (child.numero == "true")
                {
                    //Validar si es número
                    if (IsNumeric(child.Text) == false )
                    {
                        MessageBox.Show("El campo " + (string)child.external_name + " debe ser númerico.");
                        band = false;
                    }
                }            
            }
            
            return band;
        }

        //Función checa si es number 
        public static Boolean IsNumeric(string valor)
        {
            Double result;
            return Double.TryParse(valor, out result);
        }

        //Metódo para invocar al WebService
        void post_webservice(string type_accion)
        {

            //WebService
            ServiceEditarLevantamiento.EditEnviarLevantamientoPortTypeClient proxy = new ServiceEditarLevantamiento.EditEnviarLevantamientoPortTypeClient();

            ServiceEditarLevantamiento.EditEnviarLevantamientoRQType peticionType = new ServiceEditarLevantamiento.EditEnviarLevantamientoRQType();

          try
                {

                 ServiceEditarLevantamiento.enviarLevantamientoRequest peticion = new ServiceEditarLevantamiento.enviarLevantamientoRequest();

                peticion.EditEnviarLevantamientoRQ = peticionType;

                //Si hay un error no continuar          
                string strJSON = string.Empty;
                strJSON = create_string_Json(type_accion);
                if ( strJSON == string.Empty)
                {
                    MessageBox.Show("No fue posible generar la información.");
                    return;
                }
                else
                {
                    peticionType.InformacionElementos = strJSON;
                }


                Indicador.InProgress = true;
                Indicador.Visibility = Visibility.Visible;

                proxy.enviarLevantamientoCompleted += new EventHandler<ServiceEditarLevantamiento.enviarLevantamientoCompletedEventArgs>(levantamiento_completado);

                proxy.enviarLevantamientoAsync(peticion);


            }
            catch (CommunicationException ex)
            {
                MessageBox.Show(ex.Message);
                Indicador.InProgress = false;
                Indicador.Visibility = Visibility.Collapsed;
           }

        }


        void levantamiento_completado(object sender, ServiceEditarLevantamiento.enviarLevantamientoCompletedEventArgs e)
        {
            try
            {
                bool exist_box = true;
                if (e.Result == null)
                {
                    MessageBox.Show("Error de Conectividad con el Servidor");
                }
                else
                {
                    string msg = e.Result.EditEnviarLevantamientoRS.GisRespuestaProceso.CodigoRespuesta;
                    string msg2 = e.Result.EditEnviarLevantamientoRS.GisRespuestaProceso.DescripcionError;


                    if (!string.IsNullOrEmpty(msg2))
                    {
                        if (msg2.Contains("Caja de Empalme") == true)
                        {
                            exist_box = false;
                        }
                    }


                    if (msg == "OK")
                    {

                        MessageBox.Show("Proceso generado con exito.");
                    }
                    else
                    {
                       if (exist_box == true)
                       {
                        MessageBox.Show("No se realizo el proceso.");
                       }
                       else
                       {
                        MessageBox.Show(msg2);
                       }
                    }
                }
                Indicador.InProgress = false;
                Indicador.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        //Armar cadena JSON
        string create_string_Json( string type_accion )
        {
            string strLatitud = string.Empty;
            string strLongitud = string.Empty;
            string strJson = string.Empty;
            string strAtributo = string.Empty;
            if (ValidaMandatory(type_accion) == true)
            {
                if (ValidaNumber() == true)
                {

                    List<LiteEdicionSWDefinition> lista = new List<LiteEdicionSWDefinition>();

                    lista = (from tabla in doc.Descendants("table")
                             where (string)tabla.Attribute("name_external") == (string)cboTablas.SelectedValue
                             select new LiteEdicionSWDefinition
                             {
                                 Name = tabla.Attribute("name").Value,
                                 Geometry = tabla.Attribute("geometry").Value,
                                 Dataset = tabla.Attribute("dataset").Value
                             }
                                 ).ToList();


                    if (lista.Count <= 0)
                    {
                        MessageBox.Show("No es posible encontrar el nombre interno de la tabla.");
                        return "";
                    }
                    string table = string.Empty;
                    string geometry = string.Empty;
                    string dataset = string.Empty;
                    table = lista[0].Name;
                    geometry = lista[0].Geometry;
                    dataset = lista[0].Dataset;


                    //Generar Cadena de texto para guardar en la propiedad de atributo del JSON
                    //Recorrer controles
                    foreach (var child in stkDatos.Children.OfType<ComboBoxExt>())
                    {
                        if ((string)cboTablas.SelectedValue == "Optical Splitter" && child.Name == "name2")
                        {
                            child.Name = "spec_id";
                        }
                        
                            strAtributo = strAtributo + child.Name + "$" + child.SelectedValue + "||";                        
                    }

                    foreach (var child in stkDatos.Children.OfType<TextBoxEx>())
                    {
                        //SI el control es id y es alta no agregar al JSON
                        if (child.Name == "id" && type_accion == "ALTA")
                        {
                            strAtributo = strAtributo + "";
                        }
                        else
                        {
                            strAtributo = strAtributo + Convert.ToString(child.Name) + "$" + child.Text + "||";
                        }
                    }

                    foreach (var child in stkDatos.Children.OfType<DatePickerExt>())
                    {
                        string date = Convert.ToDateTime(child.Text).ToShortDateString().ToString();
                        string day = Convert.ToDateTime(child.Text).Day.ToString();
                        string month = Convert.ToDateTime(child.Text).Month.ToString();
                        string year = Convert.ToDateTime(child.Text).Year.ToString();
                        //strAtributo = strAtributo + Convert.ToString(child.Name) + "$" + date  + "||";
                        strAtributo = strAtributo + Convert.ToString(child.Name) + "$" + day.PadLeft(2, '0') + '/' + month.PadLeft(2, '0') + '/' + year.PadLeft(4, '0') + "||";
                    }

                    //Remover ultimos caracteres ||
                    var strlargo = strAtributo.Length;
                    strAtributo = strAtributo.Remove(strlargo - 2, 2);


                    //Validar que longitud y latitud se asigna, punto o trazo
                    if (!string.IsNullOrEmpty(Convert.ToString(txtLatitudP.Text)) && strgeometry == "punto")
                    {
                        strLatitud = Convert.ToString(txtLatitudP.Text);
                    }
                    else if (!string.IsNullOrEmpty(Convert.ToString(txtLatitudT.Text)) && strgeometry == "linea")
                    {
                        strLatitud = Convert.ToString(txtLatitudT.Text);
                    }
                    else if (!string.IsNullOrEmpty(Convert.ToString(txtLatitudTC.Text)) && strgeometry == "area")
                    {
                        //Crear Lista con Lat
                        int max = 0;
                        //Obtener el tamaño de la cadena
                        foreach (string cant in txtLatitudTC.Text.Split(','))
                        {
                            max++;
                        }                        
                        
                        //Crear la matriz 
                        string[] lstGeom = new string[max];

                        int i = 0;
                        //Recorrer la cadena de latitud y obtener sus elementos
                        foreach (string geom in txtLatitudTC.Text.Split(','))
                        {
                            lstGeom[i] = geom;
                            i++;
                        }


                        strLatitud = Convert.ToString(txtLatitudTC.Text);

                        strLatitud = strLatitud + "," + lstGeom[0].ToString();

                    }


                    if (!string.IsNullOrEmpty(Convert.ToString(txtLongitudP.Text)) && strgeometry == "punto")
                    {
                        strLongitud = Convert.ToString(txtLongitudP.Text);
                    }
                    else if (!string.IsNullOrEmpty(Convert.ToString(txtLongitudT.Text)) && strgeometry == "linea")
                    {
                        strLongitud = Convert.ToString(txtLongitudT.Text);
                    }
                    else if (!string.IsNullOrEmpty(Convert.ToString(txtLongitudTC.Text)) && strgeometry == "area")
                    {
                        //Crear Lista con Lon
                        int max = 0;
                        //Obtener el tamaño de la cadena
                        foreach (string cant in txtLongitudTC.Text.Split(','))
                        {
                            max++;
                        }

                        //Crear la matriz 
                        string[] lstGeom = new string[max];

                        int i = 0;
                        //Recorrer la cadena de latitud y obtener sus elementos
                        foreach (string geom in txtLongitudTC.Text.Split(','))
                        {
                            lstGeom[i] = geom;
                            i++;
                        }
                        
                        strLongitud = Convert.ToString(txtLongitudTC.Text);

                        strLongitud = strLongitud + "," + lstGeom[0].ToString();
                    }
                    
                    //Obtener la cadena JSON
                    strJson = WriteFromObject(strLatitud, strLongitud, Convert.ToString(strAtributo), table, type_accion, geometry, dataset);

                }//endif validate number
            }//endif Mandatory

            return strJson;
        }

        // Crear un Objeto y serializarlo a JSON.
        public static string WriteFromObject(string strLat, string strLon, string atributte, string name_table, string accion_type, string geometry, string vdataset)
        {
            try
            {

                //Crear Lista con Lat,Lon
                int max = 0;
                //Obtener el tamaño de la cadena
                foreach (string cant in strLat.Split(','))
                {
                    max++;
                }

                //Crear la matriz 
                string[,] lstGeom = new string[max, 2];
               
                
                int i = 0;
                //Recorrer la cadena de latitud y obtener sus elementos
                foreach (string geom in strLat.Split(','))
                {
                    lstGeom[i,0] = geom;
                    i++;
                }

                int j = 0;
                //Recorrer la cadena de longitud y obtener sus elementos
                foreach (string geom in strLon.Split(','))
                {
                    lstGeom[j,1] = geom;
                    j++;
                }


                List<Punto> lista = new List<Punto>();
                //Si viene vacio es que es una modificación o eliminación sin trazo
                if (!string.IsNullOrWhiteSpace(strLat) && !string.IsNullOrWhiteSpace(strLon))
                {
                    for (int lat = 0; lat < max; lat++)
                    {

                        //Create Points
                        Punto objPunto = new Punto()
                        {
                            lat = lstGeom[lat, 0],
                            lng = lstGeom[lat, 1]
                        };
                        lista.Add(objPunto);
                    }
                }

                //Create Geometry
                Geometria objGeometria = new Geometria() 
                {
                    
                    puntos = lista ,
                    texto = "",
                    tipo_geometria = geometry
                };

                //Create Geometrys
                Geometrias objGeometrias = new Geometrias()
                {
                    geometria = objGeometria
                };

                //Create Atributos
                Atributos objAtributos = new Atributos() 
                {
                    atributo = atributte
                };

                //Create Registro
                Registro  objRegistro = new Registro()
                {
                    geometrias = objGeometrias,
                    accion = accion_type,
                    dataset = vdataset, //"design_admin",
                    atributos = objAtributos,
                    nombre_tabla = name_table

                };


                List<Registro> lstRegistro = new List<Registro>();
                lstRegistro.Add(objRegistro);
                //Create Registros
                Registros objRegistros = new Registros() 
                {
                    registro = lstRegistro
                };


                //Create InfoFieldsis
                InfoFieldsis objInfoFieldsis = new InfoFieldsis()
                {
                    registros = objRegistros,
                    id_proyecto = "",
                    total_registros = 1,
                    id_movil = "",
                    definitivo = "1"
                };

                RootObject objRootObject = new RootObject() 
                {
                    info_fieldsis = objInfoFieldsis
                };

                


                //Create a stream to serialize the object to.
                MemoryStream ms = new MemoryStream();

                // Serializer the User object to the stream.
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(RootObject));
                ser.WriteObject(ms, objRootObject);
                byte[] json = ms.ToArray();
                ms.Close();
                return Encoding.UTF8.GetString(json, 0, json.Length);

            }
            catch (Exception e)
            {
                MessageBox.Show("Error: '{0}'" +  e.Message);
                return "";
            }
        }


        //Metódo para limpiar los controles
        void Limpia()
        {
            this.txtLatitudP.Text = string.Empty;
            this.txtLongitudP.Text = string.Empty;
            this.txtLatitudT.Text = string.Empty;
            this.txtLongitudT.Text = string.Empty;
            this.txtLatitudTC.Text = string.Empty;
            this.txtLongitudTC.Text = string.Empty;


            if (stkDatos.Children.Count > 0)
            {
                foreach (var child in stkDatos.Children.OfType<TextBoxEx>())
                {
                    var txt = child.mandatory;
                    if (!string.IsNullOrEmpty(txt) == true)
                    {
                        child.Text = String.Empty;
                    }

                    if (child.Name == "user_gsa")
                    {
                        child.Text = Convert.ToString(UserLabel.Content);

                    }
                }

                foreach (var child in stkDatos.Children.OfType<DatePickerExt>())
                {
                    child.Text = String.Empty;
                }

                foreach (var child in stkDatos.Children.OfType<ComboBoxExt>())
                {
                    if (child.Name == "status")
                    {
                        child.SelectedIndex = 0;                    
                    }


                    if (child.Name == "visible?")
                    {
                        child.IsEnabled = false;
                        child.SelectedIndex = 0;
                    }
                }

            }
        }

        //Metódo para validar si el status es Nuevo
        bool validaStatusNuevo()
        {
            bool band = true;
            if (stkDatos.Children.Count > 0)
            {
                foreach (var child in stkDatos.Children.OfType<ComboBoxExt>())
                {
                    if (child.Name == "status")
                    {
                        if (child.SelectedValue.ToString() == "NUEVO")
                        {
                            band = true;
                        }
                        else
                        {
                            band = false;
                        }
                    }
                }
            }
            return band;
        }
        #endregion



    }//end class
}//end namespace
