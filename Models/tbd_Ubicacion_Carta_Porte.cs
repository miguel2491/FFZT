//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Facturafast.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbd_Ubicacion_Carta_Porte
    {
        public int id { get; set; }
        public int id_pre_carta { get; set; }
        public int id_ubicacion { get; set; }
        public string tipo_ubicacion { get; set; }
        public Nullable<int> num_estacion { get; set; }
        public Nullable<decimal> distancia_recorrida { get; set; }
        public Nullable<System.DateTime> fca_hora_salida { get; set; }
        public string status { get; set; }
    }
}
