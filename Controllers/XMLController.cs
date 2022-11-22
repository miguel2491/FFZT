using Facturafast.CLS40;
using Facturafast.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;

namespace Facturafast.Controllers
{
    public class XMLController : Controller
    {
        static string p = "";
        static string p_xml = "";
        BD_FFEntities db;
        // GET: XML
        public ActionResult Index()
        {
            return View();
        }
        //Gen XML Factura
        public ActionResult GenXML(Int32? id_)
        {
            //************************************
            //Get Info PreFac en DB
            db = new BD_FFEntities();
            var factura = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == id_).Single();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var firma = db.tbd_Firmas.ToList<tbd_Firmas>().Where(u => u.rfc == usuario.rfc).Single();
            var receptor_c = db.tbc_Clientes.ToList<tbc_Clientes>().Where(u => u.rfc == factura.rfc_cliente && u.rfc_usuario == usuario.rfc).Single();
            //Ruta donde alojamos los Archivos
            var ruta_xml = factura.url_xml;
            string[] nom_doc = factura.url_pdf.Split('\\');
            string[] nd = nom_doc[4].Split('.');
            string nf = nd[0];
            string namefile = nf;
            //************************************
            string path = Server.MapPath("~");
            p = path;
            string pathXML = path + @"\Plantillas\" + ruta_xml + "\\" + namefile + ".xml";
            string pathCer = path + @"\Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_cer_sello;
            string pathKey = path + @"\Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_key_sello;
            string clavePrivada = firma.password_sello;
            p_xml = pathXML;
            //Obtenemos el Número de Certificado
            string numeroCertificado, aa, b, c;
            SelloDigital.leerCER(pathCer, out aa, out b, out c, out numeroCertificado);
            //----------------Llenamos la clase COMPROBANTE ---------------------------
            Comprobante oComprobante = new Comprobante();
            oComprobante.Version = "4.0";
            oComprobante.Serie = factura.serie;
            oComprobante.Folio = factura.folio;
            oComprobante.Fecha = DateTime.Now.AddMinutes(-2).ToString("yyyy-MM-ddTHH:mm:ss");
            //oComprobante.Sello = ""; //sig video
            oComprobante.FormaPago = db.tbc_Formas_Pago.Where(u => u.id_forma_pago == factura.forma_pago).Select(u => u.clave).First();//99 
            oComprobante.NoCertificado = numeroCertificado;
            //oComprobante.Certificado = ""; //sig video
            oComprobante.SubTotal = Convert.ToDecimal(factura.subtotal);//10.00m;
            oComprobante.Moneda = factura.moneda;
            oComprobante.Total = Convert.ToDecimal(factura.total);//11.60m;
            oComprobante.TipoDeComprobante = factura.tipo_comprobante;//"I";
            oComprobante.MetodoPago = db.tbc_Metodos_Pago.Where(u => u.id_metodo_pago == factura.metodo_pago).Select(u => u.clave).First();
            oComprobante.LugarExpedicion = factura.lugar_expedicion;//"20131";
            oComprobante.Descuento = 0;//Convert.ToDecimal(factura.descuento2);//""
            oComprobante.Exportacion = "01";

            oComprobante.FormaPagoSpecified = true;
            oComprobante.MetodoPagoSpecified = true;
            //Emisor
            ComprobanteEmisor oEmisor = new ComprobanteEmisor();
            oEmisor.Rfc = usuario.rfc;//"EKU9003173C9";
            oEmisor.Nombre = usuario.nombre_razon;//"ESCUELA KEMPER URGATE";
            oEmisor.RegimenFiscal = db.tbc_Regimenes.Where(u => u.id_regimen_fiscal == usuario.id_regimen_fiscal).Select(u => u.clave).First(); //"601";//
            //Receptor
            ComprobanteReceptor oReceptor = new ComprobanteReceptor();
            oReceptor.Nombre = receptor_c.nombre_razon;//"MARIA OLIVIA MARTINEZ SAGAZ";
            oReceptor.Rfc = receptor_c.rfc;//"MASO451221PM4";
            oReceptor.UsoCFDI = db.tbc_Usos_CFDI.Where(u => u.id_uso_cfdi == factura.clave_uso_cfdi).Select(u => u.clave).First();//"G03";
            oReceptor.RegimenFiscalReceptor = factura.reg_fiscal_usuario;//"606";
            oReceptor.DomicilioFiscalReceptor = receptor_c.direccion_fiscal;//"80290";
            //Asigno emisor y receptor
            oComprobante.Emisor = oEmisor;
            oComprobante.Receptor = oReceptor;
            //Conceptos
            List<ComprobanteConcepto> lstConceptos = new List<ComprobanteConcepto>();
            ComprobanteConcepto oConcepto = new ComprobanteConcepto();
            //--------------------------------------------------------------------------------------------------------
            Decimal total_trasladado = 0;
            Decimal total_retenido = 0;

            Decimal total_iva_ret = 0;
            Decimal total_isr_ret = 0;

            Decimal base_iva = 0;
            ComprobanteConceptoImpuestos conceptoImpuestos = new ComprobanteConceptoImpuestos();

            ComprobanteConceptoImpuestosTraslado comprobanteConceptoImpuestosTraslado = new ComprobanteConceptoImpuestosTraslado();
            ComprobanteImpuestosTraslado[] comprobanteImpuestosTraslados = new ComprobanteImpuestosTraslado[1];
            ComprobanteImpuestos impuesto = new ComprobanteImpuestos();
            var valorConc = db.tbd_Conceptos_Pre_Factura.ToList<tbd_Conceptos_Pre_Factura>().Where(u => u.id_pre_factura == id_).ToList();
            for (int i = 0; i < valorConc.Count; i++)
            {
                Decimal canti = Convert.ToDecimal(valorConc[i].cantidad);
                Decimal imp_unitario = Convert.ToDecimal(valorConc[i].importe_unitario);
                Decimal imp_total = Convert.ToDecimal(valorConc[i].importe_total);
                Decimal descuento = valorConc[i].descuento == "" ? 0 : Convert.ToDecimal(valorConc[i].descuento);
                oConcepto.Importe = Math.Round(imp_unitario, 2);//10.00m;
                oConcepto.ClaveProdServ = valorConc[i].c_prod_serv;//"50202306";
                oConcepto.Cantidad = Convert.ToDecimal(valorConc[i].cantidad);//1;
                oConcepto.ClaveUnidad = valorConc[i].c_unidad_medida;//"H87";
                oConcepto.Descripcion = valorConc[i].concepto;//"Refresco de Cola";
                oConcepto.ValorUnitario = Math.Round(imp_unitario, 2);//10.00m;
                oConcepto.Descuento = descuento;
                oConcepto.ObjetoImp = "02";
                oConcepto.Unidad = valorConc[i].unidad;//"Pieza";

                if (valorConc[i].iva_tasa.Trim() != "0.00")
                {
                    Decimal i_total = Convert.ToDecimal(valorConc[i].importe_total);
                    comprobanteConceptoImpuestosTraslado.Base = Math.Round(imp_unitario, 2);//10
                    comprobanteConceptoImpuestosTraslado.TasaOCuota = Convert.ToDecimal(valorConc[i].iva_tasa);//0.160000m;
                    comprobanteConceptoImpuestosTraslado.Impuesto = valorConc[i].iva_imp_traslado;//"002";
                    comprobanteConceptoImpuestosTraslado.Importe = Math.Round(Convert.ToDecimal(valorConc[i].iva_tasa_impuesto), 2);//1.60m;
                    comprobanteConceptoImpuestosTraslado.TipoFactor = valorConc[i].tipo_factor.Trim();//"Tasa";
                    comprobanteConceptoImpuestosTraslado.ImporteSpecified = true;
                    comprobanteConceptoImpuestosTraslado.TasaOCuotaSpecified = true;
                    total_trasladado += Math.Round(Convert.ToDecimal(valorConc[i].iva_tasa_impuesto), 2);
                    base_iva += Math.Round(Convert.ToDecimal(valorConc[i].importe_total), 2);
                }
                //Retenido
                if (valorConc[i].isr_ret_impuesto.Trim() != "0.00")
                {
                    Decimal i_total = Convert.ToDecimal(valorConc[i].importe_total);

                    total_retenido += Convert.ToDecimal(valorConc[i].isr_ret_impuesto);
                    total_isr_ret += Convert.ToDecimal(valorConc[i].isr_ret_impuesto);
                }
                //
                if (valorConc[i].iva_ret_impuesto.Trim() != "0.00")
                {
                    Decimal i_total = Convert.ToDecimal(valorConc[i].importe_total);

                    total_retenido += Convert.ToDecimal(valorConc[i].iva_ret_impuesto);
                    total_iva_ret += Convert.ToDecimal(valorConc[i].iva_ret_impuesto);
                }

                conceptoImpuestos.Traslados = new ComprobanteConceptoImpuestosTraslado[1];

                conceptoImpuestos.Traslados[0] = comprobanteConceptoImpuestosTraslado;

                oConcepto.Impuestos = new ComprobanteConceptoImpuestos();

                oConcepto.Impuestos.Traslados = conceptoImpuestos.Traslados;

                lstConceptos.Add(oConcepto);
                oComprobante.Conceptos = lstConceptos.ToArray();

                impuesto.TotalImpuestosTrasladados = Math.Round(Convert.ToDecimal(valorConc[i].iva_tasa_impuesto), 2);//1.60m;

                impuesto.TotalImpuestosTrasladadosSpecified = true;

            }

            //--------------------------------------------------------------------------------------------------------
            comprobanteImpuestosTraslados[0] = new ComprobanteImpuestosTraslado();

            comprobanteImpuestosTraslados[0].Base = base_iva;//10.00m;
            comprobanteImpuestosTraslados[0].TasaOCuota = 0.160000m;
            comprobanteImpuestosTraslados[0].Importe = total_trasladado;//1.60m;
            comprobanteImpuestosTraslados[0].TipoFactor = "Tasa";
            comprobanteImpuestosTraslados[0].Impuesto = "002";

            comprobanteImpuestosTraslados[0].ImporteSpecified = true;
            comprobanteImpuestosTraslados[0].TasaOCuotaSpecified = true;

            impuesto.Traslados = comprobanteImpuestosTraslados;

            oComprobante.Impuestos = impuesto;

            //Creamos el xml
            CreateXML(oComprobante);

            string cadenaOriginal = "";
            string pathxsl = path + @"cadenaoriginal_4_0.xslt";
            System.Xml.Xsl.XslCompiledTransform transformador = new System.Xml.Xsl.XslCompiledTransform(true);
            transformador.Load(pathxsl);

            using (StringWriter sw = new StringWriter())
            using (XmlWriter xwo = XmlWriter.Create(sw, transformador.OutputSettings))
            {
                transformador.Transform(pathXML, xwo);
                cadenaOriginal = sw.ToString();
            }

            SelloDigital oSelloDigital = new SelloDigital();
            oComprobante.Certificado = oSelloDigital.Certificado(pathCer);
            oComprobante.Sello = oSelloDigital.Sellar(cadenaOriginal, pathKey, clavePrivada);

