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
    
    public partial class tbd_Archivos_Descarga
    {
        public int id_archivo_descarga { get; set; }
        public string url_archivo { get; set; }
        public string rfc { get; set; }
        public string descarga_como { get; set; }
        public System.DateTime fecha_descarga { get; set; }
        public bool almacenado { get; set; }
        public string relacion { get; set; }
        public int total_xml { get; set; }
        public int num_xml { get; set; }
        public int id_solicitud_descarga { get; set; }
    }
}
