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
    
    public partial class tbd_Firmas
    {
        public int id_firma { get; set; }
        public string url_cer_sello { get; set; }
        public string url_key_sello { get; set; }
        public Nullable<System.DateTime> fecha_sello { get; set; }
        public string certificado_sello { get; set; }
        public string password_sello { get; set; }
        public string url_pfx_sello { get; set; }
        public string url_cer_fiel { get; set; }
        public string url_key_fiel { get; set; }
        public string password_fiel { get; set; }
        public Nullable<System.DateTime> fecha_fiel { get; set; }
        public string certificado_fiel { get; set; }
        public string url_pfx_fiel { get; set; }
        public System.DateTime fecha_creacion { get; set; }
        public int id_usuario { get; set; }
        public string rfc { get; set; }
        public bool es_carga_inicial { get; set; }
        public string password_ciec { get; set; }
    }
}