            //Creamos el xml
            CreateXML(oComprobante);

            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        private static void CreateXML(Comprobante oComprobante)
        {
            //SERIALIZAMOS.-------------------------------------------------

            //string pathXML = p + @"Plantillas\FacturaXML.xml";
            string pathXML = p_xml;
            XmlSerializerNamespaces xmlNameSpace = new XmlSerializerNamespaces();
            xmlNameSpace.Add("cfdi", "http://www.sat.gob.mx/cfd/4");
            xmlNameSpace.Add("xs", "http://www.w3.org/2001/XMLSchema");
            xmlNameSpace.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            XmlSerializer oXmlSerializar = new XmlSerializer(typeof(Comprobante));

            string sXml = "";

            using (var sww = new CLS40.StringWriterWithEncoding(Encoding.UTF8))
            {

                using (XmlWriter writter = XmlWriter.Create(sww))
                {
                    oXmlSerializar.Serialize(writter, oComprobante, xmlNameSpace);
                    sXml = sww.ToString();
                    var text = sXml.Substring(55); //sXml.Split("<?xml version=\"1.0\" encoding=\"utf-8\"?><cfdi:Comprobante"); //55
                    sXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><cfdi:Comprobante " + "xsi:schemaLocation=\"http://www.sat.gob.mx/cfd/4 http://www.sat.gob.mx/sitio_internet/cfd/4/cfdv40.xsd\"" + text;
                }

            }

            //guardamos el string en un archivo
            System.IO.File.WriteAllText(pathXML, sXml);
        }
        //Gen XML Pago
        public ActionResult genPagoXML(Int32? id)
        {
            //************************************
            string path = Server.MapPath("~");
            //------------------------------------
            //Get Info PreFac en DB
            db = new BD_FFEntities();
            var factura = db.tbd_Pre_Pagos.ToList<tbd_Pre_Pagos>().Where(u => u.id == id).Single();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var firma = db.tbd_Firmas.ToList<tbd_Firmas>().Where(u => u.rfc == usuario.rfc).Single();
            var receptor_c = db.tbc_Clientes.ToList<tbc_Clientes>().Where(u => u.id_cliente == factura.id_cliente).Single();
            var factura_d = db.tbd_Detalle_Prepago.ToList<tbd_Detalle_Prepago>().Where(u => u.id_pre_pago == id).First();
            //Ruta donde alojamos los Archivos
            var ruta_xml = factura.url_xml;
            string[] nom_doc = factura.url_pdf.Split('\\');
            string[] nd = nom_doc[5].Split('.');
            string nf = nd[0];
            string namefile = nf;
            //------------------------------------
            string UUIDRel = factura_d.uuid;
            p = path;
            string pathXML = path + @"\Plantillas\" + ruta_xml + "\\" + namefile + ".xml";
            string pathCer = path + @"\Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_cer_sello;
            string pathKey = path + @"\Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_key_sello;
            string clavePrivada = firma.password_sello;
            p_xml = pathXML;
            //Obtenemos el Número de Certificado
            string numeroCertificado, aa, b, c;
            SelloDigital.leerCER(pathCer, out aa, out b, out c, out numeroCertificado);
            //----------------Llenamos la clase PAGO ---------------------------
            Comprobante oComprobante = new Comprobante();
            oComprobante.Version = "4.0";
            oComprobante.Serie = factura.serie;
            oComprobante.Folio = factura.folio;
            oComprobante.Fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            //// oComprobante.Sello = "faltante"; //sig video
            //oComprobante.FormaPago = "99";
            oComprobante.NoCertificado = numeroCertificado;
            //// oComprobante.Certificado = ""; //sig video
            oComprobante.SubTotal = 0;//factura.total;
            oComprobante.Moneda = "XXX";
            oComprobante.Total = 0;//Math.Round(Convert.ToDecimal(factura.total),2);
            oComprobante.TipoDeComprobante = "P";
            //oComprobante.MetodoPago = "PPD";
            oComprobante.LugarExpedicion = receptor_c.direccion_fiscal;
            oComprobante.Descuento = 0;
            oComprobante.Exportacion = "01";
            ComprobanteEmisor oEmisor = new ComprobanteEmisor();

            oEmisor.Rfc = usuario.rfc;
            oEmisor.Nombre = usuario.nombre_razon;
            oEmisor.RegimenFiscal = db.tbc_Regimenes.Where(u => u.id_regimen_fiscal == usuario.id_regimen_fiscal).Select(u => u.clave).First();

            ComprobanteReceptor oReceptor = new ComprobanteReceptor();
            oReceptor.Nombre = receptor_c.nombre_razon;
            oReceptor.Rfc = receptor_c.rfc;
            oReceptor.UsoCFDI = "CP01";//db.tbc_Usos_CFDI.Where(u => u.id_uso_cfdi == receptor_c.id_uso_cdfi).Select(u => u.clave).First();
            oReceptor.RegimenFiscalReceptor = db.tbc_Regimenes.Where(u => u.id_regimen_fiscal == receptor_c.id_regimen_fiscal).Select(u => u.clave).First();//"606";
            oReceptor.DomicilioFiscalReceptor = receptor_c.direccion_fiscal;

            // //asigno emisor y receptor
            oComprobante.Emisor = oEmisor;
            oComprobante.Receptor = oReceptor;

            List<ComprobanteConcepto> lstConceptos = new List<ComprobanteConcepto>();
            ComprobanteConcepto oConcepto = new ComprobanteConcepto();
            oConcepto.Importe = 0;
            oConcepto.ClaveProdServ = "84111506";
            oConcepto.Cantidad = 1;
            oConcepto.ClaveUnidad = "ACT";
            oConcepto.Descripcion = "Pago";
            oConcepto.ValorUnitario = 0;
            oConcepto.Descuento = 0;
            oConcepto.ObjetoImp = "01";

            lstConceptos.Add(oConcepto);

            oComprobante.Conceptos = lstConceptos.ToArray();

            Pagos pagos = new Pagos();

            pagos.Version = "2.0";

            PagosTotales pagosTotales = new PagosTotales();

            pagosTotales.MontoTotalPagos = Math.Round(Convert.ToDecimal(factura_d.pago), 2);

            pagos.Totales = pagosTotales;

            PagosPago[] pagosPago = new PagosPago[1];
            pagosPago[0] = new PagosPago();


            pagosPago[0].FechaPago = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            pagosPago[0].FormaDePagoP = db.tbc_Formas_Pago.Where(u => u.id_forma_pago == factura_d.forma_pago).Select(u => u.clave).First();//"03";

            pagosPago[0].MonedaP = factura.tipo_moneda == 1 ? "MXN" : "USD";//"MXN";
            pagosPago[0].TipoCambioP = Convert.ToDecimal(factura.tipo_cambio) == 0 ? 1: Convert.ToDecimal(factura.tipo_cambio);
            pagosPago[0].Monto = Math.Round(Convert.ToDecimal(factura_d.pago), 2);
            pagosPago[0].NumOperacion = factura.num_operacion.ToString();

            pagosPago[0].TipoCambioPSpecified = true;

            PagosPagoDoctoRelacionado[] pagosPagoDoctoRelacionados = new PagosPagoDoctoRelacionado[1];

            pagosPagoDoctoRelacionados[0] = new PagosPagoDoctoRelacionado();

            pagosPagoDoctoRelacionados[0].IdDocumento = UUIDRel;
            pagosPagoDoctoRelacionados[0].Serie = factura.serie;
            pagosPagoDoctoRelacionados[0].Folio = factura.folio;
            pagosPagoDoctoRelacionados[0].MonedaDR = factura.tipo_moneda == 1 ? "MXN" : "USD";//"MXN";
            pagosPagoDoctoRelacionados[0].EquivalenciaDR = 1;
            pagosPagoDoctoRelacionados[0].NumParcialidad = "1";
            pagosPagoDoctoRelacionados[0].ImpPagado = Math.Round(Convert.ToDecimal(factura_d.pago), 2);
            pagosPagoDoctoRelacionados[0].ImpSaldoAnt = Math.Round(Convert.ToDecimal(factura_d.s_anterior), 2);
            pagosPagoDoctoRelacionados[0].ImpSaldoInsoluto = Math.Round(Convert.ToDecimal(factura_d.s_actual), 2);
            pagosPagoDoctoRelacionados[0].ObjetoImpDR = "01";

            pagosPagoDoctoRelacionados[0].EquivalenciaDRSpecified = true;

            pagosPago[0].DoctoRelacionado = pagosPagoDoctoRelacionados;

            pagos.Pago = pagosPago;
            XmlDocument xmlDocument = new XmlDocument();
            XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
            xmlSerializerNamespaces.Add("pago20", "http://www.sat.gob.mx/Pagos20");
            using (XmlWriter writer = xmlDocument.CreateNavigator().AppendChild())
            {
                new XmlSerializer(pagos.GetType()).Serialize(writer, pagos, xmlSerializerNamespaces);
            }

            ComprobanteComplemento comprobanteComplemento = new ComprobanteComplemento();

            XmlElement[] xmlElements = new XmlElement[1];

            xmlElements[0] = xmlDocument.DocumentElement;
            comprobanteComplemento.Any = xmlElements;

            oComprobante.Complemento = comprobanteComplemento;

            // //Creamos el xml
            CreateXMLPago(oComprobante);

            string cadenaOriginal = "";
            string pathxsl = path + @"cadenaoriginal_4_0.xslt";
            System.Xml.Xsl.XslCompiledTransform transformador = new System.Xml.Xsl.XslCompiledTransform(true);
            transformador.Load(pathxsl);

            using (StringWriter sw = new StringWriter())
            using (XmlWriter xwo = XmlWriter.Create(sw, transformador.OutputSettings))
            {

                transformador.Transform(pathXML, xwo);
                cadenaOriginal = sw.ToString();
            }


            SelloDigital oSelloDigital = new SelloDigital();
            oComprobante.Certificado = oSelloDigital.Certificado(pathCer);
            oComprobante.Sello = oSelloDigital.Sellar(cadenaOriginal, pathKey, clavePrivada);
            CreateXMLPago(oComprobante);

            return Json("Creado", JsonRequestBehavior.AllowGet);
        }
        private static void CreateXMLPago(Comprobante oComprobante)
        {
            string pathXML = p_xml;
            XmlSerializerNamespaces xmlNameSpace = new XmlSerializerNamespaces();
            xmlNameSpace.Add("cfdi", "http://www.sat.gob.mx/cfd/4");
            xmlNameSpace.Add("xs", "http://www.w3.org/2001/XMLSchema");
            xmlNameSpace.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            xmlNameSpace.Add("pago20", "http://www.sat.gob.mx/Pagos20");

            XmlSerializer oXmlSerializar = new XmlSerializer(typeof(Comprobante));

            string sXml = "";

            using (var sww = new CLS40.StringWriterWithEncoding(Encoding.UTF8))
            {
                using (XmlWriter writter = XmlWriter.Create(sww))
                {
                    oXmlSerializar.Serialize(writter, oComprobante, xmlNameSpace);

                    sXml = sww.ToString();

                    var text = sXml.Substring(55); //sXml.Split("<?xml version=\"1.0\" encoding=\"utf-8\"?><cfdi:Comprobante"); //55

                    sXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><cfdi:Comprobante " + "xsi:schemaLocation=\"http://www.sat.gob.mx/cfd/4 http://www.sat.gob.mx/sitio_internet/cfd/4/cfdv40.xsd http://www.sat.gob.mx/Pagos20 http://www.sat.gob.mx/sitio_internet/cfd/Pagos/Pagos20.xsd\"" + text;

                }
            }

            //guardamos el string en un archivo
            System.IO.File.WriteAllText(pathXML, sXml);
        }
        //Gen XML Carta Porte
        public ActionResult GenXMLCartaPorte(int id)
        {
            //Get Info PreFac en DB
            db = new BD_FFEntities();
            var carta = db.tbd_Pre_Carta_Porte.ToList<tbd_Pre_Carta_Porte>().Where(u => u.id == id).Single();
            var factura = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == carta.id_prefactura).Single();

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var firma = db.tbd_Firmas.ToList<tbd_Firmas>().Where(u => u.rfc == usuario.rfc).Single();
            var receptor_c = db.tbc_Clientes.ToList<tbc_Clientes>().Where(u => u.rfc == factura.rfc_cliente).Single();
            //Ruta donde alojamos los Archivos
            var ruta_xml = factura.url_xml;
            string[] nom_doc = factura.url_pdf.Split('\\');
            string[] nd = nom_doc[5].Split('.');
            string nf = nd[0];
            string namefile = nf;
            //************************************
            string path = Server.MapPath("~");
            p = path;
            string pathXML = path + @"\Plantillas\" + ruta_xml + "\\" + namefile + ".xml";
            string pathCer = path + @"\Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_cer_sello;
            string pathKey = path + @"\Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_key_sello;
            string clavePrivada = firma.password_sello;
            p_xml = pathXML;
            //Obtenemos el Número de Certificado
            string numeroCertificado, aa, b, c;
            SelloDigital.leerCER(pathCer, out aa, out b, out c, out numeroCertificado);
            //string UUIDRel = "5C526343-0AED-599C-BCD1-E0492F96124D";
            #region IngresoAutoTransporte
            //--------------------------------------------------------------------------------------------------Comprobante
            Comprobante oComprobante = new Comprobante();
            oComprobante.Version = "4.0";
            oComprobante.Serie = factura.serie;
            oComprobante.Folio = factura.folio;
            oComprobante.Fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            oComprobante.CondicionesDePago = "CondicionesDePago";
            oComprobante.SubTotal = 200;//Convert.ToDecimal(factura.subtotal);
            oComprobante.DescuentoSpecified = true;
            oComprobante.Descuento = 1;//Convert.ToInt32(factura.descuento2);
            oComprobante.Moneda = factura.moneda;
            oComprobante.Total = 198.96m;//Convert.ToDecimal(factura.total);
            oComprobante.TipoDeComprobante = factura.tipo_comprobante;
            oComprobante.Exportacion = "01";
            oComprobante.MetodoPagoSpecified = true;
            oComprobante.MetodoPago = "PPD";//db.tbc_Metodos_Pago.ToList<tbc_Metodos_Pago>().Where(s => s.id_metodo_pago == factura.metodo_pago).Select(u => u.clave).First();//"PPD";
            oComprobante.FormaPagoSpecified = true;
            oComprobante.FormaPago = "99";// db.tbc_Formas_Pago.ToList<tbc_Formas_Pago>().Where(s => s.id_forma_pago == factura.forma_pago).Select(u => u.clave).First();//"99";
            oComprobante.LugarExpedicion = "20000";// factura.lugar_expedicion;

            oComprobante.NoCertificado = numeroCertificado;
            //--------------------------------------------------------------------------------------------------Emisor
            ComprobanteEmisor oEmisor = new ComprobanteEmisor();
            oEmisor.Rfc = usuario.rfc;
            oEmisor.Nombre = "ESCUELA KEMPER URGATE";
            oEmisor.RegimenFiscal = "601";//db.tbc_Regimenes.Where(u => u.id_regimen_fiscal == usuario.id_regimen_fiscal).Select(u => u.clave).First();
            //Receptor
            ComprobanteReceptor oReceptor = new ComprobanteReceptor();
            oReceptor.Nombre = receptor_c.nombre_razon;
            oReceptor.Rfc = receptor_c.rfc;
            oReceptor.UsoCFDI = "G01";//db.tbc_Usos_CFDI.Where(u => u.id_uso_cfdi == receptor_c.id_uso_cdfi).Select(u => u.clave).First();
            oReceptor.RegimenFiscalReceptor = "606";//db.tbc_Regimenes.Where(u => u.id_regimen_fiscal == receptor_c.id_regimen_fiscal).Select(u => u.clave).First();
            oReceptor.DomicilioFiscalReceptor = receptor_c.direccion_fiscal;
            //asigno emisor y receptor
            oComprobante.Emisor = oEmisor;
            oComprobante.Receptor = oReceptor;
            //--------------------------------------------------------------------------------------------------Conceptos
            var valorConc = db.tbd_Conceptos_Pre_Factura.ToList<tbd_Conceptos_Pre_Factura>().Where(u => u.id_pre_factura == carta.id_prefactura).ToList();
            Decimal total_trasladado = 0;
            Decimal total_retenido = 0;

            Decimal total_iva_ret = 0;
            Decimal total_isr_ret = 0;

            Decimal base_iva = 0;
            List<ComprobanteConcepto> lstConceptos = new List<ComprobanteConcepto>();
            ComprobanteConcepto oConcepto = new ComprobanteConcepto();

            for (int i = 0; i < valorConc.Count; i++)
            {
                Decimal canti = Convert.ToDecimal(valorConc[i].cantidad);
                Decimal imp_unitario = Convert.ToDecimal(valorConc[i].importe_unitario);
                Decimal imp_total = Convert.ToDecimal(valorConc[i].importe_total);
                //---------------------
                oConcepto.ClaveProdServ = "78101500";// valorConc[i].c_prod_serv;
                oConcepto.Cantidad = 1;//Convert.ToDecimal(valorConc[i].cantidad);
                oConcepto.ClaveUnidad = "H87";//valorConc[i].c_producto;
                oConcepto.Unidad = "Pieza";//valorConc[i].unidad;
                oConcepto.Descripcion = "Cigarros";// valorConc[i].concepto;
                oConcepto.ValorUnitario = 200.00m;//imp_unitario;
                oConcepto.DescuentoSpecified = true;
                oConcepto.Descuento = 1;// Convert.ToDecimal(valorConc[i].descuento);
                oConcepto.Importe = 200.00m;// imp_total;
                oConcepto.ObjetoImp = "02";
                //--------------------------------------------------------------------------------------------------Conceptos - Impuestos
                ComprobanteConceptoImpuestos conceptoImpuestos = new ComprobanteConceptoImpuestos();
                //--------------------------------------------------------------------------------------------------Conceptos - Impuestos -Traslados
                ComprobanteConceptoImpuestosTraslado comprobanteConceptoImpuestosTraslado = new ComprobanteConceptoImpuestosTraslado();
                //Traslado
                if (valorConc[i].iva_tasa_impuesto.Trim() != "0.00")
                {
                    Decimal i_total = Convert.ToDecimal(valorConc[i].importe_total);
                    comprobanteConceptoImpuestosTraslado.Base = 1;//valorConc[i].total;
                    comprobanteConceptoImpuestosTraslado.Importe = 0.16m;//valorConc[i].iva_tasa;
                    comprobanteConceptoImpuestosTraslado.Impuesto = "002";
                    comprobanteConceptoImpuestosTraslado.TasaOCuota = /*valorConc[i].iva_tasa;*/ 0.160000m;
                    comprobanteConceptoImpuestosTraslado.TipoFactor = "Tasa";//valorConc[i].tipo_factor;

                    comprobanteConceptoImpuestosTraslado.ImporteSpecified = true;
                    comprobanteConceptoImpuestosTraslado.TasaOCuotaSpecified = true;


                    total_trasladado += Convert.ToDecimal(valorConc[i].iva_tasa_impuesto);
                    base_iva += Convert.ToDecimal(valorConc[i].importe_total);
                }
                conceptoImpuestos.Traslados = new ComprobanteConceptoImpuestosTraslado[1];
                conceptoImpuestos.Traslados[0] = comprobanteConceptoImpuestosTraslado;
                oConcepto.Impuestos = new ComprobanteConceptoImpuestos();
                oConcepto.Impuestos.Traslados = conceptoImpuestos.Traslados;

                //--------------------------------------------------------------------------------------------------Conceptos - impuestos - Retenciones
                ComprobanteConceptoImpuestosRetencion[] comprobanteConceptoImpuestosRetencion = new ComprobanteConceptoImpuestosRetencion[2];

                //Retenido
                if (valorConc[i].isr_ret_impuesto.Trim() != "0.00")
                {
                    Decimal i_total = Convert.ToDecimal(valorConc[i].importe_total);
                    String i_total_ = i_total.ToString("########.00");
                    comprobanteConceptoImpuestosRetencion[0] = new ComprobanteConceptoImpuestosRetencion();
                    comprobanteConceptoImpuestosRetencion[0].Base = /*Convert.ToDecimal(i_total_);*/ 1;
                    comprobanteConceptoImpuestosRetencion[0].Impuesto = "001";
                    comprobanteConceptoImpuestosRetencion[0].TipoFactor = "Tasa";//valorConc[i].tipo_factor.Trim();
                    comprobanteConceptoImpuestosRetencion[0].TasaOCuota = 0.100000m;//valorConc[i].isr_ret_tasa;
                    comprobanteConceptoImpuestosRetencion[0].Importe = 0.10m;//valorConc[i].isr_ret_impuesto;
                    total_retenido += Convert.ToDecimal(valorConc[i].isr_ret_impuesto);
                    total_isr_ret += Convert.ToDecimal(valorConc[i].isr_ret_impuesto);
                }
                if (valorConc[i].iva_ret_impuesto.Trim() != "0.00")
                {
                    Decimal i_total = Convert.ToDecimal(valorConc[i].importe_total);
                    String i_total_ = i_total.ToString("########.00");
                    comprobanteConceptoImpuestosRetencion[1] = new ComprobanteConceptoImpuestosRetencion();
                    comprobanteConceptoImpuestosRetencion[1].Base = 1;/*Convert.ToDecimal(i_total_);*/
                    comprobanteConceptoImpuestosRetencion[1].Impuesto = "002";
                    comprobanteConceptoImpuestosRetencion[1].TipoFactor = "Tasa";//valorConc[i].tipo_factor.Trim();
                    comprobanteConceptoImpuestosRetencion[1].TasaOCuota = 0.106666m;//valorConc[i].iva_ret_tasa;
                    comprobanteConceptoImpuestosRetencion[1].Importe = 0.10m;//valorConc[i].iva_ret_impuesto;
                    total_retenido += Convert.ToDecimal(valorConc[i].iva_ret_impuesto);
                    total_iva_ret += Convert.ToDecimal(valorConc[i].iva_ret_impuesto);
                }
                conceptoImpuestos.Retenciones = new ComprobanteConceptoImpuestosRetencion[2];
                conceptoImpuestos.Retenciones = comprobanteConceptoImpuestosRetencion;
                oConcepto.Impuestos.Retenciones = comprobanteConceptoImpuestosRetencion;
                //-----------------------------
                lstConceptos.Add(oConcepto);
                oComprobante.Conceptos = lstConceptos.ToArray();
                //--------------------------------------------------------------------------------------------------Impuestos
                ComprobanteImpuestos impuesto = new ComprobanteImpuestos();

                impuesto.TotalImpuestosTrasladados = 0.16m;//total_retenido;
                impuesto.TotalImpuestosRetenidos = 0.20m;//total_retenido;
                impuesto.TotalImpuestosRetenidosSpecified = true;


                impuesto.TotalImpuestosTrasladadosSpecified = true;

                //--------------------------------------------------------------------------------------------------Impuestos - Traslado
                ComprobanteImpuestosTraslado[] comprobanteImpuestosTraslados = new ComprobanteImpuestosTraslado[1];

                comprobanteImpuestosTraslados[0] = new ComprobanteImpuestosTraslado();
                String base_iva_ = base_iva.ToString("########.00");

                comprobanteImpuestosTraslados[0].Base = 1;//Convert.ToDecimal(base_iva_);
                comprobanteImpuestosTraslados[0].TasaOCuota = 0.160000m;
                comprobanteImpuestosTraslados[0].Importe = 0.16m;
                comprobanteImpuestosTraslados[0].TipoFactor = "Tasa";
                comprobanteImpuestosTraslados[0].Impuesto = "002";

                comprobanteImpuestosTraslados[0].ImporteSpecified = true;
                comprobanteImpuestosTraslados[0].TasaOCuotaSpecified = true;

                impuesto.Traslados = comprobanteImpuestosTraslados;
                //--------------------------------------------------------------------------------------------------Impuestos - Retenciones
                ComprobanteImpuestosRetencion[] comprobanteImpuestosRetencions = new ComprobanteImpuestosRetencion[2];
                comprobanteImpuestosRetencions[0] = new ComprobanteImpuestosRetencion();
                comprobanteImpuestosRetencions[0].Impuesto = "001";
                comprobanteImpuestosRetencions[0].Importe = 0.10m;//total_isr_ret;

                comprobanteImpuestosRetencions[1] = new ComprobanteImpuestosRetencion();
                comprobanteImpuestosRetencions[1].Impuesto = "002";
                comprobanteImpuestosRetencions[1].Importe = 0.10m;//total_iva_ret;

                impuesto.Retenciones = comprobanteImpuestosRetencions;

                oComprobante.Impuestos = impuesto;
            }

