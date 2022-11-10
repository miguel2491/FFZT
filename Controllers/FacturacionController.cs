using Facturafast.CLS40;
using Facturafast.Models;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Word = Microsoft.Office.Interop.Word;

namespace Facturafast.Controllers
{
    public class FacturacionController : Controller
    {
        BD_FFEntities db;
        // Vista ListFactura
        public ActionResult ListaFactura()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }
            DateTime Final = DateTime.Now.AddDays(1);
            DateTime Inicio = new DateTime(Final.Year, Final.Month, 1);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;
            return View();
                            
            //db.tbd_Pre_Factura.Where(s => s.rfc_usuario == usuario.rfc && s.fecha_emision >= Inicio && s.fecha_emision <= Final && s.status != 0 && s.tipo == "Factura").ToList();

        }
        //Vista Conceptos
        public ActionResult ListaComplemento()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }
            DateTime Final = DateTime.Now.AddDays(1);
            DateTime Inicio = new DateTime(Final.Year, Final.Month, 1);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;
            return View();
        }
        //Vista Form Factura
        public ActionResult Factura(Int32? id = 0)
        {
            if (id == 0)
            {
                ViewBag.id = 0;
                return View();
            }
            else
            {
                ViewBag.id = id;
                return View(id);
            }
        }
        public ActionResult Complemento(Int32? id = 0)
        {
            if (id == 0)
            {
                ViewBag.id = 0;
                return View();
            }
            else
            {
                ViewBag.id = id;
                return View(id);
            }
        }
        public ActionResult TimbrarFac(Int32? id)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (id == null)
                return RedirectToAction("Clientes", "Catalogos");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var factura = db.tbd_Pre_Factura.Where(s => s.id_pre_factura == id).Single();
            ViewBag.IdPreFactura = factura;
            ViewBag.Estatus = factura.status;
            return View(factura);
        }
        public ActionResult saveFactura(List<PreFactura> prefactura, List<CfdiUuid> uuid, List<ConceptosPreFactura> concepto, Int32? id_pre_factura)
        {

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            int id_pre_fac = 0;
            foreach (var item in prefactura)
            {
                decimal subtotal_ = Math.Round(Convert.ToDecimal(item.subtotal), 2);
                string subtotal = subtotal_.ToString();//Regex.Replace(prefac.subtotal, ",", "");
                                                       //subtotal = subtotal.Substring(1);

                decimal total_iva_ = Math.Round(Convert.ToDecimal(item.total_iva), 2);
                string total_iva = total_iva_.ToString();

                decimal total_iva_ret_ = Math.Round(Convert.ToDecimal(item.total_iva_ret), 2);
                string total_iva_ret = total_iva_ret_.ToString();

                decimal total_isr_ret_ = Math.Round(Convert.ToDecimal(item.total_isr_ret), 2);
                string total_isr_ret = total_isr_ret_.ToString();

                decimal descuento2_ = item.descuento2 == null ? 0 : Math.Round(Convert.ToDecimal(item.descuento2), 2);
                string descuento2 = descuento2_.ToString();

                decimal total_ = Math.Round(Convert.ToDecimal(item.total), 2);
                string total = total_.ToString();
                DateTime fecha_emision = Convert.ToDateTime(item.fecha_emision);
                var reg = db.tbc_Regimenes.Where(s => s.id_regimen_fiscal == usuario.id_regimen_fiscal).Single();
                tbd_Pre_Factura nuevaPre = new tbd_Pre_Factura
                {
                    id_usuario = usuario.id_usuario,
                    rfc_usuario = usuario.rfc,
                    nombre_usuario_rfc = usuario.nombre_razon,
                    serie = item.serie,
                    folio = item.folio,
                    tipo_comprobante = item.tipo_comprobante,
                    exportacion = item.exportacion,
                    reg_fiscal_usuario = reg.clave.ToString(),
                    rfc_cliente = item.rfc_cliente_pf,
                    nombre_rfc = item.nombre_rfc_pf,
                    uso_factura = item.uso_factura,
                    clave_reg_fiscal = item.clave_reg_fiscal,
                    clave_uso_cfdi = item.clave_uso_cfdi,
                    lugar_expedicion = item.lugar_expedicion,
                    tipo_factura = item.tipo_factura,
                    forma_pago = item.forma_pago,
                    metodo_pago = item.metodo_pago,
                    numero_pedido = item.numero_pedido == null ? "" : item.numero_pedido,
                    moneda = item.moneda,
                    tipo_cambio = item.tipo_cambio == null ? "" : item.tipo_cambio,
                    fecha_emision = fecha_emision,
                    numero_cuenta = item.numero_cuenta == null ? "" : item.numero_cuenta,
                    nom_banco = item.nom_banco == null ? "" : item.nom_banco,
                    cond_pago = item.cond_pago == null ? "" : item.cond_pago,
                    cuenta_predial = item.cuenta_predial == null ? "" : item.cuenta_predial,
                    observacion = item.observacion == null ? "" : item.observacion,
                    subtotal = subtotal,
                    total_iva = total_iva,
                    total_iva_ret = total_iva_ret,
                    total_isr_ret = total_isr_ret,
                    descuento2 = descuento2 == "0.00" ? "" : descuento2,
                    total = total,
                    total_imp_ret = total,
                    status = 1,
                    tipo = "Factura"
                };
                db.tbd_Pre_Factura.Add(nuevaPre);
                db.SaveChanges();
                id_pre_fac = nuevaPre.id_pre_factura;
            }
            if (uuid != null)
            {
                foreach (var item in uuid)
                {
                    tbd_Cfdi_Uuid nuevoUuid = new tbd_Cfdi_Uuid
                    {
                        id_pre_factura = id_pre_fac,
                        id_relacion = item.id_relacion,
                        uuid = item.uuid
                    };
                    db.tbd_Cfdi_Uuid.Add(nuevoUuid);
                    db.SaveChanges();
                }
            }
            if (concepto != null)
            {
                foreach (var item in concepto)
                {
                    var clv_prod = db.tbc_ProdServ.Where(u => u.id_sat == item.id_sat).Select(u => u.c_pord_serv).First();
                    tbd_Conceptos_Pre_Factura nuevoConceptoPF = new tbd_Conceptos_Pre_Factura
                    {
                        id_pre_factura = id_pre_fac,
                        c_prod_serv = clv_prod,//item.c_prod_serv,
                        c_producto = item.c_producto,
                        id_sat = item.id_sat,
                        cantidad = item.cantidad,
                        c_unidad_medida = item.c_unidad_medida,
                        unidad = item.unidad,
                        concepto = item.concepto,
                        importe_unitario = item.importe_unitario,
                        importe_total = item.importe_total,
                        descuento = item.descuento == null ? "" : item.descuento,
                        obj_impuesto = item.obj_impuesto,
                        iva_imp_traslado = item.iva_imp_traslado,
                        tipo_factor = item.tipo_factor,
                        iva_tasa = item.iva_tasa,
                        iva_tasa_impuesto = item.iva_tasa_impuesto,
                        iva_ret = item.iva_ret,
                        iva_ret_tasa = item.iva_ret_tasa,
                        iva_ret_impuesto = item.iva_ret_impuesto,
                        isr_ret = item.isr_ret,
                        isr_ret_tasa = item.isr_ret_tasa,
                        isr_ret_impuesto = item.isr_ret_impuesto,
                        tipo_ieps = item.tipo_ieps,
                        ieps = item.ieps == null ? "" : item.ieps,
                        v_ieps = item.v_ieps == null ? "" : item.v_ieps,
                        total_imp_retenido = item.total_imp_retenido,
                        total = item.total
                    };
                    db.tbd_Conceptos_Pre_Factura.Add(nuevoConceptoPF);
                    db.SaveChanges();
                }
            }
            return Json(new { id = id_pre_fac,tipo="Guardar"});
        }
        public ActionResult updateFactura(List<PreFactura> prefactura, List<CfdiUuid> uuid, List<ConceptosPreFactura> concepto, Int32? id_pref)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                //db.Configuration.LazyLoadingEnabled = false;
                //Delete
                foreach (var cfdi in db.tbd_Cfdi_Uuid.Where(x => x.id_pre_factura == id_pref))
                {
                    db.tbd_Cfdi_Uuid.Remove(cfdi);
                }
                
                foreach (var conceptos in db.tbd_Conceptos_Pre_Factura.Where(x => x.id_pre_factura == id_pref))
                {
                    db.tbd_Conceptos_Pre_Factura.Remove(conceptos);
                }
                //UpdatePrefactura
                foreach (var prefac in prefactura)
                {
                    decimal subtotal_ = Math.Round(Convert.ToDecimal(prefac.subtotal),2);
                    string subtotal = subtotal_.ToString();//Regex.Replace(prefac.subtotal, ",", "");
                    //subtotal = subtotal.Substring(1);
                    
                    decimal total_iva_ = Math.Round(Convert.ToDecimal(prefac.total_iva), 2);
                    string total_iva = total_iva_.ToString();
                    
                    decimal total_iva_ret_ = Math.Round(Convert.ToDecimal(prefac.total_iva_ret), 2);
                    string total_iva_ret = total_iva_ret_.ToString();
                    
                    decimal total_isr_ret_ = Math.Round(Convert.ToDecimal(prefac.total_isr_ret), 2);
                    string total_isr_ret = total_isr_ret_.ToString();
                    
                    decimal descuento2_ = prefac.descuento2 == null ? 0:Math.Round(Convert.ToDecimal(prefac.descuento2), 2);
                    string descuento2 = descuento2_.ToString();
                    
                    decimal total_ = Math.Round(Convert.ToDecimal(prefac.total), 2);
                    string total = total_.ToString();

                    db.Configuration.LazyLoadingEnabled = false;
                    var valor = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == id_pref).FirstOrDefault();
                    
                    valor.rfc_cliente = prefac.rfc_cliente_pf;
                    valor.nombre_rfc = prefac.nombre_rfc_pf;
                    valor.uso_factura = prefac.uso_factura;
                    valor.serie = prefac.serie;
                    valor.folio = prefac.folio;
                    valor.tipo_comprobante = prefac.tipo_comprobante;
                    valor.exportacion = prefac.exportacion;
                    valor.lugar_expedicion = prefac.lugar_expedicion;
                    valor.tipo_factura = prefac.tipo_factura;
                    valor.forma_pago = prefac.forma_pago;
                    valor.metodo_pago = prefac.metodo_pago;
                    valor.numero_pedido = prefac.numero_pedido;
                    valor.moneda = prefac.moneda;
                    valor.tipo_cambio = prefac.tipo_cambio;
                    valor.fecha_emision = Convert.ToDateTime(prefac.fecha_emision);
                    valor.numero_cuenta = prefac.numero_cuenta;
                    valor.nom_banco = prefac.nom_banco;
                    valor.cond_pago = prefac.cond_pago;
                    valor.observacion = prefac.observacion;
                    valor.subtotal = subtotal;
                    valor.total_iva = total_iva;
                    valor.total_iva_ret = total_iva_ret;
                    valor.total_isr_ret = total_isr_ret;
                    valor.descuento2 = descuento2 == "0.00" ? "" : descuento2;
                    valor.total = total;
                }
                db.SaveChanges();
                if (uuid != null)
                {
                    //SaveCfdi
                    foreach (var item in uuid)
                    {
                        tbd_Cfdi_Uuid nuevoUuid = new tbd_Cfdi_Uuid
                        {
                            id_pre_factura = (int)id_pref,
                            id_relacion = item.id_relacion,
                            uuid = item.uuid
                        };
                        db.tbd_Cfdi_Uuid.Add(nuevoUuid);
                        db.SaveChanges();
                    }
                }
                if (concepto != null)
                {
                    //PreConceptos
                    foreach (var item in concepto)
                    {
                        var clv_prod = db.tbc_ProdServ.Where(u => u.id_sat == item.id_sat).Select(u => u.c_pord_serv).First();
                        tbd_Conceptos_Pre_Factura nuevoConceptoPF = new tbd_Conceptos_Pre_Factura
                        {
                            id_pre_factura = (int)id_pref,
                            id_sat = item.id_sat,
                            c_prod_serv = clv_prod,//item.c_prod_serv,
                            c_producto = item.c_producto,
                            cantidad = item.cantidad,
                            c_unidad_medida = item.c_unidad_medida,
                            unidad = item.unidad,
                            concepto = item.concepto,
                            importe_unitario = item.importe_unitario,
                            importe_total = item.importe_total,
                            descuento = item.descuento == null ? "" : item.descuento,
                            obj_impuesto = item.obj_impuesto,
                            iva_imp_traslado = item.iva_imp_traslado,
                            tipo_factor = item.tipo_factor,
                            iva_tasa = item.iva_tasa,
                            iva_tasa_impuesto = item.iva_tasa_impuesto,
                            iva_ret = item.iva_ret,
                            iva_ret_tasa = item.iva_ret_tasa,
                            iva_ret_impuesto = item.iva_ret_impuesto,
                            isr_ret = item.isr_ret,
                            isr_ret_tasa = item.isr_ret_tasa,
                            isr_ret_impuesto = item.isr_ret_impuesto,
                            tipo_ieps = item.tipo_ieps,
                            ieps = item.ieps == null ? "" : item.ieps,
                            v_ieps = item.v_ieps == null ? "" : item.v_ieps,
                            total_imp_retenido = item.total_imp_retenido,
                            total = item.total
                        };
                        db.tbd_Conceptos_Pre_Factura.Add(nuevoConceptoPF);
                        db.SaveChanges();
                    }
                }
            }
            return Json(new { id = id_pref, tipo = "Actualizar" });
        }
        [HttpPost]
        public ActionResult editPreFactura(string Id)
        {
            return Json(new { redirectToUrl = Url.Action("VEditar", "Facturacion", new { id = Id }) });
        }
        public ActionResult FacturaDetail(Int32? id)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (id == null)
                return RedirectToAction("Clientes", "Catalogos");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var cliente = db.tbc_Clientes.Where(s => s.id_cliente == id).Single();

            if (cliente.rfc_usuario != usuario.rfc)
            {
                TempData["Mensaje"] = "No tiene acceso a ese cliente.";
                TempData["TMensaje"] = "danger";
                return RedirectToAction("Clientes", "Catalogos");
            }

            var lista = db.tbd_Servicios_Recurrentes.Where(s => s.id_cliente == id).ToList();
            ViewBag.Cliente = cliente;
            return View(lista);
        }
        public ActionResult getFacturas(int id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var leftOuterJoin = from p in db.tbd_Pre_Factura
                                    join fp in db.tbc_Formas_Pago on p.forma_pago equals fp.id_forma_pago into fpago
                                    from f_pag in fpago.DefaultIfEmpty()
                                    join cliente in db.tbc_Clientes on p.rfc_cliente equals cliente.rfc into fcliente
                                    from f_cliente in fcliente.DefaultIfEmpty()
                                    join cfdi in db.tbc_Usos_CFDI on p.clave_uso_cfdi equals cfdi.id_uso_cfdi into ucfdi
                                    from u_cfdi in ucfdi.DefaultIfEmpty()
                                    join mpago_ in db.tbc_Metodos_Pago on p.metodo_pago equals mpago_.id_metodo_pago into _mpago
                                    from mpago in _mpago.DefaultIfEmpty()
                                    where p.id_pre_factura == id
                                    select new
                                    {
                                        id = p.id_pre_factura,
                                        rfc = p.rfc_cliente,
                                        serie = p.serie,
                                        id_receptor = f_cliente.id_cliente,
                                        correo = f_cliente.correo,
                                        uso_cfdi = p.clave_uso_cfdi,
                                        c_uso_cfdi = u_cfdi.id_uso_cfdi,
                                        u_cfdi = u_cfdi.clave+" "+u_cfdi.uso_cfdi,
                                        exportacion = p.exportacion,
                                        reg_fiscal_usuario = p.reg_fiscal_usuario,
                                        folio = p.folio,
                                        n_rfc = p.nombre_rfc,
                                        uso_factura = p.uso_factura,
                                        lugar_expedicion = p.lugar_expedicion,
                                        tipo_factura = p.tipo_factura,
                                        forma_pago = p.forma_pago,
                                        id_fpago = f_pag.id_forma_pago,
                                        fpago = f_pag.clave+" "+f_pag.forma_pago,
                                        metodo_pago = p.metodo_pago,
                                        metodo_pago_ = mpago.clave+" "+mpago.metodo_pago,
                                        n_pedido = p.numero_pedido,
                                        moneda = p.moneda,
                                        tipo_cambio = p.tipo_cambio,
                                        f_emision = p.fecha_emision.ToString(),
                                        n_cuenta = p.numero_cuenta,
                                        n_banco = p.nom_banco,
                                        c_pago = p.cond_pago,
                                        obs = p.observacion,
                                        cuenta_predial = p.cuenta_predial,
                                        subtotal = p.subtotal,
                                        total_iva = p.total_iva,
                                        total_iva_ret = p.total_iva_ret,
                                        total_isr_ret = p.total_isr_ret,
                                        descuento = p.descuento2,
                                        url_pdf = p.url_pdf,
                                        url_xml = p.url_xml,
                                        total = p.total
                                    };
                return Json(leftOuterJoin.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getFacturasUuid(Int32? id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var auxcad = from uid in db.tbd_Cfdi_Uuid
                             where uid.id_pre_factura == id
                             select new
                             {
                                 id_uuid_cfdi = uid.id_cfdi_pre_factura,
                                 id_relacion = uid.id_relacion,
                                 uuid = uid.uuid
                             };
                return Json(auxcad.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getPrePagoUuid(int id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var auxcad = from uid in db.tbd_Pre_Pagos
                             where uid.id == id
                             select new
                             {
                                 uuid = uid.uuid
                             };
                return Json(auxcad.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getFacturasConPreFac(Int32? id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var auxcad = from c in db.tbd_Conceptos_Pre_Factura
                             join s in db.tbc_ProdServ on c.id_sat equals s.id_sat into rsat
                             from c_sat in rsat.DefaultIfEmpty()
                             where c.id_pre_factura == id
                             select new
                             {
                                 id = c.id_concepto_pre_factura,
                                 concepto = c.concepto,
                                 v_unidad = c.c_unidad_medida,
                                 id_sat = c.id_sat,
                                 c_prod_serv = c.c_prod_serv,
                                 c_producto = c.c_producto,
                                 c_sat = c_sat.c_pord_serv,
                                 d_sat = c_sat.descripcion,
                                 clave_interna = c.c_prod_serv,
                                 cantidad = c.cantidad,
                                 unidad = c.unidad,
                                 iva_tasa_impuesto = c.iva_tasa_impuesto,
                                 p_unitario = c.importe_unitario,
                                 descuento = c.descuento,
                                 importe = c.importe_total,
                                 iva_tasa = c.iva_tasa,
                                 iva_ret_impuesto = c.iva_ret_impuesto,
                                 iva_ret_tasa = c.iva_ret_tasa,
                                 isr_ret_tasa = c.isr_ret_tasa,
                                 isr_ret_impuesto = c.isr_ret_impuesto,
                                 iva = c.iva_imp_traslado,
                                 tipo_ieps = c.tipo_ieps,
                                 ieps = c.ieps,
                                 total = c.total,
                                 total_imp_retenido = c.total_imp_retenido,
                                 tipo_factor = c.tipo_factor
                             };
                return Json(auxcad.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getClienteR(Int32? id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var auxcad = from cp in db.tbc_Clientes
                             join d in db.tbc_Regimenes on cp.id_regimen_fiscal equals d.id_regimen_fiscal into rfiscal
                             from rgmn in rfiscal.DefaultIfEmpty()
                             join u in db.tbc_Usos_CFDI on cp.id_uso_cdfi equals u.id_uso_cfdi into ucfdi
                             from ufd in ucfdi.DefaultIfEmpty()
                             where cp.id_cliente == id
                             select new
                             {
                                 c_regimen_fiscal = rgmn.clave,
                                 c_uso_cfdi = ufd.clave,
                                 id_uso_cfdi = ufd.id_uso_cfdi
                             };
                return Json(auxcad.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getClaPro(Int32? id_sat)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var auxcad = from ps in db.tbc_ProdServ
                             where ps.id_sat == id_sat
                             select new
                             {
                                 clave = ps.c_pord_serv
                             };
                return Json(auxcad.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public String getPreFactura(String term)
        {
            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var prefactura = db.tbd_Pre_Factura.Where(s => ("[" + s.serie + "] " + s.folio).Contains(term) && s.rfc_usuario == usuario.rfc).ToList();
            if (prefactura.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in prefactura)
            {
                str += "{\"label\": \"[" + item.serie+ "] " + item.folio + "\", \"value\":" + item.id_pre_factura + ", \"name\":\"" + item.serie + "\", \"folio\":\"" + item.folio + "\"}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public ActionResult obtenerRegFiscal()
        {
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            int rf = usuario.id_regimen_fiscal;
            using (BD_FFEntities db = new BD_FFEntities())
            {

                var reg = db.tbc_Regimenes.Where(s => s.id_regimen_fiscal == rf).Single();

               

                return Json(reg.clave, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult deleteFactura(int id_)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                var valor = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == id_).FirstOrDefault();
                valor.status = 0;
                db.SaveChanges();
            }
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }
        public ActionResult filtrarFacturas(string fecha_i, string fecha_f)
        {
            DateTime f_inicial = Convert.ToDateTime(fecha_i);
            DateTime f_final = Convert.ToDateTime(fecha_f);
            using (BD_FFEntities db = new BD_FFEntities())
            {
                tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
                var lis_fac = from fac in db.tbd_Pre_Factura
                              join fp in db.tbc_Formas_Pago on fac.forma_pago equals fp.id_forma_pago into f_pago
                              from fpago in f_pago.DefaultIfEmpty()
                              join mp in db.tbc_Metodos_Pago on fac.metodo_pago equals mp.id_metodo_pago into m_pago
                              from mpago in m_pago.DefaultIfEmpty()
                              join uso_cfdi in db.tbc_Usos_CFDI on fac.clave_uso_cfdi equals uso_cfdi.id_uso_cfdi into u_cfdi
                              from ucfdi in u_cfdi.DefaultIfEmpty()
                              where fac.rfc_usuario == usuario.rfc && fac.status != 0 && fac.tipo == "Factura"  && fac.fecha_emision >= f_inicial && fac.fecha_emision <= f_final
                              orderby fac.id_pre_factura
                              select new
                              {
                                  id = fac.id_pre_factura,
                                  nombre_rfc = fac.nombre_rfc,
                                  rfc_cliente = fac.rfc_cliente,
                                  metodo_pago = mpago.clave+"-"+mpago.metodo_pago,
                                  forma_pago = fpago.clave +"-"+fpago.forma_pago,
                                  clave_uso_cfdi = ucfdi.clave+"-"+ucfdi.uso_cfdi,
                                  total = fac.total,
                                  status = fac.status
                              };
                //db.tbd_Pre_Factura.Where(s => s.rfc_usuario == usuario.rfc && s.status != 0 && s.fecha_emision >= f_inicial && s.fecha_emision <= f_final);
                return Json(lis_fac.ToList(), JsonRequestBehavior.AllowGet);
            }
            
        }
        public ActionResult obtenerUUIDFactura(string term)
        {
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var clientes = from fa in db.tbd_Facturas
                               join fp in db.tbc_Formas_Pago on fa.id_forma_pago equals fp.id_forma_pago into fpago
                               from t_fpagos in fpago.DefaultIfEmpty()
                               where fa.uuid.Contains(term) && fa.rfc_emisor == usuario.rfc
                               select new
                               {
                                   id_factura = fa.id_factura,
                                   uuid = fa.uuid,
                                   total = fa.total,
                                   id_forma_pago = fa.id_forma_pago,
                                   forma_pago = t_fpagos.forma_pago
                               };
                return Json(clientes.ToList(), JsonRequestBehavior.AllowGet);
            }
                //db.tbd_Facturas.Where(s => (s.uuid).Contains(term) && s.rfc_emisor== "SCM080611QE9").ToList();
            
        }
        //==========================================================Previsualización==========================================
        public JsonResult PreFacturar(Int32? id)
        {
            bool fileExist_ = false;
            if (Session["tbc_Usuarios"] == null)
                return Json("Error", JsonRequestBehavior.AllowGet);
            CultureInfo ci = new CultureInfo("en-us");
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            //----------------------------------------------------------------------------------------------------------------------------
            db = new BD_FFEntities();
            tbd_Pre_Factura prefactura_ = db.tbd_Pre_Factura.Where(s => s.id_pre_factura == id).Single();
            tbc_Metodos_Pago mpago_ = db.tbc_Metodos_Pago.Where(u => u.id_metodo_pago == prefactura_.metodo_pago).Single();
            tbc_Formas_Pago fpago_ = db.tbc_Formas_Pago.Where(u => u.id_forma_pago == prefactura_.forma_pago).Single();
            tbc_Usos_CFDI ucfdi_ = db.tbc_Usos_CFDI.Where(u => u.id_uso_cfdi == prefactura_.clave_uso_cfdi).Single();
            var valorCFDI = db.tbd_Cfdi_Uuid.ToList<tbd_Cfdi_Uuid>().Where(u => u.id_pre_factura == id).ToList();
            var valorConc = db.tbd_Conceptos_Pre_Factura.ToList<tbd_Conceptos_Pre_Factura>().Where(u => u.id_pre_factura == id).ToList();
            //-----------------------------------------------------------------------------------------------------------------------------
            var ruta = db.tbc_Variables_Calculo.Where(s => s.id_variable == 1).ToList().First();
            var fca_emision = prefactura_.fecha_emision.ToString();

            String[] fechaE = fca_emision.Split(' ');
            string aux_fc_emi = fechaE[0];
            String[] auxfechaE = aux_fc_emi.Split('/');
            string ax_fc_emi = auxfechaE[0]+auxfechaE[1]+auxfechaE[2];
            string DirPrg = Server.MapPath("~");
            string namefile = "";// RandomString(7) + "-"+ RandomString(4) + "-"+ RandomString(4) + "-"+ RandomString(4) + "-"+ RandomString(12);
            //ruta.url_pdf +"PRE_FAC_"+ prefactura_.id_pre_factura+ ".pdf";
            if (prefactura_.url_pdf == null || prefactura_.url_pdf == "")
            {
                namefile = RandomString(7) + "-" + RandomString(4) + "-" + RandomString(4) + "-" + RandomString(4) + "-" + RandomString(12);
            }
            else 
            {
                string[] nom_doc = prefactura_.url_pdf.Split('\\');
                string[] nd = nom_doc[4].Split('.');
                string nf = nd[0];
                namefile = nf;
            }
            string path = "Plantillas/XML/PDF/" + prefactura_.rfc_usuario + "/" + ax_fc_emi + "/" + namefile + ".pdf";
            //-----------------------------------------------------------------------------------------------------------------------------
            bool fileExist = System.IO.File.Exists(path);
            
            FileInfo file = new FileInfo(path);

            try {
                file.Delete();
                fileExist = System.IO.File.Exists(path);
            }
            catch (Exception e){
            
            }
            if (!fileExist)
            {
                string auxpath = DirPrg + "Plantillas\\XML\\PDF\\" + prefactura_.rfc_usuario+"\\"+ ax_fc_emi;
                DirectoryInfo di = Directory.CreateDirectory(auxpath);
                string auxpathdoc = DirPrg + "Plantillas\\XML\\DOCX\\" + prefactura_.rfc_usuario+ "\\" + ax_fc_emi;
                DirectoryInfo didoc = Directory.CreateDirectory(auxpathdoc);
                string nombrearchivo = "";
                object ObjMiss = System.Reflection.Missing.Value;
                Word.Application ObjWord = new Word.Application();

                nombrearchivo = "CFDI40Estandar.docx";

                Word.Document ObjDoc = ObjWord.Documents.Open(DirPrg + "/Plantillas/" + nombrearchivo, ObjMiss);

                

                //Definir Marcadores
                object nombre_razon = "RFC_Emisor";
                object razon_social_emisor = "Razon_Social_Emisor";
                object tipo_comprobante = "Tipo_Comprobante";
                object lugar_expedicion = "Lugar_Expedicion";
                object regimen_fiscal = "Regimen_Fiscal";
                object version_cfdi = "Version_CFDI";

                object forma_pago = "Forma_Pago";
                object metodo_pago = "Metodo_Pago";
                object moneda = "Moneda_";
                object exportacion = "Exportacion_";
                object folio = "Folio";
                object serie = "Serie";
                object fecha_emision = "Fecha_Emision";

                object cliente = "Nombre_Receptor";
                object rfc_receptor = "RFC_Receptor";
                object domicilio_fiscal = "Domicilio_Fiscal";
                object uso_cfdi = "Uso_CFDI";
                object regimen_fiscal_receptor = "Regimen_Fiscal_Receptor";
                
                object Tabla_productos = "Tabla_Productos";
                

                object total_letra = "Total_Letra";
                object subtotal = "Subtotal_";
                object descuento = "Descuento_";
                object iva = "Impuestos_Trasladados";
                object iva_ret = "IVA_Retenido";
                object isr_ret = "ISR_Retenido";
                object Total = "Total_";

                object tipo_relacion = "Tipo_Relacion";
                object uuid= "UUID_";
                object no_certificado_sat = "No_Cetificado_SAT";
                object fecha_timbrado = "Fecha_Timbrado";

                object sello_cfd = "Sello_CFD";
                object sello_sat = "Sello_SAT";
                object complemento_certificacion = "Complemento_Certificacion";
                //Busqueda de marcadores en la plantilla
                Word.Range nombrerazon = ObjDoc.Bookmarks.get_Item(ref nombre_razon).Range;
                Word.Range razonsocialemisor = ObjDoc.Bookmarks.get_Item(ref razon_social_emisor).Range;
                Word.Range tipocomprobante = ObjDoc.Bookmarks.get_Item(ref tipo_comprobante).Range;
                Word.Range lugarexpedicion = ObjDoc.Bookmarks.get_Item(ref lugar_expedicion).Range;
                Word.Range regimenfiscal = ObjDoc.Bookmarks.get_Item(ref regimen_fiscal).Range;
                Word.Range versioncfdi = ObjDoc.Bookmarks.get_Item(ref version_cfdi).Range;

                Word.Range formapago = ObjDoc.Bookmarks.get_Item(ref forma_pago).Range;
                Word.Range metodopago = ObjDoc.Bookmarks.get_Item(ref metodo_pago).Range;
                Word.Range moneda_ = ObjDoc.Bookmarks.get_Item(ref moneda).Range;
                Word.Range exportacion_ = ObjDoc.Bookmarks.get_Item(ref exportacion).Range;
                Word.Range folio_ = ObjDoc.Bookmarks.get_Item(ref folio).Range;
                Word.Range serie_ = ObjDoc.Bookmarks.get_Item(ref serie).Range;
                Word.Range fechaemision = ObjDoc.Bookmarks.get_Item(ref fecha_emision).Range;

                Word.Range cliente_ = ObjDoc.Bookmarks.get_Item(ref cliente).Range;
                Word.Range rfcreceptor = ObjDoc.Bookmarks.get_Item(ref rfc_receptor).Range;
                Word.Range domiciliofiscal = ObjDoc.Bookmarks.get_Item(ref domicilio_fiscal).Range;
                Word.Range usoCFDI = ObjDoc.Bookmarks.get_Item(ref uso_cfdi).Range;
                Word.Range regimenfiscalreceptor = ObjDoc.Bookmarks.get_Item(ref regimen_fiscal_receptor).Range;

                Word.Range Tablaproductos = ObjDoc.Bookmarks.get_Item(ref Tabla_productos).Range;

                Word.Range totalletra = ObjDoc.Bookmarks.get_Item(ref total_letra).Range;
                Word.Range Descuento_ = ObjDoc.Bookmarks.get_Item(ref descuento).Range;
                Word.Range Subtotal_ = ObjDoc.Bookmarks.get_Item(ref subtotal).Range;
                Word.Range Iva_ = ObjDoc.Bookmarks.get_Item(ref iva).Range;
                Word.Range ivaret = ObjDoc.Bookmarks.get_Item(ref iva_ret).Range;
                Word.Range isrret = ObjDoc.Bookmarks.get_Item(ref isr_ret).Range;
                Word.Range Total_ = ObjDoc.Bookmarks.get_Item(ref Total).Range;

                Word.Range tiporelacion = ObjDoc.Bookmarks.get_Item(ref tipo_relacion).Range;
                Word.Range uuid_ = ObjDoc.Bookmarks.get_Item(ref uuid).Range;
                Word.Range nocertificadosat = ObjDoc.Bookmarks.get_Item(ref no_certificado_sat).Range;
                Word.Range fechatimbrado = ObjDoc.Bookmarks.get_Item(ref fecha_timbrado).Range;

                Word.Range SelloCFD = ObjDoc.Bookmarks.get_Item(ref sello_cfd).Range;
                Word.Range SelloSAT = ObjDoc.Bookmarks.get_Item(ref sello_sat).Range;
                Word.Range CCertificacion = ObjDoc.Bookmarks.get_Item(ref complemento_certificacion).Range;


                

                if (prefactura_.uuid != null)
                {
                    //Crear Codigo QR
                    string fileName = prefactura_.uuid + "-" + prefactura_.rfc_cliente;
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    ASCIIEncoding ASSCII = new ASCIIEncoding();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(ASSCII.GetBytes("https://verificacfdi.facturaelectronica.sat.gob.mx/default.aspx?&id=" + prefactura_.uuid + "&re=" + usuario.rfc + "&rr=" + prefactura_.rfc_cliente + "&tt=" + prefactura_.total + "&fe=" + prefactura_.selloCFDI.Substring(prefactura_.selloCFDI.Length - 8, 8)), QRCodeGenerator.ECCLevel.H);
                    QRCode qrCode = new QRCode(qrCodeData);
                    Bitmap qrCodeImage = qrCode.GetGraphic(2);
                    qrCodeImage.Save(DirPrg + "/Plantillas/XML/DOCX/" + prefactura_.rfc_usuario + "/" + ax_fc_emi + "/" + fileName + ".jpg", ImageFormat.Jpeg);
                    //qrCodeImage.Save(@"D:\VS\Formatos\Documentos\Codigos QR\" + archivo + tipoComprobante + "" + fileName + ".jpg", ImageFormat.Jpeg);

                    object Imagen_QR = "Imagen_QR";

                    ObjDoc.Bookmarks.get_Item(ref Imagen_QR).Range.InlineShapes.AddPicture((DirPrg + "/Plantillas/XML/DOCX/" + prefactura_.rfc_usuario + "/" + ax_fc_emi + "/" + fileName + ".jpg"), false, true);
                    //Fin Crear Codigo QR
                }

                //Agregar texto al marcador
                string tc = prefactura_.tipo_comprobante;
                string auxcad = tc == "I" ? "Ingreso":tc == "E" ? "Egreso":tc== "T" ? "Traslado":tc == "N" ? "Nómina":tc == "P" ? "Pago":"Pago";
                
                nombrerazon.Text = usuario.rfc;
                razonsocialemisor.Text = prefactura_.nombre_usuario_rfc;//db.tbc_Clientes.Where(u => u.rfc_usuario == prefactura_.rfc_usuario).Select(u => u.nombre_razon).First();
                tipocomprobante.Text = auxcad;
                lugarexpedicion.Text = prefactura_.lugar_expedicion;
                regimenfiscal.Text = prefactura_.reg_fiscal_usuario +"-"+db.tbc_Regimenes.Where(u => u.clave == prefactura_.reg_fiscal_usuario).Select(u => u.regimen).First();
                versioncfdi.Text = "4.0";

                formapago.Text = fpago_.clave + "-" + fpago_.forma_pago;
                metodopago.Text = mpago_.clave + "-" + mpago_.metodo_pago;
                moneda_.Text = prefactura_.moneda;
                exportacion_.Text = prefactura_.exportacion;
                if (prefactura_.folio == null)
                {
                    folio_.Text = prefactura_.folio;
                }
                else
                {
                    folio_.Text = "-" + prefactura_.folio;
                }
                serie_.Text = prefactura_.serie;
                string auxfca = prefactura_.fecha_emision.ToString();
                fechaemision.Text = auxfca;

                cliente_.Text = prefactura_.nombre_rfc;
                rfcreceptor.Text = prefactura_.rfc_cliente;
                domiciliofiscal.Text = db.tbc_Clientes.Where(u => u.rfc_usuario == prefactura_.rfc_usuario && u.rfc == prefactura_.rfc_cliente).Select(u => u.direccion_fiscal).First();
                usoCFDI.Text = ucfdi_.clave+"-"+ucfdi_.uso_cfdi;
                regimenfiscalreceptor.Text = prefactura_.reg_fiscal_usuario+"-"+db.tbc_Regimenes.Where(u => u.clave == prefactura_.reg_fiscal_usuario).Select(u => u.regimen).First();
                //Creacion y definicion de tabla
                var cantProductos = db.tbd_Conceptos_Pre_Factura.Where(s => s.id_pre_factura== prefactura_.id_pre_factura).ToList();
                valorConc.Count();

                Word.Table TablaProd;
                TablaProd = ObjDoc.Tables.Add(Tablaproductos, valorConc.Count, 8);

                int i = 1;
                for (int z = 0; z <= valorConc.Count - 1; z++)
                {
                    var aux = valorConc[z].c_unidad_medida;
                    Decimal canti = Convert.ToDecimal(valorConc[z].cantidad);
                    Decimal total_ = Convert.ToDecimal(valorConc[z].total);
                    Decimal importet_ = Convert.ToDecimal(valorConc[z].importe_total);
                    Decimal impuesto_ = Convert.ToDecimal(valorConc[z].iva_tasa_impuesto);
                    //var query = db.tbc_ProdServ.ToList<tbc_ProdServ>().Where(s => s.c_pord_serv == valorConc[z].c_producto).Select(s => s.descripcion).First();
                    //int cu = valorConc[z].c_unidad_medida;
                    TablaProd.Cell(i, 1).Range.Text = canti.ToString("########.00");
                    TablaProd.Cell(i, 2).Range.Text = "["+ valorConc[z].c_unidad_medida + "]"+valorConc[z].unidad;
                    TablaProd.Cell(i, 3).Range.Text = valorConc[z].c_prod_serv;
                    TablaProd.Cell(i, 4).Range.Text = valorConc[z].c_producto;
                    TablaProd.Cell(i, 5).Range.Text = valorConc[z].concepto.ToString();
                    TablaProd.Cell(i, 6).Range.Text = importet_.ToString("C");
                    TablaProd.Cell(i, 6).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                    TablaProd.Cell(i, 7).Range.Text = impuesto_.ToString("C");
                    TablaProd.Cell(i, 7).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                    TablaProd.Cell(i, 8).Range.Text = total_.ToString("C");
                    TablaProd.Cell(i, 8).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    i++;
                }

                TablaProd.Columns[1].SetWidth(50, 0);
                TablaProd.Columns[2].SetWidth(50, 0);
                TablaProd.Columns[3].SetWidth(55, 0);
                TablaProd.Columns[4].SetWidth(58, 0);
                TablaProd.Columns[5].SetWidth(197, 0);
                TablaProd.Columns[6].SetWidth(53, 0);
                TablaProd.Columns[7].SetWidth(60, 0);
                TablaProd.Columns[8].SetWidth(57, 0);
                TablaProd.Borders[Word.WdBorderType.wdBorderHorizontal].Visible = false;
                TablaProd.Borders[Word.WdBorderType.wdBorderVertical].Visible = false;
                //Fin creacion y definicion de tabla

                decimal totalEntero = Convert.ToDecimal(prefactura_.total);
                decimal descuento_ = Convert.ToDecimal(prefactura_.descuento2 == "" ? "0": prefactura_.descuento2);
                decimal subtotal_ = Convert.ToDecimal(prefactura_.subtotal);
                decimal iva_ = Convert.ToDecimal(prefactura_.total_iva);
                decimal ivaret_ = Convert.ToDecimal(prefactura_.total_iva_ret);
                decimal isr_ = Convert.ToDecimal(prefactura_.total_isr_ret);

                totalletra.Text = totalEntero.NumeroALetras();
                Descuento_.Text = descuento_.ToString("C");
                Subtotal_.Text = subtotal_.ToString("C");
                Iva_.Text = iva_.ToString("C");
                ivaret.Text = ivaret_.ToString("C");
                isrret.Text = isr_.ToString("C");
                Total_.Text = totalEntero.ToString("C");
                //UUID
                tiporelacion.Text = " ";
                uuid_.Text = prefactura_.uuid;
                nocertificadosat.Text = prefactura_.ccertificacion;
                fechatimbrado.Text = prefactura_.fca_timbrado.ToString();
                //Si ya timbro
                string selloCFDI = prefactura_.status != 1 ?  db.tbd_Facturas.Where(u => u.sello_sat == prefactura_.selloSAT).Select(u => u.sello_cfdi).First():"";
                SelloCFD.Text = selloCFDI;
                SelloSAT.Text = prefactura_.selloSAT;
                

                CCertificacion.Text = prefactura_.uuid != null ? "||" + prefactura_.version_timbrado + "|" + prefactura_.uuid + "|" + prefactura_.fca_timbrado + "|" + prefactura_.selloCFDI + "|" + prefactura_.ccertificacion + "||" : "";
                //Cerrar word
                //string[] nom_doc = prefactura_.url_pdf.Split('\\');
                //string[] nd = nom_doc[4].Split('.');
                //string nf = nd[0];

                //var fileExistW = System.IO.File.Exists(DirPrg + "Plantillas\\XML\\DOCX\\" + prefactura_.rfc_cliente + "\\" + ax_fc_emi + "\\" + nf + ".docx");
                //if (!fileExistW)
                //{
                //    namefile = nf;
                //    //ObjDoc.SaveAs2(DirPrg + "/Plantillas/XML/DOCX/" + prefactura_.rfc_cliente + "/" + ax_fc_emi + "/" + namefile + ".docx");
                //}
                int x = 0;
                while (x < 30)
                {
                    System.Threading.Thread.Sleep(1000);
                    //if (System.IO.File.Exists(archivo))
                    //{
                    //    Thread.Sleep(2000);
                    //    //System.IO.File.Delete(path + nombre + ".jpg");
                    //    //System.IO.File.Delete(path + nombre + ".docx");
                    //    break;
                    //}

                    x++;
                }
                ObjDoc.SaveAs2(DirPrg + "/Plantillas/XML/DOCX/" + prefactura_.rfc_usuario + "/" + ax_fc_emi + "/" + namefile + ".docx");
                ObjDoc.Close();
                ObjWord.Quit();

                //Crear PDF
                var pdfProcess = new Process();
                pdfProcess.StartInfo.FileName = "" + ruta.url_libreoffice;
                pdfProcess.StartInfo.Arguments = "--headless --convert-to pdf " + DirPrg + "Plantillas\\XML\\DOCX\\"+prefactura_.rfc_usuario+ "\\" + ax_fc_emi +"\\" + namefile + ".docx --outdir  " + DirPrg + "Plantillas\\XML\\PDF\\"+prefactura_.rfc_usuario+ "\\" + ax_fc_emi + "\\";
                pdfProcess.Start();
                fileExist_ = System.IO.File.Exists(DirPrg + "Plantillas\\" + prefactura_.url_pdf);
                //Actualizar Rutas
                if (prefactura_.status == 1) {
                    prefactura_.url_pdf = "XML\\PDF\\" + prefactura_.rfc_usuario + "\\" + ax_fc_emi + "\\" + namefile + ".pdf";
                    prefactura_.url_xml = "XML\\PDF\\" + prefactura_.rfc_usuario + "\\" + ax_fc_emi + "\\";
                }
                db.SaveChanges();
                
            }
            //-----------------------------------------------------------------------------------------------------
            if (!fileExist_)
            {
                return Json("NG", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(path, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ComplementoPreview(Int32? id)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (id == null)
                return RedirectToAction("Clientes", "Catalogos");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var prepago = db.tbd_Pre_Pagos.Where(s => s.id == id).Single();
            ViewBag.ID = prepago.id;
            ViewBag.Estatus = prepago.status;
            return View(prepago);
        }
        //==========================================================Timbrado==========================================
        public ActionResult TimbrarFactura(Int32? id_)
        {
            try
            {
                string DirPrg = Server.MapPath("~");
                string uuidFactura = Guid.NewGuid().ToString();
                using (BD_FFEntities db = new BD_FFEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    var valor = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == id_).FirstOrDefault();
                    var valorCFDI = db.tbd_Cfdi_Uuid.ToList<tbd_Cfdi_Uuid>().Where(u => u.id_pre_factura == id_).ToList();
                    var valorConc = db.tbd_Conceptos_Pre_Factura.ToList<tbd_Conceptos_Pre_Factura>().Where(u => u.id_pre_factura == id_).ToList();
                    var valCliente = db.tbc_Clientes.ToList<tbc_Clientes>().Where(u => u.rfc== valor.rfc_cliente).FirstOrDefault();
                    string[] rutaa_ = valor.url_pdf.Split('\\');
                    string ruta_ = @"Plantillas\" + rutaa_[0] + @"\" + rutaa_[1] + @"\" + rutaa_[2] + @"\" + rutaa_[3] + @"\";

                    var CFDI = new TCFDI4(DirPrg, @"CSD_Pruebas_CFDI_EKU9003173C9.cer", @"CSD_Pruebas_CFDI_EKU9003173C9.key", "12345678a")
                    {
                        //Se recomienda asignar un nombre distinto a este archivo, por ejemplo:
                        cTmpFile = DirPrg + ruta_ + uuidFactura + ".tmp"
                    };
                    // Credenciales de timbrado
                    CFDI.aParametros[0] = "PAC";
                    CFDI.aParametros[1] = "Prueba";
                    CFDI.aParametros[2] = "FAC201027H66";
                    CFDI.aParametros[3] = "FAC-CFDI-12409=";
                    // Datos del comprobante
                    CFDI.aComprobante[0] = "4.0";               // Versión del estandar CFDI
                    CFDI.aComprobante[1] = valor.serie;         // Serie
                    CFDI.aComprobante[2] = valor.folio;         // Folio

                    CFDI.aComprobante[3] = string.Format("{0:yyyy-MM-ddTHH:mm:ss}", DateTime.Now.AddMinutes(-2), CultureInfo.CreateSpecificCulture("es-MX"));
                    var fpago_ = db.tbc_Formas_Pago.Where(u => u.id_forma_pago == valor.forma_pago).Select(u => u.clave).First();
                    var mpago_ = db.tbc_Metodos_Pago.Where(u => u.id_metodo_pago == valor.metodo_pago).Select(u => u.clave).First();
                    CFDI.aComprobante[4] = fpago_;// Forma de Pago (Catálogo: c_FormaPago)
                    CFDI.aComprobante[5] =  valor.cond_pago;                 // Condiciones de pago
                    CFDI.aComprobante[6] =  valor.subtotal;                  // SubTotal
                    CFDI.aComprobante[7] =  valor.descuento2;                // Descuento
                    CFDI.aComprobante[8] =  valor.moneda;                    // Moneda (Catálogo: c_Moneda)
                    CFDI.aComprobante[9] = "";//valor.tipo_cambio;               // Tipo de Cambio (Es requerido cuando la clave de moneda es distinta de MXN y de XXX)
                    CFDI.aComprobante[10] = valor.total;                     // TOTAL
                    CFDI.aComprobante[11] = valor.tipo_comprobante;          // Tipo de Comprobante (Catálogo: c_TipoDeComprobante)
                    CFDI.aComprobante[12] = valor.exportacion;               // Exportación (Catálogo: c_Exportacion)
                    CFDI.aComprobante[13] = mpago_;// Método de Pago (Catálogo: c_MetodoPago)
                    CFDI.aComprobante[14] = valor.lugar_expedicion;          // Lugar de expedición (Catálogo: c_CodigoPostal)
                    CFDI.aComprobante[15] = "";                              // Confirmación.

                    // Informacion global para precisar la información relacionada con el comprobante global
                    CFDI.aInformacionGlobal[0] = "";                          // Periodicidad (Catálogo: c_Periodicidad)
                    CFDI.aInformacionGlobal[1] = "";                          // Meses (Catálogo: c_Meses)
                    CFDI.aInformacionGlobal[2] = "";                          // Año (4 digitos)

                    // Datos del CFDI Relacionado
                    CFDI.cCfdiTipoRelacion = "";                              // Tipo de Relacion (Catálogo: c_TipoRelacion)

                    if (valorCFDI != null)//if (!empty(CFDI.cCfdiTipoRelacion))
                    {
                        // En caso de que TipoRelacion tenga un valor deberás especificar los UUID relacionados.
                        CFDI.aCfdiRelacionado[0] = "";                        // Folio fiscal (UUID) de un CFDI relacionado.
                        CFDI.aCfdiRelacionado[1] = "";                        // Folio fiscal (UUID) (puedes agregar varios folios)
                    }
                    // Datos del Emisor
                    CFDI.aEmisor[0] = "EKU9003173C9";//valor.rfc_usuario;        // RFC del emisor
                    CFDI.aEmisor[1] = "ESCUELA KEMPER URGATE";//valor.nombre_usuario_rfc; // Nombre del emisor
                    CFDI.aEmisor[2] = "601";//valor.reg_fiscal_usuario; // Régimen Fiscal (Catálogo: c_RegimenFiscal)
                    CFDI.aEmisor[3] = "";                       // FacAtrAdquirente (PCECFDI o PCGCFDISP)

                    // Datos del Receptor
                    var IDregFis_rec = db.tbc_Clientes.Where(u => u.rfc == valor.rfc_cliente).Select(u => u.id_regimen_fiscal).First();
                    var regFis_rec = db.tbc_Regimenes.Where(u => u.id_regimen_fiscal == IDregFis_rec).Select(u => u.clave).First();
                    var usoCFDIrec = db.tbc_Usos_CFDI.Where(u => u.id_uso_cfdi == valor.clave_uso_cfdi).Select(u => u.clave).First();
                    CFDI.aReceptor[0] = "XAXX010101000";//valor.rfc_cliente;      // RFC
                    CFDI.aReceptor[1] = "PUBLICO GENERAL";//valor.nombre_rfc;       // Nombre Cliente
                    CFDI.aReceptor[2] = "72534";//valCliente.direccion_fiscal; // DomicilioFiscalReceptor (código postal del domicilio fiscal del receptor)
                    CFDI.aReceptor[3] = "";                     // ResidenciaFiscal (Requerido cuando se trate de un extranjero)
                    CFDI.aReceptor[4] = "";                     // NumRegIdTrib (Es requerido cuando se incluya el complemento de comercio exterior)
                    CFDI.aReceptor[5] = "616";//regFis_rec;             // RegimenFiscalReceptor (Catálogo: c_RegimenFiscal) incorporar la clave del régimen fiscal del contribuyente receptor.
                    CFDI.aReceptor[6] = "G03";//usoCFDIrec;             // UsoCFDI (Catálogo: c_UsoCFDI)
                    //Conceptos
                    // Arreglo: aConceptos( Número de concepto, Atributo ) | Hasta 1,000 Conceptos.

                    Decimal total_trasladado = 0;
                    Decimal total_retenido = 0;

                    Decimal total_iva_ret = 0;
                    Decimal total_isr_ret = 0;

                    Decimal base_iva = 0;

                    for (int i = 0; i < valorConc.Count; i++)
                    {
                        Decimal canti = Convert.ToDecimal(valorConc[i].cantidad);
                        Decimal imp_unitario = Convert.ToDecimal(valorConc[i].importe_unitario);
                        Decimal imp_total = Convert.ToDecimal(valorConc[i].importe_total);
                        CFDI.aConceptos[i, 0] = "84111500";// valorConc[i].c_prod_serv;            // Clave Producto Servicio (Catálogo: c_ClaveProdServ)
                        CFDI.aConceptos[i, 1] = "HON";//valorConc[i].c_producto;             // Clave o código del producto (NoIdentificacion)
                        CFDI.aConceptos[i, 2] = "1";//canti.ToString("########.00");       // Cantidad
                        CFDI.aConceptos[i, 3] = "E48";//valorConc[i].c_unidad_medida;        // Clave Unidad de medida (Catálogo: c_ClaveUnidad)
                        CFDI.aConceptos[i, 4] = "Unidad de servicio";//valorConc[i].unidad;                 // Unidad de medida
                        CFDI.aConceptos[i, 5] = "Honorario Contable";//valorConc[i].concepto;               // Descripción del producto
                        CFDI.aConceptos[i, 6] = "1.00";//imp_unitario.ToString("########.00");// Importe unitario
                        CFDI.aConceptos[i, 7] = "1.00";//imp_total.ToString("########.00");   // Importe Total
                        CFDI.aConceptos[i, 8] = "";//valorConc[i].descuento;              // Descuento
                        CFDI.aConceptos[i, 9] = "02";                                // ObjetoImp. (Catálogo: c_ObjetoImp)

                        // Trasladado
                        if (valorConc[i].iva_tasa != "0.000000")
                        {
                            Decimal i_total = Convert.ToDecimal(valorConc[i].importe_total);
                            CFDI.aConceptosTraslado[i, 0, 0] = "1.00";// i_total.ToString(); // Base para el cálculo del impuesto
                            CFDI.aConceptosTraslado[i, 0, 1] = "002";//valorConc[i].iva_imp_traslado;   // Clave impuesto trasladado (Catálogo: c_Impuesto)
                            CFDI.aConceptosTraslado[i, 0, 2] = "Tasa";//valorConc[i].tipo_factor;        // Clave tipo de factor (Catálogo: c_TipoFactor)
                            CFDI.aConceptosTraslado[i, 0, 3] = valorConc[i].iva_tasa;           // Tasa o cuota del impuesto
                            CFDI.aConceptosTraslado[i, 0, 4] = "0.16";//valorConc[i].iva_tasa_impuesto;  // Importe del impuesto

                            total_trasladado += Convert.ToDecimal(valorConc[i].iva_tasa_impuesto);
                            base_iva += Convert.ToDecimal(valorConc[i].importe_total);
                        }
                        //CFDI.aACuentadeTerceros[0, 0] = "";                       // RfcACuentaTerceros
                        //CFDI.aACuentadeTerceros[0, 1] = "";                       // NombreACuentaTerceros
                        //CFDI.aACuentadeTerceros[0, 2] = "";                       // RegimenFiscalACuentaTerceros (Catálogo: c_RegimenFiscal)
                        //CFDI.aACuentadeTerceros[0, 3] = "";                       // DomicilioFiscalACuentaTerceros (CP)

                        //Retenido
                        if (valorConc[i].isr_ret_impuesto.Trim() != "0.00")
                        {
                            Decimal i_total = Convert.ToDecimal(valorConc[i].importe_total);
                            CFDI.aConceptosRetencion[0, 0, 0] = i_total.ToString("########.00");// Base para el cálculo del impuesto
                            CFDI.aConceptosRetencion[0, 0, 1] = valorConc[i].isr_ret;           // Clave impuesto trasladado (Catálogo: c_Impuesto)
                            CFDI.aConceptosRetencion[0, 0, 2] = valorConc[i].tipo_factor;       // Clave tipo de factor (Catálogo: c_TipoFactor)
                            CFDI.aConceptosRetencion[0, 0, 3] = valorConc[i].isr_ret_tasa;      // Tasa o cuota del impuesto
                            CFDI.aConceptosRetencion[0, 0, 4] = valorConc[i].isr_ret_impuesto;  // Importe del impuesto
                            total_retenido += Convert.ToDecimal(valorConc[i].isr_ret_impuesto);
                            total_isr_ret += Convert.ToDecimal(valorConc[i].isr_ret_impuesto);
                        }
                        if (valorConc[i].iva_ret_impuesto.Trim() != "0.00")
                        {
                            Decimal i_total = Convert.ToDecimal(valorConc[i].importe_total);
                            CFDI.aConceptosRetencion[0, 1, 0] = i_total.ToString();// Base para el cálculo del impuesto
                            CFDI.aConceptosRetencion[0, 1, 1] = valorConc[i].iva_ret;           // Clave impuesto trasladado (Catálogo: c_Impuesto)
                            CFDI.aConceptosRetencion[0, 1, 2] = valorConc[i].tipo_factor;       // Clave tipo de factor (Catálogo: c_TipoFactor)
                            CFDI.aConceptosRetencion[0, 1, 3] = valorConc[i].iva_ret_tasa;      // Tasa o cuota del impuesto
                            CFDI.aConceptosRetencion[0, 1, 4] = valorConc[i].iva_ret_impuesto;  
                            total_retenido += Convert.ToDecimal(valorConc[i].iva_ret_impuesto);
                            total_iva_ret += Convert.ToDecimal(valorConc[i].iva_ret_impuesto);
                        }
                    }
                    //valorConc.isr_ret_impuesto Importe del impuesto
                    // Total de Impuestos
                    CFDI.aImpuestos[0] = "";//total_retenido.ToString();                             // valor.total_imp_ret Total de los impuestos retenidos
                    CFDI.aImpuestos[1] = "0.16";// total_trasladado.ToString();                           // Total de los impuestos trasladados
                    // Retenciones
                    //CFDI.aRetencion[0, 0] = "001";                            // Clave impuesto trasladado (Catálogo: c_Impuesto)
                    //CFDI.aRetencion[0, 1] = "700.00";//total_isr_ret.ToString();              // Importe o monto del impuesto retenido

                    //CFDI.aRetencion[1, 0] = "002";                            // Tipo de impuesto retenido
                    //CFDI.aRetencion[1, 1] = "746.67";//total_iva_ret.ToString();              // Importe o monto del impuesto retenido
                    // Impuestos
                    // Arreglo: aTraslados( Número de elemento, Atributo ) | Hasta 3 tasas de IVA. (ampliar segun se requiera)
                    CFDI.aTraslado[0, 0] = "1.00";//base_iva.ToString("########.00");                        // Base para el cálculo del impuesto
                    CFDI.aTraslado[0, 1] = "002";                             // Clave impuesto trasladado (Catálogo: c_Impuesto)
                    CFDI.aTraslado[0, 2] = "Tasa";                            // Clave tipo de factor (Catálogo: c_TipoFactor)
                    CFDI.aTraslado[0, 3] = "0.160000";                        // Tasa o cuota del impuesto
                    CFDI.aTraslado[0, 4] = "0.16s";//total_trasladado.ToString();                         // Importe del impuesto
                    //-----
                    CFDI.id_pre[0] = valor.id_pre_factura.ToString();
                    CFDI.id_pre[1] = valor.url_pdf;
                    //-----
                    CFDI.GenerarTmp4();
                    string mensaje = CFDI.Mensaje;
                    string[] respuesta_men = mensaje.Split('|');
                    string res = respuesta_men[0];
                    string n_certificado = respuesta_men[1];
                    string sello_digital = respuesta_men[2];
                    string uuid = respuesta_men[3];
                    string cer_sat = respuesta_men[4];
                    string sello_sat = respuesta_men[5];
                    string fca_timbre = respuesta_men[6];
                    string root_xml = respuesta_men[17];
                    string[] root_xml_ = root_xml.Split('\\');

                    if (res == "Timbrado")
                    {
                        valor.url_xml = root_xml;
                        valor.selloCFDI = sello_digital;
                        valor.selloSAT = sello_sat;
                        valor.ccertificacion = n_certificado;
                        valor.fca_timbrado = Convert.ToDateTime(fca_timbre);
                        valor.status = 2;
                        db.SaveChanges();
                    }
                    return Json(res, JsonRequestBehavior.AllowGet);
                    //CFDI = null;
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        //-----------------------------------------------------------Enviar Correo---------------------------------------------
        public ActionResult EnviarCorreo(int id_, string correo_, string tipo)
        {
            String mensaje;
            String url = "https://castelanauditores.com/FFDemo/img/cuentas/";
            db = new BD_FFEntities();
            string DirPrg = Server.MapPath("~");
            string fullPath = "";
            string fullPathXML = "";
            string nombre_rfc = ""; string rfc = "";string url_pdf = ""; string url_xml = "";string title_ = "";
            if (tipo == "Prepago")
            {
                tbd_Pre_Pagos prepago_ = db.tbd_Pre_Pagos.Where(s => s.id == id_).Single();
                url_pdf = prepago_.url_pdf;
                url_xml = prepago_.url_xml;
                rfc = db.tbc_Clientes.Where(u => u.id_cliente == prepago_.id_cliente).Select(u => u.rfc).First();
                nombre_rfc = db.tbc_Clientes.Where(u => u.id_cliente == prepago_.id_cliente).Select(u => u.nombre_razon).First();
                fullPath = DirPrg + @"Plantillas\" + url_pdf;
                fullPathXML = DirPrg + @"Plantillas\" + url_xml;
                title_ = "Archivos de Prepagos (Test)";
            } 
            else if (tipo == "CartaPorte") 
            {
                tbd_Pre_Carta_Porte precarta_ = db.tbd_Pre_Carta_Porte.Where(s => s.id == id_).Single();
                var fac = db.tbd_Pre_Factura.Where(s => s.id_pre_factura == precarta_.id_prefactura).Single();
                url_pdf = fac.url_pdf;
                url_xml = fac.url_xml;
                rfc = db.tbc_Clientes.Where(u => u.id_cliente == precarta_.id_receptor).Select(u => u.rfc).First();
                nombre_rfc = db.tbc_Clientes.Where(u => u.id_cliente == precarta_.id_receptor).Select(u => u.nombre_razon).First();
                fullPath = DirPrg + @"Plantillas\" + url_pdf;
                fullPathXML = DirPrg + @"Plantillas\" + url_xml;
                title_ = "Archivos de Carta Porte (Test)";
            }
            else
            {
                tbd_Pre_Factura prefactura_ = db.tbd_Pre_Factura.Where(s => s.id_pre_factura == id_).Single();
                url_pdf = prefactura_.url_pdf;
                url_xml = prefactura_.url_xml;
                rfc = prefactura_.rfc_cliente;
                nombre_rfc = prefactura_.nombre_rfc;
                fullPath = DirPrg + @"Plantillas\" + url_pdf;
                fullPathXML = DirPrg + @"Plantillas\" + url_xml;
                title_ = "Timbrado de Facturas (Test).";
            }
            
            String cuerpo =
                @"<center>
                <style>.formEmail{font-family:'Open Sans',sans-serif;width:750px;text-align:center;}.formBorder{width:100%;height:30px;background-color:rgb(0,33,96);}</style>
                <div class='formEmail'>
                    <div class='formBorder'></div>
                    <table style='border-collapse:collapse; width:100%;'>
                        <tr>                            
                            <td style='padding:20px;text-align:center;'>
                                <h2 style='font-weight:bold;'>Apreciable</h2>
                                <h3 style='font-weight:bold;'>" + rfc + @"</h3>
                                <h4 style='font-weight:bold;'>" + nombre_rfc + @"</h4>                             
                                <p>Es un gusto para mi poder saludar y reiterarme a sus órdenes!</p>
                                <p>Me permito extender los presentes documentos.</p>
                                <p>Reitero nuevamente nuestro agradecimiento, quedando a sus órdenes.</p><br /><br />
                                <br>
                            </td>
                        </tr>
                    </table>
                    <br /><br />
                    <p>&copy; 2022 <strong>CASTELÁN AUDITORES S.C.</strong></p>
                    <div class='formBorder'></div>
                </div>
                </center>";
            try
            {
                //string email = "contabilidad@consultoriacastelan.com";
                string email = "cobranza@consultoriacastelan.com";

                MailMessage msg = new MailMessage();
                string DireccionaEnviar = correo_; //"programador1@consultoriacastelan.com";
                msg.To.Add(DireccionaEnviar);
                msg.From = new MailAddress(email, "CASTELÁN AUDITORES S.C.", System.Text.Encoding.UTF8);
                //msg.From = new MailAddress("comunicados@facturafast.mx", "FACTURAFAST ", System.Text.Encoding.UTF8);

                msg.Subject = title_;
                msg.SubjectEncoding = System.Text.Encoding.UTF8;
                msg.Body = cuerpo;
                /* Archivo adjunto */
                
                
                Attachment data = new Attachment(fullPath, MediaTypeNames.Application.Pdf);
                Attachment dataXML = new Attachment(fullPathXML);
                msg.Attachments.Add(data);
                msg.Attachments.Add(dataXML);
                /*******/
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                msg.IsBodyHtml = true;

                SmtpClient client = new SmtpClient();
                //client.Credentials = new NetworkCredential(email, "29tR#+54thfq");
                client.Credentials = new NetworkCredential(email, "C0nsultor1a*128");

                client.Port = 587;
                client.Host = "mail.consultoriacastelan.com";
                client.EnableSsl = false;
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chai, SslPolicyErrors sslPolicyErrors)
                { return true; };

                client.Send(msg);
                mensaje = "Enviado";
            }
            catch (Exception ex)
            {
                mensaje = "Ocurrio un problema";
                //nuevoCorreo.mensaje = ex.Message;
            }
            finally
            {
                GC.Collect();
            }
            return Json(mensaje, JsonRequestBehavior.AllowGet);
        }
        //--------------PrePagos---------------------
        public ActionResult getPrePago(string id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var leftOuterJoin = from pp in db.tbd_Pre_Pagos
                                    join d_pp in db.tbd_Detalle_Prepago on pp.id equals d_pp.id_pre_pago into dp
                                    from dpp in dp.DefaultIfEmpty()
                                    join cliente in db.tbc_Clientes on pp.id_cliente equals cliente.id_cliente into fcliente
                                    from f_cliente in fcliente.DefaultIfEmpty()
                                        //join mp in db.tbc_Metodos_Pago on p.metodo_pago equals mp.clave into fpago
                                        //from m_pag in fpago.DefaultIfEmpty()
                                    where f_cliente.rfc == id
                                    select new
                                    {
                                        id_prefactura = pp.id_pre_factura,
                                        id_cliente = pp.id_cliente,
                                        metodo_pago = pp.metodo_pago,
                                        uso_cfdi = pp.uso_cfdi,
                                        serie = pp.serie,
                                        folio = pp.folio,
                                        num_operacion = pp.num_operacion,
                                        tipo_moneda = pp.tipo_moneda,
                                        tipo_cambio = pp.tipo_cambio,
                                        fecha_pago = pp.fecha_pago,
                                        hora = pp.hora,
                                        total = pp.total,
                                        uuid = pp.uuid,
                                        status = pp.status,
                                        forma_pago = dpp.forma_pago,
                                        pago = dpp.pago,
                                        s_actual = dpp.s_actual,
                                        s_anterior = dpp.s_anterior
                                    };
                return Json(leftOuterJoin.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getPrePagoID(int id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var leftOuterJoin = from pp in db.tbd_Pre_Pagos
                                    join d_pp in db.tbc_Clientes on pp.id_cliente equals d_pp.id_cliente into dp
                                    from dpp in dp.DefaultIfEmpty()
                                    join mp in db.tbc_Metodos_Pago on pp.metodo_pago equals mp.id_metodo_pago into mmpago
                                    from mpago in mmpago.DefaultIfEmpty()
                                    join ufi in db.tbc_Usos_CFDI on pp.uso_cfdi equals ufi.id_uso_cfdi into ufdi
                                    from ucfdi in ufdi.DefaultIfEmpty()
                                    where pp.id == id
                                    select new
                                    {
                                        id_prefactura = pp.id_pre_factura,
                                        id_cliente = pp.id_cliente,
                                        rfc = dpp.rfc,
                                        nombre_razon = dpp.nombre_razon,
                                        correo =dpp.correo,
                                        metodo_pago = pp.metodo_pago,
                                        s_metodo_pago = mpago.clave+"-"+mpago.metodo_pago,
                                        uso_cfdi = pp.uso_cfdi,
                                        s_uso_cfdi = ucfdi.clave+"-"+ucfdi.uso_cfdi,
                                        serie = pp.serie,
                                        folio = pp.folio,
                                        num_operacion = pp.num_operacion,
                                        tipo_moneda = pp.tipo_moneda,
                                        tipo_cambio = pp.tipo_cambio,
                                        fecha_pago = pp.fecha_pago.ToString(),
                                        fecha_emision = pp.fecha_emision.ToString(),
                                        hora = pp.hora,
                                        total = pp.total,
                                        uuid = pp.uuid,
                                        url_pdf = pp.url_pdf,
                                        url_xml = pp.url_xml,
                                        status = pp.status
                                    };
                return Json(leftOuterJoin.First(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getDetallePrePago(string uuid)
        {
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var leftOuterJoin = from fac in db.tbd_Facturas
                                    join dp in db.tbd_Detalle_Prepago on fac.uuid equals dp.uuid into d_p
                                    from d_pago in d_p.DefaultIfEmpty()
                                    //from dp in db.tbd_Detalle_Prepago
                                    //join f_pago in db.tbc_Formas_Pago on dp.forma_pago equals f_pago.id_forma_pago into f_p
                                    where fac.uuid == uuid && fac.rfc_emisor == usuario.rfc
                                    select new
                                    {
                                        pago = d_pago.pago,
                                        id_forma_pago = fac.id_forma_pago,
                                        s_actual = d_pago.s_actual,
                                        s_anterior = d_pago.s_anterior
                                    };
                return Json(leftOuterJoin.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getDetallePrePagoId(int id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var leftOuterJoin = from dp in db.tbd_Detalle_Prepago
                                    join fp in db.tbc_Formas_Pago on dp.forma_pago equals fp.id_forma_pago into fpago
                                    from f_pago in fpago.DefaultIfEmpty()
                                    where dp.id_pre_pago == id
                                    select new
                                    {
                                        uuid = dp.uuid,
                                        d_forma_pago = dp.forma_pago,
                                        forma_pago = f_pago.clave+"-"+f_pago.forma_pago,
                                        num_pago = dp.num_pago,
                                        pago = dp.pago,
                                        s_actual = dp.s_actual,
                                        s_anterior = dp.s_anterior
                                    };
                return Json(leftOuterJoin.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getListPrePago(DateTime fecha_i, DateTime fecha_f) {
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var lista = from pp in db.tbd_Pre_Pagos
                            join c in db.tbc_Clientes on pp.id_cliente equals c.id_cliente into rcliente
                            from cliente in rcliente.DefaultIfEmpty()
                            join mp in db.tbc_Metodos_Pago on pp.metodo_pago equals mp.id_metodo_pago into mmpago
                            from mpago in mmpago.DefaultIfEmpty()
                            where pp.id_usuario == usuario.id_usuario && pp.fecha_pago >= fecha_i && pp.fecha_pago <= fecha_f
                            select new
                            {
                                id = pp.id,
                                rfc_cliente = cliente.rfc,
                                nombre_razon = cliente.nombre_razon,
                                total_pago = pp.total,
                                fecha_pago = pp.fecha_pago.ToString(),
                                metodo_pago = mpago.clave +" "+mpago.metodo_pago,
                                serie = pp.serie,
                                folio = pp.folio,
                                status = pp.status
                            };
                return Json(lista.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult SavePrePago(List<PrePago> prepago, List<DetallePrePago> dprepago)
        {
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            int id_pre_pago = 0;
            foreach (var item in prepago) 
            {
                tbd_Pre_Pagos nuevoPago = new tbd_Pre_Pagos
                {
                    id_pre_factura = item.id_factura,
                    id_cliente = item.id_cliente,
                    id_usuario = usuario.id_usuario,
                    metodo_pago = item.metodo_pago,
                    uso_cfdi = item.uso_cfdi,
                    serie = item.serie,
                    folio = item.folio,
                    num_operacion = item.num_operacion,
                    tipo_moneda = item.tipo_moneda,
                    tipo_cambio = item.tipo_cambio == null ? "0":item.tipo_cambio,
                    fecha_pago = item.fecha_pago,
                    fecha_emision = item.f_emision,
                    hora = item.hora,
                    total = item.total,
                    uuid = item.uuid,
                    status = 1
                };
                db.tbd_Pre_Pagos.Add(nuevoPago);
                db.SaveChanges();
                id_pre_pago = nuevoPago.id;
            }
            foreach (var item in dprepago) 
            {
                string santerior_ = item.s_anterior;
                santerior_ = santerior_.Substring(1);
                santerior_ = Regex.Replace(santerior_, ",", "");
                decimal santerior = Convert.ToDecimal(santerior_);
                string pago_ = item.pago;
                pago_ = pago_.Substring(1);
                pago_ = Regex.Replace(pago_, ",", "");
                decimal pago = Convert.ToDecimal(pago_);
                string sactual_ = item.s_actual;
                sactual_ = sactual_.Substring(1);
                sactual_ = Regex.Replace(sactual_, ",", "");
                decimal sactual = Convert.ToDecimal(sactual_);
                tbd_Detalle_Prepago ndetalle = new tbd_Detalle_Prepago
                {
                    id_pre_pago = id_pre_pago,
                    uuid = item.uuid,
                    forma_pago = item.d_forma_pago,
                    num_pago = item.pago_no,
                    s_anterior = santerior,
                    pago = pago,
                    s_actual = sactual,
                    status = 1
                };
                db.tbd_Detalle_Prepago.Add(ndetalle);
                db.SaveChanges();
            }
            return Json(new { redirectToUrl = Url.Action("ListaComplemento", "Facturacion", new { tipo = "Guardar" }) });
        }
        public ActionResult UpdatePrePago(List<PrePago> prepago, List<DetallePrePago> dprepago, int id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                if (dprepago != null)
                {
                    foreach (var ddp in db.tbd_Detalle_Prepago.Where(x => x.id_pre_pago == id))
                    {
                        db.tbd_Detalle_Prepago.Remove(ddp);
                    }
                    foreach (var item in dprepago)
                    {
                        string santerior_ = item.s_anterior;
                            santerior_ = santerior_.Substring(1);
                            santerior_ = Regex.Replace(santerior_,",", "");
                        decimal santerior = Convert.ToDecimal(santerior_);
                        string pago_ = item.pago;
                        pago_ = pago_.Substring(1);
                        pago_ = Regex.Replace(pago_, ",", "");
                        decimal pago = Convert.ToDecimal(pago_);
                        string sactual_ = item.s_actual;
                        sactual_ = sactual_.Substring(1);
                        sactual_ = Regex.Replace(sactual_, ",", "");
                        decimal sactual = Convert.ToDecimal(sactual_);

                        tbd_Detalle_Prepago ndetalle = new tbd_Detalle_Prepago
                        {
                            id_pre_pago = id,
                            uuid = item.uuid,
                            forma_pago = item.d_forma_pago,
                            num_pago = item.pago_no,
                            s_anterior = santerior,
                            pago = pago,
                            s_actual = sactual
                        };
                        db.tbd_Detalle_Prepago.Add(ndetalle);
                        db.SaveChanges();
                    }
                }
                foreach (var item in prepago)
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    var valor = db.tbd_Pre_Pagos.ToList<tbd_Pre_Pagos>().Where(u => u.id == id).FirstOrDefault();
                    valor.id_cliente = item.id_cliente;
                    valor.metodo_pago = item.metodo_pago;
                    valor.uso_cfdi = item.uso_cfdi;
                    valor.serie = item.serie;
                    valor.folio = item.folio;
                    valor.num_operacion = item.num_operacion;
                    valor.tipo_moneda = item.tipo_moneda;
                    valor.tipo_cambio = item.tipo_cambio == null ? "0" : item.tipo_cambio;
                    valor.fecha_pago = item.fecha_pago;
                    valor.fecha_emision = item.f_emision;
                    valor.hora = item.hora;
                    valor.total = item.total;
                    valor.uuid = item.uuid;
                    
                    db.SaveChanges();
                }
               
                return Json(new { redirectToUrl = Url.Action("ListaComplemento", "Facturacion", new { tipo = "Guardar" }) });
            }
            
            
        }
        public ActionResult TimbrarPrePago(Int32? id_)
        {
            try
            {
                string DirPrg = Server.MapPath("~");
                string uuidFactura = Guid.NewGuid().ToString();
                using (BD_FFEntities db = new BD_FFEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    var valor = db.tbd_Pre_Pagos.ToList<tbd_Pre_Pagos>().Where(u => u.id == id_).FirstOrDefault();
                    var valorDP = db.tbd_Detalle_Prepago.ToList<tbd_Detalle_Prepago>().Where(u => u.id_pre_pago == id_).FirstOrDefault();
                    var emisor = db.tbc_Clientes.ToList<tbc_Clientes>().Where(u => u.id_cliente == valor.id_usuario).FirstOrDefault();
                    var receptor = db.tbc_Clientes.ToList<tbc_Clientes>().Where(u => u.id_cliente == valor.id_cliente).FirstOrDefault();
                    string[] rutaa_ = valor.url_pdf.Split('\\');
                    string ruta_ = @"Plantillas\" + rutaa_[0] + @"\" + rutaa_[1] + @"\" + rutaa_[2] + @"\" + rutaa_[3] + @"\";
                    var CFDI = new TCFDI_ComPago(DirPrg, @"CSD_Pruebas_CFDI_EKU9003173C9.cer", @"CSD_Pruebas_CFDI_EKU9003173C9.key", "12345678a")
                    {
                        //Se recomienda asignar un nombre distinto a este archivo, por ejemplo:
                        cTmpFile = DirPrg + ruta_ + uuidFactura + ".tmp"
                    };
                    // Credenciales de timbrado
                    CFDI.aParametros[0] = "PAC";
                    CFDI.aParametros[1] = "Prueba";
                    CFDI.aParametros[2] = "FAC201027H66";
                    CFDI.aParametros[3] = "FAC-CFDI-12409=";
                    // Datos del comprobante
                    CFDI.aComprobante[0] = "4.0";               // Versión del estandar CFDI
                    CFDI.aComprobante[1] = valor.serie;         // Serie
                    CFDI.aComprobante[2] = valor.folio;         // Folio

                    CFDI.aComprobante[3] = string.Format("{0:yyyy-MM-ddTHH:mm:ss}", DateTime.Now.AddMinutes(-2), CultureInfo.CreateSpecificCulture("es-MX"));

                    CFDI.aComprobante[4] = "0";// valor.total.ToString();            // SubTotal
                    CFDI.aComprobante[5] = "XXX";                             // Moneda (Se debe registrar la clave "XXX")
                    CFDI.aComprobante[6] = "0";//valor.total.ToString();            // TOTAL
                    CFDI.aComprobante[7] = "P";                               // Tipo de Comprobante (Catálogo: c_TipoDeComprobante)
                    CFDI.aComprobante[8] = "01";                              // Exportación (Catálogo: c_Exportacion)
                    CFDI.aComprobante[9] = "11000";                           // Lugar de expedición (Catálogo: c_CodigoPostal)
                    CFDI.aComprobante[10] = "";                               // Confirmación.

                    // Datos del CFDI Relacionado
                    // Nota: Se debe registrar cuando aplique la clave "04" (Sustitución de los CFDI previos) de la relación que existe entre
                    //       este comprobante que se está generando y el CFDI que se sustituye.
                    CFDI.cCfdiTipoRelacion = "";
                    var ValorCFDI = "";
                    // Tipo de Relacion (Catálogo: c_TipoRelacion)
                    if (ValorCFDI != null)
                    {
                        CFDI.aCfdiRelacionado[0] = "";                        // Folio fiscal (UUID) de un CFDI relacionado.
                        CFDI.aCfdiRelacionado[1] = "";                        // Folio fiscal (UUID) (puedes agregar varios folios)                    
                    }
                    // Datos de la
                    var reg_fiscal = db.tbc_Regimenes.Where(u => u.id_regimen_fiscal == emisor.id_regimen_fiscal).Select(u => u.clave).First();
                    CFDI.aEmisor[0] = emisor.rfc;                         // RFC del emisor
                    CFDI.aEmisor[1] = emisor.nombre_razon;                // Nombre del emisor
                    CFDI.aEmisor[2] = "601";//reg_fiscal;// Régimen Fiscal. (Catálogo: c_RegimenFiscal) 

                    // Datos del Receptor
                    CFDI.aReceptor[0] = "H&E951128469";// receptor.rfc;                       // RFC del Receptor.
                    CFDI.aReceptor[1] = "HERRERIA & ELECTRICOS";//receptor.nombre_razon;              // Nombre del Receptor.
                    CFDI.aReceptor[2] = "55555";//receptor.direccion_fiscal;          // DomicilioFiscalReceptor (código postal del domicilio fiscal del receptor)
                    CFDI.aReceptor[3] = "";                                 // ResidenciaFiscal (este campo debe ir en blanco)
                    CFDI.aReceptor[4] = "";                                 // NumRegIdTrib (este campo debe ir en blanco)
                    CFDI.aReceptor[5] = "601";//db.tbc_Regimenes.Where(u => u.id_regimen_fiscal == receptor.id_regimen_fiscal).Select(u => u.clave).First();// RegimenFiscalReceptor (Catálogo: c_RegimenFiscal) incorporar la clave del régimen fiscal del contribuyente receptor.
                    CFDI.aReceptor[6] = "CP01";//db.tbc_Usos_CFDI.Where(u => u.id_uso_cfdi == receptor.id_uso_cdfi).Select(u => u.clave).First();// UsoCFDI (Se debe registrar la clave "CP01" Pagos)

                    // Conceptos.
                    // Sólo aplica un concepto.

                    CFDI.aConceptos[0, 0] = "84111506";                       // Clave Producto Servicio (Catálogo: c_ClaveProdServ)
                    CFDI.aConceptos[0, 1] = "";                               // NoIdentificacion (este campo debe ir en blanco)
                    CFDI.aConceptos[0, 2] = "1";                             // Cantidad
                    CFDI.aConceptos[0, 3] = "ACT";                            // Clave Unidad de medida (se debe registrar el valor "ACT")
                    CFDI.aConceptos[0, 4] = "";                               // Unidad de medida (este campo debe ir en blanco)
                    CFDI.aConceptos[0, 5] = "Pago";                           // Descripción del producto (se debe registrar el valor "Pago")
                    CFDI.aConceptos[0, 6] = "0";                              // Importe unitario (se debe registrar el valor "0")
                    CFDI.aConceptos[0, 7] = "0";                              // Importe Total (se debe registrar el valor "0")
                    CFDI.aConceptos[0, 8] = "";                               // Descuento. (este campo debe ir en blanco)
                    CFDI.aConceptos[0, 9] = "01";                             // ObjetoImp. (Catálogo: c_ObjetoImp)

                    // Datos del Complemento de Pago v2.0
                    CFDI.aPagos[0] = "2.0";                                   // Versión del Complemento de Pago.

                    // Arreglo: aTotales requerido para especificar el monto total de los pagos y el total de los impuestos.
                    // Nota: Deben ser expresados en MXN.
                    CFDI.aTotales[0] = "";                                    // TotalRetencionesIVA, total de los impuestos retenidos de IVA.
                    CFDI.aTotales[1] = "";                                    // TotalRetencionesISR, total de los impuestos retenidos de ISR.
                    CFDI.aTotales[2] = "";                                    // TotalRetencionesIEPS, total de los impuestos retenidos de IEPS.
                    CFDI.aTotales[3] = "";                                    // TotalTrasladosBaseIVA16, total de la base de IVA trasladado a la tasa del 16%.
                    CFDI.aTotales[4] = "";                                    // TotalTrasladosImpuestoIVA16, total de los impuestos de IVA trasladado a la tasa del 16%.
                    CFDI.aTotales[5] = "";                                    // TotalTrasladosBaseIVA8, total de la base de IVA trasladado a la tasa del 8%.
                    CFDI.aTotales[6] = "";                                    // TotalTrasladosImpuestoIVA8, total de los impuestos de IVA trasladado a la tasa del 8%.
                    CFDI.aTotales[7] = "";                                    // TotalTrasladosBaseIVA0, total de la base de IVA trasladado a la tasa del 0%.
                    CFDI.aTotales[8] = "";                                    // TotalTrasladosImpuestoIVA0, total de los impuestos de IVA trasladado a la tasa del 0%.
                    CFDI.aTotales[9] = "";                                    // TotalTrasladosBaseIVAExento, total de la base de IVA trasladado exento.
                    CFDI.aTotales[10] = valorDP.pago.ToString();//"1000.00";                            // MontoTotalPagos, total de los pagos que se desprenden de los nodos Pago.

                    // Arreglo: aPago( Número de pago, Atributo ) | Hasta 10 pagos por CFDI.
                    var fpago = db.tbc_Formas_Pago.Where(u => u.id_forma_pago == valorDP.forma_pago).Select(u => u.clave).First();
                    
                    CFDI.aPago[0, 0] = "2021-03-22T18:10:24";                 // Fecha de Pago
                    CFDI.aPago[0, 1] = "03";//fpago;                          // Forma de Pago (Catálogo: c_FormaPago)
                    CFDI.aPago[0, 2] = "MXN";                                 // Moneda (Catálogo: c_Moneda)
                    CFDI.aPago[0, 3] = "1";//valor.tipo_cambio;               // Tipo de Cambio
                    CFDI.aPago[0, 4] = valorDP.pago.ToString();               // Monto
                    CFDI.aPago[0, 5] = "123";//valor.num_operacion.ToString();// Número de Operación
                    CFDI.aPago[0, 6] = "";                                    // RFC de la emisora de la cuenta origen, es decir, la operadora, banco, institución financiera, etc. (Ordenante)
                    CFDI.aPago[0, 7] = "";                                    // Nombre del banco ordenante extranjero
                    CFDI.aPago[0, 8] = "";                                    // Cuenta Ordenante 
                    CFDI.aPago[0, 9] = "";                                    // RFC de la cuenta destino, es decir, la operadora, banco, institución financiera, etc. (Beneficiario)
                    CFDI.aPago[0, 10] = "";                                   // Cuenta del Beneficiario
                    CFDI.aPago[0, 11] = "";                                   // Tipo Cadena Pago (Catálogo: c_TipoCadenaPago)
                    CFDI.aPago[0, 12] = "";                                   // Texto en Base64 del Certificado que corresponde al pago
                    CFDI.aPago[0, 13] = "";                                   // Cadena original del comprobante de pago generado por la entidad emisora de la cuenta beneficiaria
                    CFDI.aPago[0, 14] = "";                                   // Sello digital que se asocie al pago

                    // Datos del documento relacionado.
                    // Nota: Se debe expresar el listado de los documentos relacionados con los pagos de la operación inicial. 
                    //       Por cada documento relacionado deberá agregar un elemento al arreglo aDoctoRelacionado.  

                    // Arreglo: aDoctoRelacionado( Número de pago, Elemento documento relacionado, Atributos ) | 20 documentos.
                    CFDI.aDoctoRelacionado[0, 0, 0] = "9D79AFC9-58DB-490B-B512-0E357FDA59C5";//valorDP.uuid.ToString(); // UUID de la factura relacionada con el pago
                    CFDI.aDoctoRelacionado[0, 0, 1] = "A";                    // Serie
                    CFDI.aDoctoRelacionado[0, 0, 2] = "150";                  // Folio
                    CFDI.aDoctoRelacionado[0, 0, 3] = "MXN";                  // Moneda (Catálogo: c_Moneda)
                    CFDI.aDoctoRelacionado[0, 0, 4] = "1";                    // Equivalencia
                    CFDI.aDoctoRelacionado[0, 0, 5] = "1";                    // Número de Parcialidad
                    CFDI.aDoctoRelacionado[0, 0, 6] = valorDP.s_anterior.ToString();// Monto del saldo insoluto de la parcialidad anterior
                    CFDI.aDoctoRelacionado[0, 0, 7] = valorDP.pago.ToString();      // Importe pagado que corresponde al documento relacionado.
                    CFDI.aDoctoRelacionado[0, 0, 8] = valorDP.s_actual.ToString();  // Diferencia entre el importe del saldo anterior y el monto del pago
                    CFDI.aDoctoRelacionado[0, 0, 9] = "01";                   // ObjetoImp. (Catálogo: c_ObjetoImp)

                    //-----------------------------------------------
                    CFDI.id_pre[0] = valor.id_pre_factura.ToString();
                    CFDI.id_pre[1] = valor.url_pdf;
                    //-----
                    CFDI.GenerarCP();
                    string mensaje = CFDI.Mensaje;
                    
                    string[] respuesta_men = mensaje.Split('|');
                    string res = respuesta_men[0];
                    string ruta_xml = respuesta_men[1];
                    string n_certificado = respuesta_men[2];
                    string sello_digital = respuesta_men[3];
                    string uuid = respuesta_men[4];
                    string cer_sat = respuesta_men[5];
                    string sello_sat = respuesta_men[6];
                    string fca_timbre = respuesta_men[7];
                    
                    if (res == "Timbrado")
                    {
                        valor.selloCFDI = sello_digital;
                        valor.selloSAT = sello_sat;
                        valor.ccertificacion = n_certificado;
                        //valor.fca_timbrado = Convert.ToDateTime(fca_timbre);
                        valor.url_xml = ruta_xml;
                        valor.status = 2;
                        db.SaveChanges();
                    }

                    return Json(res, JsonRequestBehavior.AllowGet);
                    //CFDI = null;
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        //--------------------PREVIEW--------------------------------
        public JsonResult VisPrePagos(Int32? id)
        {
            if (Session["tbc_Usuarios"] == null)
                return Json("Error", JsonRequestBehavior.AllowGet);
            CultureInfo ci = new CultureInfo("en-us");
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            //----------------------------------------------------------------------------------------------------------------------------
            db = new BD_FFEntities();
            tbd_Pre_Pagos prepago_ = db.tbd_Pre_Pagos.Where(s => s.id == id).Single();
            var dprepago = db.tbd_Detalle_Prepago.ToList<tbd_Detalle_Prepago>().Where(u => u.id_pre_pago == id).ToList();
            tbc_Clientes cliente = db.tbc_Clientes.Where(u => u.id_cliente == prepago_.id_cliente).Single();
            tbd_Facturas facturas_ = db.tbd_Facturas.Where(s => s.id_factura == prepago_.id_pre_factura).Single();
            bool fileExist_ = false;
            //-----------------------------------------------------------------------------------------------------------------------------
            var ruta = db.tbc_Variables_Calculo.Where(s => s.id_variable == 1).ToList().First();
            var fca_pago = prepago_.fecha_pago.ToString();
            string namefile = "";
            String[] fechaE = fca_pago.Split(' ');
            string aux_fc_emi = fechaE[0];
            String[] auxfechaE = aux_fc_emi.Split('/');
            string ax_fc_emi = auxfechaE[0] + auxfechaE[1] + auxfechaE[2];
            string DirPrg = Server.MapPath("~");
            if (prepago_.url_pdf == null || prepago_.url_pdf == "")
            {
                namefile = RandomString(7) + "-" + RandomString(4) + "-" + RandomString(4) + "-" + RandomString(4) + "-" + RandomString(12);
            }
            else
            {
                string[] nom_doc = prepago_.url_pdf.Split('\\');
                string[] nd = nom_doc[5].Split('.');
                string nf = nd[0];
                namefile = nf;
            }
            string path = "Plantillas/PREPAGO/XML/PDF/" + cliente.rfc + "/" + ax_fc_emi + "/" + namefile + ".pdf";
            //ruta.url_pdf +"PRE_FAC_"+ prefactura_.id_pre_factura+ ".pdf";
            //-----------------------------------------------------------------------------------------------------------------------------
            bool fileExist = System.IO.File.Exists(path);
            FileInfo file = new FileInfo(path);
            try
            {
                file.Delete();
                fileExist = System.IO.File.Exists(path);
            }
            catch (Exception e)
            {
            }
            if (!fileExist)
            {
                string auxpath = DirPrg + "Plantillas\\PREPAGO\\XML\\PDF\\" + cliente.rfc + "\\" + ax_fc_emi;
                DirectoryInfo di = Directory.CreateDirectory(auxpath);
                string auxpathdoc = DirPrg + "Plantillas\\PREPAGO\\XML\\DOCX\\" + cliente.rfc + "\\" + ax_fc_emi;
                DirectoryInfo didoc = Directory.CreateDirectory(auxpathdoc);
                string nombrearchivo = "";
                object ObjMiss = System.Reflection.Missing.Value;
                Word.Application ObjWord = new Word.Application();

                nombrearchivo = "ComplementoPago.docx";

                Word.Document ObjDoc = ObjWord.Documents.Open(DirPrg + "/Plantillas/" + nombrearchivo, ObjMiss);

                //Definir Marcadores
                object nombre_emisor = "Nombre_Emisor";
                object serie_folio2 = "Serie_Folio2";
                object rfc_emisor = "RFC_Emisor";
                object lugar_expedicion = "Lugar_Expedicion";
                object serie_folio = "Serie_Folio";
                object fecha_ = "Fecha_";

                object nombre_cliente = "Nombre_Cliente";
                //object c_p_receptor = "C_P_Receptor";
                object rfc_cliente = "RFC_Cliente";
                object no_certificado = "No_Certificado";
                object regimen_fiscal = "Regimen_Fiscal";
                object direccion = "Direccion";
                object ciudad = "Ciudad";

                object fecha_hora_pago = "Fecha_Hora_Pago";
                object forma_pago = "Forma_Pago";
                object total_pago = "Total_Pago";
                object cantidad_letra = "Cantidad_Letra";
                
                object Tabla_ = "Agregar_Tabla";
                object Tabla_cfdirelacionados = "Agregar_Tabla_CFDIRelacionados";

                object cadena_original = "Cadena_Original";
                object sello_cfd = "Sello_CFD";
                object sello_sat = "Sello_SAT";
                object fecha_timbrado = "Fecha_Timbrado";
                object version_timbre = "Version_Timbre";
                object certificado_sat = "Certificado_SAT";
                object uuid = "UUID_";
                //Busqueda de marcadores en la plantilla
                Word.Range nombreemisor = ObjDoc.Bookmarks.get_Item(ref nombre_emisor).Range;
                Word.Range seriefolio2 = ObjDoc.Bookmarks.get_Item(ref serie_folio2).Range;
                Word.Range rfcemisor = ObjDoc.Bookmarks.get_Item(ref rfc_emisor).Range;
                Word.Range lugarexpedicion = ObjDoc.Bookmarks.get_Item(ref lugar_expedicion).Range;
                Word.Range seriefolio = ObjDoc.Bookmarks.get_Item(ref serie_folio).Range;
                Word.Range fecha = ObjDoc.Bookmarks.get_Item(ref fecha_).Range;

                Word.Range nombrecliente = ObjDoc.Bookmarks.get_Item(ref nombre_cliente).Range;
                Word.Range direccion_ = ObjDoc.Bookmarks.get_Item(ref direccion).Range;
                Word.Range ciudad_ = ObjDoc.Bookmarks.get_Item(ref ciudad).Range;
                //Word.Range cpreceptor = ObjDoc.Bookmarks.get_Item(ref c_p_receptor).Range;
                Word.Range rfccliente = ObjDoc.Bookmarks.get_Item(ref rfc_cliente).Range;
                Word.Range regimenfiscal = ObjDoc.Bookmarks.get_Item(ref regimen_fiscal).Range;
                Word.Range nocertificado = ObjDoc.Bookmarks.get_Item(ref no_certificado).Range;

                Word.Range fechahorappago = ObjDoc.Bookmarks.get_Item(ref fecha_hora_pago).Range;
                Word.Range formapago = ObjDoc.Bookmarks.get_Item(ref forma_pago).Range;
                Word.Range totalpago = ObjDoc.Bookmarks.get_Item(ref total_pago).Range;
                Word.Range cantidadletra = ObjDoc.Bookmarks.get_Item(ref cantidad_letra).Range;
                
                Word.Range Tabla = ObjDoc.Bookmarks.get_Item(ref Tabla_).Range;
                Word.Range Tabla_CFDIRelacionados = ObjDoc.Bookmarks.get_Item(ref Tabla_cfdirelacionados).Range;

                Word.Range cadenaoriginal = ObjDoc.Bookmarks.get_Item(ref cadena_original).Range;
                Word.Range sellocfd = ObjDoc.Bookmarks.get_Item(ref sello_cfd).Range;
                Word.Range sellosat = ObjDoc.Bookmarks.get_Item(ref sello_sat).Range;
                Word.Range fechatimbrado = ObjDoc.Bookmarks.get_Item(ref fecha_timbrado).Range;
                Word.Range versiontimbre = ObjDoc.Bookmarks.get_Item(ref version_timbre).Range;
                Word.Range certificadosat = ObjDoc.Bookmarks.get_Item(ref certificado_sat).Range;
                Word.Range uuid_ = ObjDoc.Bookmarks.get_Item(ref uuid).Range;

                //Agregar texto al marcador
                nombreemisor.Text = usuario.nombre_razon;
                seriefolio2.Text = prepago_.folio;//db.tbc_Clientes.Where(u => u.rfc == prefactura_.rfc_usuario).Select(u => u.nombre_razon).First();
                rfcemisor.Text = usuario.rfc;
                lugarexpedicion.Text = usuario.cp;
                seriefolio.Text = prepago_.serie+"-"+prepago_.folio;
                fecha.Text = prepago_.fecha_emision.ToString();

                nombrecliente.Text = db.tbc_Clientes.Where(u => u.id_cliente == prepago_.id_cliente).Select(u => u.nombre_razon).First();
                rfccliente.Text = cliente.rfc;
                direccion_.Text = usuario.calle + " " +usuario.num_ext+" "+usuario.num_int+","+usuario.colonia+","+ usuario.localidad;
                ciudad_.Text = usuario.municipio + "," + usuario.estado;
                nocertificado.Text = "";//db.tbd_Firmas.Where(u => u.rfc == cliente.rfc).Select(u => u.certificado_fiel).First();
                regimenfiscal.Text = cliente.id_regimen_fiscal.ToString() + "-" + db.tbc_Regimenes.Where(u => u.id_regimen_fiscal == cliente.id_regimen_fiscal).Select(u => u.regimen).First();

                var fpago = db.tbc_Metodos_Pago.ToList<tbc_Metodos_Pago>().Where(s => s.id_metodo_pago == Convert.ToInt32(prepago_.metodo_pago)).Single();
                decimal totalEntero = Convert.ToDecimal(prepago_.total);
                fechahorappago.Text = prepago_.fecha_pago.ToString();
                formapago.Text = fpago.clave + "-" + fpago.metodo_pago;
                totalpago.Text = totalEntero.ToString("C");
                cantidadletra.Text = totalEntero.NumeroALetras();
                //Creacion y definicion de tabla
                var cantProductos = db.tbd_Detalle_Prepago.Where(s => s.id_pre_pago == prepago_.id).ToList();
                dprepago.Count();
                //-------------------------------------------------------------------------------------------------------------
                Word.Table TablaConcepto;
                TablaConcepto = ObjDoc.Tables.Add(Tabla, dprepago.Count, 6);

                //var query = db.tbd_Pre_Pagos.ToList<tbd_Pre_Pagos>().Where(s => s.id == dprepago[z].id_pre_pago).Single();
                TablaConcepto.Cell(1, 1).Range.Text = "84111506";
                TablaConcepto.Cell(1, 2).Range.Text = "1";
                TablaConcepto.Cell(1, 3).Range.Text = "Pago";
                TablaConcepto.Cell(1, 4).Range.Text = "ACT";
                TablaConcepto.Cell(1, 5).Range.Text = "$0";
                TablaConcepto.Cell(1, 6).Range.Text = "$0";
                
                TablaConcepto.Columns[1].SetWidth(80, 0);
                TablaConcepto.Columns[2].SetWidth(80, 0);
                TablaConcepto.Columns[3].SetWidth(200, 0);
                TablaConcepto.Columns[4].SetWidth(80, 0);
                TablaConcepto.Columns[5].SetWidth(100, 0);
                TablaConcepto.Columns[6].SetWidth(80, 0);
                TablaConcepto.Borders[Word.WdBorderType.wdBorderHorizontal].Visible = false;
                TablaConcepto.Borders[Word.WdBorderType.wdBorderVertical].Visible = false;
                //-------------------------------------------------------------------------------------------------------------
                int i = 1;
                Word.Table TablaDet;
                TablaDet = ObjDoc.Tables.Add(Tabla_CFDIRelacionados, dprepago.Count, 7);

                for (int z = 0; z <= dprepago.Count - 1; z++)
                {
                    var query = db.tbd_Pre_Pagos.ToList<tbd_Pre_Pagos>().Where(s => s.id == dprepago[z].id_pre_pago).Single();
                    var fpagos = db.tbc_Formas_Pago.ToList<tbc_Formas_Pago>().Where(s => s.id_forma_pago == Convert.ToInt32(dprepago[z].forma_pago)).Single();
                    decimal saldo_a = Convert.ToDecimal(dprepago[z].s_anterior);
                    decimal saldo_actual = Convert.ToDecimal(dprepago[z].s_actual);
                    decimal pagado = Convert.ToDecimal(dprepago[z].pago);
                    decimal total_ = Convert.ToDecimal(query.total);
                    TablaDet.Cell(i, 1).Range.Text = dprepago[z].uuid;
                    TablaDet.Cell(i, 2).Range.Text = query.serie + "-" + query.folio;
                    TablaDet.Cell(i, 3).Range.Text = fpagos.clave+"-"+fpagos.forma_pago;
                    TablaDet.Cell(i, 4).Range.Text = saldo_a.ToString("C");
                    TablaDet.Cell(i, 5).Range.Text = saldo_a.ToString("C");
                    TablaDet.Cell(i, 6).Range.Text = saldo_actual.ToString("C");
                    TablaDet.Cell(i, 7).Range.Text = pagado.ToString("C");
                    i++;
                }

                TablaDet.Columns[1].SetWidth(170, 0);
                TablaDet.Columns[2].SetWidth(50, 0);
                TablaDet.Columns[3].SetWidth(60, 0);
                TablaDet.Columns[4].SetWidth(80, 0);
                TablaDet.Columns[5].SetWidth(80, 0);
                TablaDet.Columns[6].SetWidth(80, 0);
                TablaDet.Columns[7].SetWidth(100, 0);
                TablaDet.Borders[Word.WdBorderType.wdBorderHorizontal].Visible = false;
                TablaDet.Borders[Word.WdBorderType.wdBorderVertical].Visible = false;
                //Fin creacion y definicion de tabla

                cadenaoriginal.Text = "";//db.tbd_Firmas.Where(u => u.rfc == cliente.rfc).Select(u => u.certificado_fiel).First();
                sellocfd.Text = prepago_.selloCFDI;
                sellosat.Text = prepago_.selloSAT;
                fechatimbrado.Text = prepago_.fca_timbrado.ToString();
                versiontimbre.Text = prepago_.version_cfdi;
                certificadosat.Text = prepago_.ccertificacion;
                uuid_.Text = prepago_.uuid;
                //Cerrar word
                ObjDoc.SaveAs2(DirPrg + "/Plantillas/PREPAGO/XML/DOCX/" + cliente.rfc + "/" + ax_fc_emi + "/" + namefile + ".docx");
                ObjDoc.Close();
                ObjWord.Quit();

                //Crear PDF
                var pdfProcess = new Process();
                pdfProcess.StartInfo.FileName = "" + ruta.url_libreoffice;
                pdfProcess.StartInfo.Arguments = "--headless --convert-to pdf " + DirPrg + "Plantillas\\PREPAGO\\XML\\DOCX\\" + cliente.rfc + "\\" + ax_fc_emi + "\\" + namefile + ".docx --outdir  " + DirPrg + "Plantillas\\PREPAGO\\XML\\PDF\\" + cliente.rfc + "\\" + ax_fc_emi + "\\";
                pdfProcess.Start();
                fileExist_ = System.IO.File.Exists(DirPrg + "Plantillas\\" + prepago_.url_pdf);
                //Actualizar 
                if (prepago_.status == 1) 
                {
                    prepago_.url_pdf = "PREPAGO\\XML\\PDF\\" + cliente.rfc + "\\" + ax_fc_emi + "\\" + namefile + ".pdf";
                    prepago_.url_xml = "PREPAGO\\XML\\PDF\\" + cliente.rfc + "\\" + ax_fc_emi + "\\";
                }
                db.SaveChanges();
            }
            //-----------------------------------------------------------------------------------------------------
            if (!fileExist_)
            {
                return Json("NG", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(path, JsonRequestBehavior.AllowGet);
            }
        }
        //===============================================================================================================================================================================================================================================================
        public JsonResult GetFirma()
        {
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var bandera = false;
            var ruta = Server.MapPath("/Plantillas/FIRMAS/" + usuario.rfc);
            if (Directory.Exists(ruta))
            {
                bandera = true;
            }
            return Json(bandera,JsonRequestBehavior.AllowGet);
        }
        public string RandomString(int length)
        {
            BD_FFEntities db = new BD_FFEntities();
            String clave = "";
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            clave = DateTime.Now.ToString("yyyyMM") + "-" + new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            if (db.tbd_Notas_Venta.Where(s => s.clave_nota == clave).Count() > 0)
            {
                return RandomString(8);
            }
            else
            {
                return clave;
            }
        }
        //================================================================================================================================================================================================================================
        public JsonResult getUFacturas()
        {
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var lista = from pf in db.tbd_Pre_Factura
                            where pf.id_usuario == usuario.id_usuario
                            select new
                            {
                                id = pf.id_pre_factura,
                                cliente = pf.nombre_rfc,
                                tipo = pf.tipo,
                                fca_emision = pf.fecha_emision.ToString(),
                                status = pf.status
                            };
                return Json(lista.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        //================================================================================================================================================================================================================================
        public JsonResult test() {
            //string mensaje = "Timbrado|30001000000400002434|io3VbJ3+Fr3VaP5lS+iMOugm2oH+vyquRrq0WNwPTzlA2Z4JwiDC+KkTCOzKEyiZ2OfAV7/Qq+5Cp6mSLd84VARWCb5ApJg3woTmQV+S5SBtdFvyMh1Rw/u9sGWT6OfR3WwnQcvRp/YGCf5mW0i853PJUPNy2aYxrGIs65gq7PDQbN/cVb6HJKMPfg0YSPlQyIqyhywv+tru06WrgA6psElBDY2RbJGv2uREXfJKae9o6oXs+Z++87Bv1AN+jb9HoJ+mZ/uaMU17PSXFIZAiFwwuvh1bMrz8ELGZl4hS9aBuwKpcuHdQrlocgZhKcLnD31qmpqR7HtbLYYb+c6FdNg==|b15fcd50-b82f-436d-9859-ccb0fd4dc257|30001000000400002495|LUPN32N8qtZcr/OI7Kqj1zth2LHWcaGiJDybtiwbXH+w0ek5Xykjfr6mixD9NX+mEf+6Rr/Mus2/JxmMdy4A1D+IzGKTVvQf/MHgKCwvbxH5yRi9MK2UFAnqM6+AeljHSgeNfQYB9Yj1j6bLov+RHyB4LFXlZlwzCyCdmzBSbNB8tDXdJAS5INPPFxWKQp0pME73rofjNN3Rdx0ZfIEPVeNQuBLIwzASMw/cGSaMO129wUn25KqRCOOkiLCW16EF7cV6V0wNoMUOHhVfeQjckV/r2zz1htZ1HOT4dsxlx1DAi00rPkhoMX4XOBLZzZdwossdXRHWkKlxOg0a4zTRkA==|2022-09-29T13:49:05|||1.1|b15fcd50-b82f-436d-9859-ccb0fd4dc257|2022-09-29T13:49:05|SPR190613I52|io3VbJ3+Fr3VaP5lS+iMOugm2oH+vyquRrq0WNwPTzlA2Z4JwiDC+KkTCOzKEyiZ2OfAV7/Qq+5Cp6mSLd84VARWCb5ApJg3woTmQV+S5SBtdFvyMh1Rw/u9sGWT6OfR3WwnQcvRp/YGCf5mW0i853PJUPNy2aYxrGIs65gq7PDQbN/cVb6HJKMPfg0YSPlQyIqyhywv+tru06WrgA6psElBDY2RbJGv2uREXfJKae9o6oXs+Z++87Bv1AN+jb9HoJ+mZ/uaMU17PSXFIZAiFwwuvh1bMrz8ELGZl4hS9aBuwKpcuHdQrlocgZhKcLnD31qmpqR7HtbLYYb+c6FdNg==|30001000000400002495|||C:\\Users\\Desarrollo\\source\\repos\\Facturafast\\Plantillas\\XML\\PDF\\JES900109Q90\\29092022\\01fbd84b-b318-4bd2-9632-1f01d8f44b56.xml";
            string mensaje = "XML\\PDF\\JES900109Q90\\01092022\\PRE_FAC_10.pdf";
            string[] respuesta_men = mensaje.Split('|');
            
            //string res = respuesta_men[0];
            //string n_certificado = respuesta_men[1];
            //string sello_digital = respuesta_men[2];
            //string uuid = respuesta_men[3];
            //string cer_sat = respuesta_men[4];
            //string sello_sat = respuesta_men[5];
            //string fca_timbre = respuesta_men[6];
            return Json(respuesta_men, JsonRequestBehavior.AllowGet);
        }
    }
}