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
    
    public partial class tbd_Pre_Factura
    {
        public int id_pre_factura { get; set; }
        public int id_usuario { get; set; }
        public string rfc_usuario { get; set; }
        public string uuid { get; set; }
        public string nombre_usuario_rfc { get; set; }
        public string reg_fiscal_usuario { get; set; }
        public string rfc_cliente { get; set; }
        public string nombre_rfc { get; set; }
        public string serie { get; set; }
        public string folio { get; set; }
        public string tipo_comprobante { get; set; }
        public string exportacion { get; set; }
        public string uso_factura { get; set; }
        public string lugar_expedicion { get; set; }
        public string tipo_factura { get; set; }
        public Nullable<int> forma_pago { get; set; }
        public Nullable<int> metodo_pago { get; set; }
        public string numero_pedido { get; set; }
        public string moneda { get; set; }
        public string tipo_cambio { get; set; }
        public Nullable<System.DateTime> fecha_emision { get; set; }
        public string numero_cuenta { get; set; }
        public string nom_banco { get; set; }
        public string cond_pago { get; set; }
        public string cuenta_predial { get; set; }
        public string observacion { get; set; }
        public string clave_reg_fiscal { get; set; }
        public Nullable<int> clave_uso_cfdi { get; set; }
        public string subtotal { get; set; }
        public string total_iva { get; set; }
        public string total_iva_ret { get; set; }
        public string total_isr_ret { get; set; }
        public string descuento2 { get; set; }
        public string total { get; set; }
        public string total_imp_ret { get; set; }
        public string tipo { get; set; }
        public string selloCFDI { get; set; }
        public string selloSAT { get; set; }
        public string ccertificacion { get; set; }
        public Nullable<System.DateTime> fca_timbrado { get; set; }
        public string url_pdf { get; set; }
        public string url_xml { get; set; }
        public string url_img { get; set; }
        public Nullable<byte> status { get; set; }
        public string version_timbrado { get; set; }
    }
}