            //--------------------------------------------------------------------------------------------------Carta Porte
            CartaPorte cartaporte = new CartaPorte();
            cartaporte.Version = "2.0";
            cartaporte.TotalDistRecSpecified = true;
            cartaporte.TotalDistRec = Math.Round(Convert.ToDecimal(carta.total_distancia_rec), 2);
            cartaporte.TranspInternac = carta.transporte_inter;

            //--------------------------------------------------------------------------------------------------Carta Porte - Ubicaciones
            var valorCP = db.tbd_Ubicacion_Carta_Porte.ToList<tbd_Ubicacion_Carta_Porte>().Where(u => u.id_pre_carta == carta.id).ToList();
            CartaPorteUbicacion[] cartaporteubicacion = new CartaPorteUbicacion[valorCP.Count];
            CartaPorteUbicacionDomicilio[] cartaporteubicaciondomicilio = new CartaPorteUbicacionDomicilio[valorCP.Count];
            for (int i = 0; i < valorCP.Count; i++)
            {
                string fca_hs = valorCP[i].fca_hora_salida != null ? valorCP[i].fca_hora_salida.Value.ToString("yyyy-MM-ddTHH:mm:ss") : "";
                cartaporteubicacion[i] = new CartaPorteUbicacion();
                cartaporteubicacion[i].IDUbicacion = db.tbc_Ubicaciones.ToList<tbc_Ubicaciones>().Where(u => u.id_ubicacion == valorCP[i].id_ubicacion).Select(u => u.id_origen_destino).FirstOrDefault();//"OR101010";
                cartaporteubicacion[i].TipoUbicacion = valorCP[i].tipo_ubicacion.Trim();//"Origen";
                cartaporteubicacion[i].RFCRemitenteDestinatario = "EKU9003173C9";//db.tbc_Ubicaciones.ToList<tbc_Ubicaciones>().Where(u => u.id_ubicacion == valorCP[i].id).Select(u => u.rfc_usuario).FirstOrDefault();//"EKU9003173C9";
                cartaporteubicacion[i].FechaHoraSalidaLlegada = fca_hs;
                if (valorCP[i].tipo_ubicacion.Trim() == "Destino")
                {
                    cartaporteubicacion[i].DistanciaRecorrida = Convert.ToInt32(valorCP[i].distancia_recorrida);
                    cartaporteubicacion[i].DistanciaRecorridaSpecified = true;
                }
                //--------------------------------------------------------------------------------------------------Carta Porte - Ubicaciones - Domicilio
                cartaporteubicaciondomicilio[i] = new CartaPorteUbicacionDomicilio();
                cartaporteubicaciondomicilio[i].CodigoPostal = db.tbc_Ubicaciones.ToList<tbc_Ubicaciones>().Where(u => u.id_ubicacion == valorCP[i].id_ubicacion).Select(u => u.codigo_postal).FirstOrDefault();//"25350";
                cartaporteubicaciondomicilio[i].Pais = db.tbc_Ubicaciones.ToList<tbc_Ubicaciones>().Where(u => u.id_ubicacion == valorCP[i].id_ubicacion).Select(u => u.pais).FirstOrDefault();
                cartaporteubicaciondomicilio[i].Estado = db.tbc_Ubicaciones.ToList<tbc_Ubicaciones>().Where(u => u.id_ubicacion == valorCP[i].id_ubicacion).Select(u => u.estado).FirstOrDefault();
                cartaporteubicaciondomicilio[i].Municipio = db.tbc_Ubicaciones.ToList<tbc_Ubicaciones>().Where(u => u.id_ubicacion == valorCP[i].id_ubicacion).Select(u => u.municipio).FirstOrDefault();
                cartaporteubicaciondomicilio[i].Localidad = db.tbc_Ubicaciones.ToList<tbc_Ubicaciones>().Where(u => u.id_ubicacion == valorCP[i].id_ubicacion).Select(u => u.localidad).FirstOrDefault();
                cartaporteubicaciondomicilio[i].Colonia = db.tbc_Ubicaciones.ToList<tbc_Ubicaciones>().Where(u => u.id_ubicacion == valorCP[i].id_ubicacion).Select(u => u.colonia).FirstOrDefault();//"0347";
                cartaporteubicaciondomicilio[i].NumeroExterior = db.tbc_Ubicaciones.ToList<tbc_Ubicaciones>().Where(u => u.id_ubicacion == valorCP[i].id_ubicacion).Select(u => u.num_ext).FirstOrDefault();//"211";
                cartaporteubicaciondomicilio[i].Calle = db.tbc_Ubicaciones.ToList<tbc_Ubicaciones>().Where(u => u.id_ubicacion == valorCP[i].id_ubicacion).Select(u => u.calle).FirstOrDefault();//"Calle";
                cartaporteubicaciondomicilio[i].Referencia = db.tbc_Ubicaciones.ToList<tbc_Ubicaciones>().Where(u => u.id_ubicacion == valorCP[i].id_ubicacion).Select(u => u.referencia).FirstOrDefault();//"Casa Blanca 1";

                cartaporteubicacion[i].Domicilio = cartaporteubicaciondomicilio[i];
            }
            cartaporte.Ubicaciones = cartaporteubicacion;
            #region pt
            //--------------------------------------------------------------------------------------------------Carta Porte - Mercancias
            var valorM = db.tbd_Mercancias.ToList<tbd_Mercancias>().Where(u => u.id_mercancia == carta.id_mercancia).ToList();
            CartaPorteMercancias cartaportemercancias = new CartaPorteMercancias();
            CartaPorteMercanciasMercancia[] cartaportemercanciasmercancia = new CartaPorteMercanciasMercancia[valorCP.Count];

            for (int i = 0; i < valorM.Count; i++)
            {
                cartaportemercancias.NumTotalMercancias = Convert.ToInt32(valorM[i].numero_piezas);
                cartaportemercancias.UnidadPeso = db.tbc_Unidades_Peso.ToList<tbc_Unidades_Peso>().Where(u => u.id_unidad_peso == valorM[i].id_unidad_peso_m).Select(u => u.clave_unidad).FirstOrDefault();//valorM[i].unidad;
                String p_bruto = valorM[i].peso_bruto.ToString("########.0");
                cartaportemercancias.PesoBrutoTotal = 1.0m;//Convert.ToDecimal(p_bruto);
                for (int j = 0; j < valorCP.Count; j++)
                {
                    if (valorCP[j].tipo_ubicacion.Trim() == "Destino")
                    {
                        String cantidad = valorM[i].cantidad.ToString("########.00");
                        String peso_kg = valorM[i].peso_kg.ToString("########.00");
                        cartaportemercanciasmercancia[j] = new CartaPorteMercanciasMercancia();
                        cartaportemercanciasmercancia[j].BienesTransp = "11121900";//valorM[i]
                        cartaportemercanciasmercancia[j].Descripcion = valorM[i].descripcion;//"Productos de perfumería";
                        cartaportemercanciasmercancia[j].Cantidad = Convert.ToDecimal(cantidad);
                        cartaportemercanciasmercancia[j].ClaveUnidad = db.tbc_Unidades_Medida.ToList<tbc_Unidades_Medida>().Where(u => u.id_unidad_medida == valorM[i].id_unidad_medida).Select(u => u.clave).FirstOrDefault();//valorM[i].unidad;
                        cartaportemercanciasmercancia[j].PesoEnKg = Convert.ToDecimal(peso_kg);
                        cartaportemercanciasmercancia[j].MaterialPeligrosoSpecified = true;
                        cartaportemercanciasmercancia[j].MaterialPeligroso = valorM[i].material_peligroso == "Si" ? "Sí" : valorM[i].material_peligroso;
                        cartaportemercanciasmercancia[j].CveMaterialPeligrosoSpecified = true;
                        cartaportemercanciasmercancia[j].CveMaterialPeligroso = "1266";//SIN LA M db.tbc_Materiales_Peligrosos.ToList<tbc_Materiales_Peligrosos>().Where(u => u.id_material_peligroso == valorM[i].id_material_peligroso).Select(u => u.clave_material_peligroso).FirstOrDefault();
                        cartaportemercanciasmercancia[j].EmbalajeSpecified = true;
                        cartaportemercanciasmercancia[j].Embalaje = db.tbc_Tipos_Embalaje.ToList<tbc_Tipos_Embalaje>().Where(u => u.id_tipo_embalaje == valorM[i].id_tipo_embalaje).Select(u => u.clave_designacion).FirstOrDefault(); ;//"4H2";

                        CartaPorteMercanciasMercanciaCantidadTransporta[] cartaPorteMercanciasMercanciaCantidadTransportas = new CartaPorteMercanciasMercanciaCantidadTransporta[1];
                        cartaPorteMercanciasMercanciaCantidadTransportas[0] = new CartaPorteMercanciasMercanciaCantidadTransporta();
                        cartaPorteMercanciasMercanciaCantidadTransportas[0].Cantidad = 1;
                        cartaPorteMercanciasMercanciaCantidadTransportas[0].IDOrigen = "OR101010";
                        cartaPorteMercanciasMercanciaCantidadTransportas[0].IDDestino = db.tbc_Ubicaciones.ToList<tbc_Ubicaciones>().Where(u => u.id_ubicacion == valorCP[j].id_ubicacion).Select(u => u.id_origen_destino).FirstOrDefault();
                        cartaportemercanciasmercancia[j].CantidadTransporta = cartaPorteMercanciasMercanciaCantidadTransportas;
                        cartaportemercancias.Mercancia = cartaportemercanciasmercancia;
                    }
                }

                //--------------------------------------------------------------------------------------------------Carta Porte - Mercancias - Mercancia

            }
            //cartaportemercanciasmercancia[0].Moneda = "MXN";                                    
            //cartaportemercanciasmercancia[0].ValorMercancia = 90000;            
            //cartaportemercanciasmercancia[0].Dimensiones = "5/25/5cm";
            //
            //--------------------------------------------------------------------------------------------------Carta Porte - Mercancias - Mercancia - CantidadTransporta

            //--------------------------------------------------------------------------------------------------Carta Porte - Mercancias - Autotransporte
            var valorMA = db.tbd_Autotransporte.ToList<tbd_Autotransporte>().Where(u => u.id_autotransporte == carta.id_autotransporte).First();
            CartaPorteMercanciasAutotransporte cartaportemercanciasautotransporte = new CartaPorteMercanciasAutotransporte();

            cartaportemercanciasautotransporte.NumPermisoSCT = valorMA.num_permiso_sct;
            cartaportemercanciasautotransporte.PermSCT = "TPAF01";//db.tbc_Tipos_Permiso.ToList<tbc_Tipos_Permiso>().Where(u => u.id_tipo_permiso == valorMA.id_tipo_permiso).Select(u => u.clave).FirstOrDefault();
            cartaportemercancias.Autotransporte = cartaportemercanciasautotransporte;

            //--------------------------------------------------------------------------------------------------Carta Porte - Mercancias - Autotransporte - Identificacion Vehicular
            CartaPorteMercanciasAutotransporteIdentificacionVehicular cartaportemercanciasautotransporteidentificacionvehicular = new CartaPorteMercanciasAutotransporteIdentificacionVehicular();
            cartaportemercanciasautotransporteidentificacionvehicular.AnioModeloVM = valorMA.anio_modelo_vm;
            cartaportemercanciasautotransporteidentificacionvehicular.PlacaVM = "plac892";//valorMA.placa_vm;
            cartaportemercanciasautotransporteidentificacionvehicular.ConfigVehicular = "VL";//db.tbc_Config_AutoTransporte.ToList<tbc_Config_AutoTransporte>().Where(u => u.id_conf_autotrans == valorMA.id_conf_autotrans).Select(u => u.clave).FirstOrDefault();//"VL";
            cartaportemercanciasautotransporte.IdentificacionVehicular = cartaportemercanciasautotransporteidentificacionvehicular;

            //--------------------------------------------------------------------------------------------------Carta Porte - Mercancias - Autotransporte - Seguros
            CartaPorteMercanciasAutotransporteSeguros cartaportemercanciasautotransporteseguros = new CartaPorteMercanciasAutotransporteSeguros();
            cartaportemercanciasautotransporteseguros.PolizaRespCivil = valorMA.poliza_resp_civil;
            cartaportemercanciasautotransporteseguros.AseguraRespCivil = "SW Seguros";// valorMA.asegura_resp_civil;
            cartaportemercanciasautotransporteseguros.AseguraCarga = "SW Seguros";//valorMA.asegura_carga;
            cartaportemercanciasautotransporteseguros.AseguraMedAmbiente = /*valorMA.asegura_med_ambiente;*/ "SW Seguros Ambientales";
            cartaportemercanciasautotransporteseguros.PolizaMedAmbiente = /*valorMA.poliza_med_ambiente;*/"123456789";
            cartaportemercanciasautotransporte.Seguros = cartaportemercanciasautotransporteseguros;
            //--------------------------------------------------------------------------------------------------Carta Porte - Mercancias - Autotransporte - Remolques
            CartaPorteMercanciasAutotransporteRemolque[] cartaportemercanciasautotransporteremolque = new CartaPorteMercanciasAutotransporteRemolque[1];
            cartaportemercanciasautotransporteremolque[0] = new CartaPorteMercanciasAutotransporteRemolque();
            cartaportemercanciasautotransporteremolque[0].Placa = "ABC123";//valorMA.placa_vm;
            cartaportemercanciasautotransporteremolque[0].SubTipoRem = "CTR021";
            cartaportemercanciasautotransporte.Remolques = cartaportemercanciasautotransporteremolque;

            cartaporte.Mercancias = cartaportemercancias;
            #endregion
            //--------------------------------------------------------------------------------------------------Carta Porte Figuras
            var valorFig = db.tbd_Figuras.ToList<tbd_Figuras>().Where(u => u.id_figura == carta.id_figura).First();
            CartaPorteTiposFigura[] cartaportetiposfiguras = new CartaPorteTiposFigura[2];
            cartaportetiposfiguras[0] = new CartaPorteTiposFigura();
            cartaportetiposfiguras[0].NumLicencia = /*valorFig.num_licencia;*/"a234567890";
            //cartaportetiposfiguras[0].NombreFigura = "Roberto Gómez Flores";
            cartaportetiposfiguras[0].RFCFigura = /*valorFig.rfc_figura;*/"VAAM130719H60";
            cartaportetiposfiguras[0].TipoFigura = db.tbc_Figuras_Transporte.ToList<tbc_Figuras_Transporte>().Where(u => u.id_figura_transporte == valorFig.id_figura_transporte).Select(u => u.clave_figura_transporte).FirstOrDefault();//"01";
            cartaporte.FiguraTransporte = cartaportetiposfiguras;

            #endregion Ingreso AutoTransporte Carta Porte
            XmlDocument xmlDocument = new XmlDocument();
            XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
            xmlSerializerNamespaces.Add("cartaporte20", "http://www.sat.gob.mx/CartaPorte20");
            using (XmlWriter writer = xmlDocument.CreateNavigator().AppendChild())
            {
                new XmlSerializer(cartaporte.GetType()).Serialize(writer, cartaporte, xmlSerializerNamespaces);
            }

            ComprobanteComplemento comprobanteComplemento = new ComprobanteComplemento();

            XmlElement[] xmlElements = new XmlElement[1];

            xmlElements[0] = xmlDocument.DocumentElement;
            comprobanteComplemento.Any = xmlElements;

            oComprobante.Complemento = comprobanteComplemento;

            //Creamos el xml
            CreateXMLCartaPorte(oComprobante);

            string cadenaOriginal = "";
            string pathxsl = path + @"cadenaoriginal_4_0.xslt";
            System.Xml.Xsl.XslCompiledTransform transformador = new System.Xml.Xsl.XslCompiledTransform(true);
            transformador.Load(pathxsl);

            using (StringWriter sw = new StringWriter())
            using (XmlWriter xwo = XmlWriter.Create(sw, transformador.OutputSettings))
            {
                transformador.Transform(pathXML, xwo);
                cadenaOriginal = sw.ToString();
            }

            SelloDigital oSelloDigital = new SelloDigital();
            oComprobante.Certificado = oSelloDigital.Certificado(pathCer);
            oComprobante.Sello = oSelloDigital.Sellar(cadenaOriginal, pathKey, clavePrivada);

