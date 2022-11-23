using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facturafast.Models
{
    public class Certificado
    {
        public String NoCertificado { get; set; }
        public DateTime FechaVigencia { get; set; }
        public Int32 Estatus { get; set; }
        public String Mensaje { get; set; }
        public String RFC { get; set; }
    }

    public class PFXResponse
    {
        public Int32 Estatus { get; set; }
        public String Mensaje { get; set; }
        public String URL { get; set; }
    }

    public class ConceptosNota
    {
        public string seleccion { get; set; }
        public string acciones { get; set; }
        public string clave { get; set; }
        public string concepto { get; set; }
        public int id_sat { get; set; }
        public string clave_sat { get; set; }
        public decimal cantidad { get; set; }
        public int id_unidad_medida { get; set; }
        public string unidad { get; set; }
        public decimal precio_unitario { get; set; }
        public decimal importe { get; set; }
        public int id_iva { get; set; }
        public int id_iva_ret { get; set; }
        public int id_isr { get; set; }
        public decimal total_iva { get; set; }
        public decimal total_iva_ret { get; set; }
        public decimal total_isr { get; set; }
        public int tipo_descuento { get; set; }
        public decimal descuento { get; set; }
        public decimal total_descuento { get; set; }
        public decimal total { get; set; }
        public int id_detalle_nota_venta { get; set; }
        public int id_nota_venta { get; set; }
        public string c_prod_serv { get; set; }
        public string c_producto { get; set; }
        public string c_unidad_medida { get; set; }
        public string importe_unitario { get; set; }
        public string importe_total { get; set; }
        public string obj_impuesto { get; set; }
        public string iva_imp_traslado { get; set; }
        public string tipo_factor { get; set; }
        public string iva_tasa { get; set; }
        public string iva_ret_tasa { get; set; }
        public string iva_tasa_impuesto { get; set; }
        public string iva_ret { get; set; }
        public string iva_ret_impuesto { get; set; }
        public string isr_ret { get; set; }
        public string isr_ret_tasa { get; set; }
        public string isr_ret_impuesto { get; set; }
        public decimal tipo_ieps { get; set; }
        public string ieps { get; set; }
        public string total_imp_retenido { get; set; }
    }

    public class ClientesNota
    {
        public int id_cliente { get; set; }
        public int num_notas { get; set; }
        public int notas_pagas { get; set; }
        public int notas_pendientes { get; set; }
        public int notas_canceladas { get; set; }
        public decimal saldo_restante { get; set; }
        public decimal saldo_pagado { get; set; }
        public decimal saldo_acumulado { get; set; }
        public string rfc { get; set; }
        public string nombre_razon { get; set; }
    }

    public class ListaCorreos
    {
        public string nombre { get; set; }
        public string puesto { get; set; }
        public string correo { get; set; }
        public string rfc { get; set; }
        public string nombre_razon { get; set; }
    }

    public class PreFactura
    {
        public string reg_fiscal_usuario { get; set; }
        public string serie { get; set; }
        public string nombre_usuario_rfc { get; set; }
        public string rfc_usuario { get; set; }
        //public int id_pre_factura { get; set; }
        public string folio { get; set; }
        public string tipo_comprobante { get; set; }
        public string exportacion { get; set; }
        public string nombre_rfc_pf { get; set; }
        public string rfc_cliente_pf { get; set; }
        public string clave_reg_fiscal { get; set; }
        public int clave_uso_cfdi { get; set; }
        public string uso_factura { get; set; }
        public string lugar_expedicion { get; set; }
        public string tipo_factura { get; set; }
        public int forma_pago { get; set; }
        public int metodo_pago { get; set; }
        public string numero_pedido { get; set; }
        public string moneda { get; set; }
        public string tipo_cambio { get; set; }
        public string fecha_emision { get; set; }
        public string numero_cuenta { get; set; }
        public string nom_banco { get; set; }
        public string cond_pago { get; set; }
        public string observacion { get; set; }
        public string cuenta_predial { get; set; }
        public string subtotal { get; set; }
        public string total_iva { get; set; }
        public string total_iva_ret { get; set; }
        public string total_isr_ret { get; set; }
        public string descuento2 { get; set; }
        public string total { get; set; }
        public string status { get; set; }
    }

    public class CfdiUuid
    {
        public string id_relacion { get; set; }
        public string uuid { get; set; }
    }

    public class ConceptosPreFactura
    {
        public int id_pre_factura { get; set; }
        public int id_sat { get; set; }
        public string c_prod_serv { get; set; }
        public string c_producto { get; set; }
        public string cantidad { get; set; }
        public string c_unidad_medida { get; set; }
        public string unidad { get; set; }
        public string concepto { get; set; }
        public string importe_unitario { get; set; }
        public string importe_total { get; set; }
        public string descuento { get; set; }
        public string obj_impuesto { get; set; }
        public string iva_tasa { get; set; }
        public string iva_imp_traslado { get; set; }
        public string tipo_factor { get; set; }
        public string iva_tasa_impuesto { get; set; }
        public string iva_ret { get; set; }
        public string iva_ret_tasa { get; set; }
        public string iva_ret_impuesto { get; set; }
        public string isr_ret { get; set; }
        public string isr_ret_tasa { get; set; }
        public string isr_ret_impuesto { get; set; }
        public string tipo_ieps { get; set; }
        public string ieps { get; set; }
        public string v_ieps { get; set; }
        public string total { get; set; }
        public string total_imp_retenido { get; set; }

    }

    public class PrePago
    {
        public int id_factura { get; set; }
        public int id_cliente { get; set; }
        public DateTime fecha_pago { get; set; }
        public DateTime f_emision { get; set; }
        public string hora { get; set; }
        public int metodo_pago { get; set; }
        public int num_operacion { get; set; }
        public string serie { get; set; }
        public string folio { get; set; }
        public string tipo_cambio { get; set; }
        public int tipo_moneda { get; set; }
        public decimal total { get; set; }
        public int uso_cfdi { get; set; }
        public string uuid { get; set; }
        public string url_pdf { get; set; }
        public string url_xml { get; set; }
        public int status { get; set; }
    }

    public class DetallePrePago
    {
        public int id_pre_pago { get; set; }
        public string forma_pago { get; set; }
        public int d_forma_pago { get; set; }
        public string uuid { get; set; }
        public int pago_no { get; set; }
        public string s_anterior { get; set; }
        public string pago { get; set; }
        public string s_actual { get; set; }
        public int status { get; set; }
    }

    public class PreCarta
    {
        public int id { get; set; }
        public int id_cfdi { get; set; }
        public int id_emisor { get; set; }
        public int id_receptor { get; set; }
        public int id_prefactura { get; set; }
        public int id_ubicacion_o { get; set; }
        public int id_ubicacion_d { get; set; }
        public int id_mercancia { get; set; }
        public int id_figura { get; set; }
        public int id_autotransporte { get; set; }
        public string transporte_inter { get; set; }
        public string e_s_mercancia { get; set; }
        public string pais_ori_des { get; set; }
        public string total_distancia_rec { get; set; }
        public string num_estacion_o { get; set; }
        public string dista_rec_o { get; set; }
        public string dista_rec_d { get; set; }
        public string num_estacion_d { get; set; }
        public DateTime fca_hora_o { get; set; }
        public DateTime fca_hora_d { get; set; }
        public string tipo_figura { get; set; }
        public string res_fiscal_figura { get; set; }
        public string status { get; set; }

    }
    public class AutoTransporte {
        public int id_autotransporte { get; set; }
        public int id_usuario { get; set; }
        public int id_cliente { get; set; }
        public int id_tipo_permiso{ get; set; }
        public string num_permiso_sct{ get; set; }
        public int id_conf_autotrans{ get; set; }
        public string placa_vm { get; set; }
        public int anio_modelo_vm { get; set; }
        public string asegura_resp_civil { get; set; }
        public string poliza_resp_civil { get; set; }
        public string asegura_med_ambiente { get; set; }
        public string poliza_med_ambiente { get; set; }
        public string asegura_carga { get; set; }
        public string poliza_carga { get; set; }
        public decimal prima_seguro { get; set; }
        public DateTime fecha_creacion { get; set; }
    }
    public class UbicacionCartaPorte 
    {
        public int id { get; set; }
        public int id_pre_carta { get; set; }
        public int idubicacion { get; set; }
        public string tipo_ubicacion { get; set; }
        public int num_estacion { get; set; }
        public decimal distancia_recorrida { get; set; }
        public DateTime fca_hora_salida { get; set; }
        public string status { get; set; }
    }
    //-----------------------------------------------------------------------------------
}