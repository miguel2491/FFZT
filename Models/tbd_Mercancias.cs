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
    
    public partial class tbd_Mercancias
    {
        public int id_mercancia { get; set; }
        public int id_usuario { get; set; }
        public Nullable<int> id_cliente { get; set; }
        public int id_sat { get; set; }
        public Nullable<int> id_clave_stcc { get; set; }
        public string descripcion { get; set; }
        public decimal cantidad { get; set; }
        public int id_unidad_medida { get; set; }
        public string unidad { get; set; }
        public string dimensiones { get; set; }
        public string material_peligroso { get; set; }
        public Nullable<int> id_material_peligroso { get; set; }
        public Nullable<int> id_tipo_embalaje { get; set; }
        public string descrip_embalaje { get; set; }
        public decimal peso_kg { get; set; }
        public Nullable<decimal> valor_mercancia { get; set; }
        public Nullable<int> id_moneda { get; set; }
        public Nullable<int> id_fraccion_arancelaria { get; set; }
        public string uuid_comercio_ext { get; set; }
        public int id_unidad_peso_m { get; set; }
        public decimal peso_bruto { get; set; }
        public decimal peso_neto { get; set; }
        public decimal peso_tara { get; set; }
        public Nullable<int> numero_piezas { get; set; }
        public System.DateTime fecha_creacion { get; set; }
    }
}