            CreateXMLCartaPorte(oComprobante);
            return Json("Creado", JsonRequestBehavior.AllowGet);
        }
        private static void CreateXMLCartaPorte(Comprobante oComprobante)
        {
            string pathXML = p_xml;
            //SERIALIZAMOS.-------------------------------------------------
            XmlSerializerNamespaces xmlNameSpace = new XmlSerializerNamespaces();
            xmlNameSpace.Add("cfdi", "http://www.sat.gob.mx/cfd/4");
            xmlNameSpace.Add("xs", "http://www.w3.org/2001/XMLSchema");
            xmlNameSpace.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            xmlNameSpace.Add("cartaporte20", "http://www.sat.gob.mx/CartaPorte20");

            XmlSerializer oXmlSerializar = new XmlSerializer(typeof(Comprobante));
            string sXml = "";
            using (var sww = new CLS40.StringWriterWithEncoding(Encoding.UTF8))
            {

                using (XmlWriter writter = XmlWriter.Create(sww))
                {
                    oXmlSerializar.Serialize(writter, oComprobante, xmlNameSpace);

                    sXml = sww.ToString();

                    var text = sXml.Substring(55);

                    sXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><cfdi:Comprobante " + "xsi:schemaLocation=\"http://www.sat.gob.mx/cfd/4 http://www.sat.gob.mx/sitio_internet/cfd/4/cfdv40.xsd http://www.sat.gob.mx/CartaPorte20 http://www.sat.gob.mx/sitio_internet/cfd/CartaPorte/CartaPorte20.xsd\"" + text;
                }
            }
            //guardamos el string en un archivo
            System.IO.File.WriteAllText(pathXML, sXml);
        }
        //XML Nota de Venta
        public ActionResult GenXMLNota(Int32? id_)
        {
            //************************************
            //Get Info Nota de Venta en DB
            db = new BD_FFEntities();
            var factura = db.tbd_Notas_Venta.ToList<tbd_Notas_Venta>().Where(u => u.id_nota_venta == id_).Single();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var firma = db.tbd_Firmas.ToList<tbd_Firmas>().Where(u => u.rfc == usuario.rfc).Single();
            //DatosCliente
            var receptor_c = db.tbc_Clientes.ToList<tbc_Clientes>().Where(u => u.id_cliente == factura.id_cliente && u.rfc_usuario == usuario.rfc).Single();
            //Ruta donde alojamos los Archivos
            var fca_emision = factura.fecha_creacion.ToString();

            String[] fechaE = fca_emision.Split(' ');
            string aux_fc_emi = fechaE[0];
            String[] auxfechaE = aux_fc_emi.Split('/');
            string ax_fc_emi = auxfechaE[0] + auxfechaE[1] + auxfechaE[2];

            var ruta_xml = @"NotasVenta\PDF\"+usuario.rfc+"\\"+ax_fc_emi;//factura.url_xml;
            //string[] nom_doc = "Temp_";//factura.url_pdf.Split('\\');
            //string[] nd = nom_doc[4].Split('.');
            //string nf = nd[0];
            string namefile = "tempXML";
            //************************************
            string aux_path = ruta_xml + @"\" + namefile + ".xml";
            string path = Server.MapPath("~");
            p = path;
            string pathXML = path +"Plantillas\\"+ ruta_xml + "\\" + namefile + ".xml";
            string pathCer = path + @"Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_cer_sello;
            string pathKey = path + @"Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_key_sello;
            string clavePrivada = firma.password_sello;
            p_xml = pathXML;
            //Obtenemos el Número de Certificado
            string numeroCertificado, aa, b, c;
            SelloDigital.leerCER(pathCer, out aa, out b, out c, out numeroCertificado);
            //----------------Llenamos la clase COMPROBANTE ---------------------------
            Comprobante oComprobante = new Comprobante();
            oComprobante.Version = "4.0";
            oComprobante.Serie = factura.serie;
            oComprobante.Folio = factura.folio;
            oComprobante.Fecha = DateTime.Now.AddMinutes(-2).ToString("yyyy-MM-ddTHH:mm:ss");
            //oComprobante.Sello = ""; //sig video
            oComprobante.FormaPago = db.tbc_Formas_Pago.Where(u => u.id_forma_pago == factura.id_forma_pago).Select(u => u.clave).First(); 
            oComprobante.NoCertificado = numeroCertificado;
            //oComprobante.Certificado = ""; //sig video
            oComprobante.SubTotal = Math.Round(Convert.ToDecimal(factura.subtotal),2);
            oComprobante.Moneda = "MXN";
            oComprobante.Total = Math.Round(Convert.ToDecimal(factura.total),2);
            oComprobante.TipoDeComprobante = "I";
            oComprobante.MetodoPago = db.tbc_Metodos_Pago.Where(u => u.id_metodo_pago == factura.id_forma_pago).Select(u => u.clave).First();
            oComprobante.LugarExpedicion = factura.lugar_expedicion;
            oComprobante.Descuento = 0;
            oComprobante.Exportacion = "01";

            oComprobante.FormaPagoSpecified = true;
            oComprobante.MetodoPagoSpecified = true;
            //Emisor
            ComprobanteEmisor oEmisor = new ComprobanteEmisor();
            oEmisor.Rfc = usuario.rfc;
            oEmisor.Nombre = usuario.nombre_razon;
            oEmisor.RegimenFiscal = db.tbc_Regimenes.Where(u => u.id_regimen_fiscal == usuario.id_regimen_fiscal).Select(u => u.clave).First();
            //Receptor
            ComprobanteReceptor oReceptor = new ComprobanteReceptor();
            oReceptor.Nombre = receptor_c.nombre_razon;
            oReceptor.Rfc = receptor_c.rfc;
            oReceptor.UsoCFDI = db.tbc_Usos_CFDI.Where(u => u.id_uso_cfdi == factura.id_uso_cfdi).Select(u => u.clave).First();
            oReceptor.RegimenFiscalReceptor = db.tbc_Regimenes.Where(u => u.id_regimen_fiscal == receptor_c.id_regimen_fiscal).Select(u => u.clave).First();
            oReceptor.DomicilioFiscalReceptor = receptor_c.direccion_fiscal;
            //Asigno emisor y receptor
            oComprobante.Emisor = oEmisor;
            oComprobante.Receptor = oReceptor;
            //Conceptos
            List<ComprobanteConcepto> lstConceptos = new List<ComprobanteConcepto>();
            ComprobanteConcepto oConcepto = new ComprobanteConcepto();
            //--------------------------------------------------------------------------------------------------------
            Decimal total_trasladado = 0;
            Decimal total_retenido = 0;

            Decimal total_iva_ret = 0;
            Decimal total_isr_ret = 0;

            Decimal base_iva = 0;
            ComprobanteConceptoImpuestos conceptoImpuestos = new ComprobanteConceptoImpuestos();

            ComprobanteConceptoImpuestosTraslado comprobanteConceptoImpuestosTraslado = new ComprobanteConceptoImpuestosTraslado();
            ComprobanteImpuestosTraslado[] comprobanteImpuestosTraslados = new ComprobanteImpuestosTraslado[1];
            ComprobanteImpuestos impuesto = new ComprobanteImpuestos();
            var valorConc = db.tbd_Conceptos_Nota_Venta.ToList<tbd_Conceptos_Nota_Venta>().Where(u => u.id_nota_venta == id_).ToList();
            for (int i = 0; i < valorConc.Count; i++)
            {
                Decimal canti = Convert.ToDecimal(valorConc[i].cantidad);
                Decimal imp_unitario = Convert.ToDecimal(valorConc[i].precio_unitario);
                Decimal imp_total = Convert.ToDecimal(valorConc[i].total);
                Decimal descuento = Convert.ToDecimal(valorConc[i].total_descuento) == 0 ? 0 : Convert.ToDecimal(valorConc[i].total_descuento);
                var cprodserv = db.tbc_ProdServ.ToList<tbc_ProdServ>().Where(u => u.id_sat == valorConc[i].id_sat).Select(u => u.c_pord_serv).Single();
                var clv_unidad = db.tbc_Unidades_Medida.ToList<tbc_Unidades_Medida>().Where(u => u.id_unidad_medida == valorConc[i].id_unidad_medida).Select(u => u.clave).Single();
                oConcepto.Importe = Math.Round(imp_unitario, 2);
                oConcepto.ClaveProdServ = cprodserv;
                oConcepto.Cantidad = Convert.ToDecimal(valorConc[i].cantidad);
                oConcepto.ClaveUnidad = clv_unidad;
                oConcepto.Descripcion = valorConc[i].concepto;
                oConcepto.ValorUnitario = Math.Round(imp_unitario, 2);
                oConcepto.Descuento = descuento;
                oConcepto.ObjetoImp = "02";
                //oConcepto.Unidad = valorConc[i].unidad;//"Pieza";

                if (valorConc[i].total_iva > 0)
                {
                    Decimal i_total = Convert.ToDecimal(valorConc[i].total);
                    comprobanteConceptoImpuestosTraslado.Base = Math.Round(imp_unitario, 2);//10
                    comprobanteConceptoImpuestosTraslado.TasaOCuota = 0.160000m;
                    comprobanteConceptoImpuestosTraslado.Impuesto = "002";
                    comprobanteConceptoImpuestosTraslado.Importe = Math.Round(Convert.ToDecimal(valorConc[i].total_iva), 2);
                    comprobanteConceptoImpuestosTraslado.TipoFactor = "Tasa";
                    comprobanteConceptoImpuestosTraslado.ImporteSpecified = true;
                    comprobanteConceptoImpuestosTraslado.TasaOCuotaSpecified = true;
                    total_trasladado += Math.Round(Convert.ToDecimal(valorConc[i].total_iva), 2);
                    base_iva += Math.Round(Convert.ToDecimal(imp_unitario), 2);
                }
                //Retenido
                if (factura.iva_ret > 0)
                {
                    Decimal i_total = Convert.ToDecimal(factura.total);

                    total_iva_ret += Convert.ToDecimal(factura.isr_ret);
                }
                //
                if (factura.isr_ret > 0)
                {
                    Decimal i_total = Convert.ToDecimal(factura.total);

                    total_retenido += Convert.ToDecimal(factura.iva_ret);
                }

                conceptoImpuestos.Traslados = new ComprobanteConceptoImpuestosTraslado[1];

                conceptoImpuestos.Traslados[0] = comprobanteConceptoImpuestosTraslado;

                oConcepto.Impuestos = new ComprobanteConceptoImpuestos();

                oConcepto.Impuestos.Traslados = conceptoImpuestos.Traslados;

                lstConceptos.Add(oConcepto);
                oComprobante.Conceptos = lstConceptos.ToArray();

                impuesto.TotalImpuestosTrasladados = Math.Round(Convert.ToDecimal(factura.iva), 2);//1.60m;

                impuesto.TotalImpuestosTrasladadosSpecified = true;

            }

            //--------------------------------------------------------------------------------------------------------
            comprobanteImpuestosTraslados[0] = new ComprobanteImpuestosTraslado();

            comprobanteImpuestosTraslados[0].Base = base_iva;
            comprobanteImpuestosTraslados[0].TasaOCuota = 0.160000m;
            comprobanteImpuestosTraslados[0].Importe = total_trasladado;
            comprobanteImpuestosTraslados[0].TipoFactor = "Tasa";
            comprobanteImpuestosTraslados[0].Impuesto = "002";

            comprobanteImpuestosTraslados[0].ImporteSpecified = true;
            comprobanteImpuestosTraslados[0].TasaOCuotaSpecified = true;

            impuesto.Traslados = comprobanteImpuestosTraslados;

            oComprobante.Impuestos = impuesto;

            //Creamos el xml
            CreateXMLNota(oComprobante);

            string cadenaOriginal = "";
            string pathxsl = path + @"cadenaoriginal_4_0.xslt";
            System.Xml.Xsl.XslCompiledTransform transformador = new System.Xml.Xsl.XslCompiledTransform(true);
            transformador.Load(pathxsl);

            using (StringWriter sw = new StringWriter())
            using (XmlWriter xwo = XmlWriter.Create(sw, transformador.OutputSettings))
            {
                transformador.Transform(pathXML, xwo);
                cadenaOriginal = sw.ToString();
            }

            SelloDigital oSelloDigital = new SelloDigital();
            oComprobante.Certificado = oSelloDigital.Certificado(pathCer);
            oComprobante.Sello = oSelloDigital.Sellar(cadenaOriginal, pathKey, clavePrivada);

            //Creamos el xml
            CreateXMLNota(oComprobante);
            factura.url_xml = aux_path;
            db.SaveChanges();
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        private static void CreateXMLNota(Comprobante oComprobante)
        {
            //SERIALIZAMOS.-------------------------------------------------

            //string pathXML = p + @"Plantillas\FacturaXML.xml";
            string pathXML = p_xml;
            XmlSerializerNamespaces xmlNameSpace = new XmlSerializerNamespaces();
            xmlNameSpace.Add("cfdi", "http://www.sat.gob.mx/cfd/4");
            xmlNameSpace.Add("xs", "http://www.w3.org/2001/XMLSchema");
            xmlNameSpace.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            XmlSerializer oXmlSerializar = new XmlSerializer(typeof(Comprobante));

            string sXml = "";

            using (var sww = new CLS40.StringWriterWithEncoding(Encoding.UTF8))
            {

                using (XmlWriter writter = XmlWriter.Create(sww))
                {
                    oXmlSerializar.Serialize(writter, oComprobante, xmlNameSpace);
                    sXml = sww.ToString();
                    var text = sXml.Substring(55); //sXml.Split("<?xml version=\"1.0\" encoding=\"utf-8\"?><cfdi:Comprobante"); //55
                    sXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><cfdi:Comprobante " + "xsi:schemaLocation=\"http://www.sat.gob.mx/cfd/4 http://www.sat.gob.mx/sitio_internet/cfd/4/cfdv40.xsd\"" + text;
                }

            }

            //guardamos el string en un archivo
            System.IO.File.WriteAllText(pathXML, sXml);
        }
        //XML Pagos Servicio
        public ActionResult genXMLPagosServicio(Int32? id_)
        {
            //************************************************************************************************************************************************
            //Get Info Nota de Venta en DB
            db = new BD_FFEntities();
            var factura = db.tbd_Cobros.ToList<tbd_Cobros>().Where(u => u.id_cobro == id_).Single();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var firma = db.tbd_Firmas.ToList<tbd_Firmas>().Where(u => u.rfc == usuario.rfc).Single();
            //DatosIVA
            var tipo = factura.timbres;
            decimal iva_ = 0;
            decimal s_iva = 0;
            switch (tipo)
            {
                case 1:
                    iva_ = 1.60m;
                    break;
                case 15:
                    iva_ = 54.76m;
                    break;
                case 50:
                    iva_ = 91.03m;
                    break;
                case 500:
                    iva_ = 288.69m;
                    break;
                case 1000:
                    iva_ = 467.59m;
                    break;
                case 2000:
                    iva_ = 544.83m;
                    break;
                case 3000:
                    iva_ = 668.14m;
                    break;
                case 4000:
                    iva_ = 886.07m;
                    break;
                case 5000:
                    iva_ = 1465.38m;
                    break;
            }
            s_iva = factura.total - iva_;
            //Ruta donde alojamos los Archivos
            var fca_emision = factura.fecha_cobro.ToString();

            String[] fechaE = fca_emision.Split(' ');
            string aux_fc_emi = fechaE[0];
            String[] auxfechaE = aux_fc_emi.Split('/');
            string ax_fc_emi = auxfechaE[0] + auxfechaE[1] + auxfechaE[2];

            var ruta_xml = @"XML\PDF\Facturafast\" + ax_fc_emi;//factura.url_xml;
            //string[] nom_doc = "Temp_";//factura.url_pdf.Split('\\');
            //string[] nd = nom_doc[4].Split('.');
            //string nf = nd[0];
            string namefile = "tempXML";
            //************************************
            string DirPrg_ = Server.MapPath("~");
            bool fileExist = System.IO.File.Exists(DirPrg_+"Plantillas/"+ruta_xml);
            FileInfo file = new FileInfo(DirPrg_+"Plantillas/"+ruta_xml);
            try
            {
                file.Delete();
                fileExist = System.IO.File.Exists(DirPrg_+"Plantillas/"+ruta_xml);
            }
            catch (Exception e)
            {

            }
            if (!fileExist)
            {
                DirectoryInfo didoc = Directory.CreateDirectory(DirPrg_+"Plantillas/"+ruta_xml);
            }
            string aux_path = ruta_xml + @"\" + namefile + ".xml";
            string path = Server.MapPath("~");
            p = path;
            string pathXML = path + @"Plantillas\"+ruta_xml + "\\" + namefile + ".xml";
            
            string pathCer = path + @"\Plantillas\Firmas\Facturafast\00001000000506285169.cer";
            string pathKey = path + @"\Plantillas\Firmas\Facturafast\CSD_FACTURAFAST_SA_DE_CV_FAC201027H66_20210129_131834.key";
            string clavePrivada = "HUEXOTITLA2021";
            
            p_xml = pathXML;
            //Obtenemos el Número de Certificado
            string numeroCertificado, aa, b, c;
            SelloDigital.leerCER(pathCer, out aa, out b, out c, out numeroCertificado);
            //----------------Llenamos la clase COMPROBANTE ---------------------------
            Comprobante oComprobante = new Comprobante();
            oComprobante.Version = "4.0";
            oComprobante.Serie = "A";// factura.serie;
            oComprobante.Folio = "1";// factura.folio;
            oComprobante.Fecha = DateTime.Now.AddMinutes(-2).ToString("yyyy-MM-ddTHH:mm:ss");
            //oComprobante.Sello = ""; //sig video
            oComprobante.FormaPago = "03";//db.tbc_Formas_Pago.Where(u => u.id_forma_pago == factura.id_forma_pago).Select(u => u.clave).First();
            oComprobante.NoCertificado = numeroCertificado;
            //oComprobante.Certificado = ""; //sig video
            oComprobante.SubTotal = s_iva;//Math.Round(Convert.ToDecimal(factura.importe), 2);
            oComprobante.Moneda = "MXN";
            oComprobante.Total = Math.Round(Convert.ToDecimal(factura.total), 2);
            oComprobante.TipoDeComprobante = "I";
            oComprobante.MetodoPago = "PUE";// db.tbc_Metodos_Pago.Where(u => u.id_metodo_pago == factura.id_forma_pago).Select(u => u.clave).First();
            oComprobante.LugarExpedicion = "72534";
            oComprobante.Descuento = 0;
            oComprobante.Exportacion = "01";

            oComprobante.FormaPagoSpecified = true;
            oComprobante.MetodoPagoSpecified = true;
            //Emisor
            ComprobanteEmisor oEmisor = new ComprobanteEmisor();
            oEmisor.Rfc = "FAC201027H66";//usuario.rfc;
            oEmisor.Nombre = "FACTURAFAST";// usuario.nombre_razon;
            oEmisor.RegimenFiscal = "601";
            //Receptor
            ComprobanteReceptor oReceptor = new ComprobanteReceptor();
            oReceptor.Nombre = usuario.nombre_razon;
            oReceptor.Rfc = usuario.rfc;
            oReceptor.UsoCFDI = "G03";//db.tbc_Usos_CFDI.Where(u => u.id_uso_cfdi == factura.id_uso_cfdi).Select(u => u.clave).First();
            oReceptor.RegimenFiscalReceptor = db.tbc_Regimenes.Where(u => u.id_regimen_fiscal == usuario.id_regimen_fiscal).Select(u => u.clave).First();
            oReceptor.DomicilioFiscalReceptor = usuario.cp;
            //Asigno emisor y receptor
            oComprobante.Emisor = oEmisor;
            oComprobante.Receptor = oReceptor;
            //Conceptos
            List<ComprobanteConcepto> lstConceptos = new List<ComprobanteConcepto>();
            ComprobanteConcepto oConcepto = new ComprobanteConcepto();
            //--------------------------------------------------------------------------------------------------------
            Decimal total_trasladado = 0;
            
            Decimal base_iva = 0;
            ComprobanteConceptoImpuestos conceptoImpuestos = new ComprobanteConceptoImpuestos();

            ComprobanteConceptoImpuestosTraslado comprobanteConceptoImpuestosTraslado = new ComprobanteConceptoImpuestosTraslado();
            ComprobanteImpuestosTraslado[] comprobanteImpuestosTraslados = new ComprobanteImpuestosTraslado[1];
            ComprobanteImpuestos impuesto = new ComprobanteImpuestos();
            
            
            Decimal imp_unitario = Convert.ToDecimal(factura.importe);
            Decimal imp_total = Convert.ToDecimal(factura.total);
            Decimal descuento = 0;

            oConcepto.Importe = s_iva;//Math.Round(imp_unitario, 2);
            oConcepto.ClaveProdServ = "84111506";
            oConcepto.Cantidad = 1;
            oConcepto.ClaveUnidad = "E48";
            oConcepto.Descripcion = db.tbc_Paquetes.Where(u => u.id_paquete== factura.id_paquete).Select(u => u.nombre_paquete).First(); ;
            oConcepto.ValorUnitario = s_iva;//Math.Round(imp_unitario, 2);
            oConcepto.Descuento = descuento;
            oConcepto.ObjetoImp = "02";
            //{        
            Decimal i_total = s_iva;//Convert.ToDecimal(factura.total);
            comprobanteConceptoImpuestosTraslado.Base = s_iva;//Math.Round(imp_unitario, 2);//10
                    comprobanteConceptoImpuestosTraslado.TasaOCuota = 0.160000m;
                    comprobanteConceptoImpuestosTraslado.Impuesto = "002";
                    comprobanteConceptoImpuestosTraslado.Importe = Math.Round(iva_, 2);
                    comprobanteConceptoImpuestosTraslado.TipoFactor = "Tasa";
                    comprobanteConceptoImpuestosTraslado.ImporteSpecified = true;
                    comprobanteConceptoImpuestosTraslado.TasaOCuotaSpecified = true;
                    total_trasladado += Math.Round(iva_, 2);
                    base_iva += Math.Round(Convert.ToDecimal(imp_unitario), 2);
                //}
                
                conceptoImpuestos.Traslados = new ComprobanteConceptoImpuestosTraslado[1];

                conceptoImpuestos.Traslados[0] = comprobanteConceptoImpuestosTraslado;

                oConcepto.Impuestos = new ComprobanteConceptoImpuestos();

                oConcepto.Impuestos.Traslados = conceptoImpuestos.Traslados;

                lstConceptos.Add(oConcepto);
                oComprobante.Conceptos = lstConceptos.ToArray();

                impuesto.TotalImpuestosTrasladados = Math.Round(iva_, 2);

                impuesto.TotalImpuestosTrasladadosSpecified = true;

            //}

            //--------------------------------------------------------------------------------------------------------
            comprobanteImpuestosTraslados[0] = new ComprobanteImpuestosTraslado();

            comprobanteImpuestosTraslados[0].Base = s_iva;
            comprobanteImpuestosTraslados[0].TasaOCuota = 0.160000m;
            comprobanteImpuestosTraslados[0].Importe = total_trasladado;
            comprobanteImpuestosTraslados[0].TipoFactor = "Tasa";
            comprobanteImpuestosTraslados[0].Impuesto = "002";

            comprobanteImpuestosTraslados[0].ImporteSpecified = true;
            comprobanteImpuestosTraslados[0].TasaOCuotaSpecified = true;

            impuesto.Traslados = comprobanteImpuestosTraslados;

            oComprobante.Impuestos = impuesto;

            //Creamos el xml
            CreateXMLPagos(oComprobante);

            string cadenaOriginal = "";
            string pathxsl = path + @"cadenaoriginal_4_0.xslt";
            System.Xml.Xsl.XslCompiledTransform transformador = new System.Xml.Xsl.XslCompiledTransform(true);
            transformador.Load(pathxsl);

            using (StringWriter sw = new StringWriter())
            using (XmlWriter xwo = XmlWriter.Create(sw, transformador.OutputSettings))
            {
                transformador.Transform(pathXML, xwo);
                cadenaOriginal = sw.ToString();
            }

            SelloDigital oSelloDigital = new SelloDigital();
            oComprobante.Certificado = oSelloDigital.Certificado(pathCer);
            oComprobante.Sello = oSelloDigital.Sellar(cadenaOriginal, pathKey, clavePrivada);

            //Creamos el xml
            CreateXMLPagos(oComprobante);
            factura.url_xml = aux_path;
            db.SaveChanges();
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public static void CreateXMLPagos(Comprobante oComprobante)
        {
            //SERIALIZAMOS.-------------------------------------------------

            //string pathXML = p + @"Plantillas\FacturaXML.xml";
            string pathXML = p_xml;
            XmlSerializerNamespaces xmlNameSpace = new XmlSerializerNamespaces();
            xmlNameSpace.Add("cfdi", "http://www.sat.gob.mx/cfd/4");
            xmlNameSpace.Add("xs", "http://www.w3.org/2001/XMLSchema");
            xmlNameSpace.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            XmlSerializer oXmlSerializar = new XmlSerializer(typeof(Comprobante));

            string sXml = "";

            using (var sww = new CLS40.StringWriterWithEncoding(Encoding.UTF8))
            {

                using (XmlWriter writter = XmlWriter.Create(sww))
                {
                    oXmlSerializar.Serialize(writter, oComprobante, xmlNameSpace);
                    sXml = sww.ToString();
                    var text = sXml.Substring(55); //sXml.Split("<?xml version=\"1.0\" encoding=\"utf-8\"?><cfdi:Comprobante"); //55
                    sXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><cfdi:Comprobante " + "xsi:schemaLocation=\"http://www.sat.gob.mx/cfd/4 http://www.sat.gob.mx/sitio_internet/cfd/4/cfdv40.xsd\"" + text;
                }

            }

            //guardamos el string en un archivo
            System.IO.File.WriteAllText(pathXML, sXml);
        }

        //Requisitos para XML
        void FabricaPEM(String cer, String key, String pass, String passCSDoFIEL, String rfc)
        {
            Dictionary<String, String> DicArchivos = new Dictionary<String, String>();
            String ConvierteCerAPem;
            String ConvierteKeyAPem;
            String EncriptaKey;
            String ArchivoCer = cer;
            String ArchivoKey = key;
            String NombreArchivoCertificado = Path.GetFileName(ArchivoCer);
            String NombreArchivoLlave = Path.GetFileName(ArchivoKey);
            //String usuario;
            //usuario = Environment.UserName;
            //MessageBox.Show(usuario);
            String url;
            url = Server.MapPath("~") + "\\Plantillas\\Firmas\\" + rfc + "\\";
            String ruta;
            ruta = @"C:\OpenSSL\bin\";//Esta ruta es donde tiene ubicado el .exe del OpenSSL
            ConvierteCerAPem = ruta + "openssl.exe x509 -inform DER -outform PEM -in " + ArchivoCer + " -pubkey -out " + url + NombreArchivoCertificado + ".pem";
            ConvierteKeyAPem = ruta + "openssl.exe pkcs8 -inform DER -in " + ArchivoKey + " -passin pass:" + passCSDoFIEL + " -out " + url + NombreArchivoLlave + ".pem";
            EncriptaKey = ruta + "openssl.exe rsa -in " + url + NombreArchivoLlave + ".pem" + " -des3 -out " + url + NombreArchivoLlave + ".enc -passout pass:" + pass;

            //Crea el archivo Certificado.BAT
            System.IO.StreamWriter oSW = new System.IO.StreamWriter(url + "CERyKEY.bat");
            oSW.WriteLine(ConvierteCerAPem);
            oSW.WriteLine(ConvierteKeyAPem);
            oSW.WriteLine(EncriptaKey);
            oSW.Flush();
            oSW.Close();

            Process.Start(url + "CERyKEY.bat").WaitForExit();
        }
        //XML a Base64
        public byte[] stringToBase64ByteArray(String input)
        {
            Byte[] ret = Encoding.UTF8.GetBytes(input);
            String s = Convert.ToBase64String(ret);
            ret = Convert.FromBase64String(s);
            return ret;
        }
        //Cancelar XML
        public JsonResult setCancelar(int id, string ffiscal, string motivo, string folio_, string tipo)
        {
            string mensaje = "";
            //com.finkok.demo.cancelResponse cancelresponse = new com.finkok.demo.cancelResponse();
            CancelarProductivo.cancelResponse cancelResponse = new CancelarProductivo.cancelResponse();

            //Get Info PreFac en DB
            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var firma = db.tbd_Firmas.ToList<tbd_Firmas>().Where(u => u.rfc == usuario.rfc).Single();
            //----------------------------------------------------------------
            //modifiquen por su path
            string path = Server.MapPath("~");
            //Obtener numero certificado------------------------------------------------------------
            string DireccionCer = path + @"Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_cer_sello;
            string DireccionKey = path + @"Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_key_sello;
            string PasswordFinkok = "F4ctur4f4st_C@st3l4n";
            string PasswordCer = firma.password_sello;
            string username = "cfdi@facturafast.mx";

            string estatusuuid = "";
            //com.finkok.demo.CancelSOAP cancela = new com.finkok.demo.CancelSOAP();
            //com.finkok.demo.cancel can = new com.finkok.demo.cancel();
            CancelarProductivo.CancelSOAP cancela = new CancelarProductivo.CancelSOAP();
            CancelarProductivo.cancel can = new CancelarProductivo.cancel();
            try
            {
                FabricaPEM(DireccionCer, DireccionKey, PasswordFinkok, PasswordCer, usuario.rfc);
                String cer;
                String key;

                //Para importar clase TextFieldParser, ingresas al menú Proyecto-- > Agregar Referencia-- > Ensamblados-- > Seleccionar Microsotf.VisualBasic-- > Aceptar
                using (TextFieldParser fileReader = new TextFieldParser(path + @"\Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_cer_sello + ".pem"))
                    cer = fileReader.ReadToEnd();

                using (TextFieldParser fileReader = new TextFieldParser(path + @"\Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_key_sello + ".enc"))
                    key = fileReader.ReadToEnd();

                List<CancelarProductivo.UUID> lista = new List<CancelarProductivo.UUID>();
                lista.Add(new CancelarProductivo.UUID { UUID1 = ffiscal, FolioSustitucion = folio_, Motivo = motivo });

                can.username = username;
                can.password = PasswordFinkok;
                can.taxpayer_id = usuario.rfc;//"EKU9003173C9";
                can.UUIDS = lista.ToArray();
                can.cer = stringToBase64ByteArray(cer);
                can.key = stringToBase64ByteArray(key);

                cancelResponse = cancela.cancel(can);

                if (cancelResponse.cancelResult.CodEstatus == null)
                {
                    String emisor = cancelResponse.cancelResult.RfcEmisor;
                    String acuse = cancelResponse.cancelResult.Acuse;
                    String fecha = cancelResponse.cancelResult.Fecha;
                    //MessageBox.Show("Acuse: " + acuse + "\nFecha: " + fecha + "\nRFC Emisor: " + emisor);
                    Array folio = cancelResponse.cancelResult.Folios;
                    if (cancelResponse.cancelResult.Folios.Length > 0)
                    {
                        Array foliofiscal = cancelResponse.cancelResult.Folios;
                        var estatusCancelacionFiel = "";
                        for (int pos = 0; pos < foliofiscal.Length; pos++)
                        {
                            estatusCancelacionFiel = cancelResponse.cancelResult.Folios[pos].EstatusUUID;
                            mensaje = "Cancelado|UUID: " + cancelResponse.cancelResult.Folios[pos].UUID +
                                "|Estatus cancelación: " + cancelResponse.cancelResult.Folios[pos].EstatusCancelacion +
                                "|Estatus UUID: " + cancelResponse.cancelResult.Folios[pos].EstatusUUID;

                        }
                        using (BD_FFEntities db = new BD_FFEntities())
                        {
                            
                            db.Configuration.LazyLoadingEnabled = false;
                            if (tipo != "Pago")
                            {
                                //var valor = db.tbd_Pre_Carta_Porte.ToList<tbd_Pre_Carta_Porte>().Where(u => u.id_prefactura == id).FirstOrDefault();
                                var valorPreFac = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == id).FirstOrDefault();
                                //valor.status = "3";
                                valorPreFac.status = 3;
                                //if (estatusCancelacionFiel == "201") {
                                //    var updFact = db.tbd_Facturas.ToList<tbd_Facturas>().Where(u => u.uuid == valorPreFac.uuid).FirstOrDefault();
                                //    updFact.id_estatus = 8;
                                //}
                            }
                            else
                            {
                                var valor = db.tbd_Pre_Pagos.ToList<tbd_Pre_Pagos>().Where(u => u.id == id).FirstOrDefault();
                                valor.status = 3;
                            }

                            tbd_Cancelacion_Factura cancelaFac = new tbd_Cancelacion_Factura
                            {
                                id_pre_fac = id,
                                uuid = ffiscal,
                                folio_sustitucion = cancelResponse.cancelResult.Folios[0].UUID,
                                motivo = motivo,
                                acuse = acuse,
                                rfc_emisor = emisor,
                                fecha = fecha,
                                estatus_camcelacion = cancelResponse.cancelResult.Folios[0].EstatusCancelacion,
                                estatus_uuid = cancelResponse.cancelResult.Folios[0].EstatusUUID
                            };
                            db.tbd_Cancelacion_Factura.Add(cancelaFac);
                            if (estatusCancelacionFiel == "201" || estatusCancelacionFiel == "202")
                            {
                                var factura = db.tbd_Facturas.ToList<tbd_Facturas>().Where(u => u.uuid == ffiscal).FirstOrDefault();
                                factura.id_estatus = 8;
                            }
                            
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    mensaje = "Error: " + cancelResponse.cancelResult.CodEstatus;
                    //MessageBox.Show(estatusUuid);
                }
            }
            catch (Exception ex)
            {

                mensaje = "Error: " + ex;
            }
            return Json(mensaje, JsonRequestBehavior.AllowGet);
        }
        //Get Status Factura
        public JsonResult getStatusXML(int id, string uuid, string tipo)
        {
            //Get Info PreFac en DB
            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var firma = db.tbd_Firmas.ToList<tbd_Firmas>().Where(u => u.rfc == usuario.rfc).Single();
            var rfc_rec = "";
            var uuid_ = "";
            if (tipo == "Pago")
            {
                var prePago_ = db.tbd_Pre_Pagos.ToList<tbd_Pre_Pagos>().Where(u => u.id == id).Single();
                var cliente = db.tbc_Clientes.ToList<tbc_Clientes>().Where(u => u.id_cliente == prePago_.id_cliente).Single();
                rfc_rec = cliente.rfc;
                uuid_ = prePago_.uuid;
            }
            else
            {
                var preFac_ = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == id).Single();
                var cliente = db.tbc_Clientes.ToList<tbc_Clientes>().Where(u => u.rfc == preFac_.rfc_cliente).Single();
                rfc_rec = cliente.rfc;
                uuid_ = db.tbd_Cfdi_Uuid.ToList<tbd_Cfdi_Uuid>().Where(u => u.id_pre_factura == preFac_.id_pre_factura).Select(u => u.uuid).First();
            }

            //----------------------------------------------------------------
            //com.finkok.demo.CancelSOAP selloSOAP = new com.finkok.demo.CancelSOAP();
            //com.finkok.demo.get_sat_status consulta = new com.finkok.demo.get_sat_status();
            //com.finkok.demo.get_sat_statusResponse getResponse = new com.finkok.demo.get_sat_statusResponse();
            CancelarProductivo.CancelSOAP selloSOAP = new CancelarProductivo.CancelSOAP();
            CancelarProductivo.get_sat_status consulta = new CancelarProductivo.get_sat_status();
            CancelarProductivo.get_sat_statusResponse getResponse = new CancelarProductivo.get_sat_statusResponse();

            //consulta.username = "programador1@consultoriacastelan.com";
            //consulta.password = "Programador1*";

            consulta.username = "cfdi@facturafast.mx";
            consulta.password = "F4ctur4f4st_C@st3l4n";

            consulta.taxpayer_id = usuario.rfc;
            consulta.rtaxpayer_id = rfc_rec;
            consulta.uuid = uuid_;
            consulta.total = "0.00";

            getResponse = selloSOAP.get_sat_status(consulta);

            String Escancelable = getResponse.get_sat_statusResult.sat.EsCancelable;
            String CodigoEstatus = getResponse.get_sat_statusResult.sat.CodigoEstatus;
            String Estado = getResponse.get_sat_statusResult.sat.Estado;
            String estatusUuid = getResponse.get_sat_statusResult.error;
            string mensaje = "";
            try
            {
                mensaje = "S|" + Escancelable + "|" + CodigoEstatus + "|" + Estado;
            }
            catch (Exception)
            {
                mensaje = "Error:" + estatusUuid;
            }
            return Json(mensaje, JsonRequestBehavior.AllowGet);
        }
        //Timbrar XML
        public JsonResult TimbrarXML(int id_, string n_doc, string tipo)
        {
            string DirPrg = Server.MapPath("~");
            //Get Info PreFac en DB
            db = new BD_FFEntities();
            var ruta_xml = "";
            string namefile = "";
            string ruta_pdf = "";
            tbc_Usuarios usuario_ = Session["tbc_Usuarios"] as tbc_Usuarios;
            if (tipo == "Pago")
            {
                var factura = db.tbd_Pre_Pagos.ToList<tbd_Pre_Pagos>().Where(u => u.id == id_).Single();
                ruta_xml = factura.url_xml;
                ruta_pdf = factura.url_pdf;
                string[] nom_doc = factura.url_pdf.Split('\\');
                string[] nd = nom_doc[5].Split('.');
                string nf = nd[0];
                namefile = nf;
            }
            else if (tipo == "CartaPorte")
            {
                var cartaPorte = db.tbd_Pre_Carta_Porte.ToList<tbd_Pre_Carta_Porte>().Where(u => u.id == id_).Single();
                var factura = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == cartaPorte.id_prefactura).Single();
                ruta_pdf = "Plantillas\\" + factura.url_pdf;
                ruta_xml = "Plantillas\\" + factura.url_xml;
                string[] nom_doc = factura.url_pdf.Split('\\');
                string[] nd = nom_doc[5].Split('.');
                string nf = nd[0];
                namefile = nf;
            }
            else if (tipo == "FacturaNV") {
                var factura = db.tbd_Notas_Venta.ToList<tbd_Notas_Venta>().Where(u => u.id_nota_venta == id_).Single();
                ruta_xml = factura.url_xml;
                ruta_pdf = factura.url_pdf;
                string[] nom_doc = factura.url_pdf.Split('/');
                string[] nd = nom_doc[4].Split('.');
                ruta_xml = nom_doc[0]+"\\"+ nom_doc[1]+"\\"+nom_doc[2]+"\\"+nom_doc[3]+"\\";
                string nf = nd[0];
                namefile = "tempXML";
            } 
            else
            {
                var factura = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == id_).Single();
                ruta_xml = factura.url_xml;
                ruta_pdf = factura.url_pdf;
                string[] nom_doc = factura.url_pdf.Split('\\');
                string[] nd = nom_doc[4].Split('.');
                string nf = nd[0];
                namefile = nf;
            }
            n_doc = namefile + ".xml";
            //------------------------------------------------------------------------------------------------------------------------------------
            //Instancia
            SR.StampSOAP selloSOAP = new SR.StampSOAP();
            SR.stamp fx = new SR.stamp();
            //SR.stampResponse selloResponse = new SR.stampResponse();


            //Productivo
            TimbradoProductivo.StampSOAP SOAP = new TimbradoProductivo.StampSOAP();
            TimbradoProductivo.stamp stamp = new TimbradoProductivo.stamp();
            TimbradoProductivo.stampResponse selloResponse = new TimbradoProductivo.stampResponse();

            //Parametros
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(DirPrg +"Plantillas\\"+ruta_xml + "\\" + n_doc);
            string rutaO = DirPrg + ruta_pdf;// + "\\" + n_doc;
            string rutaM = "";
            //Conviertes el archivo en Byte
            byte[] byteXmlDocument = Encoding.UTF8.GetBytes(xmlDocument.OuterXml);
            string stringByteXmlDocument = Convert.ToBase64String(byteXmlDocument);
            byteXmlDocument = Convert.FromBase64String(stringByteXmlDocument);

            //Timbras el Archivo
            //fx.xml = byteXmlDocument;
            //fx.username = "programador1@consultoriacastelan.com";
            //fx.password = "Programador1*";

            stamp.xml = byteXmlDocument;
            stamp.username = "cfdi@facturafast.mx";
            stamp.password = "F4ctur4f4st_C@st3l4n";


            //Generamos Request
            String usuario;
            usuario = Environment.UserName;
            String url = DirPrg + "Plantillas\\"+ruta_xml;
            StreamWriter XML = new StreamWriter(url + "SOAP_Request.xml");
            //Direccion donde guardaremos el SOAP Envelope
            XmlSerializer soap = new XmlSerializer(stamp.GetType());
            //Obtenemos los datos del objeto oStamp que contiene los parámetros de envió y es de tipo stamp()
            soap.Serialize(XML, stamp);
            XML.Close();

            //Recibes la respuesta de Timbrado
            //selloResponse = selloSOAP.stamp(fx);

            selloResponse = SOAP.stamp(stamp);


            string mensaje = "";
            string uuidR = "";
            try
            {
                mensaje = "No se timbro el XML|" + selloResponse.stampResult.Incidencias[0].CodigoError.ToString() +
                    "|Mensaje: " + selloResponse.stampResult.Incidencias[0].MensajeIncidencia;
            }
            catch (Exception)
            {
                mensaje = "Timbrado|" + selloResponse.stampResult.CodEstatus.ToString() + "|" +
                    selloResponse.stampResult.Fecha.ToString() + "|" +
                    selloResponse.stampResult.UUID.ToString() + "|" +
                    selloResponse.stampResult.xml.ToString();
                uuidR = selloResponse.stampResult.UUID.ToString();
                StreamWriter XMLL = new StreamWriter(url + "\\" + uuidR + ".xml");
                XMLL.Write(selloResponse.stampResult.xml);
                XMLL.Close();
                //Cambiar nombre a PDF
                rutaM = DirPrg + ruta_xml + "\\" + uuidR + ".pdf";
                var rutaXMLO = DirPrg + ruta_xml + "\\" + n_doc;
                if (System.IO.File.Exists(rutaO))
                {
                    System.IO.File.Move(rutaO, rutaM);
                    System.IO.File.Delete(rutaO);
                }
                if (System.IO.File.Exists(rutaXMLO))
                {
                    System.IO.File.Delete(rutaXMLO);
                }
                ////Actualizar Estado XML
                using (BD_FFEntities db = new BD_FFEntities())
                {
                    if (tipo == "CartaPorte")
                    {
                        db.Configuration.LazyLoadingEnabled = false;
                        var valorCartaP = db.tbd_Pre_Carta_Porte.ToList<tbd_Pre_Carta_Porte>().Where(u => u.id == id_).FirstOrDefault();
                        var valorPreFac = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == valorCartaP.id_prefactura).FirstOrDefault();
                        valorPreFac.selloSAT = selloResponse.stampResult.SatSeal;
                        valorPreFac.ccertificacion = selloResponse.stampResult.NoCertificadoSAT;
                        valorPreFac.url_xml = ruta_xml + uuidR + ".xml";
                        valorPreFac.url_pdf = ruta_xml + uuidR + ".pdf";
                        valorPreFac.status = 2;
                        //Guardar CFDI
                        tbd_Cfdi_Uuid cfdi = new tbd_Cfdi_Uuid
                        {
                            id_pre_factura = id_,
                            id_relacion = "1",
                            uuid = uuidR
                        };
                        db.tbd_Cfdi_Uuid.Add(cfdi);
                        db.SaveChanges();
                    }
                    else if (tipo != "Pago")
                    {
                        if (tipo == "FacturaNV") {
                            db.Configuration.LazyLoadingEnabled = false;
                            var valorNota = db.tbd_Notas_Venta.ToList<tbd_Notas_Venta>().Where(u => u.id_nota_venta == id_).FirstOrDefault();
                            valorNota.id_estatus = 7;
                            valorNota.uuid = uuidR;
                            valorNota.url_xml = ruta_xml + uuidR + ".xml";
                            //valorNota.url_pdf = ruta_xml + uuidR + ".pdf";
                            db.SaveChanges();
                        } else {
                            db.Configuration.LazyLoadingEnabled = false;
                            var valorPreFac = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == id_).FirstOrDefault();
                            valorPreFac.selloSAT = selloResponse.stampResult.SatSeal;
                            valorPreFac.ccertificacion = selloResponse.stampResult.NoCertificadoSAT;
                            valorPreFac.url_xml = ruta_xml + uuidR + ".xml";
                            valorPreFac.url_pdf = ruta_xml + uuidR + ".pdf";
                            valorPreFac.status = 2;
                            //Guardar CFDI
                            tbd_Cfdi_Uuid cfdi = new tbd_Cfdi_Uuid
                            {
                                id_pre_factura = id_,
                                id_relacion = "1",
                                uuid = uuidR
                            };
                            db.tbd_Cfdi_Uuid.Add(cfdi);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        db.Configuration.LazyLoadingEnabled = false;
                        var valorPrePago = db.tbd_Pre_Pagos.ToList<tbd_Pre_Pagos>().Where(u => u.id == id_).FirstOrDefault();
                        valorPrePago.selloSAT = selloResponse.stampResult.SatSeal;
                        valorPrePago.ccertificacion = selloResponse.stampResult.NoCertificadoSAT;
                        valorPrePago.uuid = uuidR;
                        valorPrePago.url_xml = ruta_xml + uuidR + ".xml";
                        valorPrePago.url_pdf = ruta_xml + uuidR + ".pdf";
                        valorPrePago.status = 2;
                        db.SaveChanges();
                    }
                }
                //Restar timbre
                tbc_Timbres timbres = db.tbc_Timbres.Where(s => s.rfc_usuario == usuario_.rfc).Single();

                timbres.timbres_usados++;
                timbres.timbres_disponibles--;

                db.SaveChanges();
                //---------Agregar a base Facturas-------------------

                string root_xml = DirPrg  +"Plantillas\\"+ ruta_xml + "\\" + uuidR + ".xml";

                LeerArchivo(root_xml, usuario_.rfc, id_, tipo);
            }
            return Json(mensaje, JsonRequestBehavior.AllowGet);
        }
        //Almacenar Facturas
        public void LeerArchivo(string root, String rfc, int id_prefac, string tipo)
        {
            BD_FFEntities db = new BD_FFEntities();
            tbc_Variables_Calculo variable = db.tbc_Variables_Calculo.Single();
            //creamos un flujo el cual recibe nuestro xml
            using (StreamReader reader = new StreamReader(root))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(reader);
                //try
                //{
                foreach (var Comprobante in xmlDoc.ChildNodes)
                {
                    if (Comprobante.GetType() == typeof(XmlElement))
                    {
                        var _comprobante = (Comprobante as XmlElement);
                        if (_comprobante.LocalName == "Comprobante")
                        {

                            List<tbd_Conceptos_Factura> conceptos = new List<tbd_Conceptos_Factura>();

                            String _version = "";

                            //!Comprobante
                            DateTime _fechaEmision;
                            Decimal _tipoCambio = 0;
                            String _serie = "";
                            String _folio = "";
                            String _moneda = "";
                            Decimal _total_original = 0;
                            Decimal _subtotal = 0;
                            String _metodoPago = "";
                            String _formaPago = "";
                            String _tipoComprobante = "";
                            Decimal _descuento = 0;
                            String _lugarExpedicion = "";
                            String _exportacion = "";

                            //Emisor
                            String _rfcEmisor = "";
                            String _nombreEmisor = "";
                            String _regimenFiscalEmisor = "";


                            //!Receptor
                            String _rfcReceptor = "";
                            String _nombreReceptor = "";
                            String _usoCFDI = "";
                            String _regimenFiscalReceptor = "";
                            String _domicilioFiscalReceptor = "";

                            //!TimbreFiscalDigital
                            DateTime _fechaTimbrado = DateTime.Now;
                            String _uuid = "";
                            String _versionTimbreFiscalDigital = "";
                            String _certificadoSAT = "";
                            String _selloDigital = "";
                            String _selloSAT = "";


                            //!CartaPorte
                            String _transpInter = "";
                            String _totalDistancia = "";

                            //!CartaPorte - Ubicaciones
                            String _idUbicacionOrigen = "";
                            String _rfcRemDesOrigen = "";
                            String _nombreRemDesOrigen = "";
                            String _distanciaRecorridaOrigen = "";
                            DateTime _fechaHoraOrigen = DateTime.Now;
                            String _calleOrigen = "";
                            String _coloniaOrigen = "";
                            String _localidadOrigen = "";
                            String _municipioOrigen = "";
                            String _estadoOrigen = "";
                            String _paisOrigen = "";
                            String _cpOrigen = "";
                            String _direccionOrigen = "";

                            String _idUbicacionDestino = "";
                            String _rfcRemDesDestino = "";
                            String _nombreRemDesDestino = "";
                            String _distanciaRecorridaDestino = "";
                            DateTime _fechaHoraDestino = DateTime.Now;
                            String _calleDestino = "";
                            String _coloniaDestino = "";
                            String _localidadDestino = "";
                            String _municipioDestino = "";
                            String _estadoDestino = "";
                            String _paisDestino = "";
                            String _cpDestino = "";
                            String _direccionDestino = "";

                            //!CartaPorte - Mercancias
                            String _pesoBrutoTotal = "";
                            String _unidadPeso = "";
                            String _numTotalMercancias = "";

                            //!CartaPorte - Mercancia
                            String _bienesTrans = "";
                            String _descripcionMerca = "";
                            String _cantidadMerca = "";
                            String _claveUnidadMerca = "";
                            String _unidadMerca = "";
                            String _pesoKG = "";

                            //!CartaPorte - AutoTransporte
                            String _permSCT = "";
                            String _numPermisoSCT = "";

                            //!CartaPorte - IdentificacionVehicular
                            String _configVehivular = "";
                            String _placaVM = "";
                            String _anioModelo = "";

                            //!CartaPorte - Seguros
                            String _aseguraRespCivil = "";
                            String _polizaRespCivil = "";

                            //!CartaPorte - Remolques
                            String _subTipoRem = "";
                            String _placaRemolque = "";


                            //!CartaPorte - FiguraTransporte
                            String _tipoFigura = "";
                            String _rfcFigura = "";
                            String _numLicenciaFigura = "";
                            String _nombreFigura = "";


                            //! Version del CFDI
                            if (_comprobante.HasAttribute("Version"))
                            {
                                _version = _comprobante.GetAttribute("Version").ToString();
                            }
                            //! Datos generales
                            _fechaEmision = _comprobante.HasAttribute("Fecha") ? DateTime.Parse(_comprobante.GetAttribute("Fecha").ToString()) : DateTime.Now;
                            _tipoCambio = _comprobante.HasAttribute("TipoCambio") ? Convert.ToDecimal(_comprobante.GetAttribute("TipoCambio").ToString()) : 1;
                            _serie = _comprobante.HasAttribute("Serie") ? _comprobante.GetAttribute("Serie").ToString() : "";
                            _folio = _comprobante.HasAttribute("Folio") ? _comprobante.GetAttribute("Folio").ToString() : "";
                            _lugarExpedicion = _comprobante.HasAttribute("LugarExpedicion") ? _comprobante.GetAttribute("LugarExpedicion").ToString() : "";
                            _exportacion = _comprobante.HasAttribute("Exportacion") ? _comprobante.GetAttribute("Exportacion").ToString() : "";
                            _moneda = _comprobante.HasAttribute("Moneda") ? _comprobante.GetAttribute("Moneda").ToString() : "";
                            _total_original = _comprobante.HasAttribute("Total") ? Convert.ToDecimal(_comprobante.GetAttribute("Total").ToString()) : 0;
                            _subtotal = _comprobante.HasAttribute("SubTotal") ? Convert.ToDecimal(_comprobante.GetAttribute("SubTotal").ToString()) : 0;
                            _tipoComprobante = _comprobante.HasAttribute("TipoDeComprobante") ? _comprobante.GetAttribute("TipoDeComprobante").ToString() : "";
                            _formaPago = _comprobante.HasAttribute("FormaPago") ? _comprobante.GetAttribute("FormaPago").ToString() : "";
                            _metodoPago = _comprobante.HasAttribute("MetodoPago") ? _comprobante.GetAttribute("MetodoPago").ToString() : "";
                            _descuento = _comprobante.HasAttribute("Descuento") ? Convert.ToDecimal(_comprobante.GetAttribute("Descuento").ToString()) : 0;
                            //version_timbrado = _comprobante.HasAttribute("Version") ? _comprobante.GetAttribute("Version").ToString() : "";
                            //!Nodo Principales
                            foreach (var Nodos in _comprobante.ChildNodes)
                            {
                                if (Nodos.GetType() == typeof(XmlElement))
                                {
                                    var _nodo = (Nodos as XmlElement);
                                    if (_nodo.LocalName == "Complemento")
                                    {
                                        //!Complementos
                                        foreach (var Complemento in _nodo.ChildNodes)
                                        {
                                            if (Complemento.GetType() == typeof(XmlElement))
                                            {
                                                var _complemento = (Complemento as XmlElement);
                                                if (_complemento.LocalName == "TimbreFiscalDigital")
                                                {
                                                    //!Timbre Fiscal Digital
                                                    _uuid = _complemento.HasAttribute("UUID") ? _complemento.GetAttribute("UUID").ToString() : "";
                                                    _fechaTimbrado = _complemento.HasAttribute("FechaTimbrado") ? DateTime.Parse(_complemento.GetAttribute("FechaTimbrado").ToString()) : DateTime.Now;
                                                    _versionTimbreFiscalDigital = _complemento.HasAttribute("Version") ? _complemento.GetAttribute("Version").ToString() : "";
                                                    _certificadoSAT = _complemento.HasAttribute("NoCertificadoSAT") ? _complemento.GetAttribute("NoCertificadoSAT").ToString() : "";
                                                    _selloDigital = _complemento.HasAttribute("SelloCFD") ? _complemento.GetAttribute("SelloCFD").ToString() : "";
                                                    _selloSAT = _complemento.HasAttribute("SelloSAT") ? _complemento.GetAttribute("SelloSAT").ToString() : "";

                                                }

                                                if (_complemento.LocalName == "CartaPorte")
                                                {
                                                    _transpInter = _complemento.HasAttribute("TranspInternac") ? _complemento.GetAttribute("TranspInternac").ToString() : "";

                                                    _totalDistancia = _complemento.HasAttribute("TotalDistRec") ? _complemento.GetAttribute("TotalDistRec").ToString() : "";

                                                    foreach (var CartaPorte in _complemento.ChildNodes)
                                                    {
                                                        if (CartaPorte.GetType() == typeof(XmlElement))
                                                        {
                                                            var _cartaPorte = (CartaPorte as XmlElement);



                                                            if (_cartaPorte.LocalName == "Ubicaciones")
                                                            {
                                                                foreach (var Ubicaciones in _cartaPorte.ChildNodes)
                                                                {
                                                                    if (Ubicaciones.GetType() == typeof(XmlElement))
                                                                    {
                                                                        var _ubicacion = (Ubicaciones as XmlElement);
                                                                        String tipo_Ubicacion = _ubicacion.HasAttribute("TipoUbicacion") ? _ubicacion.GetAttribute("TipoUbicacion").ToString() : "";
                                                                        if (tipo_Ubicacion == "Origen")
                                                                        {
                                                                            _idUbicacionOrigen = _ubicacion.HasAttribute("IDUbicacion") ? _ubicacion.GetAttribute("IDUbicacion").ToString() : "";
                                                                            _rfcRemDesOrigen = _ubicacion.HasAttribute("RFCRemitenteDestinatario") ? _ubicacion.GetAttribute("RFCRemitenteDestinatario").ToString() : "";
                                                                            _nombreRemDesOrigen = _ubicacion.HasAttribute("NombreRemitenteDestinatario") ? _ubicacion.GetAttribute("NombreRemitenteDestinatario").ToString() : "";
                                                                            _fechaHoraOrigen = _ubicacion.HasAttribute("FechaHoraSalidaLlegada") ? DateTime.Parse(_ubicacion.GetAttribute("FechaHoraSalidaLlegada").ToString()) : DateTime.Now;
                                                                            _distanciaRecorridaOrigen = _ubicacion.HasAttribute("DistanciaRecorrida") ? _ubicacion.GetAttribute("DistanciaRecorrida").ToString() : "";



                                                                            var _domicilio = _ubicacion.FirstChild;
                                                                            if (_domicilio.GetType() == typeof(XmlElement))
                                                                            {
                                                                                var _domi = (_domicilio as XmlElement);

                                                                                _calleOrigen = _domi.HasAttribute("Calle") ? _domi.GetAttribute("Calle").ToString() : "";
                                                                                _coloniaOrigen = _domi.HasAttribute("Colonia") ? _domi.GetAttribute("Colonia").ToString() : "";
                                                                                _localidadOrigen = _domi.HasAttribute("Localidad") ? _domi.GetAttribute("Localidad").ToString() : "";
                                                                                _municipioOrigen = _domi.HasAttribute("Municipio") ? _domi.GetAttribute("Municipio").ToString() : "";
                                                                                _estadoOrigen = _domi.HasAttribute("Estado") ? _domi.GetAttribute("Estado").ToString() : "";
                                                                                _paisOrigen = _domi.HasAttribute("Pais") ? _domi.GetAttribute("Pais").ToString() : "";
                                                                                _cpOrigen = _domi.HasAttribute("CodigoPostal") ? _domi.GetAttribute("CodigoPostal").ToString() : "";

                                                                                _direccionOrigen = _calleOrigen + ", " + _coloniaOrigen + ", " + _localidadOrigen + ", " + _municipioOrigen + ", " + _estadoOrigen + ", " + _paisOrigen + ", " + _cpOrigen;
                                                                            }


                                                                        }
                                                                        else if (tipo_Ubicacion == "Destino")
                                                                        {
                                                                            _idUbicacionDestino = _ubicacion.HasAttribute("IDUbicacion") ? _ubicacion.GetAttribute("IDUbicacion").ToString() : "";
                                                                            _rfcRemDesDestino = _ubicacion.HasAttribute("RFCRemitenteDestinatario") ? _ubicacion.GetAttribute("RFCRemitenteDestinatario").ToString() : "";
                                                                            _nombreRemDesDestino = _ubicacion.HasAttribute("NombreRemitenteDestinatario") ? _ubicacion.GetAttribute("NombreRemitenteDestinatario").ToString() : "";
                                                                            _fechaHoraDestino = _ubicacion.HasAttribute("FechaHoraSalidaLlegada") ? DateTime.Parse(_ubicacion.GetAttribute("FechaHoraSalidaLlegada").ToString()) : DateTime.Now;
                                                                            _distanciaRecorridaDestino = _ubicacion.HasAttribute("DistanciaRecorrida") ? _ubicacion.GetAttribute("DistanciaRecorrida").ToString() : "";



                                                                            var _domicilio = _ubicacion.FirstChild;
                                                                            if (_domicilio.GetType() == typeof(XmlElement))
                                                                            {
                                                                                var _domi = (_domicilio as XmlElement);

                                                                                _calleDestino = _domi.HasAttribute("Calle") ? _domi.GetAttribute("Calle").ToString() : "";
                                                                                _coloniaDestino = _domi.HasAttribute("Colonia") ? _domi.GetAttribute("Colonia").ToString() : "";
                                                                                _localidadDestino = _domi.HasAttribute("Localidad") ? _domi.GetAttribute("Localidad").ToString() : "";
                                                                                _municipioDestino = _domi.HasAttribute("Municipio") ? _domi.GetAttribute("Municipio").ToString() : "";
                                                                                _estadoDestino = _domi.HasAttribute("Estado") ? _domi.GetAttribute("Estado").ToString() : "";
                                                                                _paisDestino = _domi.HasAttribute("Pais") ? _domi.GetAttribute("Pais").ToString() : "";
                                                                                _cpDestino = _domi.HasAttribute("CodigoPostal") ? _domi.GetAttribute("CodigoPostal").ToString() : "";

                                                                                _direccionDestino = _calleDestino + ", " + _coloniaDestino + ", " + _localidadDestino + ", " + _municipioDestino + ", " + _estadoDestino + ", " + _paisDestino + ", " + _cpDestino;
                                                                            }
                                                                        }

                                                                    }
                                                                }

                                                            }
                                                            else if (_cartaPorte.LocalName == "Mercancias")
                                                            {
                                                                _pesoBrutoTotal = _cartaPorte.HasAttribute("PesoBrutoTotal") ? _cartaPorte.GetAttribute("PesoBrutoTotal").ToString() : "";
                                                                _unidadPeso = _cartaPorte.HasAttribute("UnidadPeso") ? _cartaPorte.GetAttribute("UnidadPeso").ToString() : "";
                                                                _numTotalMercancias = _cartaPorte.HasAttribute("NumTotalMercancias") ? _cartaPorte.GetAttribute("NumTotalMercancias").ToString() : "";



                                                                foreach (var Mercancias in _cartaPorte.ChildNodes)
                                                                {
                                                                    if (Mercancias.GetType() == typeof(XmlElement))
                                                                    {
                                                                        var _mercancias = (Mercancias as XmlElement);
                                                                        if (_mercancias.LocalName == "Mercancia")
                                                                        {
                                                                            _bienesTrans = _mercancias.HasAttribute("BienesTransp") ? _mercancias.GetAttribute("BienesTransp").ToString() : "";
                                                                            _descripcionMerca = _mercancias.HasAttribute("Descripcion") ? _mercancias.GetAttribute("Descripcion").ToString() : "";
                                                                            _cantidadMerca = _mercancias.HasAttribute("Cantidad") ? _mercancias.GetAttribute("Cantidad").ToString() : "";
                                                                            _claveUnidadMerca = _mercancias.HasAttribute("ClaveUnidad") ? _mercancias.GetAttribute("ClaveUnidad").ToString() : "";
                                                                            _unidadMerca = _mercancias.HasAttribute("Unidad") ? _mercancias.GetAttribute("Unidad").ToString() : "";
                                                                            _pesoKG = _mercancias.HasAttribute("PesoEnKg") ? _mercancias.GetAttribute("PesoEnKg").ToString() : "";



                                                                        }
                                                                        else if (_mercancias.LocalName == "Autotransporte")
                                                                        {
                                                                            _permSCT = _mercancias.HasAttribute("PermSCT") ? _mercancias.GetAttribute("PermSCT").ToString() : "";
                                                                            _numPermisoSCT = _mercancias.HasAttribute("NumPermisoSCT") ? _mercancias.GetAttribute("NumPermisoSCT").ToString() : "";


                                                                            foreach (var AutoTransportes in _mercancias.ChildNodes)
                                                                            {
                                                                                if (AutoTransportes.GetType() == typeof(XmlElement))
                                                                                {
                                                                                    var _autoTransporte = (AutoTransportes as XmlElement);
                                                                                    if (_autoTransporte.LocalName == "IdentificacionVehicular")
                                                                                    {
                                                                                        _configVehivular = _autoTransporte.HasAttribute("ConfigVehicular") ? _autoTransporte.GetAttribute("ConfigVehicular").ToString() : "";
                                                                                        _placaVM = _autoTransporte.HasAttribute("PlacaVM") ? _autoTransporte.GetAttribute("PlacaVM").ToString() : "";
                                                                                        _anioModelo = _autoTransporte.HasAttribute("AnioModeloVM") ? _autoTransporte.GetAttribute("AnioModeloVM").ToString() : "";


                                                                                    }
                                                                                    else if (_autoTransporte.LocalName == "Seguros")
                                                                                    {
                                                                                        _aseguraRespCivil = _autoTransporte.HasAttribute("AseguraRespCivil") ? _autoTransporte.GetAttribute("AseguraRespCivil").ToString() : "";
                                                                                        _polizaRespCivil = _autoTransporte.HasAttribute("PolizaRespCivil") ? _autoTransporte.GetAttribute("PolizaRespCivil").ToString() : "";


                                                                                    }
                                                                                    else if (_autoTransporte.LocalName == "Remolques")
                                                                                    {
                                                                                        foreach (var Remolques in _autoTransporte.ChildNodes)
                                                                                        {
                                                                                            if (Remolques.GetType() == typeof(XmlElement))
                                                                                            {
                                                                                                var _remolque = (Remolques as XmlElement);
                                                                                                if (_remolque.LocalName == "Remolque")
                                                                                                {
                                                                                                    _subTipoRem = _remolque.HasAttribute("SubTipoRem") ? _remolque.GetAttribute("SubTipoRem").ToString() : "";
                                                                                                    _placaRemolque = _remolque.HasAttribute("Placa") ? _remolque.GetAttribute("Placa").ToString() : "";


                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else if (_cartaPorte.LocalName == "FiguraTransporte")
                                                            {
                                                                foreach (var FiguraTrans in _cartaPorte.ChildNodes)
                                                                {
                                                                    if (FiguraTrans.GetType() == typeof(XmlElement))
                                                                    {
                                                                        var _tiposFigura = (FiguraTrans as XmlElement);
                                                                        if (_tiposFigura.LocalName == "TiposFigura")
                                                                        {
                                                                            _tipoFigura = _tiposFigura.HasAttribute("TipoFigura") ? _tiposFigura.GetAttribute("TipoFigura").ToString() : "";
                                                                            _rfcFigura = _tiposFigura.HasAttribute("RFCFigura") ? _tiposFigura.GetAttribute("RFCFigura").ToString() : "";
                                                                            _numLicenciaFigura = _tiposFigura.HasAttribute("NumLicencia") ? _tiposFigura.GetAttribute("NumLicencia").ToString() : "";
                                                                            _nombreFigura = _tiposFigura.HasAttribute("NombreFigura") ? _tiposFigura.GetAttribute("NombreFigura").ToString() : "";


                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (_nodo.LocalName == "Emisor")
                                    {
                                        //!Emisor
                                        _rfcEmisor = _nodo.HasAttribute("Rfc") ? _nodo.GetAttribute("Rfc").ToString() : "";
                                        _nombreEmisor = _nodo.HasAttribute("Nombre") ? _nodo.GetAttribute("Nombre").ToString() : "";
                                        _regimenFiscalEmisor = _nodo.HasAttribute("RegimenFiscal") ? _nodo.GetAttribute("RegimenFiscal").ToString() : "";
                                    }
                                    else if (_nodo.LocalName == "Receptor")
                                    {
                                        //!Receptor
                                        _rfcReceptor = _nodo.HasAttribute("Rfc") ? _nodo.GetAttribute("Rfc").ToString() : "";
                                        _nombreReceptor = _nodo.HasAttribute("Nombre") ? _nodo.GetAttribute("Nombre").ToString() : "";
                                        _usoCFDI = _nodo.HasAttribute("UsoCFDI") ? _nodo.GetAttribute("UsoCFDI").ToString() : "";
                                        _regimenFiscalReceptor = _nodo.HasAttribute("RegimenFiscalReceptor") ? _nodo.GetAttribute("RegimenFiscalReceptor").ToString() : "";
                                        _domicilioFiscalReceptor = _nodo.HasAttribute("DomicilioFiscalReceptor") ? _nodo.GetAttribute("DomicilioFiscalReceptor").ToString() : "";
                                    }

                                    else if (_nodo.LocalName == "Conceptos")
                                    {
                                        foreach (var Conceptos in _nodo.ChildNodes)
                                        {
                                            if (Conceptos.GetType() == typeof(XmlElement))
                                            {
                                                var _concepto = (Conceptos as XmlElement);
                                                if (_concepto.LocalName == "Concepto")
                                                {
                                                    tbd_Conceptos_Factura _conceptoFactura = new tbd_Conceptos_Factura();
                                                    _conceptoFactura.c_pord_serv = _concepto.HasAttribute("ClaveProdServ") ? _concepto.GetAttribute("ClaveProdServ").ToString() : "";
                                                    _conceptoFactura.cantidad = _concepto.HasAttribute("Cantidad") ? Convert.ToDecimal(_concepto.GetAttribute("Cantidad").ToString()) : 0;
                                                    _conceptoFactura.c_unidad = _concepto.HasAttribute("ClaveUnidad") ? _concepto.GetAttribute("ClaveUnidad").ToString() : "";
                                                    _conceptoFactura.unidad = _concepto.HasAttribute("Unidad") ? _concepto.GetAttribute("Unidad").ToString() : "";
                                                    _conceptoFactura.descripcion = _concepto.HasAttribute("Descripcion") ? _concepto.GetAttribute("Descripcion").ToString() : "";
                                                    _conceptoFactura.valor_unitario = _concepto.HasAttribute("ValorUnitario") ? Convert.ToDecimal(_concepto.GetAttribute("ValorUnitario").ToString()) : 0;
                                                    _conceptoFactura.importe = _concepto.HasAttribute("Importe") ? Convert.ToDecimal(_concepto.GetAttribute("Importe").ToString()) : 0;
                                                    _conceptoFactura.descuento = _concepto.HasAttribute("Descuento") ? Convert.ToDecimal(_concepto.GetAttribute("Descuento").ToString()) : 0;

                                                    conceptos.Add(_conceptoFactura);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            //Condicion                                
                            tbd_Facturas factura = db.tbd_Facturas.Where(s => s.uuid == _uuid).SingleOrDefault();
                            if (factura == null)
                            {
                                tbd_Facturas nuevaFactura = new tbd_Facturas();
                                nuevaFactura.version_cfdi = _version;
                                nuevaFactura.fecha_emision = _fechaEmision;
                                nuevaFactura.tipo_cambio = _tipoCambio;
                                nuevaFactura.serie = _serie;
                                nuevaFactura.folio = _folio;
                                nuevaFactura.lugar_expedicion = _lugarExpedicion;
                                nuevaFactura.exportacion = _exportacion;
                                nuevaFactura.moneda = _moneda;
                                nuevaFactura.total_original = _total_original;

                                tbc_Tipos_Comprobante tbc_Tipos_Comprobante = db.tbc_Tipos_Comprobante.Where(s => s.tipo_comprobante == _tipoComprobante).SingleOrDefault();
                                nuevaFactura.id_tipo_comprobante = tbc_Tipos_Comprobante != null ? tbc_Tipos_Comprobante.id_tipo_comprobante : 1;

                                tbc_Formas_Pago tbc_Formas_Pago = db.tbc_Formas_Pago.Where(s => s.clave == _formaPago).SingleOrDefault();
                                nuevaFactura.id_forma_pago = tbc_Formas_Pago != null ? tbc_Formas_Pago.id_forma_pago : 1;

                                tbc_Metodos_Pago tbc_Metodos_Pago = db.tbc_Metodos_Pago.Where(s => s.clave == _metodoPago).SingleOrDefault();
                                nuevaFactura.id_metodo_pago = tbc_Metodos_Pago != null ? tbc_Metodos_Pago.id_metodo_pago : 1;

                                nuevaFactura.uuid = _uuid;
                                nuevaFactura.fecha_timbrado = _fechaTimbrado;
                                nuevaFactura.certificado_sat = _certificadoSAT;
                                nuevaFactura.sello_cfdi = _selloDigital;
                                nuevaFactura.sello_sat = _selloSAT;

                                nuevaFactura.rfc_emisor = _rfcEmisor;
                                nuevaFactura.nombre_emisor = _nombreEmisor;

                                tbc_Regimenes tbc_Regimenes = db.tbc_Regimenes.Where(s => s.clave == _regimenFiscalEmisor).SingleOrDefault();
                                nuevaFactura.id_regimen_fiscal_emisor = tbc_Regimenes != null ? tbc_Regimenes.id_regimen_fiscal : 0;

                                nuevaFactura.rfc_receptor = _rfcReceptor;
                                nuevaFactura.nombre_receptor = _nombreReceptor;

                                tbc_Usos_CFDI tbc_Usos_CFDI = db.tbc_Usos_CFDI.Where(s => s.clave == _usoCFDI).Single();
                                nuevaFactura.id_uso_cfdi = tbc_Usos_CFDI != null ? tbc_Usos_CFDI.id_uso_cfdi : 13;

                                tbc_Regimenes tbc_Regimenes_Receptor = db.tbc_Regimenes.Where(s => s.clave == _regimenFiscalReceptor).SingleOrDefault();
                                nuevaFactura.id_regimen_fiscal_receptor = tbc_Regimenes_Receptor != null ? tbc_Regimenes_Receptor.id_regimen_fiscal : 0;

                                nuevaFactura.domicio_fiscal_receptor = _domicilioFiscalReceptor;

                                nuevaFactura.subtotal = _subtotal * _tipoCambio;
                                nuevaFactura.total = _total_original * _tipoCambio;
                                nuevaFactura.descuento = _descuento * _tipoCambio;

                                String DirectoryFecha = _fechaTimbrado.ToString("yyyyMMdd") + "\\";
                                //------------------------------ Buscar url -----------------------------
                                var url_xml_ = 0;
                                var r_xml = "";
                                var r_pdf = "";
                                var usuario_id = 0;
                                if (tipo != "Pago")
                                {
                                    url_xml_ = db.tbd_Cfdi_Uuid.Where(s => s.uuid == _uuid).Select(u => u.id_pre_factura).SingleOrDefault();
                                    if (url_xml_ == 0 && tipo != "FacturaNV")
                                    {
                                        url_xml_ = db.tbd_Pre_Pagos.Where(s => s.uuid == _uuid).Select(u => u.id_pre_factura).SingleOrDefault();
                                    }else if (tipo == "FacturaNV") {
                                        r_xml = "Plantillas/"+db.tbd_Notas_Venta.Where(s => s.id_nota_venta == id_prefac).Select(u => u.url_xml).SingleOrDefault();
                                        r_pdf = "Plantillas/" + db.tbd_Notas_Venta.Where(s => s.id_nota_venta == id_prefac).Select(u => u.url_pdf).SingleOrDefault();
                                        usuario_id = db.tbd_Notas_Venta.Where(s => s.id_nota_venta == id_prefac).Select(u => u.id_usuario).SingleOrDefault();
                                    }
                                    else {
                                        r_xml = db.tbd_Pre_Factura.Where(s => s.id_pre_factura == url_xml_).Select(u => u.url_xml).SingleOrDefault();

                                        if (r_xml == null)
                                        {
                                            r_xml = db.tbd_Pre_Pagos.Where(s => s.id_pre_factura == url_xml_).Select(u => u.url_xml).SingleOrDefault();
                                        }
                                    }
                                }else {
                                    r_xml = db.tbd_Pre_Pagos.Where(s => s.uuid == _uuid).Select(u => u.url_xml).SingleOrDefault();
                                }
                                
                                //----------------------------------------------------------------------------------------------------------------------------- 
                                String Url_Almacen = Server.MapPath("~") + r_xml;//variable.url_facturas + DirectoryFecha;
                                //if (!Directory.Exists(Url_Almacen))
                                    //Directory.CreateDirectory(Url_Almacen); //! Directorio Por Fecha (yyyyMMdd) dependiendo de la fecha de timbrado

                                String Url_XML = Url_Almacen;//+ _uuid + ".xml";

                                nuevaFactura.url_xml = (DirectoryFecha + _uuid + ".xml").ToUpper();
                                nuevaFactura.fecha_creacion = DateTime.Now;
                                nuevaFactura.id_estatus = 5; //Vigente
                                nuevaFactura.fecha_validacion = DateTime.Now;

                                if (nuevaFactura.rfc_emisor == rfc || nuevaFactura.rfc_receptor == rfc)
                                {
                                    try
                                    {
                                        db.tbd_Facturas.Add(nuevaFactura);
                                        //Add A PreFactura
                                        db.Configuration.LazyLoadingEnabled = false;
                                        if (tipo != "Pago")
                                        {
                                            if (tipo == "FacturaNV") {
                                                var prefac = db.tbd_Cobros.ToList<tbd_Cobros>().Where(u => u.id_cobro == id_prefac).FirstOrDefault();
                                                //Guardar a PreFactura
                                                tbd_Pre_Factura preFac = new tbd_Pre_Factura
                                                {
                                                    id_usuario = usuario_id,
                                                    rfc_usuario = nuevaFactura.rfc_emisor,
                                                    nombre_usuario_rfc = nuevaFactura.nombre_emisor,
                                                    uuid = nuevaFactura.uuid,
                                                    rfc_cliente = nuevaFactura.rfc_receptor,
                                                    nombre_rfc = nuevaFactura.nombre_receptor,
                                                    tipo = tipo,
                                                    exportacion = "01",
                                                    reg_fiscal_usuario = db.tbc_Regimenes.ToList<tbc_Regimenes>().Where(u => u.id_regimen_fiscal == nuevaFactura.id_regimen_fiscal_emisor).Select(u => u.clave).FirstOrDefault(),
                                                    serie = nuevaFactura.serie,
                                                    folio = nuevaFactura.folio,
                                                    tipo_comprobante = db.tbc_Tipos_Comprobante.ToList<tbc_Tipos_Comprobante>().Where(u => u.id_tipo_comprobante == nuevaFactura.id_tipo_comprobante).Select(u => u.tipo_comprobante).Single(),
                                                    lugar_expedicion = nuevaFactura.lugar_expedicion,
                                                    moneda = nuevaFactura.moneda,
                                                    forma_pago = nuevaFactura.id_forma_pago,//db.tbc_Formas_Pago.ToList<tbc_Formas_Pago>().Where(u => u.id_forma_pago == nuevaFactura.id_forma_pago).Select(u => u.forma_pago).Single(),
                                                    metodo_pago = nuevaFactura.id_metodo_pago,
                                                    tipo_cambio = Convert.ToString(nuevaFactura.tipo_cambio),
                                                    clave_reg_fiscal = db.tbc_Regimenes.ToList<tbc_Regimenes>().Where(u => u.id_regimen_fiscal == nuevaFactura.id_regimen_fiscal_receptor).Select(u => u.clave).FirstOrDefault(),
                                                    clave_uso_cfdi = nuevaFactura.id_uso_cfdi,
                                                    fecha_emision = DateTime.Now,
                                                    subtotal = nuevaFactura.subtotal.ToString(),
                                                    total = nuevaFactura.total_original.ToString(),
                                                    total_imp_ret = nuevaFactura.total.ToString(),
                                                    total_isr_ret = "0.0",
                                                    total_iva_ret = "0.0",
                                                    descuento2 = nuevaFactura.descuento.ToString(),
                                                    selloCFDI = nuevaFactura.sello_cfdi,
                                                    selloSAT = nuevaFactura.sello_sat,
                                                    ccertificacion = nuevaFactura.certificado_sat,
                                                    fca_timbrado = nuevaFactura.fecha_timbrado,
                                                    version_timbrado = _versionTimbreFiscalDigital,
                                                    url_pdf = r_pdf,
                                                    url_xml = r_xml,
                                                    status = 2
                                                };
                                                db.tbd_Pre_Factura.Add(preFac);
                                                db.SaveChanges();
                                            }
                                            else {
                                                var prefac = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == id_prefac).FirstOrDefault();
                                                prefac.selloSAT = nuevaFactura.sello_sat;
                                                prefac.ccertificacion = nuevaFactura.certificado_sat;
                                                prefac.version_timbrado = _versionTimbreFiscalDigital;
                                                prefac.uuid = nuevaFactura.uuid;
                                                prefac.selloCFDI = nuevaFactura.sello_cfdi;
                                                prefac.fca_timbrado = nuevaFactura.fecha_timbrado;
                                            }
                                        }
                                        else {
                                            var prefac = db.tbd_Pre_Pagos.ToList<tbd_Pre_Pagos>().Where(u => u.id == id_prefac).FirstOrDefault();
                                            prefac.selloSAT = nuevaFactura.sello_sat;
                                            prefac.ccertificacion = nuevaFactura.certificado_sat;
                                            prefac.version_timbrado = _versionTimbreFiscalDigital;
                                            prefac.uuid = nuevaFactura.uuid;
                                            prefac.selloCFDI = nuevaFactura.sello_cfdi;
                                            prefac.fca_timbrado = nuevaFactura.fecha_timbrado;
                                        }
                                        
                                        //--------------
                                        db.SaveChanges();

                                        foreach (var item in conceptos)
                                        {
                                            item.id_factura = nuevaFactura.id_factura;
                                        }

                                        db.tbd_Conceptos_Factura.AddRange(conceptos);
                                        db.SaveChanges();

                                        //if (!File.Exists(Url_XML))
                                        //{
                                        //    file.SaveAs(Url_XML);
                                        //}

                                        //!Carta porte
                                        if (_tipoComprobante == "T")
                                        {
                                            tbd_Carta_Porte carta = new tbd_Carta_Porte();
                                            carta.trans_inter = _transpInter;
                                            carta.total_dist_rec = Convert.ToDecimal(_totalDistancia);
                                            carta.id_origen = _idUbicacionOrigen;
                                            carta.rfc_origen = _rfcRemDesOrigen;
                                            carta.nombre_razon_origen = _nombreRemDesOrigen;
                                            carta.fecha_salida = _fechaHoraOrigen;
                                            carta.direccion_origen = _direccionOrigen;
                                            carta.id_destino = _idUbicacionDestino;
                                            carta.rfc_destino = _rfcRemDesDestino;
                                            carta.nombre_razon_destino = _nombreRemDesDestino;
                                            carta.distancia_recorrida = Convert.ToDecimal(_distanciaRecorridaDestino);
                                            carta.fecha_llegada = _fechaHoraDestino;
                                            carta.direccion_destino = _direccionDestino;
                                            carta.peso_bruto = Convert.ToDecimal(_pesoBrutoTotal);
                                            carta.unidad_peso = _unidadPeso;
                                            carta.num_mercancias = _numTotalMercancias;
                                            carta.bienes_trans = _bienesTrans;
                                            carta.descripcion = _descripcionMerca;
                                            carta.cantidad = Convert.ToDecimal(_cantidadMerca);
                                            carta.clave_unidad = _claveUnidadMerca;
                                            carta.unidad = _unidadMerca;
                                            carta.peso_kg = Convert.ToDecimal(_pesoKG);
                                            carta.permiso_sct = _permSCT;
                                            carta.num_permiso_sct = _numPermisoSCT;
                                            carta.config_vehicular = _configVehivular;
                                            carta.placa = _placaVM;
                                            carta.modelo = _anioModelo;
                                            carta.asegura_resp_civil = _aseguraRespCivil;
                                            carta.poliza_resp_civil = _polizaRespCivil;
                                            carta.sub_tipo_remolque = _subTipoRem;
                                            carta.placa_remolque = _placaRemolque;
                                            carta.tipo_figura = _tipoFigura;
                                            carta.rfc_figura = _rfcFigura;
                                            carta.num_licencia = _numLicenciaFigura;
                                            carta.nomnre_figura = _nombreFigura;
                                            carta.id_factura = nuevaFactura.id_factura;
                                            carta.url_pdf = "";
                                            db.tbd_Carta_Porte.Add(carta);
                                            db.SaveChanges();
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        tbd_Log_Errores error = new tbd_Log_Errores();
                                        error.fecha = DateTime.Now;
                                        error.funcion = "LecturaXML";
                                        error.mensaje = ex.Message; //+ "[" + file.FileName + "]";
                                        db.tbd_Log_Errores.Add(error);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
                //}
                //catch (Exception ex)
                //{
                //    tbd_Log_Errores error = new tbd_Log_Errores();
                //    error.fecha = DateTime.Now;
                //    error.funcion = "LecturaXML";
                //    error.mensaje = ex.Message;//+ "[" + file.FileName + "]";
                //    db.tbd_Log_Errores.Add(error);
                //    db.SaveChanges();
                //}
            }
        }

        public JsonResult AddCliente()
        {
            string mensaje = "";
            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var firma = db.tbd_Firmas.ToList<tbd_Firmas>().Where(u => u.rfc == usuario.rfc).Single();
            //----------------------------------------------------------------
            //modifiquen por su path
            string path = Server.MapPath("~");
            //Obtener numero certificado------------------------------------------------------------
            string DireccionCer = path + @"Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_cer_sello;
            string DireccionKey = path + @"Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_key_sello;
            string PasswordFinkok = "Programador1*";
            string PasswordCer = firma.password_sello;
            //-----------------------------------------------------------
            FabricaPEM(DireccionCer, DireccionKey, PasswordFinkok, PasswordCer, usuario.rfc);
            String cer;
            String key;

            //Para importar clase TextFieldParser, ingresas al menú Proyecto-- > Agregar Referencia-- > Ensamblados-- > Seleccionar Microsotf.VisualBasic-- > Aceptar
            using (TextFieldParser fileReader = new TextFieldParser(path + @"\Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_cer_sello + ".pem"))
                cer = fileReader.ReadToEnd();

            using (TextFieldParser fileReader = new TextFieldParser(path + @"\Plantillas\Firmas\" + usuario.rfc + "\\" + firma.url_key_sello + ".enc"))
                key = fileReader.ReadToEnd();
            //Instancia
            PanelProductivo.add can = new PanelProductivo.add();
            PanelProductivo.RegistrationSOAP SoapAdd = new PanelProductivo.RegistrationSOAP();
            PanelProductivo.addResponse respuesta = new PanelProductivo.addResponse();
            can.reseller_username = "cfdi@facturafast.mx";
            can.reseller_password = "F4ctur4f4st_C@st3l4n";
            can.taxpayer_id = "CAU020313Q94";//RFC EMISOR PARA AGREGAR TIMBRAR
            can.type_user = "P";
            //can.added = "";//OPCIONAL
            //can.cer = stringToBase64ByteArray(cer);
            //can.key = stringToBase64ByteArray(key);
            //can.passphrase = PasswordCer;//Contraseña llave privada
            //-------------------------------------------------------------------------------------------------------------------------
            //respuesta = SoapAdd.add(can);
            //mensaje = respuesta.addResult.message;
            //return Json(mensaje, JsonRequestBehavior.AllowGet);




            PanelProductivo.assign asign = new PanelProductivo.assign();

            PanelProductivo.assignResponse resp = new PanelProductivo.assignResponse();

            asign.credit = "10";
            asign.username = "cfdi@facturafast.mx";
            asign.password = "F4ctur4f4st_C@st3l4n";
            asign.taxpayer_id = "CAU020313Q94";

            resp = SoapAdd.assign(asign);
            mensaje = resp.assignResult.message;
            return Json(mensaje, JsonRequestBehavior.AllowGet);
        }
        //-----------------------------------------------------------------------
    }
}