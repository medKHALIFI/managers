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
using System.Collections.Generic;


//namespace Lite
//{
    /*
    public class LiteEdicionSwObjetoJson
    {
        public string info_fieldsis { get; set; } 

        public string registros { get; set; }

        public string id_proyecto { get; set; }

        public string total_registros { get; set; }

        public string id_movil { get; set; }

        public string definitivo { get; set; }

        //Pertenecen a registros
        public string registro { get; set; }

        public string geometrias { get; set; }

        public string accion { get; set; }

        public string dataset { get; set; }

        public string atributos { get; set; }       

        public string nombre_tabla { get; set; }

        //Pertenecen a pl_geometrias
        public string texto { get; set; }

        public string tipo_geometria { get; set; }

        public string puntos { get; set; }

        //Pertencen a lst_puntos
        public string lng { get; set; }

        public string lat { get; set; }

        //Pertenece a atributos
        public string atributo { get; set; }
    }
    */

    ///////////////////////////////////////////////
    public class Punto
    {
        public string lat { get; set; }
        public string lng { get; set; }
    }

    public class Geometria
    {
        public List<Punto> puntos { get; set; }
        public string texto { get; set; }
        public string tipo_geometria { get; set; }
    }

    public class Geometrias
    {
        public Geometria geometria { get; set; }
    }

    public class Atributos
    {
        public string atributo { get; set; }
    }

    public class Registro
    {
        public Geometrias geometrias { get; set; }
        public string accion { get; set; }
        public string dataset { get; set; }
        public Atributos atributos { get; set; }
        public string nombre_tabla { get; set; }
    }

    public class Registros
    {
        public List<Registro> registro { get; set; }
    }

    public class InfoFieldsis
    {
        public Registros registros { get; set; }
        public string id_proyecto { get; set; }
        public int total_registros { get; set; }
        public string id_movil { get; set; }
        public string definitivo { get; set; }
    }

    public class RootObject
    {
        public InfoFieldsis info_fieldsis { get; set; }
    }
//}
