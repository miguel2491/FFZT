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
    
    public partial class tbd_Conceptos_Factura
    {
        public int id_concepto_factura { get; set; }
        public int id_factura { get; set; }
        public decimal cantidad { get; set; }
        public string c_pord_serv { get; set; }
        public string c_unidad { get; set; }
        public string descripcion { get; set; }
        public decimal descuento { get; set; }
        public decimal importe { get; set; }
        public string unidad { get; set; }
        public decimal valor_unitario { get; set; }
    }
}
