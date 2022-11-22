using Facturafast.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Facturafast.Controllers
{
    public class ReportesController : Controller
    {
        BD_FFEntities db;
        public ActionResult Ingresos()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            DateTime Final = DateTime.Now;
            DateTime Inicio = new DateTime(Final.Year, Final.Month, 1);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var lista = db.tbd_Facturas.Where(s => s.rfc_emisor == usuario.rfc && s.fecha_timbrado >= Inicio && s.fecha_timbrado <= Final).ToList();
            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }
            return View(lista);
        }

        [HttpPost]
        public ActionResult Ingresos(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            DateTime Final = Convert.ToDateTime(formCollection["txtFechaFinal"]).AddDays(1).AddMinutes(-1);
            DateTime Inicio = Convert.ToDateTime(formCollection["txtFechaInicial"]);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var lista = db.tbd_Facturas.Where(s => s.rfc_emisor == usuario.rfc && s.fecha_timbrado >= Inicio && s.fecha_timbrado <= Final).ToList();
            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }
            return View(lista);
        }

        public ActionResult Gastos()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            DateTime Final = DateTime.Now;
            DateTime Inicio = new DateTime(Final.Year, Final.Month, 1);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var lista = db.tbd_Facturas.Where(s => s.rfc_receptor == usuario.rfc && s.fecha_timbrado >= Inicio && s.fecha_timbrado <= Final).ToList();
            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }
            return View(lista);
        }

        [HttpPost]
        public ActionResult Gastos(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            DateTime Final = Convert.ToDateTime(formCollection["txtFechaFinal"]).AddDays(1).AddMinutes(-1);
            DateTime Inicio = Convert.ToDateTime(formCollection["txtFechaInicial"]);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var lista = db.tbd_Facturas.Where(s => s.rfc_receptor == usuario.rfc && s.fecha_timbrado >= Inicio && s.fecha_timbrado <= Final).ToList();
            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }
            return View(lista);
        }


        public ActionResult CargaXML()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            return View();
        }

        [HttpPost]
        public String CargarArchivos(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return "{\"Estatus\":0, \"Mensaje\":\"Su sesión ha expirado.\"}";

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            foreach (string file in Request.Files)
            {
                if (Request.Files[file].ContentLength > 0)
                {
                    var File = Request.Files[file];

                    if (File.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        LecturaXML xml = new LecturaXML();
                        xml.LeerArchivo(File, usuario.rfc);
                    }
                }
            }
            return "{\"Estatus\":1, \"Mensaje\":\"Los datos se cargaron correctamente.\"}";
        }

        public ActionResult NotasVentas()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            DateTime Final = DateTime.Now;
            DateTime Inicio = new DateTime(Final.Year, Final.Month, 1);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            List<ClientesNota> clientes = new List<ClientesNota>();
            var lista = db.tbd_Notas_Venta.Where(s => s.id_usuario == usuario.id_usuario && s.fecha_creacion >= Inicio && s.fecha_creacion <= Final).ToList();
            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }
            else
            {
                foreach (var item in lista)
                {
                    if (clientes.Where(s => s.id_cliente == item.id_cliente).Count() == 0)
                    {
                        tbc_Clientes cli = db.tbc_Clientes.Where(s => s.id_cliente == item.id_cliente).Single();
                        clientes.Add(new ClientesNota
                        {
                            id_cliente = item.id_cliente,
                            saldo_acumulado = 0,
                            nombre_razon = cli.nombre_razon,
                            num_notas = 0,
                            rfc = cli.rfc,
                            saldo_pagado = 0,
                            saldo_restante = 0
                        });
                    }

                    var actualizar = clientes.Where(s => s.id_cliente == item.id_cliente).Single();
                    if (item.id_estatus == 1)
                    {
                        actualizar.saldo_acumulado += item.total;
                        actualizar.notas_pendientes++;
                    }
                    if (item.id_estatus == 6)
                    {
                        actualizar.notas_canceladas++;
                    }
                    if (item.id_estatus == 7)
                    {
                        actualizar.saldo_acumulado += item.total;
                        //actualizar.saldo_pagado += item.total;
                        actualizar.notas_pagas++;
                    }

                    var pagos = db.tbd_Pagos_Nota_Venta.Where(s => s.id_nota_venta == item.id_nota_venta && s.fecha_pago >= Inicio && s.fecha_pago <= Final).ToList();

                    foreach (var abonos in pagos)
                    {
                        actualizar.saldo_pagado += abonos.total_pagado;
                    }

                    actualizar.num_notas++;
                    actualizar.saldo_restante = actualizar.saldo_acumulado - actualizar.saldo_pagado;
                }
            }
            return View(clientes);
        }


        [HttpPost]
        public ActionResult NotasVentas(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            DateTime Final = Convert.ToDateTime(formCollection["txtFechaFinal"]).AddDays(1).AddMinutes(-1);
            DateTime Inicio = Convert.ToDateTime(formCollection["txtFechaInicial"]);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            List<ClientesNota> clientes = new List<ClientesNota>();
            var lista = db.tbd_Notas_Venta.Where(s => s.id_usuario == usuario.id_usuario && s.fecha_creacion >= Inicio && s.fecha_creacion <= Final).ToList();
            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }
            else
            {
                foreach (var item in lista)
                {
                    if (clientes.Where(s => s.id_cliente == item.id_cliente).Count() == 0)
                    {
                        tbc_Clientes cli = db.tbc_Clientes.Where(s => s.id_cliente == item.id_cliente).Single();
                        clientes.Add(new ClientesNota
                        {
                            id_cliente = item.id_cliente,
                            saldo_acumulado = 0,
                            nombre_razon = cli.nombre_razon,
                            num_notas = 0,
                            rfc = cli.rfc,
                            saldo_pagado = 0,
                            saldo_restante = 0
                        });
                    }

                    var actualizar = clientes.Where(s => s.id_cliente == item.id_cliente).Single();
                    if (item.id_estatus == 1)
                    {
                        actualizar.saldo_acumulado += item.total;
                        actualizar.notas_pendientes++;
                    }
                    if (item.id_estatus == 6)
                    {
                        actualizar.notas_canceladas++;
                    }
                    if (item.id_estatus == 7)
                    {
                        actualizar.saldo_acumulado += item.total;
                        //actualizar.saldo_pagado += item.total;
                        actualizar.notas_pagas++;
                    }

                    var pagos = db.tbd_Pagos_Nota_Venta.Where(s => s.id_nota_venta == item.id_nota_venta && s.fecha_pago >= Inicio && s.fecha_pago <= Final).ToList();

                    foreach (var abonos in pagos)
                    {
                        actualizar.saldo_pagado += abonos.total_pagado;
                    }

                    actualizar.num_notas++;
                    actualizar.saldo_restante = actualizar.saldo_acumulado - actualizar.saldo_pagado;
                }
            }
            return View(clientes);
        }

        public string obtenerHistoricoFechas(Int32? id, String fi, String ff)
        {
            if (Session["tbc_Usuarios"] == null)
                return "";

            if (id == null)
                return "";

            db = new BD_FFEntities();

            DateTime fechaI = Convert.ToDateTime(fi);
            DateTime fechaF = Convert.ToDateTime(ff).AddDays(1).AddSeconds(-1);

            var lista = db.tbd_Notas_Venta.Where(s => s.id_cliente == id && s.fecha_creacion >= fechaI && s.fecha_creacion <= fechaF && s.id_estatus != 6).OrderBy(s => s.fecha_creacion).ToList();
            Decimal total = 0;
            String list = "";
            foreach (var item in lista)
            {
                total += item.total;
                list += "<tr><td></td> <td>" + String.Join(", ", db.tbd_Conceptos_Nota_Venta.Where(s => s.id_nota_venta == item.id_nota_venta).Select(s => s.concepto)) + " (" + (item.serie + "-" + item.folio) + ")</td> <td>"+ item.fecha_creacion.ToString("yyyy/MM/dd HH:mm") +"</td> <td>" + item.total.ToString("c") + "</td> <td></td> <td>"+ total.ToString("c") +"</td></tr>";
                var pagos = db.tbd_Pagos_Nota_Venta.Where(s => s.id_nota_venta == item.id_nota_venta && s.id_estatus == 5).ToList();
                foreach (var p in pagos)
                {
                    total -= p.total_pagado;
                    list += "<tr class='table-success'><td></td> <td>" + ("Pago: " +p.tipo_pago) + " (" + (item.serie + "-" + item.folio) + ")</td> <td>" + p.fecha_pago.ToString("yyyy/MM/dd HH:mm") + "</td> <td></td> <td>" + p.total_pagado.ToString("c") + "</td> <td>" + total.ToString("c") + "</td></tr>";
                }
            }

            return list;
        }

        public string obtenerHistoricoActualizado(Int32? id)
        {
            if (Session["tbc_Usuarios"] == null)
                return "";

            if (id == null)
                return "";

            db = new BD_FFEntities();            

            var lista = db.tbd_Notas_Venta.Where(s => s.id_cliente == id && s.id_estatus == 1).OrderBy(s => s.fecha_creacion).ToList();
            Decimal total = 0;
            String list = "";
            foreach (var item in lista)
            {
                total += item.total;
                list += "<tr><td></td> <td>" + String.Join(", ", db.tbd_Conceptos_Nota_Venta.Where(s => s.id_nota_venta == item.id_nota_venta).Select(s => s.concepto)) + " (" + (item.serie + "-" + item.folio) + ")</td> <td>" + item.fecha_creacion.ToString("yyyy/MM/dd HH:mm") + "</td> <td>" + item.total.ToString("c") + "</td> <td></td> <td>" + total.ToString("c") + "</td></tr>";
                var pagos = db.tbd_Pagos_Nota_Venta.Where(s => s.id_nota_venta == item.id_nota_venta && s.id_estatus == 5).ToList();
                foreach (var p in pagos)
                {
                    total -= p.total_pagado;
                    list += "<tr class='table-success'><td></td> <td>" + ("Pago: " + p.tipo_pago) + " (" + (item.serie + "-" + item.folio) + ")</td> <td>" + p.fecha_pago.ToString("yyyy/MM/dd HH:mm") + "</td> <td></td> <td>" + p.total_pagado.ToString("c") + "</td> <td>" + total.ToString("c") + "</td></tr>";
                }
            }

            return list;
        }

        public ActionResult Nominas()
        {
            return View();
        }

        public ActionResult CartaPorte()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            DateTime Final = DateTime.Now;
            DateTime Inicio = new DateTime(Final.Year, Final.Month, 1);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var lista = db.tbd_Facturas.Where(s => s.rfc_emisor == usuario.rfc && s.fecha_timbrado >= Inicio && s.fecha_timbrado <= Final && s.id_tipo_comprobante == 5).ToList();
            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }
            return View(lista);
        }

        [HttpPost]
        public ActionResult CartaPorte(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            DateTime Final = Convert.ToDateTime(formCollection["txtFechaFinal"]).AddDays(1).AddMinutes(-1);
            DateTime Inicio = Convert.ToDateTime(formCollection["txtFechaInicial"]);
            int _tipo = Convert.ToInt32(formCollection["cmbTipo"]);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;

            if (_tipo == 1)
            {
                var lista = db.tbd_Facturas.Where(s => s.rfc_emisor == usuario.rfc && s.fecha_timbrado >= Inicio && s.fecha_timbrado <= Final && s.id_tipo_comprobante == 5).ToList();
                if (lista.Count == 0)
                {
                    ViewBag.Mensaje = "No se encontraron registros.";
                    ViewBag.TMensaje = "warning";
                }
                return View(lista);
            }
            else
            {
                var lista = db.tbd_Facturas.Where(s => s.rfc_receptor == usuario.rfc && s.fecha_timbrado >= Inicio && s.fecha_timbrado <= Final && s.id_tipo_comprobante == 5).ToList();
                if (lista.Count == 0)
                {
                    ViewBag.Mensaje = "No se encontraron registros.";
                    ViewBag.TMensaje = "warning";
                }
                return View(lista);
            }            
        }

        [HttpPost]
        public String AlmacenarCartaPorte(FormCollection formCollection)
        {
            Int32 _idFactura = Convert.ToInt32(formCollection["idFactura"]);
            db = new BD_FFEntities();
            tbc_Variables_Calculo variables = db.tbc_Variables_Calculo.Single();
            try
            {
                
                foreach (string file in Request.Files)
                {
                    if (file == "docx" && Request.Files[file].ContentLength > 0)
                    {
                        tbd_Carta_Porte carta = db.tbd_Carta_Porte.Where(s => s.id_factura == _idFactura).Single();
                        if (carta.url_pdf == "")
                        {
                            string nombre = Guid.NewGuid().ToString();
                            string nombreWord = nombre + ".DOCX";
                            //! Ruta completa
                            Request.Files[file].SaveAs((variables.url_docx + nombreWord));

                            //! Creamos PDF
                            var pdfProcess = new Process();
                            pdfProcess.StartInfo.FileName = variables.url_libreoffice;
                            pdfProcess.StartInfo.Arguments = "--headless --convert-to pdf \"" + (variables.url_docx + nombreWord) + "\" --outdir  \"" + variables.url_pdf + "\"";
                            pdfProcess.Start();

                            carta.url_pdf = nombre + ".PDF";
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                db = new BD_FFEntities();
                tbd_Log_Errores error = new tbd_Log_Errores
                {
                    fecha = DateTime.Now,
                    funcion = "AlmacenarCartaPorte - [Factura: " + _idFactura + "]",
                    mensaje = ex.Message
                };
                db.tbd_Log_Errores.Add(error);
                db.SaveChanges();
            }
            
            return "";
        }

        public ActionResult DescargarCartaPorte(Int32? idFactura)
        {
            db = new BD_FFEntities();
            tbc_Variables_Calculo variables = db.tbc_Variables_Calculo.Single();
            tbd_Facturas fact = db.tbd_Facturas.Where(s => s.id_factura == idFactura).Single();
            tbd_Carta_Porte porte = db.tbd_Carta_Porte.Where(s => s.id_factura == idFactura).Single();
            string fullPath = variables.url_pdf + "\\" + porte.url_pdf;
            int i = 0;
            while (i < 30)
            {
                Thread.Sleep(1000);
                if (System.IO.File.Exists(fullPath))
                    break;
            }
            return File(fullPath, "application/pdf", "CartaPorte " + fact.serie + "_" + fact.folio + ".PDF");
        }

    }
}