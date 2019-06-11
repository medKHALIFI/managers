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

namespace Lite
{
    public class LiteEdicionSWDefinition
    {

        private string _tables;
        private string _table;
        private string _name;
        private string _name_external;
        private string _geometry;
        private string _dataset;
        private string _internal_name;
        private string _external_name;
        private string _type;
        private string _visible;
        private string _datalist;
        private string _size;
        private Double _number;
        private string _mandatory;
        private string _enabled;




        public string Tables
        {
            get { return _tables; }
            set { _tables = value; }
        }

        public string Table
        {
            get { return _table; }
            set { _table = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Name_External
        {
            get { return _name_external; }
            set { _name_external = value; }
        }

        public string Geometry
        {
            get { return _geometry; }
            set { _geometry = value; }
        }

        public string Dataset
        {
            get { return _dataset; }
            set { _dataset = value; }
        }

        public string Internal_Name
        {
            get { return _internal_name; }
            set { _internal_name = value; }
        }

        public string External_Name
        {
            get { return _external_name; }
            set { _external_name = value; }
        }

        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }
        public string Datalist
        {
            get { return _datalist; }
            set { _datalist = value; }
        }

        public string Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public Double Number
        {
            get { return _number; }
            set { _number = value; }
        }

        public string Mandatory
        {
            get { return _mandatory; }
            set { _mandatory = value; }
        }

        public string Enabled
        {
            get { return _enabled; }
            set { _enabled = value; } 
        }
    }
}
