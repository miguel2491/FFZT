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
    
    public partial class tbd_Servicios_Recurrentes
    {
        public int id_servicio_recurrente { get; set; }
        public int id_usuario { get; set; }
        public string rfc_usuario { get; set; }
        public int id_cliente { get; set; }
        public System.DateTime fecha_creacion { get; set; }
        public System.DateTime fecha_inicio { get; set; }
        public int id_periodicidad { get; set; }
        public string serie { get; set; }
        public int id_estatus { get; set; }
        public decimal total { get; set; }
        public System.DateTime fecha_proxima { get; set; }
        public System.DateTime fecha_ultima { get; set; }
        public int id_cuenta_bancaria { get; set; }
    }
}
