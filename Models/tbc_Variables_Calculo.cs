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
    
    public partial class tbc_Variables_Calculo
    {
        public int id_variable { get; set; }
        public decimal comision { get; set; }
        public string access_token_mp { get; set; }
        public string public_key_mp { get; set; }
        public int periodo_descarga { get; set; }
        public string url_proyecto { get; set; }
        public string url_descargas { get; set; }
        public string url_facturas { get; set; }
        public string url_back_mp { get; set; }
        public string url_pdf { get; set; }
        public string url_docx { get; set; }
        public string url_libreoffice { get; set; }
        public string url_servicio_recurrente { get; set; }
    }
}