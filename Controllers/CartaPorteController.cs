using Facturafast.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Word = Microsoft.Office.Interop.Word;
using System.Web.Mvc;
using System.Diagnostics;
using Facturafast.CLS40;
using System.Xml;
using System.Text;
using System.Xml.Serialization;
using Microsoft.VisualBasic.FileIO;

namespace Facturafast.Controllers
{
    public class CartaPorteController : Controller
    {
        BD_FFEntities db;
        #region Ubicaciones
        public ActionResult Ubicaciones()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var lista = db.tbc_Ubicaciones.Where(s => s.rfc_usuario == usuario.rfc).ToList();
            return View(lista);
        }
        [HttpPost]
        public ActionResult GuardarUbicacion(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            int _idUbicacion = Convert.ToInt32(formCollection["txtIdUbicacion"]);
            string _nombreUbicacion = formCollection["txtNombreUbicacion"];
            string _identificador = formCollection["txtIdentificador"];
            string _rfc = formCollection["txtRFC"];
            string _pais = formCollection["txtPais"];
            string _cp = formCollection["txtCodigoPostal"];
            string _nombreRazon = formCollection["txtNombreRazon"];
            string _calle = formCollection["txtCalle"];
            string _numExt = formCollection["txtNumExt"];
            string _numInt = formCollection["txtNumInt"];
            string _colonia = formCollection["txtColonia"];
            string _localidad = formCollection["txtLocalidad"];
            string _municipio = formCollection["txtMunicipio"];
            string _estado = formCollection["txtEstado"];
            string _referencia = formCollection["txtReferencia"];
            int _idTipoUbicacion = Convert.ToInt32(formCollection["cmbTipoUbicacion"]);


            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();

            if (_idUbicacion == 0)
            {
                tbc_Ubicaciones nuevo = new tbc_Ubicaciones
                {
                    id_usuario = usuario.id_usuario,
                    rfc_origen_destino = _rfc.ToUpper(),
                    nombre_origen_destino = _nombreRazon.ToUpper(),
                    fecha_creacion = DateTime.Now,
                    rfc_usuario = usuario.rfc,
                    id_estatus = 1,
                    id_origen_destino = _identificador.ToUpper(),
                    calle = _calle,
                    codigo_postal = _cp,
                    colonia = _colonia,
                    estado = _estado,
                    id_tipo_ubicacion = _idTipoUbicacion,
                    localidad = _localidad,
                    municipio = _municipio,
                    nombre_ubicacion = _nombreUbicacion,
                    num_ext = _numExt,
                    num_int = _numInt,
                    pais = _pais,
                    referencia = _referencia
                };

                db.tbc_Ubicaciones.Add(nuevo);
                db.SaveChanges();

                TempData["Mensaje"] = "Los datos de la ubicación fueron almacenados correctamente.";
                TempData["TMensaje"] = "success";

            }
            else
            {
                tbc_Ubicaciones actualizar = db.tbc_Ubicaciones.Where(s => s.id_ubicacion == _idUbicacion).Single();
                actualizar.rfc_origen_destino = _rfc.ToUpper();
                actualizar.nombre_origen_destino = _nombreRazon.ToUpper();
                actualizar.id_origen_destino = _identificador.ToUpper();
                actualizar.calle = _calle;
                actualizar.codigo_postal = _cp;
                actualizar.colonia = _colonia;
                actualizar.estado = _estado;
                actualizar.id_tipo_ubicacion = _idTipoUbicacion;
                actualizar.localidad = _localidad;
                actualizar.municipio = _municipio;
                actualizar.nombre_ubicacion = _nombreUbicacion;
                actualizar.num_ext = _numExt;
                actualizar.num_int = _numInt;
                actualizar.pais = _pais;
                actualizar.referencia = _referencia;

                TempData["Mensaje"] = "Los datos de la ubicación fueron actualizados correctamente.";
                TempData["TMensaje"] = "success";

                db.SaveChanges();
            }

            return RedirectToAction("Ubicaciones", "CartaPorte");

        }
        #endregion
        #region Mercancías
        public ActionResult Mercancias()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            DateTime Final = DateTime.Now;
            DateTime Inicio = new DateTime(Final.Year, Final.Month, 1);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var lista = db.tbd_Mercancias.Where(s => s.id_usuario == usuario.id_usuario && s.fecha_creacion >= Inicio && s.fecha_creacion <= Final).ToList();

            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }


            //var lista = db.tbc_Ubicaciones.Where(s => s.rfc_usuario == usuario.rfc).ToList();
            return View(lista);
        }
        [HttpPost]
        public ActionResult Mercancias(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            DateTime Final = Convert.ToDateTime(formCollection["txtFechaFinal"]).AddDays(1).AddMinutes(-1);
            DateTime Inicio = Convert.ToDateTime(formCollection["txtFechaInicial"]);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var lista = db.tbd_Mercancias.Where(s => s.id_usuario == usuario.id_usuario && s.fecha_creacion >= Inicio && s.fecha_creacion <= Final).ToList();

            if (lista.Count == 0)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            return View(lista);
        }
        public ActionResult guardarMercancia(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            int _idMercancias = Convert.ToInt32(formCollection["txtIdMercancias"]);
            int _idsat = Convert.ToInt32(formCollection["txtIdBienesTransp"]);
            int _clavestcc = Convert.ToInt32(formCollection["txtIdClaveSTCC"]);
            string _descripcion = formCollection["txtDescripcion"];
            decimal _cantidad = Convert.ToDecimal(formCollection["txtCantidad"]);
            int _unidadmedida = Convert.ToInt32(formCollection["txtIdClaveUnidad"]);
            string _unidad = formCollection["txtUnidadP"];
            string dimensiones = formCollection["txtDimensiones"];
            string _materialpeligroso = formCollection["cmbMaterialPeligroso"];
            int _idmaterialpeligroso = Convert.ToInt32(formCollection["txtIdClaveMaterialPeligroso"]);
            int _idtipoembalaje = Convert.ToInt32(formCollection["txtIdEmbalaje"]);
            string _descripembalaje = formCollection["txtDescripcionEmbalaje"];
            decimal _pesokg = Convert.ToDecimal(formCollection["txtPesoKG"]);
            decimal _valormercancia = Convert.ToDecimal(formCollection["txtValorMercancia"]);
            int _idmoneda = Convert.ToInt32(formCollection["txtIdMoneda"]);
            int _idfraccionarancelaria = Convert.ToInt32(formCollection["txtIdFraccionArancelaria"]);
            string _uuid = formCollection["txtUUIDComercioExt"];
            int _idunidadpesom = Convert.ToInt32(formCollection["txtIdUnidadPesoMercancia"]);
            decimal _pesobruto = Convert.ToDecimal(formCollection["txtPesoBruto"]);
            decimal _pesoneto = Convert.ToDecimal(formCollection["txtPesoNeto"]);
            decimal _pesotara = Convert.ToDecimal(formCollection["txtPesoTara"]);
            int _numeropiezas = Convert.ToInt32(formCollection["txtNumeroPiezas"]);

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();

            if (_idMercancias == 0)
            {
                tbd_Mercancias nuevo = new tbd_Mercancias
                {
                    id_usuario = usuario.id_usuario,
                    id_sat = _idsat,
                    id_clave_stcc = _clavestcc,
                    descripcion = _descripcion,
                    cantidad = _cantidad,
                    id_unidad_medida = _unidadmedida,
                    unidad = _unidad,
                    dimensiones = dimensiones,
                    material_peligroso = _materialpeligroso,
                    id_material_peligroso = _idmaterialpeligroso,
                    id_tipo_embalaje = _idtipoembalaje,
                    descrip_embalaje = _descripembalaje,
                    peso_kg = _pesokg,
                    valor_mercancia = _valormercancia,
                    id_moneda = _idmoneda,
                    id_fraccion_arancelaria = _idfraccionarancelaria,
                    uuid_comercio_ext = _uuid,
                    id_unidad_peso_m = _idunidadpesom,
                    peso_bruto = _pesobruto,
                    peso_neto = _pesoneto,
                    peso_tara = _pesotara,
                    numero_piezas = _numeropiezas,
                    fecha_creacion = DateTime.Now
                };
                db.tbd_Mercancias.Add(nuevo);
                db.SaveChanges();

                TempData["Mensaje"] = "Los datos de la Mercancía fueron almacenados correctamente.";
                TempData["TMensaje"] = "success";
            }
            else
            {
                tbd_Mercancias actualizar = db.tbd_Mercancias.Where(s => s.id_mercancia == _idMercancias).Single();

                actualizar.id_sat = _idsat;
                actualizar.id_clave_stcc = _clavestcc;
                actualizar.descripcion = _descripcion;
                actualizar.cantidad = _cantidad;
                actualizar.id_unidad_medida = _unidadmedida;
                actualizar.unidad = _unidad;
                actualizar.dimensiones = dimensiones;
                actualizar.material_peligroso = _materialpeligroso;
                actualizar.id_material_peligroso = _idmaterialpeligroso;
                actualizar.id_tipo_embalaje = _idtipoembalaje;
                actualizar.descrip_embalaje = _descripembalaje;
                actualizar.peso_kg = _pesokg;
                actualizar.valor_mercancia = _valormercancia;
                actualizar.id_moneda = _idmoneda;
                actualizar.id_fraccion_arancelaria = _idfraccionarancelaria;
                actualizar.uuid_comercio_ext = _uuid;
                actualizar.id_unidad_peso_m = _idunidadpesom;
                actualizar.peso_bruto = _pesobruto;
                actualizar.peso_neto = _pesoneto;
                actualizar.peso_tara = _pesotara;
                actualizar.numero_piezas = _numeropiezas;

                TempData["Mensaje"] = "Los datos de la Mercancía fueron actualizados correctamente.";
                TempData["TMensaje"] = "success";

                db.SaveChanges();
            }
            return RedirectToAction("Mercancias", "CartaPorte");
        }
        #endregion
        #region Figuras
        public ActionResult Figuras()
        {
            //esta
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            //
            DateTime Final = DateTime.Now;
            DateTime Inicio = new DateTime(Final.Year, Final.Month, 1);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var lista = db.tbd_Figuras.Where(s => s.id_usuario == usuario.id_usuario && s.fecha_creacion >= Inicio && s.fecha_creacion <= Final).ToList();

            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }
            //


            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            return View(lista);
            //fin esta                                               
        }
        [HttpPost]
        public ActionResult Figuras(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");


            DateTime Final = Convert.ToDateTime(formCollection["txtFechaFinal"]).AddDays(1).AddMinutes(-1);
            DateTime Inicio = Convert.ToDateTime(formCollection["txtFechaInicial"]);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var lista = db.tbd_Figuras.Where(s => s.id_usuario == usuario.id_usuario && s.fecha_creacion >= Inicio && s.fecha_creacion <= Final).ToList();

            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            return View(lista);
        }
        public String guardarFiguras(List<tbd_Parte_Transporte_Figura> conceptos, Int32 txtIdFigura, Int32 cmbTipoFigura, String txtRFCFigura, String txtNumLicencia, String txtNombreFigura, String txtNumRegIdTribPropietario, Int32 txtIdDomicilioFiscal, String txtCalle, Int32 txtNumExt, Int32 txtNumInt,
            String txtColonia, Int32 txtIdLocalidad, String txtReferencia, String txtMunicipio, Int32 txtIdEstado, Int32 txtIdPais, Int32 txtCodigoPostal)
        {
            if (Session["tbc_Usuarios"] == null)
                return "{\"Estatus\":0, \"Mensaje\":\"Su sesión a caducado. Vuelva a iniciar sesión nuevamente.\"}";

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();

            if (txtIdFigura == 0)
            {
                //Int32 nextFolio = 0;
                //var maxFolio = db.tbd_Notas_Venta.Where(s => s.serie == txtSerie && s.rfc_usuario == usuario.rfc).OrderByDescending(s => s.fecha_creacion).FirstOrDefault();
                //if (maxFolio != null)
                //{
                //    nextFolio = Convert.ToInt32(maxFolio.folio);
                //}

                tbd_Figuras nueva = new tbd_Figuras
                {
                    id_usuario = usuario.id_usuario,
                    id_figura_transporte = cmbTipoFigura,
                    rfc_figura = txtRFCFigura,
                    num_licencia = txtNumLicencia,
                    nombre_figura = txtNombreFigura,
                    num_reg_id_trib_figura = txtNumRegIdTribPropietario,
                    residencia_fiscal_id_pais = txtIdDomicilioFiscal,
                    calle = txtCalle,
                    num_exterior = txtNumExt,
                    num_interiror = txtNumInt,
                    colonia = txtColonia,
                    id_localidad = txtIdLocalidad,
                    referencia = txtReferencia,
                    municipio = txtMunicipio,
                    id_estado = txtIdEstado,
                    id_pais = txtIdPais,
                    codigo_postal = txtCodigoPostal,
                    fecha_creacion = DateTime.Now

                };
                db.tbd_Figuras.Add(nueva);
                db.SaveChanges();

                foreach (var item in conceptos)
                {
                    tbd_Parte_Transporte_Figura nuevoConcepto = new tbd_Parte_Transporte_Figura
                    {
                        id_figura = nueva.id_figura,
                        id_parte_transporte = item.id_parte_transporte

                    };
                    db.tbd_Parte_Transporte_Figura.Add(nuevoConcepto);

                }
                db.SaveChanges();
                return "{\"Estatus\":1, \"Mensaje\":\"\"}";
            }

            return "{\"Estatus\":0, \"Mensaje\":\"Ocurrio un error al procesar su petición, inténtelo más tarde.\"}";

        }
        public String guardarFigurasST(Int32 txtIdFigura, Int32 cmbTipoFigura, String txtRFCFigura, String txtNumLicencia, String txtNombreFigura, String txtNumRegIdTribPropietario, Int32 txtIdDomicilioFiscal, String txtCalle, Int32 txtNumExt, Int32 txtNumInt,
                    String txtColonia, Int32 txtIdLocalidad, String txtReferencia, String txtMunicipio, Int32 txtIdEstado, Int32 txtIdPais, Int32 txtCodigoPostal)
        {
            if (Session["tbc_Usuarios"] == null)
                return "{\"Estatus\":0, \"Mensaje\":\"Su sesión a caducado. Vuelva a iniciar sesión nuevamente.\"}";

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();

            if (txtIdFigura == 0)
            {
                //Int32 nextFolio = 0;
                //var maxFolio = db.tbd_Notas_Venta.Where(s => s.serie == txtSerie && s.rfc_usuario == usuario.rfc).OrderByDescending(s => s.fecha_creacion).FirstOrDefault();
                //if (maxFolio != null)
                //{
                //    nextFolio = Convert.ToInt32(maxFolio.folio);
                //}

                tbd_Figuras nueva = new tbd_Figuras
                {
                    id_usuario = usuario.id_usuario,
                    id_figura_transporte = cmbTipoFigura,
                    rfc_figura = txtRFCFigura,
                    num_licencia = txtNumLicencia,
                    nombre_figura = txtNombreFigura,
                    num_reg_id_trib_figura = txtNumRegIdTribPropietario,
                    residencia_fiscal_id_pais = txtIdDomicilioFiscal,
                    calle = txtCalle,
                    num_exterior = txtNumExt,
                    num_interiror = txtNumInt,
                    colonia = txtColonia,
                    id_localidad = txtIdLocalidad,
                    referencia = txtReferencia,
                    municipio = txtMunicipio,
                    id_estado = txtIdEstado,
                    id_pais = txtIdPais,
                    codigo_postal = txtCodigoPostal,
                    fecha_creacion = DateTime.Now

                };
                db.tbd_Figuras.Add(nueva);
                db.SaveChanges();

                //foreach (var item in conceptos)
                //{
                //    tbd_Parte_Transporte_Figura nuevoConcepto = new tbd_Parte_Transporte_Figura
                //    {
                //        id_figura = nueva.id_figura,
                //        id_parte_transporte = item.id_parte_transporte

                //    };
                //    db.tbd_Parte_Transporte_Figura.Add(nuevoConcepto);

                //}
                //db.SaveChanges();
                return "{\"Estatus\":1, \"Mensaje\":\"\"}";
            }

            return "{\"Estatus\":0, \"Mensaje\":\"Ocurrio un error al procesar su petición, inténtelo más tarde.\"}";

        }
        #endregion
        #region FerroViarios
        public ActionResult ListFerroviario()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }
            DateTime Final = DateTime.Now;
            DateTime Inicio = new DateTime(Final.Year, Final.Month, 1);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            //var lista = db.tbc_Ferroviarios.Where(s => s.rfc_usuario == usuario.rfc).ToList();
            return View();
        }

        public ActionResult Ferroviario(Int32? id)
        {
            if (id == 0)
            {
                return View();
            }
            else
            {
                ViewBag.id = id;
                return View(id);
            }
        }

        public ActionResult getListFerroviario(string fecha_i, string fecha_f)
        {
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            DateTime f_inicial = Convert.ToDateTime(fecha_i);
            DateTime f_final = Convert.ToDateTime(fecha_f);
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var q = from f in db.tbc_Ferroviarios
                        join dpaso in db.tbc_Derecho_Paso on f.id_derecho equals dpaso.id_derecho_paso into dp
                        from derpaso in dp.DefaultIfEmpty()
                        where f.id_usuario == usuario.id_usuario && f.fecha_creacion >= f_inicial && f.fecha_creacion <= f_final
                        select new
                        {
                            id_ferroviario = f.id_ferroviario,
                            fecha_creacion = f.fecha_creacion.ToString(),
                            id_estatus = f.id_estatus,
                            id_derecho = derpaso.clave_derecho_paso,
                            tipo_carro = f.tipo_carro,
                            tipo_contenedor = f.tipo_contenedor,
                            km_pagado = f.km_pagado,
                            matriculada = f.matricula_carro,
                            guia = f.guia_carro,
                            peso_neta = f.peso_neto_mercancia,
                            peso_vacio = f.peso_contenedor_vacio
                        };

                return Json(q.ToList(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult getCatalogoF(int id)
        {
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var q = from f in db.tbc_Ferroviarios
                        where f.id_ferroviario == id
                        select new
                        {
                            id_ferroviario = f.id_ferroviario,
                            fecha_creacion = f.fecha_creacion.ToString(),
                            id_estatus = f.id_estatus,
                            id_derecho = f.id_derecho,
                            tipo_carro = f.tipo_carro,
                            tipo_contenedor = f.tipo_contenedor,
                            km_pagado = f.km_pagado,
                            matriculada = f.matricula_carro,
                            guia = f.guia_carro,
                            peso_neta = f.peso_neto_mercancia,
                            peso_vacio = f.peso_contenedor_vacio
                        };

                return Json(q.ToList(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GuardarFerroviario(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            int _idFerroviario = Convert.ToInt32(formCollection["txtIdFerroviario"]);
            int t_derecho_paso = Convert.ToInt32(formCollection["cmbTipoDerecho"]);
            decimal km_pagado = Convert.ToDecimal(formCollection["txtKmPagado"]);
            string tipoCarro = formCollection["cmbTipoCarro"];
            string matricula = formCollection["txtMatricula"];
            string guia = formCollection["txtGuia"];
            string toneladas = formCollection["txtToneladas"];
            string tipoContenedor = formCollection["cmbTipoContenedor"];
            int peso_vacio = Convert.ToInt32(formCollection["txtPesoVacio"]);
            int peso_neto = Convert.ToInt32(formCollection["txtPesoNeto"]);

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();

            if (_idFerroviario == 0)
            {
                tbc_Ferroviarios nuevo = new tbc_Ferroviarios
                {
                    id_usuario = usuario.id_usuario,
                    rfc_usuario = usuario.rfc.ToUpper(),
                    fecha_creacion = DateTime.Now,
                    id_estatus = 1,
                    id_derecho = t_derecho_paso,
                    km_pagado = km_pagado,
                    tipo_carro = tipoCarro,
                    matricula_carro = matricula,
                    guia_carro = guia,
                    tipo_contenedor = tipoContenedor,
                    peso_contenedor_vacio = peso_vacio,
                    peso_neto_mercancia = peso_neto
                };

                db.tbc_Ferroviarios.Add(nuevo);
                db.SaveChanges();

                TempData["Mensaje"] = "Los datos fueron almacenados correctamente.";
                TempData["TMensaje"] = "success";

            }
            else
            {
                tbc_Ferroviarios actualizar = db.tbc_Ferroviarios.Where(s => s.id_ferroviario == _idFerroviario).Single();
                actualizar.id_derecho = t_derecho_paso;
                actualizar.km_pagado = km_pagado;
                actualizar.tipo_carro = tipoCarro;
                actualizar.matricula_carro = matricula;
                actualizar.guia_carro = guia;
                actualizar.tipo_contenedor = tipoContenedor;
                actualizar.peso_contenedor_vacio = peso_vacio;
                actualizar.peso_neto_mercancia = peso_neto;

                TempData["Mensaje"] = "Los datos de la ubicación fueron actualizados correctamente.";
                TempData["TMensaje"] = "success";

                db.SaveChanges();
            }

            return RedirectToAction("ListFerroviario", "CartaPorte");

        }
        public ActionResult delFerroviario(Int32? id, byte tipo)
        {
            db = new BD_FFEntities();

            tbc_Ferroviarios actualizar = db.tbc_Ferroviarios.Where(s => s.id_ferroviario == id).Single();
            actualizar.id_estatus = tipo;
            db.SaveChanges();
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region CartaPorte
        public ActionResult ListCarPorte()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            DateTime Final = DateTime.Now;
            DateTime Inicio = new DateTime(Final.Year, Final.Month, 1);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;
            ViewBag.Rfc = usuario.rfc;
            ViewBag.cp = usuario.cp;
            var lista = db.tbc_Ubicaciones.Where(s => s.rfc_usuario == usuario.rfc).ToList();
            return View();
        }
        public ActionResult CartaPorte(Int32? id)
        {
            if (id == 0)
            {
                return View();
            }
            else
            {
                ViewBag.id = id;
                return View(id);
            }
        }
        public String obtenerUbica(String term)
        {
            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var ubicaciones = db.tbc_Ubicaciones.Where(s => ("[" + s.rfc_origen_destino + "] " + s.nombre_origen_destino).Contains(term) && s.rfc_usuario == usuario.rfc).ToList();
            if (ubicaciones.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in ubicaciones)
            {
                str += "{\"label\": \"[" + item.rfc_origen_destino + "] " + item.nombre_origen_destino + "\", \"value\":" + item.id_ubicacion + ", \"name\":\"" + item.nombre_origen_destino + "\", \"tipo\":\"" + item.id_tipo_ubicacion + "\", \"id_ubicacion\":\"" + item.id_origen_destino + "\"}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerMercancia(String term)
        {
            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var mercancias = db.tbd_Mercancias.Where(s => (s.descripcion).Contains(term)).ToList();
            if (mercancias.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in mercancias)
            {
                str += "{\"label\": \"[" + item.id_mercancia + "] " + item.descripcion + "\", \"value\":" + item.id_mercancia + ", \"name\":\"" + item.descripcion + "\"}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerFigura(String term)
        {
            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var figuras = db.tbd_Figuras.Where(s => ("[" + s.nombre_figura + "] " + s.rfc_figura).Contains(term) && s.id_usuario == usuario.id_usuario).ToList();
            if (figuras.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in figuras)
            {
                //str += "{\"label\": \"[" + item.rfc_figura + "] " + item.nombre_figura + "\", \"value\":" + item.id_figura + ", \"num_registro\":" + item.num_reg_id_trib_figura + ", \"name\":\"" + item.nombre_figura + "\"}, ";
                str += "{\"label\": \"[" + item.rfc_figura + "] " + item.nombre_figura+ "\", \"value\":" + item.id_figura + ", \"name\":\"" + item.nombre_figura + "\"}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public ActionResult getMercancia(int id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var q = from r in db.tbd_Mercancias
                        join um in db.tbc_Unidades_Medida on r.id_unidad_medida equals um.id_unidad_medida into umedida
                        from u_medida in umedida.DefaultIfEmpty()
                        where r.id_mercancia == id
                        select new
                        {
                            id_mercancia = r.id_mercancia,
                            peso_bruto = r.peso_bruto,
                            id_u_peso = r.id_unidad_peso_m,
                            u_medida = "[" + u_medida.clave + "]" + u_medida.descripcion,
                            peso_neto = r.peso_neto,
                            descripcion = r.descripcion,
                            t_mercancia = r.numero_piezas
                        };

                return Json(q.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getFigura(int id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var q = from r in db.tbd_Figuras
                        where r.id_figura == id
                        select new
                        {
                            id_figura = r.id_figura,
                            t_figura = r.id_figura_transporte,
                            n_licencia = r.num_licencia,
                            rfc_figura = r.rfc_figura,
                            n_figura = r.nombre_figura,
                            no_id_fiscal = r.num_reg_id_trib_figura
                        };
                return Json(q.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getListCarta(string fecha_i, string fecha_f) 
        {
            DateTime f_inicial = Convert.ToDateTime(fecha_i);
            DateTime f_final = Convert.ToDateTime(fecha_f);
            using (BD_FFEntities db = new BD_FFEntities())
            {
                tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
                var lis_card = from pcp in db.tbd_Pre_Carta_Porte
                                    join clie in db.tbc_Clientes on pcp.id_receptor equals clie.id_cliente into cl
                                    from cliente in cl.DefaultIfEmpty()
                                    join pref in db.tbd_Pre_Factura on pcp.id_prefactura equals pref.id_pre_factura into pf
                                    from prefac in pf.DefaultIfEmpty()
                                    where pcp.id_emisor == usuario.id_usuario && pcp.status != "0" && pcp.fecha_creacion >= f_inicial && pcp.fecha_creacion <= f_final
                                    select new
                                    {
                                        id = pcp.id,
                                        rfc_receptor = cliente.rfc,
                                        nombre_receptor = cliente.nombre_razon,
                                        total = prefac.total,
                                        tipo_comprobante = prefac.tipo_comprobante,
                                        fca_timbrado = pcp.fecha_creacion.ToString(),
                                        status = prefac.status
                                    };
                return Json(lis_card.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getCartaPorte(int id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var lis_card = from pcp in db.tbd_Pre_Carta_Porte
                               join clie in db.tbc_Clientes on pcp.id_receptor equals clie.id_cliente into cl
                               from cliente in cl.DefaultIfEmpty()
                               where pcp.id == id
                               select new
                               {
                                   id = pcp.id,
                                   id_pre_fac = pcp.id_prefactura,
                                   id_receptor = pcp.id_receptor,
                                   id_autotrans = pcp.id_autotransporte,
                                   rfc_receptor = cliente.rfc,
                                   nombre_receptor = cliente.nombre_razon,
                                   correo_receptor = cliente.correo,
                                   id_mercancia = pcp.id_mercancia,
                                   id_figura = pcp.id_figura,
                                   t_internacional = pcp.transporte_inter,
                                   e_s_mercancia = pcp.e_s_mercancia,
                                   pais_o = pcp.pais_ori_des,
                                   t_distancia_r = pcp.total_distancia_rec,
                                   tipo_figura = pcp.tipo_figura,
                                   res_fiscal_figura = pcp.res_fiscal_figura,
                                   status = pcp.status
                               };
                return Json(lis_card.First(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getUbicaciones(int id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var lis_ubi = from ubi in db.tbd_Ubicacion_Carta_Porte
                              join ub in db.tbc_Ubicaciones on ubi.id_ubicacion equals ub.id_ubicacion into ubica
                              from u in ubica.DefaultIfEmpty()
                              where ubi.id_pre_carta == id
                              select new
                              {
                                idubicacion = u.id_origen_destino,
                                id_ubicacion = ubi.id_ubicacion,
                                tipo_ubicacion = ubi.tipo_ubicacion, 
                                distancia_recorrida = ubi.distancia_recorrida,
                                fca_hora_salida = ubi.fca_hora_salida.ToString(),
                                id =  ubi.id,
                                num_estacion =  ubi.num_estacion
                              };
                return Json(lis_ubi.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getDerechosPaso()
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var lis_derecho = from dpaso in db.tbc_Derecho_Paso
                              where dpaso.status == 1
                              select new
                              {
                                  id_derecho = dpaso.id_derecho_paso,
                                  clave = dpaso.clave_derecho_paso,
                                  derecho = dpaso.derecho_paso,
                                  otorga_recibe = dpaso.otorga_recibe,
                                  concecionario = dpaso.concesionario
                              };
                return Json(lis_derecho.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult obtenerPermiso()
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var lis_permiso = from dpermiso in db.tbc_Tipos_Permiso
                                  select new
                                  {
                                      id_permiso = dpermiso.id_tipo_permiso,
                                      clave = dpermiso.clave,
                                      descripcion = dpermiso.descripcion
                                  };
                return Json(lis_permiso.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getConfVe()
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var lis_conf = from dconf in db.tbc_Config_AutoTransporte
                               where dconf.estatus == 1
                                  select new
                                  {
                                      id_permiso = dconf.id_conf_autotrans,
                                      clave = dconf.clave,
                                      descripcion = dconf.descripcion
                                  };
                return Json(lis_conf.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getRemolque()
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var lis_remolque = from dremo in db.tbc_Sub_Tipo_Rem
                               //where dremo.estatus == 1
                               select new
                               {
                                   id_permiso = dremo.id_remolque,
                                   clave = dremo.clave_remolque,
                                   remolque = dremo.remolque
                               };
                return Json(lis_remolque.ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult saveCartaPorte(List<PreFactura> prefactura, List<UbicacionCartaPorte> ubicacion, List<ConceptosPreFactura> concepto, List<PreCarta> precarta, List<AutoTransporte> autotrans, Int32? id)
        {
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            int id_pre_factura = 0;
            int id_pre_carta = 0;
            foreach (var item in prefactura)
            {
                string descuento2 = Regex.Replace(item.descuento2, ",", "");
                descuento2 = descuento2.Substring(1);
                DateTime fecha_emision = Convert.ToDateTime(item.fecha_emision);
                tbd_Pre_Factura nuevaPre = new tbd_Pre_Factura
                {
                    id_usuario = usuario.id_usuario,
                    rfc_usuario = usuario.rfc,
                    nombre_usuario_rfc = usuario.nombre_razon,
                    serie = item.serie,
                    folio = item.folio,
                    tipo_comprobante = item.tipo_comprobante,
                    exportacion = item.exportacion,
                    reg_fiscal_usuario = item.reg_fiscal_usuario,
                    rfc_cliente = item.nombre_rfc_pf,
                    nombre_rfc = item.rfc_cliente_pf,
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
                    subtotal = item.subtotal,
                    total_iva = item.total_iva,
                    total_iva_ret = item.total_iva_ret,
                    total_isr_ret = item.total_isr_ret,
                    descuento2 = descuento2 == "0.00" ? "" : descuento2,
                    total = item.total,
                    total_imp_ret = item.total,
                    status = 1,
                    tipo = "CartaPorte"
                };
                db.tbd_Pre_Factura.Add(nuevaPre);
                db.SaveChanges();
                id_pre_factura = nuevaPre.id_pre_factura;
            }

            if (concepto != null)
            {
                foreach (var item in concepto)
                {
                    tbd_Conceptos_Pre_Factura nuevoConceptoPF = new tbd_Conceptos_Pre_Factura
                    {
                        id_pre_factura = id_pre_factura,
                        c_prod_serv = item.c_prod_serv,
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
            foreach (var item in precarta)
            {
                DateTime fecha_hora_o = Convert.ToDateTime(item.fca_hora_o);
                DateTime fecha_hora_d = Convert.ToDateTime(item.fca_hora_d);
                tbd_Pre_Carta_Porte nuevaCarta = new tbd_Pre_Carta_Porte
                {
                    id_cfdi = item.id_cfdi,
                    id_emisor = usuario.id_usuario,
                    id_receptor = item.id_receptor,
                    id_prefactura = id_pre_factura,
                    id_mercancia = item.id_mercancia,
                    id_figura = item.id_figura,
                    id_autotransporte = item.id_autotransporte,
                    id = item.id_autotransporte,
                    transporte_inter = item.transporte_inter,
                    e_s_mercancia = item.e_s_mercancia,
                    pais_ori_des = item.pais_ori_des,
                    total_distancia_rec = item.total_distancia_rec,
                    status = "1",
                    tipo_figura = item.tipo_figura,
                    res_fiscal_figura = item.res_fiscal_figura,
                    fecha_creacion = DateTime.Now
                };
                db.tbd_Pre_Carta_Porte.Add(nuevaCarta);
                db.SaveChanges();
                id_pre_carta = nuevaCarta.id;
            }
            if (ubicacion != null)
            {
                foreach (var item in ubicacion)
                {
                    tbd_Ubicacion_Carta_Porte nuevaUbicacion = new tbd_Ubicacion_Carta_Porte
                    {
                        id_pre_carta = id_pre_carta,
                        id_ubicacion = item.idubicacion,
                        tipo_ubicacion = item.tipo_ubicacion,
                        num_estacion = item.num_estacion,
                        distancia_recorrida = item.distancia_recorrida,
                        fca_hora_salida = item.fca_hora_salida,
                        status = "1"
                    };
                    db.tbd_Ubicacion_Carta_Porte.Add(nuevaUbicacion);
                    db.SaveChanges();
                };
            }
            
            return Json(new { id = id_pre_carta, tipo = "Guardar" });
        }
        public ActionResult updateCartaPorte(List<PreFactura> prefactura, List<UbicacionCartaPorte> ubicacion, List<ConceptosPreFactura> concepto, List<PreCarta> precarta, List<AutoTransporte> autotrans, Int32? id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var id_carta_porte = 0;
                //Delete
                foreach (var conceptos in db.tbd_Conceptos_Pre_Factura.Where(x => x.id_pre_factura == id))
                {
                    db.tbd_Conceptos_Pre_Factura.Remove(conceptos);
                }
                //Edit Prefactura
                foreach (var prefac in prefactura)
                {
                    DateTime fecha_emision = Convert.ToDateTime(prefac.fecha_emision);
                    //----------------------------------------------
                    db.Configuration.LazyLoadingEnabled = false;
                    var valor = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == id).FirstOrDefault();
                    valor.serie = prefac.serie;
                    valor.folio = prefac.folio;
                    valor.tipo_comprobante = prefac.tipo_comprobante;
                    valor.exportacion = prefac.exportacion;
                    valor.reg_fiscal_usuario = prefac.reg_fiscal_usuario;
                    valor.rfc_cliente = prefac.rfc_cliente_pf;
                    valor.nombre_rfc = prefac.nombre_rfc_pf;
                    valor.uso_factura = prefac.uso_factura;
                    valor.clave_reg_fiscal = prefac.clave_reg_fiscal;
                    valor.clave_uso_cfdi = prefac.clave_uso_cfdi;
                    valor.lugar_expedicion = prefac.lugar_expedicion;
                    valor.tipo_factura = prefac.tipo_factura;
                    valor.forma_pago = prefac.forma_pago;
                    valor.metodo_pago = prefac.metodo_pago;
                    valor.numero_pedido = prefac.numero_pedido == null ? "" : prefac.numero_pedido;
                    valor.moneda = prefac.moneda;
                    valor.tipo_cambio = prefac.tipo_cambio == null ? "" : prefac.tipo_cambio;
                    valor.fecha_emision = fecha_emision;
                    valor.subtotal = prefac.subtotal;
                    valor.total_iva = prefac.total_iva;
                    valor.total_iva_ret = prefac.total_iva_ret;
                    valor.total_isr_ret = prefac.total_isr_ret;
                    valor.descuento2 = prefac.descuento2 == "0.00" ? "" : prefac.descuento2;
                    valor.total = prefac.total;
                    valor.total_imp_ret = prefac.total;
                }
                db.SaveChanges();
                //PreConceptos
                foreach (var item in concepto)
                {
                    tbd_Conceptos_Pre_Factura nuevoConceptoPF = new tbd_Conceptos_Pre_Factura
                    {
                        id_pre_factura = (int)id,
                        id_sat = item.id_sat,
                        c_prod_serv = item.c_prod_serv,
                        c_producto = item.c_producto,
                        cantidad = item.cantidad,
                        c_unidad_medida = item.c_unidad_medida,//item.c_unidad_medida,
                        unidad = item.unidad,
                        concepto = item.concepto,
                        importe_unitario = item.importe_unitario == null ? "0.00xl": item.importe_unitario,
                        importe_total = item.importe_total == null ? "0.00": item.importe_total,
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
                //Edit PreCarta
                foreach (var precar in precarta)
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    var valor = db.tbd_Pre_Carta_Porte.ToList<tbd_Pre_Carta_Porte>().Where(x => x.id_prefactura == id).FirstOrDefault();
                    id_carta_porte = valor.id;
                    valor.id_autotransporte = precar.id_autotransporte;
                    valor.id_receptor = precar.id_receptor;
                    valor.id_mercancia = precar.id_mercancia;
                    valor.id_figura = precar.id_figura;
                    valor.transporte_inter = precar.transporte_inter;
                    valor.e_s_mercancia = precar.e_s_mercancia;
                    valor.pais_ori_des = precar.pais_ori_des;
                    valor.tipo_figura = precar.tipo_figura;
                    valor.res_fiscal_figura = precar.res_fiscal_figura;
                }
                db.SaveChanges();
                //Edit Ubicaciones
                foreach (var ubicaciones in db.tbd_Ubicacion_Carta_Porte.Where(x => x.id_pre_carta == id_carta_porte))
                {
                    db.tbd_Ubicacion_Carta_Porte.Remove(ubicaciones);
                }
                foreach (var item in ubicacion)
                {
                    tbd_Ubicacion_Carta_Porte nuevaUbicacion = new tbd_Ubicacion_Carta_Porte
                    {
                        id_pre_carta = id_carta_porte,
                        id_ubicacion = item.idubicacion,
                        tipo_ubicacion = item.tipo_ubicacion,
                        num_estacion = item.num_estacion,
                        distancia_recorrida = item.distancia_recorrida,
                        fca_hora_salida = item.fca_hora_salida,
                        status = "1"
                    };
                    db.tbd_Ubicacion_Carta_Porte.Add(nuevaUbicacion);
                    db.SaveChanges();
                };
                return Json(new { id = id_carta_porte, tipo = "Actualizar" });
            }
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
                                    where p.id_pre_factura == id
                                    select new
                                    {
                                        id = p.id_pre_factura,
                                        rfc = p.rfc_cliente,
                                        serie = p.serie,
                                        id_receptor = f_cliente.id_cliente,
                                        correo = f_cliente.correo,
                                        uso_cfdi = p.clave_uso_cfdi,
                                        exportacion = p.exportacion,
                                        reg_fiscal_usuario = p.reg_fiscal_usuario,
                                        folio = p.folio,
                                        n_rfc = p.nombre_rfc,
                                        uso_factura = p.uso_factura,
                                        lugar_expedicion = p.lugar_expedicion,
                                        tipo_factura = p.tipo_factura,
                                        forma_pago = p.forma_pago,
                                        id_fpago = f_pag.id_forma_pago,
                                        metodo_pago = p.metodo_pago,
                                        n_pedido = p.numero_pedido,
                                        moneda = p.moneda,
                                        tipo_cambio = p.tipo_cambio,
                                        tipo_comprobante = p.tipo_comprobante,
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
        public ActionResult delCartaPorte(int id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                var valor = db.tbd_Pre_Carta_Porte.ToList<tbd_Pre_Carta_Porte>().Where(x => x.id == id).FirstOrDefault();
                valor.status = "0";
                db.SaveChanges();
                return Json("Ok Eliminado", JsonRequestBehavior.AllowGet);
            }   
        }
        //**************PREVIEW_CARTA_PORTE**********************************
        public ActionResult PreviewCartaPorte(Int32? id)
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
            var cartaporte = db.tbd_Pre_Carta_Porte.Where(s => s.id == id).Single();
            ViewBag.ID = cartaporte.id;
            ViewBag.ID_PREFAC = cartaporte.id_prefactura;
            ViewBag.ESTATUS = cartaporte.status;
            return View(cartaporte);
        }
        public JsonResult VisCartaPorte(Int32? id)
        {
            if (Session["tbc_Usuarios"] == null)
                return Json("Error", JsonRequestBehavior.AllowGet);
            CultureInfo ci = new CultureInfo("en-us");
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            //----------------------------------------------------------------------------------------------------------------------------
            db = new BD_FFEntities();
            tbd_Pre_Carta_Porte cartaporte_ = db.tbd_Pre_Carta_Porte.Where(s => s.id == id).Single();
            var id_pre_fac = cartaporte_.id_prefactura;
            var emisor = db.tbc_Usuarios.ToList<tbc_Usuarios>().Where(u => u.id_usuario == cartaporte_.id_emisor).Single();
            var dprefac = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == id_pre_fac).Single();
            var conceptos = db.tbd_Conceptos_Pre_Factura.ToList<tbd_Conceptos_Pre_Factura>().Where(u => u.id_pre_factura == id_pre_fac).ToList();
            var ubicaciones = db.tbd_Ubicacion_Carta_Porte.ToList<tbd_Ubicacion_Carta_Porte>().Where(u => u.id_pre_carta == id).ToList();
            tbd_Mercancias mercancias_ = db.tbd_Mercancias.Where(s => s.id_mercancia == cartaporte_.id_mercancia).Single();
            tbd_Figuras figuras_ = db.tbd_Figuras.Where(s => s.id_figura == cartaporte_.id_figura).Single();
            tbc_Clientes cliente = db.tbc_Clientes.Where(u => u.id_cliente == cartaporte_.id_receptor).Single();
            tbd_Autotransporte autotrans = db.tbd_Autotransporte.Where(u => u.id_autotransporte == id).Single();
            bool fileExist_ = false;
            //-----------------------------------------------------------------------------------------------------------------------------
            var ruta = db.tbc_Variables_Calculo.Where(s => s.id_variable == 1).ToList().First();
            var fca_pago = dprefac.fecha_emision.ToString();

            String[] fechaE = fca_pago.Split(' ');
            string aux_fc_emi = fechaE[0];
            String[] auxfechaE = aux_fc_emi.Split('/');
            string ax_fc_emi = auxfechaE[0] + auxfechaE[1] + auxfechaE[2];
            string DirPrg = Server.MapPath("~");
            string namefile = "";
            if (dprefac.url_pdf == null || dprefac.url_pdf == "")
            {
                namefile = RandomString(7) + "-" + RandomString(4) + "-" + RandomString(4) + "-" + RandomString(4) + "-" + RandomString(12);
            }
            else
            {
                string[] nom_doc = dprefac.url_pdf.Split('\\');
                string[] nd = nom_doc[5].Split('.');
                string nf = nd[0];
                namefile = nf;
            }
            string path = "Plantillas/CARTAPORTE/XML/PDF/" + cliente.rfc + "/" + ax_fc_emi + "/" + namefile + ".pdf";
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
                string auxpath = DirPrg + "Plantillas\\CARTAPORTE\\XML\\PDF\\" + cliente.rfc + "\\" + ax_fc_emi;
                DirectoryInfo di = Directory.CreateDirectory(auxpath);
                string auxpathdoc = DirPrg + "Plantillas\\CARTAPORTE\\XML\\DOCX\\" + cliente.rfc + "\\" + ax_fc_emi;
                DirectoryInfo didoc = Directory.CreateDirectory(auxpathdoc);
                string nombrearchivo = "";
                object ObjMiss = System.Reflection.Missing.Value;
                Word.Application ObjWord = new Word.Application();

                nombrearchivo = "CartaPorte.docx";

                Word.Document ObjDoc = ObjWord.Documents.Open(DirPrg + "/Plantillas/" + nombrearchivo, ObjMiss);

                //Definir Marcadores
                object nombre_emisor = "Nombre_Emisor";
                object rfc_emisor = "RFC_Emisor";
                object direccion = "Domicilio_";
                object regimen_fiscal = "Regimen_Fiscal";
                object lugar_expedicion = "Lugar_Expedicion";

                //object serie = "Serie";
                //object folio = "Folio";
                object fecha_timbrado = "Fecha_Timbrado";
                object fecha_emision = "Fecha_Emisión";
                object config_autotransporte = "Config_Autotransporte";

                object nombre_receptor = "Nombre_Receptor";
                object direccion_receptor = "Domicilio_Receptor";
                object rfc_receptor = "RFC_Receptor";
                object folio_fiscal = "Folio_Fiscal";
                object no_certificado = "Certificado_Digital";
                object certificado_sat = "Certificado_SAT";

                object m_pago = "Metodo_Pago";
                object f_pago = "Forma_Pago";
                object uso_cfdi = "Uso_CFDI";
                object t_comprobante = "Tipo_Comprobante";
                object moneda = "Moneda_";
                object t_cambio = "Tipo_Cambio";

                //object m_transporte = "Medio_Transporte";
                object t_internacional = "Transporte_Internacional";

                object permiso_stcc = "Permiso_STC";
                object num_permiso = "Num_Permiso_STC";
                object aseguradora = "Nombre_Aseguradora";
                object num_poliza = "Numero_Poliza";
                object placa = "Placa_";
                object modelo = "Anio_";

                object Tabla_Mercancia = "Agregar_Tabla_Mercancias";
                object Tabla_Ubicacion = "Agregar_Tabla_Remitentes";
                object Tabla_Figuras = "Agregar_Tabla_Operadores";
                object Tabla_Conceptos = "Agregar_Tabla";

                object cadena_original = "Cadena_Original";
                object sello_cfd = "Sello_CFDI";
                object sello_sat = "Sello_SAT";
                object cantidad_letra = "Importe_Letra";
                object subtotal = "Subtotal_";
                object descuento = "Descuento_";
                object total = "Total_";
                //Busqueda de marcadores en la plantilla
                Word.Range nombreemisor = ObjDoc.Bookmarks.get_Item(ref nombre_emisor).Range;
                Word.Range rfcemisor_ = ObjDoc.Bookmarks.get_Item(ref rfc_emisor).Range;
                Word.Range direccion_ = ObjDoc.Bookmarks.get_Item(ref direccion).Range;
                Word.Range regimenFiscal = ObjDoc.Bookmarks.get_Item(ref regimen_fiscal).Range;
                Word.Range lugarExpedicion = ObjDoc.Bookmarks.get_Item(ref lugar_expedicion).Range;

                //Word.Range serie_ = ObjDoc.Bookmarks.get_Item(ref serie).Range;
                //Word.Range folio_ = ObjDoc.Bookmarks.get_Item(ref folio).Range;
                Word.Range fechatimbrado = ObjDoc.Bookmarks.get_Item(ref fecha_timbrado).Range;
                Word.Range fechaemision = ObjDoc.Bookmarks.get_Item(ref fecha_emision).Range;
                Word.Range certificadosat = ObjDoc.Bookmarks.get_Item(ref certificado_sat).Range;
                Word.Range configautotransporte = ObjDoc.Bookmarks.get_Item(ref config_autotransporte).Range;

                Word.Range nombrereceptor = ObjDoc.Bookmarks.get_Item(ref nombre_receptor).Range;
                Word.Range direccionreceptor = ObjDoc.Bookmarks.get_Item(ref direccion_receptor).Range;
                Word.Range rfcreceptor = ObjDoc.Bookmarks.get_Item(ref rfc_receptor).Range;
                Word.Range folioFiscal = ObjDoc.Bookmarks.get_Item(ref folio_fiscal).Range;
                Word.Range noCertificado = ObjDoc.Bookmarks.get_Item(ref no_certificado).Range;

                Word.Range mPago = ObjDoc.Bookmarks.get_Item(ref m_pago).Range;
                Word.Range fPago = ObjDoc.Bookmarks.get_Item(ref f_pago).Range;
                Word.Range usoCFDI = ObjDoc.Bookmarks.get_Item(ref uso_cfdi).Range;
                Word.Range tComprobante = ObjDoc.Bookmarks.get_Item(ref t_comprobante).Range;
                Word.Range moneda_ = ObjDoc.Bookmarks.get_Item(ref moneda).Range;
                Word.Range tCambio = ObjDoc.Bookmarks.get_Item(ref t_cambio).Range;

                //Word.Range mTransporte = ObjDoc.Bookmarks.get_Item(ref m_transporte).Range;
                Word.Range tInternacional = ObjDoc.Bookmarks.get_Item(ref t_internacional).Range;

                Word.Range permisoStcc = ObjDoc.Bookmarks.get_Item(ref permiso_stcc).Range;
                Word.Range numpermiso = ObjDoc.Bookmarks.get_Item(ref num_permiso).Range;
                Word.Range aseguradora_ = ObjDoc.Bookmarks.get_Item(ref aseguradora).Range;
                Word.Range numpoliza = ObjDoc.Bookmarks.get_Item(ref num_poliza).Range;
                Word.Range placa_ = ObjDoc.Bookmarks.get_Item(ref placa).Range;
                Word.Range modelo_ = ObjDoc.Bookmarks.get_Item(ref modelo).Range;

                Word.Range TablaMercancia = ObjDoc.Bookmarks.get_Item(ref Tabla_Mercancia).Range;
                Word.Range TablaUbicacion = ObjDoc.Bookmarks.get_Item(ref Tabla_Ubicacion).Range;
                Word.Range TablaFiguras = ObjDoc.Bookmarks.get_Item(ref Tabla_Figuras).Range;
                Word.Range TablaConceptos = ObjDoc.Bookmarks.get_Item(ref Tabla_Conceptos).Range;

                Word.Range cadenaoriginal = ObjDoc.Bookmarks.get_Item(ref cadena_original).Range;
                Word.Range sellocfd = ObjDoc.Bookmarks.get_Item(ref sello_cfd).Range;
                Word.Range sellosat = ObjDoc.Bookmarks.get_Item(ref sello_sat).Range;
                Word.Range cantidadletra = ObjDoc.Bookmarks.get_Item(ref cantidad_letra).Range;
                Word.Range subtotal_ = ObjDoc.Bookmarks.get_Item(ref subtotal).Range;
                Word.Range descuento_ = ObjDoc.Bookmarks.get_Item(ref descuento).Range;
                Word.Range total_ = ObjDoc.Bookmarks.get_Item(ref total).Range;

                //Agregar texto al marcador
                nombreemisor.Text = emisor.nombre_razon;
                rfcemisor_.Text = emisor.rfc;
                direccion_.Text = emisor.calle+" "+ emisor.num_ext+" "+emisor.num_int;
                regimenFiscal.Text = db.tbc_Regimenes.Where(u => u.id_regimen_fiscal == cliente.id_regimen_fiscal).Select(u => u.regimen).First();
                lugarExpedicion.Text = emisor.cp;

                //serie_.Text = dprefac.serie;
                //folio_.Text = dprefac.folio;//db.tbc_Clientes.Where(u => u.rfc == prefactura_.rfc_usuario).Select(u => u.nombre_razon).First();
                fechatimbrado.Text = dprefac.fecha_emision.ToString();
                fechaemision.Text = dprefac.fecha_emision.ToString();
                certificadosat.Text = dprefac.ccertificacion;
                configautotransporte.Text = autotrans.id_conf_autotrans.ToString();

                nombrereceptor.Text = cliente.nombre_razon;
                direccionreceptor.Text = " ";
                rfcreceptor.Text = cliente.rfc;
                folioFiscal.Text = db.tbc_Regimenes.Where(u => u.id_regimen_fiscal == cliente.id_regimen_fiscal).Select(u => u.regimen).First();
                noCertificado.Text = dprefac.ccertificacion;

                var mpago = db.tbc_Metodos_Pago.ToList<tbc_Metodos_Pago>().Where(s => s.id_metodo_pago == dprefac.metodo_pago).Single();
                var fpago = db.tbc_Formas_Pago.ToList<tbc_Formas_Pago>().Where(s => s.id_forma_pago == dprefac.forma_pago).Single();

                mPago.Text = dprefac.metodo_pago +"-"+mpago.metodo_pago;
                fPago.Text = fpago.clave + "-" + fpago.forma_pago;
                usoCFDI.Text = dprefac.clave_uso_cfdi+"-"+ db.tbc_Usos_CFDI.ToList<tbc_Usos_CFDI>().Where(s => s.id_uso_cfdi == dprefac.clave_uso_cfdi).Select(u=>u.uso_cfdi).First();
                tComprobante.Text = dprefac.tipo_comprobante+"-"+dprefac.tipo_comprobante == "I" ? "Ingreso":"Traslado";
                moneda_.Text = dprefac.moneda;
                tCambio.Text = " ";//dprefac.tipo_cambio;

                //mTransporte.Text = cartaporte_.transporte_inter;
                tInternacional.Text = cartaporte_.transporte_inter;

                permisoStcc.Text = autotrans.id_tipo_permiso.ToString();
                numpermiso.Text = autotrans.num_permiso_sct;
                aseguradora_.Text = autotrans.asegura_resp_civil;
                numpoliza.Text = autotrans.poliza_resp_civil;
                placa_.Text = autotrans.placa_vm;
                modelo_.Text = autotrans.anio_modelo_vm.ToString();
                
                decimal totalEntero = Convert.ToDecimal(dprefac.total);
                cadenaoriginal.Text = dprefac.ccertificacion;
                total_.Text = totalEntero.ToString("C");
                cantidadletra.Text = totalEntero.NumeroALetras();
                sellocfd.Text = dprefac.selloCFDI;
                sellosat.Text = dprefac.selloSAT;
                subtotal_.Text = dprefac.subtotal;
                //descuento_.Text = totalEntero.NumeroALetras();

                //descuento_.InlineShapes.AddPicture(dprefac.url_xml, false, true, descuento_);
                //Creacion y definicion de tabla
                var cantMercancias = db.tbd_Mercancias.Where(s => s.id_mercancia == cartaporte_.id_mercancia).ToList();
                cantMercancias.Count();
                //-------------------------------------------------------------------------------------------------------------
                //Table Mercancia
                int i = 1;
                Word.Table TablaMer;
                TablaMer = ObjDoc.Tables.Add(TablaMercancia, cantMercancias.Count, 7);

                for (int z = 0; z <= cantMercancias.Count - 1; z++)
                {
                    TablaMer.Cell(i, 1).Range.Text = Convert.ToString(cantMercancias[z].cantidad.ToString("N2"));
                    TablaMer.Cell(i, 2).Range.Text = "0";//db.tbc_Unidades_Medida.Where(u => u.id_unidad_medida == cantMercancias[z].id_unidad_medida).Select(u => u.descripcion).First();
                    TablaMer.Cell(i, 3).Range.Text = cantMercancias[z].descripcion;
                    TablaMer.Cell(i, 4).Range.Text = Convert.ToString(cantMercancias[z].valor_mercancia);
                    TablaMer.Cell(i, 5).Range.Text = "";
                    TablaMer.Cell(i, 6).Range.Text = "";
                    TablaMer.Cell(i, 7).Range.Text = Convert.ToString(cantMercancias[z].peso_kg.ToString("N2"));
                    i++;
                }

                TablaMer.Columns[1].SetWidth(100, 0);
                TablaMer.Columns[2].SetWidth(60, 0);
                TablaMer.Columns[3].SetWidth(60, 0);
                TablaMer.Columns[4].SetWidth(80, 0);
                TablaMer.Columns[5].SetWidth(80, 0);
                TablaMer.Columns[6].SetWidth(180, 0);
                TablaMer.Columns[7].SetWidth(180, 0);
                TablaMer.Borders[Word.WdBorderType.wdBorderHorizontal].Visible = false;
                TablaMer.Borders[Word.WdBorderType.wdBorderVertical].Visible = false;
                //Fin creacion y definicion de tabla
                //Table Ubicación
                var cantUbicacion = db.tbd_Ubicacion_Carta_Porte.Where(s => s.id_pre_carta == cartaporte_.id).ToList();
                cantUbicacion.Count();
                int ub = 1;
                Word.Table TablaUb;
                TablaUb = ObjDoc.Tables.Add(TablaUbicacion, cantUbicacion.Count, 9);

                for (int z = 0; z <= cantUbicacion.Count - 1; z++)
                {
                    var ubicaciones_u = db.tbc_Ubicaciones.ToList<tbc_Ubicaciones>().Where(s => s.id_ubicacion == cantUbicacion[z].id_ubicacion).First();
                    TablaUb.Cell(ub, 1).Range.Text = Convert.ToString(cantUbicacion[z].tipo_ubicacion);
                    TablaUb.Cell(ub, 2).Range.Text = Convert.ToString(cantUbicacion[z].distancia_recorrida);
                    TablaUb.Cell(ub, 3).Range.Text = ubicaciones_u.rfc_usuario;
                    TablaUb.Cell(ub, 4).Range.Text = ubicaciones_u.nombre_ubicacion;
                    TablaUb.Cell(ub, 5).Range.Text = "MEX";
                    TablaUb.Cell(ub, 6).Range.Text = "";
                    TablaUb.Cell(ub, 7).Range.Text = ubicaciones_u.fecha_creacion.ToString();
                    TablaUb.Cell(ub, 8).Range.Text = Convert.ToString(cantUbicacion[z].num_estacion);
                    TablaUb.Cell(ub, 9).Range.Text = ubicaciones_u.calle + " " + ubicaciones_u.num_ext + "," + ubicaciones_u.colonia + " " + ubicaciones_u.codigo_postal + "," + ubicaciones_u.estado;
                    ub++;
                }

                TablaUb.Columns[1].SetWidth(40, 0);
                TablaUb.Columns[2].SetWidth(50, 0);
                TablaUb.Columns[3].SetWidth(80, 0);
                TablaUb.Columns[4].SetWidth(80, 0);
                TablaUb.Columns[5].SetWidth(50, 0);
                TablaUb.Columns[6].SetWidth(50, 0);
                TablaUb.Columns[7].SetWidth(100, 0);
                TablaUb.Columns[8].SetWidth(50, 0);
                TablaUb.Columns[9].SetWidth(100, 0);
                TablaUb.Borders[Word.WdBorderType.wdBorderHorizontal].Visible = false;
                TablaUb.Borders[Word.WdBorderType.wdBorderVertical].Visible = false;
                //Fin creacion y definicion de tabla
                //Table Figura
                var cantFigura = db.tbd_Figuras.Where(s => s.id_figura == cartaporte_.id_figura).ToList();
                cantFigura.Count();
                int f = 1;
                Word.Table TablaFig;
                TablaFig = ObjDoc.Tables.Add(TablaFiguras, cantFigura.Count, 6);

                for (int z = 0; z <= cantFigura.Count - 1; z++)
                {
                    TablaFig.Cell(f, 1).Range.Text = "Operador";
                    TablaFig.Cell(f, 2).Range.Text = Convert.ToString(cantFigura[z].rfc_figura);
                    TablaFig.Cell(f, 3).Range.Text = Convert.ToString(cantFigura[z].nombre_figura);
                    TablaFig.Cell(f, 4).Range.Text = Convert.ToString(cantFigura[z].num_licencia);
                    TablaFig.Cell(f, 5).Range.Text = "";
                    TablaFig.Cell(f, 6).Range.Text = "MEX";
                    f++;
                }

                TablaFig.Columns[1].SetWidth(90, 0);
                TablaFig.Columns[2].SetWidth(120, 0);
                TablaFig.Columns[3].SetWidth(160, 0);
                TablaFig.Columns[4].SetWidth(90, 0);
                TablaFig.Columns[5].SetWidth(80, 0);
                TablaFig.Columns[6].SetWidth(50, 0);
                TablaFig.Borders[Word.WdBorderType.wdBorderHorizontal].Visible = false;
                TablaFig.Borders[Word.WdBorderType.wdBorderVertical].Visible = false;
                //Fin creacion y definicion de tabla
                //Table Concepto
                var cantConcepto = db.tbd_Conceptos_Pre_Factura.Where(s => s.id_pre_factura == cartaporte_.id_prefactura).ToList();
                cantConcepto.Count();
                int c = 1;
                Word.Table TablaCon;
                TablaCon = ObjDoc.Tables.Add(TablaConceptos, cantConcepto.Count, 6);

                for (int z = 0; z <= cantConcepto.Count - 1; z++)
                {
                    decimal i_unitario = Convert.ToDecimal(cantConcepto[z].importe_unitario);
                    decimal ti_retenido = Convert.ToDecimal(cantConcepto[z].total_imp_retenido);
                    decimal i_total = Convert.ToDecimal(cantConcepto[z].importe_total);
                    
                    TablaCon.Cell(c, 1).Range.Text = Convert.ToString(cantConcepto[z].cantidad);
                    TablaCon.Cell(c, 2).Range.Text = Convert.ToString(cantConcepto[z].unidad);
                    TablaCon.Cell(c, 3).Range.Text = cantConcepto[z].concepto;
                    TablaCon.Cell(c, 4).Range.Text = i_unitario.ToString("C");
                    TablaCon.Cell(c, 5).Range.Text = ti_retenido.ToString("C");
                    TablaCon.Cell(c, 6).Range.Text = i_total.ToString("C");
                    c++;
                }

                TablaCon.Columns[1].SetWidth(100, 0);
                TablaCon.Columns[2].SetWidth(80, 0);
                TablaCon.Columns[3].SetWidth(60, 0);
                TablaCon.Columns[4].SetWidth(80, 0);
                TablaCon.Columns[5].SetWidth(80, 0);
                TablaCon.Columns[6].SetWidth(80, 0);
                TablaCon.Borders[Word.WdBorderType.wdBorderHorizontal].Visible = false;
                TablaCon.Borders[Word.WdBorderType.wdBorderVertical].Visible = false;
                
                //Cerrar word
                ObjDoc.SaveAs2(DirPrg + "/Plantillas/CARTAPORTE/XML/DOCX/" + cliente.rfc + "/" + ax_fc_emi + "/" + namefile + ".docx");
                ObjDoc.Close();
                ObjWord.Quit();

                //Crear PDF
                var pdfProcess = new Process();
                pdfProcess.StartInfo.FileName = "" + ruta.url_libreoffice;
                pdfProcess.StartInfo.Arguments = "--headless --convert-to pdf " + DirPrg + "Plantillas\\CARTAPORTE\\XML\\DOCX\\" + cliente.rfc + "\\" + ax_fc_emi + "\\" + namefile + ".docx --outdir  " + DirPrg + "Plantillas\\CARTAPORTE\\XML\\PDF\\" + cliente.rfc + "\\" + ax_fc_emi + "\\";
                pdfProcess.Start();
                fileExist_ = System.IO.File.Exists(DirPrg + "Plantillas\\" + dprefac.url_pdf);
                //Actualizar Rutas
                if (dprefac.status == 1)
                {
                    dprefac.url_pdf = "CARTAPORTE\\XML\\PDF\\" + cliente.rfc + "\\" + ax_fc_emi + "\\" + namefile + ".pdf";
                    dprefac.url_xml = "CARTAPORTE\\XML\\PDF\\" + cliente.rfc + "\\" + ax_fc_emi + "\\";
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
        public ActionResult TimbrarCarta(Int32? id_)
        {
            try
            {
                string DirPrg = Server.MapPath("~");
                string uuidFactura = Guid.NewGuid().ToString();
                using (BD_FFEntities db = new BD_FFEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    var cartaPorte = db.tbd_Pre_Carta_Porte.ToList<tbd_Pre_Carta_Porte>().Where(u => u.id == id_).FirstOrDefault();
                    var valor = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == cartaPorte.id_prefactura).FirstOrDefault();
                    var valorCFDI = db.tbd_Cfdi_Uuid.ToList<tbd_Cfdi_Uuid>().Where(u => u.id_pre_factura == cartaPorte.id_prefactura).ToList();
                    var valorConc = db.tbd_Conceptos_Pre_Factura.ToList<tbd_Conceptos_Pre_Factura>().Where(u => u.id_pre_factura == cartaPorte.id_prefactura).ToList();
                    var valorUbic = db.tbd_Ubicacion_Carta_Porte.ToList<tbd_Ubicacion_Carta_Porte>().Where(u => u.id_pre_carta == cartaPorte.id).ToList();
                    var valorMerc = db.tbd_Mercancias.ToList<tbd_Mercancias>().Where(u => u.id_mercancia == cartaPorte.id_mercancia).First();
                    //var vMercaInd = 0;//db.tbd_Conceptos_Mercancia.ToList<tbd_Conceptos_Mercancia>().Where(u => u.id_mercancias == valorMerc.id_mercancias).ToList();
                    var valorFigu = db.tbd_Figuras.ToList<tbd_Figuras>().Where(u => u.id_figura == cartaPorte.id_figura).FirstOrDefault();
                    var valorAuto = db.tbd_Autotransporte.ToList<tbd_Autotransporte>().Where(u => u.id_autotransporte == cartaPorte.id).FirstOrDefault();
                    var valorClie = db.tbc_Clientes.ToList<tbc_Clientes>().Where(u => u.id_cliente == cartaPorte.id_receptor).FirstOrDefault();
                    var valorUsua = db.tbc_Usuarios.ToList<tbc_Usuarios>().Where(u => u.id_usuario == cartaPorte.id_emisor).FirstOrDefault();
                    var tipo_comprobante = valor.tipo_comprobante;
                    
                    string ruta_aux = @"PREPAGO\XML\PDF\"+valor.rfc_cliente+"\\";
                    string[] rutaa_ = cartaPorte.url_pdf != null ? ruta_aux.Split('\\') : cartaPorte.url_pdf.Split('\\');
                    string ruta_ = @"Plantillas\" + rutaa_[0] + @"\" + rutaa_[1] + @"\" + rutaa_[2] + @"\" + rutaa_[3] + @"\";
                    //----------------------------------------------------------------------------------------------------------------------------------
                    var CFDI = new TCFDI_CP(DirPrg, @"CSD_Pruebas_CFDI_EKU9003173C9.cer", @"CSD_Pruebas_CFDI_EKU9003173C9.key", "12345678a")
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
                    var t_com = Convert.ToInt32(valor.tipo_cambio);
                    CFDI.aComprobante[0] = "4.0";                       // Versión del estandar CFDI
                    CFDI.aComprobante[1] = valor.serie.ToUpper();       // Serie
                    CFDI.aComprobante[2] = valor.folio;                 // Folio

                    CFDI.aComprobante[3] = string.Format("{0:yyyy-MM-ddTHH:mm:ss}", DateTime.Now.AddMinutes(-2), CultureInfo.CreateSpecificCulture("es-MX"));

                    CFDI.aComprobante[4] = "";//valor.forma_pago.ToString();      // Forma de Pago (Catálogo: c_FormaPago)
                    CFDI.aComprobante[5] = "";//valor.cond_pago;// Condiciones de pago
                    CFDI.aComprobante[6] = "0";//valor.subtotal;                   // SubTotal
                    CFDI.aComprobante[7] = "";// valor.descuento2;                 // Descuento
                    CFDI.aComprobante[8] = "XXX";//valor.moneda;                     // Moneda (Catálogo: c_Moneda)
                    CFDI.aComprobante[9] = "";//t_com.ToString();                 // Tipo de Cambio (Es requerido cuando la clave de moneda es distinta de MXN y de XXX)
                    CFDI.aComprobante[10] = "0";// valor.total;                     // TOTAL
                    CFDI.aComprobante[11] = "T";//valor.tipo_comprobante;          // Tipo de Comprobante (Catálogo: c_TipoDeComprobante)
                    CFDI.aComprobante[12] = "";//valor.metodo_pago.ToString();     // Método de Pago (Catálogo: c_MetodoPago)
                    CFDI.aComprobante[13] = "11000";// valor.lugar_expedicion;     // Lugar de expedición (Catálogo: c_CodigoPostal)
                    CFDI.aComprobante[14] = "";          // Confirmación 
                    // Datos del CFDI Relacionado
                    CFDI.cCfdiTipoRelacion = "";                              // Tipo de Relacion (Catálogo: c_TipoRelacion)

                    if (valorCFDI != null)//if (!empty(CFDI.cCfdiTipoRelacion))
                    {
                        // En caso de que TipoRelacion tenga un valor deberás especificar los UUID relacionados.
                        CFDI.aCfdiRelacionado[0] = "";                        // Folio fiscal (UUID) de un CFDI relacionado.
                        CFDI.aCfdiRelacionado[1] = "";                        // Folio fiscal (UUID) (puedes agregar varios folios)

                    }
                    // Datos del Emisor
                    CFDI.aEmisor[0] = "EKU9003173C9";                         //valor.rfc_usuario// RFC del emisor
                    CFDI.aEmisor[1] = "La Empresa Inválida, S.A. de C.V.";    //valor.nombre_usuario_rfc// Nombre del emisor
                    CFDI.aEmisor[2] = "601";                                  //valor.reg_fiscal_usuario// Régimen Fiscal (Catálogo: c_RegimenFiscal)
                    
                    // Datos del Receptor
                    CFDI.aReceptor[0] = "EKU9003173C9";//valor.rfc_cliente  // RFC
                    CFDI.aReceptor[1] = "Nombre o Razon Social del Cliente";//nombre_rfc  // Nombre Cliente
                    CFDI.aReceptor[2] = "";                                 //ResidenciaFiscal (Requerido cuando se trate de un extranjero)
                    CFDI.aReceptor[3] = "";//valor.clave_reg_fiscal;        //NumRegIdTrib (Es requerido cuando se incluya el complemento de comercio exterior)
                    CFDI.aReceptor[4] = "P01";// valor.clave_uso_cfdi;      //UsoCFDI (Catálogo: c_UsoCFDI)
                    
                    //Conceptos
                    // Arreglo: aConceptos( Número de concepto, Atributo ) | Hasta 1,000 Conceptos.
                    Decimal total_trasladado = 0;
                    Decimal total_retenido = 0;

                    Decimal total_iva_ret = 0;
                    Decimal total_isr_ret = 0;

                    Decimal base_iva = 0;

                    //for (int i = 0; i < valorConc.Count; i++)
                    //{
                    //    CFDI.aConceptos[i, 0] = "46182201";//valorConc[i].c_prod_serv;// Clave Producto Servicio (Catálogo: c_ClaveProdServ)
                    //    CFDI.aConceptos[i, 1] = "055-03-08-068";// valorConc[i].c_producto;  // Clave o código del producto (NoIdentificacion)
                    //    CFDI.aConceptos[i, 2] = valorConc[i].cantidad;        // Cantidad
                    //    CFDI.aConceptos[i, 3] = valorConc[i].c_unidad_medida;// Clave Unidad de medida (Catálogo: c_ClaveUnidad)
                    //    CFDI.aConceptos[i, 4] = valorConc[i].unidad;    // Unidad de medida
                    //    CFDI.aConceptos[i, 5] = valorConc[i].concepto;     // Descripción del producto
                    //    CFDI.aConceptos[i, 6] = "0.00";//valorConc[i].importe_unitario;// Importe unitario
                    //    CFDI.aConceptos[i, 7] = "0.00";//valorConc[i].importe_total;// Importe Total
                    //    CFDI.aConceptos[i, 8] = "";//valorConc[i].descuento;              // Descuento
                    //    //Si es traslado no llevara Impuestos
                    //    if (tipo_comprobante == "I")
                    //    {
                    //        // Trasladado
                    //        if (valorConc[i].iva_tasa != "0.000000")
                    //        //if (true)
                    //        {
                    //            CFDI.aConceptosTraslado[i, 0, 0] = valorConc[i].importe_total;      // Base para el cálculo del impuesto
                    //            CFDI.aConceptosTraslado[i, 0, 1] = valorConc[i].iva_imp_traslado;   // Clave impuesto trasladado (Catálogo: c_Impuesto)
                    //            CFDI.aConceptosTraslado[i, 0, 2] = valorConc[i].tipo_factor;        // Clave tipo de factor (Catálogo: c_TipoFactor)
                    //            CFDI.aConceptosTraslado[i, 0, 3] = valorConc[i].iva_tasa;           // Tasa o cuota del impuesto
                    //            CFDI.aConceptosTraslado[i, 0, 4] = valorConc[i].iva_tasa_impuesto;  // Importe del impuesto

                    //            total_trasladado += Convert.ToDecimal(valorConc[i].iva_tasa_impuesto);
                    //            base_iva += Convert.ToDecimal(valorConc[i].importe_total);
                    //        }
                    //        //Retenido
                    //        if (valorConc[i].isr_ret_tasa != "0.000000")
                    //        {
                    //            CFDI.aConceptosRetencion[i, 0, 0] = valorConc[i].importe_total;    // Base para el cálculo del impuesto
                    //            CFDI.aConceptosRetencion[i, 0, 1] = valorConc[i].isr_ret;          // Clave impuesto trasladado (Catálogo: c_Impuesto)
                    //            CFDI.aConceptosRetencion[i, 0, 2] = valorConc[i].tipo_factor;      // Clave tipo de factor (Catálogo: c_TipoFactor)
                    //            CFDI.aConceptosRetencion[i, 0, 3] = valorConc[i].isr_ret_tasa;     // Tasa o cuota del impuesto
                    //            CFDI.aConceptosRetencion[i, 0, 4] = valorConc[i].isr_ret_impuesto; // Importe del impuesto

                    //            total_retenido += Convert.ToDecimal(valorConc[i].isr_ret_impuesto);
                    //            total_isr_ret += Convert.ToDecimal(valorConc[i].isr_ret_impuesto);
                    //        }

                    //        if (valorConc[i].iva_ret_tasa != "0.000000")
                    //        {
                    //            CFDI.aConceptosRetencion[i, 1, 0] = valorConc[i].importe_total;     // Base para el cálculo del impuesto
                    //            CFDI.aConceptosRetencion[i, 1, 1] = valorConc[i].iva_ret;           // Clave impuesto trasladado (Catálogo: c_Impuesto)
                    //            CFDI.aConceptosRetencion[i, 1, 2] = valorConc[i].tipo_factor;       // Clave tipo de factor (Catálogo: c_TipoFactor)
                    //            CFDI.aConceptosRetencion[i, 1, 3] = valorConc[i].iva_ret_tasa;      // Tasa o cuota del impuesto
                    //            CFDI.aConceptosRetencion[i, 1, 4] = valorConc[i].iva_ret_impuesto;  //Importe del Impuesto

                    //            total_retenido += Convert.ToDecimal(valorConc[i].iva_ret_impuesto);
                    //            total_iva_ret += Convert.ToDecimal(valorConc[i].iva_ret_impuesto);
                    //        }
                    //    }
                    //}
                    CFDI.aConceptos[0, 0] = "46182201";//valorConc[i].c_prod_serv;// Clave Producto Servicio (Catálogo: c_ClaveProdServ)
                    CFDI.aConceptos[0, 1] = "055-03-08-068";// valorConc[i].c_producto;  // Clave o código del producto (NoIdentificacion)
                    CFDI.aConceptos[0, 2] = "1";// valorConc[i].cantidad;        // Cantidad
                    CFDI.aConceptos[0, 3] = "H87";// valorConc[i].c_unidad_medida;// Clave Unidad de medida (Catálogo: c_ClaveUnidad)
                    CFDI.aConceptos[0, 4] = "PZA";// valorConc[i].unidad;    // Unidad de medida
                    CFDI.aConceptos[0, 5] = "FAJA ELASTICA INDUSTRIAL GRANDE";// valorConc[i].concepto;     // Descripción del producto
                    CFDI.aConceptos[0, 6] = "0.00";//valorConc[i].importe_unitario;// Importe unitario
                    CFDI.aConceptos[0, 7] = "0.00";//valorConc[i].importe_total;// Importe Total
                    CFDI.aConceptos[0, 8] = "";//valorConc[i].descuento;              // Descuento

                    CFDI.aConceptos[1, 0] = "46182201";//valorConc[i].c_prod_serv;// Clave Producto Servicio (Catálogo: c_ClaveProdServ)
                    CFDI.aConceptos[1, 1] = "055-03-08-067";// valorConc[i].c_producto;  // Clave o código del producto (NoIdentificacion)
                    CFDI.aConceptos[1, 2] = "1";// valorConc[i].cantidad;        // Cantidad
                    CFDI.aConceptos[1, 3] = "H87";// valorConc[i].c_unidad_medida;// Clave Unidad de medida (Catálogo: c_ClaveUnidad)
                    CFDI.aConceptos[1, 4] = "PZA";// valorConc[i].unidad;    // Unidad de medida
                    CFDI.aConceptos[1, 5] = "FAJA INDUSTRIAL EXTRA-GDE.REFORZADA SOBRE PEDIDO";// valorConc[i].concepto;     // Descripción del producto
                    CFDI.aConceptos[1, 6] = "0.00";//valorConc[i].importe_unitario;// Importe unitario
                    CFDI.aConceptos[1, 7] = "0.00";//valorConc[i].importe_total;// Importe Total
                    CFDI.aConceptos[1, 8] = "";//valorConc[i].descuento;              // Descuento
                    //Carta Porte v2.0
                    CFDI.aCartaPorte[0] = "2.0";                         // Version
                    CFDI.aCartaPorte[1] = "No";// cartaPorte.transporte_inter;   // TranspInternac
                    CFDI.aCartaPorte[2] = "";//cartaPorte.e_s_mercancia;      // EntradaSalidaMerc
                    CFDI.aCartaPorte[3] = "";//cartaPorte.pais_ori_des;       // PaisOrigenDestino
                    CFDI.aCartaPorte[4] = "";                            // ViaEntradaSalida
                    CFDI.aCartaPorte[5] = "100";//cartaPorte.total_distancia_rec;// TotalDistRec
                    //Ubicación
                    // Origen
                    CFDI.aUbicación[0, 0] = "Origen";                         // TipoUbicacion
                    CFDI.aUbicación[0, 1] = "";                               // IDUbicacion
                    CFDI.aUbicación[0, 2] = "VAMA710703378";                  // RFCRemitenteDestinatario
                    CFDI.aUbicación[0, 3] = "Demo";                           // NombreRemitenteDestinatario
                    CFDI.aUbicación[0, 4] = "";                               // NumRegIdTrib
                    CFDI.aUbicación[0, 5] = "";                               // ResidenciaFiscal
                    CFDI.aUbicación[0, 6] = "";                               // NumEstacion
                    CFDI.aUbicación[0, 7] = "";                               // NombreEstacion
                    CFDI.aUbicación[0, 8] = "";                               // NavegacionTrafico
                    CFDI.aUbicación[0, 9] = "2021-12-08T10:43:19";            // FechaHoraSalidaLlegada
                    CFDI.aUbicación[0, 10] = "";                              // TipoEstacion
                    CFDI.aUbicación[0, 11] = "";                              // DistanciaRecorrida

                    // Domicilio origen
                    CFDI.aUbicación_Domicilio[0, 0] = "calle";                // Calle
                    CFDI.aUbicación_Domicilio[0, 1] = "211";                  // NumeroExterior
                    CFDI.aUbicación_Domicilio[0, 2] = "";                     // NumeroInterior
                    CFDI.aUbicación_Domicilio[0, 3] = "0347";                 // Colonia
                    CFDI.aUbicación_Domicilio[0, 4] = "23";                   // Localidad
                    CFDI.aUbicación_Domicilio[0, 5] = "casa blanca 1";        // Referencia
                    CFDI.aUbicación_Domicilio[0, 6] = "004";                  // Municipio
                    CFDI.aUbicación_Domicilio[0, 7] = "COA";                  // Estado
                    CFDI.aUbicación_Domicilio[0, 8] = "MEX";                  // Pais
                    CFDI.aUbicación_Domicilio[0, 9] = "25350";                // CodigoPostal

                    // Destino
                    CFDI.aUbicación[1, 0] = "Destino";                        // TipoUbicacion
                    CFDI.aUbicación[1, 1] = "";                               // IDUbicacion
                    CFDI.aUbicación[1, 2] = "VAMA710703378";                   // RFCRemitenteDestinatario
                    CFDI.aUbicación[1, 3] = "BEBIDAS MUNDIALES S DE R L DE C V"; // NombreRemitenteDestinatario
                    CFDI.aUbicación[1, 4] = "";                               // NumRegIdTrib
                    CFDI.aUbicación[1, 5] = "";                               // ResidenciaFiscal
                    CFDI.aUbicación[1, 6] = "";                               // NumEstacion
                    CFDI.aUbicación[1, 7] = "";                               // NombreEstacion
                    CFDI.aUbicación[1, 8] = "";                               // NavegacionTrafico
                    CFDI.aUbicación[1, 9] = "2021-12-08T11:43:19";            // FechaHoraSalidaLlegada
                    CFDI.aUbicación[1, 10] = "";                              // TipoEstacion
                    CFDI.aUbicación[1, 11] = "100";                              // DistanciaRecorrida

                    // Domicilio destino
                    CFDI.aUbicación_Domicilio[1, 0] = "calle";                // Calle
                    CFDI.aUbicación_Domicilio[1, 1] = "214";                  // NumeroExterior
                    CFDI.aUbicación_Domicilio[1, 2] = "";                     // NumeroInterior
                    CFDI.aUbicación_Domicilio[1, 3] = "0347";                 // Colonia
                    CFDI.aUbicación_Domicilio[1, 4] = "23";                   // Localidad
                    CFDI.aUbicación_Domicilio[1, 5] = "casa blanca 2";        // Referencia
                    CFDI.aUbicación_Domicilio[1, 6] = "004";                  // Municipio
                    CFDI.aUbicación_Domicilio[1, 7] = "COA";                  // Estado
                    CFDI.aUbicación_Domicilio[1, 8] = "MEX";                  // Pais
                    CFDI.aUbicación_Domicilio[1, 9] = "25350";                // CodigoPostal
                    /*
                    for (int a = 0; a < valorUbic.Count; a++)
                    {
                        var ubica = db.tbc_Ubicaciones.ToList<tbc_Ubicaciones>().Where(u => u.id_ubicacion == valorUbic[a].id_ubicacion).First();
                        var tipoU = Regex.Replace(valorUbic[a].tipo_ubicacion, @"\s+", String.Empty);
                        CFDI.aUbicación[a, 0] = tipoU;      // TipoUbicacion
                        CFDI.aUbicación[a, 1] = "";// ubica.id_origen_destino;          // IDUbicacion
                        CFDI.aUbicación[a, 2] = ubica.rfc_origen_destino;         // RFCRemitenteDestinatario
                        CFDI.aUbicación[a, 3] = ubica.nombre_origen_destino;      // NombreRemitenteDestinatario
                        CFDI.aUbicación[a, 4] = "";                               // NumRegIdTrib
                        CFDI.aUbicación[a, 5] = "";                               // ResidenciaFiscal
                        CFDI.aUbicación[a, 6] = "";//valorUbic[a].num_estacion.ToString();        // NumEstacion
                        CFDI.aUbicación[a, 7] = "";                               // NombreEstacion
                        CFDI.aUbicación[a, 8] = "";                               // NavegacionTrafico
                        CFDI.aUbicación[a, 9] = valorUbic[a].fca_hora_salida.ToString();// FechaHoraSalidaLlegada
                        CFDI.aUbicación[a, 10] = "";                              // TipoEstacion
                        CFDI.aUbicación[a, 11] = valorUbic[a].distancia_recorrida.ToString() == "0.00" ? "": valorUbic[a].distancia_recorrida.ToString();// DistanciaRecorrida
                        //Domicilio
                        CFDI.aUbicación_Domicilio[a, 0] = ubica.calle;            // Calle
                        CFDI.aUbicación_Domicilio[a, 1] = ubica.num_ext;          // NumeroExterior
                        CFDI.aUbicación_Domicilio[a, 2] = ubica.num_int;          // NumeroInterior
                        CFDI.aUbicación_Domicilio[a, 3] = ubica.colonia;// Colonia
                        CFDI.aUbicación_Domicilio[a, 4] = ubica.localidad;// Localidad
                        CFDI.aUbicación_Domicilio[a, 5] = ubica.referencia;// Referencia
                        CFDI.aUbicación_Domicilio[a, 6] = ubica.municipio;// Municipio
                        CFDI.aUbicación_Domicilio[a, 7] = ubica.estado;// Estado
                        CFDI.aUbicación_Domicilio[a, 8] = "MEX";                  // Pais
                        CFDI.aUbicación_Domicilio[a, 9] = ubica.codigo_postal;// CodigoPostal
                    }
                    */
                    //Mercancias
                    var c_unidad = db.tbc_Unidades_Medida.Where(u => u.id_unidad_medida == valorMerc.id_unidad_medida).Select(u => u.clave).First();
                    var upeso = db.tbc_Unidades_Medida.Where(u => u.id_unidad_medida == valorMerc.id_unidad_peso_m).Select(u => u.clave).First();
                    //var c_material = 
                    var t_embalaje = db.tbc_Tipos_Embalaje.Where(u => u.id_tipo_embalaje == valorMerc.id_tipo_embalaje).Select(u => u.clave_designacion).First();
                    CFDI.aMercancias[0] = "5";// valorMerc.peso_bruto.ToString();// PesoBrutoTotal
                    CFDI.aMercancias[1] = "KGM";//upeso;// UnidadPeso
                    CFDI.aMercancias[2] = "";//valorMerc.peso_neto.ToString();// PesoNetoTotal
                    CFDI.aMercancias[3] = "2";//valorMerc.numero_piezas.ToString();// NumTotalMercancias
                    CFDI.aMercancias[4] = "";// CargoPorTasacion

                    CFDI.aMercancia[0, 0] = "46182201";// BienesTransp
                    CFDI.aMercancia[0, 1] = "";//valorMerc.id_clave_stcc.ToString();// ClaveSTCC
                    CFDI.aMercancia[0, 2] = "FAJA ELASTICA INDUSTRIAL GRANDE";// valorMerc.descripcion;// Descripcion
                    CFDI.aMercancia[0, 3] = "1";//valorMerc.cantidad.ToString();// Cantidad
                    CFDI.aMercancia[0, 4] = "H87";//c_unidad;// ClaveUnidad
                    CFDI.aMercancia[0, 5] = "";// valorMerc.unidad.ToString();// Unidad
                    CFDI.aMercancia[0, 6] = "";// valorMerc.dimensiones.ToString();// Dimensiones
                    CFDI.aMercancia[0, 7] = "";// valorMerc.material_peligroso;// MaterialPeligroso
                    CFDI.aMercancia[0, 8] = "";//db.tbc_Materiales_Peligrosos.Where(u => u.id_material_peligroso == valorMerc.id_material_peligroso).Select(u => u.clave_material_peligroso).First();// CveMaterialPeligroso
                    CFDI.aMercancia[0, 9] = "";//t_embalaje;// Embalaje
                    CFDI.aMercancia[0, 10] = "";// valorMerc.descrip_embalaje;// DescripEmbalaje
                    CFDI.aMercancia[0, 11] = "1.5";//valorMerc.peso_kg.ToString();// PesoEnKg
                    CFDI.aMercancia[0, 12] = "82.31";//valorMerc.valor_mercancia.ToString();// ValorMercancia
                    CFDI.aMercancia[0, 13] = "MXN";//valorMerc.id_moneda;// Moneda 
                    CFDI.aMercancia[0, 14] = "";//valorMerc.id_fraccion_arancelaria;// FraccionArancelaria 
                    CFDI.aMercancia[0, 15] = "";//valorMerc.uuid_comercio_ext;// UUIDComercioExt
                    
                    CFDI.aPedimentos[0, 0, 0] = "";
                    
                    CFDI.aCantidadTransporta[0, 0, 0] = "";             // Cantidad
                    CFDI.aCantidadTransporta[0, 0, 1] = "";             // IDOrigen
                    CFDI.aCantidadTransporta[0, 0, 2] = "";             // IDDestino
                    CFDI.aCantidadTransporta[0, 0, 3] = "";             // CvesTransporte

                    CFDI.aMercancia[1, 0] = "46182201";// BienesTransp
                    CFDI.aMercancia[1, 1] = "";//valorMerc.id_clave_stcc.ToString();// ClaveSTCC
                    CFDI.aMercancia[1, 2] = "FAJA INDUSTRIAL EXTRA-GDE.REFORZADA SOBRE PEDIDO";// Descripcion
                    CFDI.aMercancia[1, 3] = "1";//valorMerc.cantidad.ToString();// Cantidad
                    CFDI.aMercancia[1, 4] = "H87";//c_unidad;// ClaveUnidad
                    CFDI.aMercancia[1, 5] = "";// valorMerc.unidad.ToString();// Unidad
                    CFDI.aMercancia[1, 6] = "";// valorMerc.dimensiones.ToString();// Dimensiones
                    CFDI.aMercancia[1, 7] = "";// valorMerc.material_peligroso;// MaterialPeligroso
                    CFDI.aMercancia[1, 8] = "";//db.tbc_Materiales_Peligrosos.Where(u => u.id_material_peligroso == valorMerc.id_material_peligroso).Select(u => u.clave_material_peligroso).First();// CveMaterialPeligroso
                    CFDI.aMercancia[1, 9] = "";//t_embalaje;// Embalaje
                    CFDI.aMercancia[1, 10] = "";//valorMerc.descrip_embalaje;// DescripEmbalaje
                    CFDI.aMercancia[1, 11] = "3.5";//valorMerc.peso_kg.ToString();// PesoEnKg
                    CFDI.aMercancia[1, 12] = "89.7028";//valorMerc.valor_mercancia.ToString();// ValorMercancia
                    CFDI.aMercancia[1, 13] = "MXN";//valorMerc.id_moneda;// Moneda 
                    CFDI.aMercancia[1, 14] = "";//valorMerc.id_fraccion_arancelaria;// FraccionArancelaria 
                    CFDI.aMercancia[1, 15] = "";//valorMerc.uuid_comercio_ext;// UUIDComercioExt
                    //-----------------------------------------------------------------------------------------------------------------------------------------------------------

                    CFDI.aPedimentos[1, 0, 0] = "";
                    //Cantidad a Transportar
                    
                    CFDI.aCantidadTransporta[1, 0, 0] = "";             // Cantidad
                    CFDI.aCantidadTransporta[1, 0, 1] = "";             // IDOrigen
                    CFDI.aCantidadTransporta[1, 0, 2] = "";             // IDDestino
                    CFDI.aCantidadTransporta[1, 0, 3] = "";             // CvesTransporte
                    //--------------
                    var permsct = db.tbc_Tipos_Permiso.Where(u => u.id_tipo_permiso == valorAuto.id_tipo_permiso).Select(u => u.clave).First();
                    //AutoTransporte
                    CFDI.aAutotransporte[0] = "TPAF02";// permsct;    //PermSCT
                    CFDI.aAutotransporte[1] = "1000C";// valorAuto.num_permiso_sct;//NumPermSCT
                    //Identt Vehicular
                    var c_vehi = db.tbc_Config_AutoTransporte.Where(u => u.id_conf_autotrans == valorAuto.id_conf_autotrans).Select(u => u.clave).First();
                    CFDI.aIdentificacionVehicular[0] = "VL";//c_vehi;// ConfigVehicular
                    CFDI.aIdentificacionVehicular[1] = "plac892";//valorAuto.placa_vm;// PlacaVM
                    CFDI.aIdentificacionVehicular[2] = "2020";//valorAuto.anio_modelo_vm.ToString();// AnioModeloVM
                    //Seguros
                    CFDI.aSeguros[0] = "BANORTE";// valorAuto.asegura_resp_civil;// AseguraRespCivil
                    CFDI.aSeguros[1] = "POL100";//valorAuto.poliza_resp_civil;// PolizaRespCivil
                    CFDI.aSeguros[2] = "";//valorAuto.asegura_med_ambiente;// AseguraMedAmbiente
                    CFDI.aSeguros[3] = "";//valorAuto.poliza_med_ambiente;// PolizaMedAmbiente
                    CFDI.aSeguros[4] = "";//valorAuto.asegura_carga;// AseguraCarga
                    CFDI.aSeguros[5] = "";//valorAuto.poliza_carga;// PolizaCarga
                    CFDI.aSeguros[6] = "";// PrimaSeguro
                    //Remolque
                    var id_sub_remolque = db.tbd_Remolque.Where(u => u.id_remolque == valorAuto.id_conf_autotrans).Select(u => u.id_remolque_sub).First();
                    var sub_remolque = db.tbc_Sub_Tipo_Rem.Where(u => u.id_remolque == id_sub_remolque).Select(u => u.clave_remolque).First();
                    var placa_remolque = db.tbd_Remolque.Where(u => u.id_remolque == valorAuto.id_autotransporte).Select(u => u.placa).First();

                    CFDI.aRemolque[0, 0] = "CTR021";//sub_remolque;// SubTipoRem
                    CFDI.aRemolque[0, 1] = "ABC123";//placa_remolque;// Placa
                    // TiposFigura
                    var t_figura = db.tbc_Figuras_Transporte.Where(u => u.id_figura_transporte == valorFigu.id_figura_transporte).Select(u => u.clave_figura_transporte).First();
                    CFDI.aTiposFigura[0, 0] = "01";//t_figura;// TipoFigura
                    CFDI.aTiposFigura[0, 1] = "VAMA710703378";//valorFigu.rfc_figura;// RFCFigura
                    CFDI.aTiposFigura[0, 2] = "LIC101";//valorFigu.num_licencia;// NumLicencia
                    CFDI.aTiposFigura[0, 3] = "FER EL CHOFER";//valorFigu.nombre_figura;// NombreFigura
                    CFDI.aTiposFigura[0, 4] = "";//valorFigu.num_reg_id_trib_figura;// NumRegIdTribFigura
                    CFDI.aTiposFigura[0, 5] = "";//ResidenciaFiscalFigura

                    // TiposFigura / ParteTransporte
                    CFDI.aPartesTransporte[0, 0, 0] = "";                     // ParteTransporte
                    // TiposFigura / ParteTransporte / Domicilio
                    CFDI.aPartesTransporte_Domicilio[0, 0] = "";// valorFigu.calle;// Calle
                    CFDI.aPartesTransporte_Domicilio[0, 1] = "";// valorFigu.num_exterior.ToString();// NumeroExterior
                    CFDI.aPartesTransporte_Domicilio[0, 2] = "";// valorFigu.num_interiror.ToString();// NumeroInterior
                    CFDI.aPartesTransporte_Domicilio[0, 3] = "";// valorFigu.colonia;// Colonia
                    CFDI.aPartesTransporte_Domicilio[0, 5] = "";//valorFigu.referencia;// Referencia
                    CFDI.aPartesTransporte_Domicilio[0, 6] = "";//valorFigu.municipio;// Municipio
                    CFDI.aPartesTransporte_Domicilio[0, 7] = "";// valorFigu.id_estado.ToString();// Estado
                    CFDI.aPartesTransporte_Domicilio[0, 8] = "";// valorFigu.id_pais.ToString();// Pais
                    CFDI.aPartesTransporte_Domicilio[0, 9] = "";// valorFigu.codigo_postal.ToString();// CodigoPostal
                    //--------------------------------------------------------------------------------
                    //CFDI.id_pre[0] = valor.id_pre_factura.ToString();
                    //CFDI.id_pre[1] = valor.url_pdf;
                    //-----
                    CFDI.GenerarTmp();
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
                        valor.fca_timbrado = Convert.ToDateTime(fca_timbre);
                        valor.status = 2;
                        valor.url_xml = ruta_xml; 
                        db.SaveChanges();
                    }
                    valor.status = 2;
                    db.SaveChanges();
                    return Json(CFDI.Mensaje, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
            
        }
        #endregion
        #region Autotransporte
        public ActionResult AutoTransporte()
        {
            if (Session["tbc_Usuarios"] == null)
            {
                return RedirectToAction("Inicio", "Sesion");
            }

            DateTime Final = DateTime.Now;
            DateTime Inicio = new DateTime(Final.Year, Final.Month, 1);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var lista = db.tbd_Autotransporte.Where(s => s.id_usuario == usuario.id_usuario).ToList();

            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }
            return View(lista);
        }
        [HttpPost]
        public ActionResult AutoTransporte(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
            {
                return RedirectToAction("Inicio", "Sesion");
            }

            DateTime Final = Convert.ToDateTime(formCollection["txtFechaFinal"]).AddDays(1).AddMinutes(-1);
            DateTime Inicio = Convert.ToDateTime(formCollection["txtFechaInicial"]);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var lista = db.tbd_Autotransporte.Where(s => s.id_usuario == usuario.id_usuario && s.fecha_creacion >= Inicio && s.fecha_creacion <= Final).ToList();

            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            return View(lista);
        }

        public String guardarAutotransporte(List<tbd_Remolque> conceptos, Int32 txtIdAutotransporte, Int32 txtIdPermisoSCT, String txtNumPermisoSCT, Int32 txtIdConfigVehicular, String txtPlacaVM, Int32 txtAnioModeloVM, String txtAsegRespCivil, String txtPolizaRespCivil, String txtAsegMedAmbiente, String txtPolizaMedAmbiente, String txtAsegCarga, String txtPolCarga, Decimal txtPrimSeguro)
        {
            if (Session["tbc_Usuarios"] == null)
            {
                return "{\"Estatus\":0, \"Mensaje\":\"Su sesión a caducado. Vuelva a iniciar sesión nuevamente.\"}";
            }

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();

            if (txtIdAutotransporte == 0)
            {
                tbd_Autotransporte nueva = new tbd_Autotransporte
                {
                    id_usuario = usuario.id_usuario,
                    id_tipo_permiso = txtIdPermisoSCT,
                    num_permiso_sct = txtNumPermisoSCT,
                    id_conf_autotrans = txtIdConfigVehicular,
                    placa_vm = txtPlacaVM,
                    anio_modelo_vm = txtAnioModeloVM,
                    asegura_resp_civil = txtAsegRespCivil,
                    poliza_resp_civil = txtPolizaRespCivil,
                    asegura_med_ambiente = txtAsegMedAmbiente,
                    poliza_med_ambiente = txtPolizaMedAmbiente,
                    asegura_carga = txtAsegCarga,
                    poliza_carga = txtPolCarga,
                    prima_seguro = txtPrimSeguro,
                    fecha_creacion = DateTime.Now
                };
                db.tbd_Autotransporte.Add(nueva);
                db.SaveChanges();

                foreach (var item in conceptos)
                {
                    tbd_Remolque nuevoRemolque = new tbd_Remolque
                    {
                        id_autotransporte = nueva.id_autotransporte,
                        id_remolque_sub = item.id_remolque_sub,
                        placa = item.placa
                    };
                    db.tbd_Remolque.Add(nuevoRemolque);
                }
                db.SaveChanges();
                return "{\"Estatus\":1, \"Mensaje\":\"\"}";
            }
            return "{\"Estatus\":0, \"Mensaje\":\"Ocurrio un error al procesar su petición, inténtelo más tarde.\"}";
        }
        public ActionResult getAutoTransporte(int id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var lis_auto = from dauto in db.tbd_Autotransporte
                               where dauto.id_autotransporte == id
                               select new
                               {
                                   id = dauto.id_autotransporte,
                                   id_permiso_sct = dauto.id_tipo_permiso,
                                   no_permiso_sct = dauto.num_permiso_sct,
                                   id_conf_vehicular = dauto.id_conf_autotrans,
                                   placa_vm = dauto.placa_vm.ToUpper(),
                                   ano_modelo = dauto.anio_modelo_vm,
                                   asegura_resp_civil = dauto.asegura_resp_civil,
                                   poliza_resp_civil = dauto.poliza_resp_civil,
                                   asegura_medio_amb = dauto.asegura_med_ambiente,
                                   poliza_medio_amb = dauto.poliza_med_ambiente,
                                   asegura_carga = dauto.asegura_carga,
                                   poliza_carga = dauto.poliza_carga
                               };
                return Json(lis_auto.First(), JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult getRemolqueById(int id)
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var lis_remolque = from dremolque in db.tbd_Remolque
                               where dremolque.id_autotransporte == id
                               select new
                               {
                                   id = dremolque.id_remolque,
                                   id_remolque_sub = dremolque.id_remolque_sub,
                                   placa = dremolque.placa
                               };
                if (lis_remolque.Count() > 0)
                {
                    return Json(lis_remolque.First(), JsonRequestBehavior.AllowGet);
                }
                else {
                    return Json("Sin Resultados", JsonRequestBehavior.AllowGet);
                }
                
            }
        }
        #endregion
        #region Transporte Aereo
        public ActionResult TransporteAereo()
        {
            //esta
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            //
            DateTime Final = DateTime.Now;
            DateTime Inicio = new DateTime(Final.Year, Final.Month, 1);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var lista = db.tbd_Transporte_Aereo.Where(s => s.id_usuario == usuario.id_usuario && s.fecha_creacion >= Inicio && s.fecha_creacion <= Final).ToList();

            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }
            //


            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            return View(lista);
            //fin esta                                      
        }
        [HttpPost]
        public ActionResult TransporteAereo(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");


            DateTime Final = Convert.ToDateTime(formCollection["txtFechaFinal"]).AddDays(1).AddMinutes(-1);
            DateTime Inicio = Convert.ToDateTime(formCollection["txtFechaInicial"]);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var lista = db.tbd_Transporte_Aereo.Where(s => s.id_usuario == usuario.id_usuario && s.fecha_creacion >= Inicio && s.fecha_creacion <= Final).ToList();

            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            return View(lista);
        }
        [HttpPost]
        public ActionResult guardarTransporteAereo(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
            {
                return RedirectToAction("Inicio", "Sesion");
            }

            int _idTransporteAereo = Convert.ToInt32(formCollection["txtIdTransporteAereo"]);
            int _idTipoPermiso = Convert.ToInt32(formCollection["txtIdPermisoSCT"]);
            string _NumPermSCT = formCollection["txtNumPermSCT"];
            string _MatriculaAeronave = formCollection["txtMatAeronave"];
            string _NombreAseg = formCollection["txtNombreAseguradora"];
            string _NumPolizaSeguro = formCollection["txtNumeroPolizaSeguro"];
            string _NumeroGuia = formCollection["txtNumeroGuia"];
            string _LugarContrato = formCollection["txtLugarContrato"];
            string _RFCTransportista = formCollection["txtRFCTransportista"];
            int _idCodigoTransporte = Convert.ToInt32(formCollection["txtIdCodigoTransportista"]);
            string _numRegIdTribTranspor = formCollection["txtNumRegFisTrans"];
            int _idPaisTranspor = Convert.ToInt32(formCollection["txtIdResFiscTrans"]);
            string _NombreTransportista = formCollection["txtNombreTrans"];
            string _RFCEmbarcador = formCollection["txtRFCEmbarcador"];
            string _NumRegIdTribEmbarc = formCollection["txtNumIdenRegFiscEmba"];
            int _IdPaisEmbarc = Convert.ToInt32(formCollection["txtIdResDiscEmbarcador"]);
            string _NombreEmbarcador = formCollection["txtNombreEmbarcador"];

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();

            if (_idTransporteAereo == 0)
            {
                tbd_Transporte_Aereo nuevo = new tbd_Transporte_Aereo
                {
                    id_usuario = usuario.id_usuario,
                    id_tipo_permiso = _idTipoPermiso,
                    num_permiso_sct = _NumPermSCT,
                    matricula_aeronave = _MatriculaAeronave,
                    nombre_aseg = _NombreAseg,
                    num_poliza_seguro = _NumPolizaSeguro,
                    numero_guia = _NumeroGuia,
                    lugar_contrato = _LugarContrato,
                    rfc_transportista = _RFCTransportista,
                    id_codigo_transporte_aereo = _idCodigoTransporte,
                    num_reg_id_trib_transpor = _numRegIdTribTranspor,
                    id_pais_transpor = _idPaisTranspor,
                    nombre_transportista = _NombreTransportista,
                    rfc_embarcador = _RFCEmbarcador,
                    num_reg_id_trib_embarc = _NumRegIdTribEmbarc,
                    id_pais_embarc = _IdPaisEmbarc,
                    nombre_embarcador = _NombreEmbarcador,
                    fecha_creacion = DateTime.Now
                };
                db.tbd_Transporte_Aereo.Add(nuevo);
                db.SaveChanges();

                TempData["Mensaje"] = "Los datos del Transporte Aéreo fueron almacenados correctamente.";
                TempData["TMensaje"] = "success";
            }
            else
            {
                tbd_Transporte_Aereo actualizar = db.tbd_Transporte_Aereo.Where(s => s.id_transporte_aereo == _idTransporteAereo).Single();

                actualizar.id_tipo_permiso = _idTipoPermiso;
                actualizar.num_permiso_sct = _NumPermSCT;
                actualizar.matricula_aeronave = _MatriculaAeronave;
                actualizar.nombre_aseg = _NombreAseg;
                actualizar.num_poliza_seguro = _NumPolizaSeguro;
                actualizar.numero_guia = _NumeroGuia;
                actualizar.lugar_contrato = _LugarContrato;
                actualizar.rfc_transportista = _RFCTransportista;
                actualizar.id_codigo_transporte_aereo = _idCodigoTransporte;
                actualizar.num_reg_id_trib_transpor = _numRegIdTribTranspor;
                actualizar.id_pais_transpor = _idPaisTranspor;
                actualizar.nombre_transportista = _NombreTransportista;
                actualizar.rfc_embarcador = _RFCEmbarcador;
                actualizar.num_reg_id_trib_embarc = _NumRegIdTribEmbarc;
                actualizar.id_pais_embarc = _IdPaisEmbarc;
                actualizar.nombre_embarcador = _NombreEmbarcador;

                TempData["Mensaje"] = "Los datos del Transporte Aéreo fueron actualizados correctamente.";
                TempData["TMensaje"] = "success";

                db.SaveChanges();

            }
            return RedirectToAction("TransporteAereo", "CartaPorte");
        }
        #endregion
        #region Transporte Maritimo
        public ActionResult TransporteMaritimo()
        {
            //esta
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            //
            DateTime Final = DateTime.Now;
            DateTime Inicio = new DateTime(Final.Year, Final.Month, 1);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var lista = db.tbd_Transporte_Maritimo.Where(s => s.id_usuario == usuario.id_usuario).ToList();

            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }
            //


            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            return View(lista);
            //fin esta                      
        }
        [HttpPost]
        public ActionResult TransporteMaritimo(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");


            DateTime Final = Convert.ToDateTime(formCollection["txtFechaFinal"]).AddDays(1).AddMinutes(-1);
            DateTime Inicio = Convert.ToDateTime(formCollection["txtFechaInicial"]);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var lista = db.tbd_Transporte_Maritimo.Where(s => s.id_usuario == usuario.id_usuario && s.fecha_creacion >= Inicio && s.fecha_creacion <= Final).ToList();

            if (lista.Count == 0)
            {
                ViewBag.Mensaje = "No se encontraron registros.";
                ViewBag.TMensaje = "warning";
            }

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            return View(lista);
        }
        public String guardarTransporteMaritimo(List<tbd_Conceptos_Contenedor_Maritimo> conceptos, Int32 txtIdTransporteMaritimo, Int32 txtIdPermisoSCT, String txtNumeroPermisoSCT, String txtNombreAseguradora, String txtNumeroPolizaSeguro,
            Int32 txtIdTipoEmbarcacion, String txtMatricula, String txtNumeroOMI, String txtAnioEmbarcacion, String txtNombreEmbarcacion, Int32 txtIdNacionalidadEmbarcacion, Decimal txtUnidadesArqueoBruto, Int32 txtIdTipoCarga,
            String txtNumeroCertificadoITC, Decimal txtEslora, Decimal txtManga, Decimal txtCalado, String txtLineaNaviera, String txtNombreAgenteNaviero, Int32 txtIdNumeroAutorizacionNaviero, String txtNumeroViaje, String txtNumeroConocimientoEmbarque)
        {
            if (Session["tbc_Usuarios"] == null)
                return "{\"Estatus\":0, \"Mensaje\":\"Su sesión a caducado. Vuelva a iniciar sesión nuevamente.\"}";

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();

            if (txtIdTransporteMaritimo == 0)
            {

                tbd_Transporte_Maritimo nueva = new tbd_Transporte_Maritimo
                {
                    id_usuario = usuario.id_usuario,
                    permiso_sct = txtIdPermisoSCT,
                    num_permiso_sct = txtNumeroPermisoSCT,
                    nombre_aseg = txtNombreAseguradora,
                    num_poliza_seguro = txtNumeroPolizaSeguro,
                    tipo_embarcacion = txtIdTipoEmbarcacion,
                    matricula = txtMatricula,
                    numero_omi = txtNumeroOMI,
                    anio_embarcacion = Convert.ToInt32(txtAnioEmbarcacion),
                    nombre_embarcacion = txtNombreEmbarcacion,
                    nacionalidad_embarcacion = txtIdNacionalidadEmbarcacion,
                    unidades_arq_bruto = txtUnidadesArqueoBruto,
                    tipo_carga = txtIdTipoCarga,
                    num_cert_itc = txtNumeroCertificadoITC,
                    eslora = txtEslora,
                    manga = txtManga,
                    calado = txtCalado,
                    linea_naviera = txtLineaNaviera,
                    nombre_agente_naviero = txtNombreAgenteNaviero,
                    num_autorizacion_naviero = txtIdNumeroAutorizacionNaviero,
                    num_viaje = txtNumeroViaje,
                    num_conoc_embarc = txtNumeroConocimientoEmbarque,
                    fecha_creacion = DateTime.Now

                };
                db.tbd_Transporte_Maritimo.Add(nueva);
                db.SaveChanges();

                foreach (var item in conceptos)
                {
                    tbd_Conceptos_Contenedor_Maritimo nuevoConcepto = new tbd_Conceptos_Contenedor_Maritimo
                    {
                        id_transporte_maritimo = nueva.id_transporte_maritimo,
                        matricula_contenedor = item.matricula_contenedor,
                        id_contenedor_maritimo = item.id_contenedor_maritimo,
                        num_precinto = item.num_precinto

                    };
                    db.tbd_Conceptos_Contenedor_Maritimo.Add(nuevoConcepto);

                }
                db.SaveChanges();
                return "{\"Estatus\":1, \"Mensaje\":\"\"}";
            }

            return "{\"Estatus\":0, \"Mensaje\":\"Ocurrio un error al procesar su petición, inténtelo más tarde.\"}";
        }
        #endregion
        //---------------------------------------------------------------------------------------
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
        //---TEST---
        public JsonResult setTimbrar()
        {
            string DirPrg = Server.MapPath("~");
            //Instancia
            SR.StampSOAP selloSOAP = new SR.StampSOAP();
            SR.stamp fx = new SR.stamp();
            SR.stampResponse selloResponse = new SR.stampResponse();



            //Parametros
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(DirPrg+ @"Plantillas\XML\DOCX\MEAX871031JG8\04102022\prueba.xml");
            
            //Conviertes el archivo en Byte
            byte[] byteXmlDocument = Encoding.UTF8.GetBytes(xmlDocument.OuterXml);
            string stringByteXmlDocument = Convert.ToBase64String(byteXmlDocument);
            byteXmlDocument = Convert.FromBase64String(stringByteXmlDocument);
            
            //Timbras el Archivo
            fx.xml = byteXmlDocument;
            fx.username = "programador1@consultoriacastelan.com";
            fx.password = "Programador1*";
            
            //Generamos Request
            String usuario;
            usuario = Environment.UserName;
            String url = DirPrg+"\\Plantillas";
            StreamWriter XML = new StreamWriter(url + "SOAP_Request.xml");
            //Direccion donde guardaremos el SOAP Envelope
            XmlSerializer soap = new XmlSerializer(fx.GetType());
            //Obtenemos los datos del objeto oStamp que contiene los parámetros de envió y es de tipo stamp()
            soap.Serialize(XML, fx);
            XML.Close();

            //Recibes la respuesta de Timbrado
            selloResponse = selloSOAP.stamp(fx);
            
            string mensaje = "";
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
                StreamWriter XMLL = new StreamWriter(url + "responsepruebas.xml");
                XMLL.Write(selloResponse.stampResult.xml);
                XMLL.Close();

                
            }

            return Json(mensaje, JsonRequestBehavior.AllowGet);

        }

        public JsonResult setCancelar(int id, string ffiscal, string motivo, string folio_)
        {
            string mensaje = "";
            com.finkok.demo.cancelResponse cancelresponse = new com.finkok.demo.cancelResponse();

            //modifiquen por su path
            string path = Server.MapPath("~");
            //Obtener numero certificado------------------------------------------------------------
            string DireccionCer = path + @"CSD_Pruebas_CFDI_EKU9003173C9.cer";
            string DireccionKey = path + @"CSD_Pruebas_CFDI_EKU9003173C9.key";
            string PasswordFinkok = "Programador1*";
            string PasswordCer = "12345678a";
            string username = "programador1@consultoriacastelan.com";
            string estatusuuid = "";
            com.finkok.demo.CancelSOAP cancela = new com.finkok.demo.CancelSOAP();
            com.finkok.demo.cancel can = new com.finkok.demo.cancel();
            try
            {
                FabricaPEM(DireccionCer, DireccionKey, PasswordFinkok, PasswordCer);
                String cer;
                String key;

                //Para importar clase TextFieldParser, ingresas al menú Proyecto-- > Agregar Referencia-- > Ensamblados-- > Seleccionar Microsotf.VisualBasic-- > Aceptar
                using (TextFieldParser fileReader = new TextFieldParser(path + "CSD_Pruebas_CFDI_EKU9003173C9.cer.pem"))
                    cer = fileReader.ReadToEnd();

                using (TextFieldParser fileReader = new TextFieldParser(path + "CSD_Pruebas_CFDI_EKU9003173C9.key.enc"))
                    key = fileReader.ReadToEnd();

                List<com.finkok.demo.UUID> lista = new List<com.finkok.demo.UUID>();
                lista.Add(new com.finkok.demo.UUID { UUID1 = "61FC01D1-7EE9-52BC-A679-D7415DFDA135", FolioSustitucion = "", Motivo = "03" });

                can.username = username;
                can.password = PasswordFinkok;
                can.taxpayer_id = "EKU9003173C9";
                can.UUIDS = lista.ToArray();
                can.cer = stringToBase64ByteArray(cer);
                can.key = stringToBase64ByteArray(key);

                cancelresponse = cancela.cancel(can);

                if (cancelresponse.cancelResult.CodEstatus == null)
                {
                    String emisor = cancelresponse.cancelResult.RfcEmisor;
                    String acuse = cancelresponse.cancelResult.Acuse;
                    String fecha = cancelresponse.cancelResult.Fecha;

                    //MessageBox.Show("Acuse: " + acuse + "\nFecha: " + fecha + "\nRFC Emisor: " + emisor);

                    Array folio = cancelresponse.cancelResult.Folios;
                    if (cancelresponse.cancelResult.Folios.Length > 0)
                    {
                        Array foliofiscal = cancelresponse.cancelResult.Folios;
                        for (int pos = 0; pos < foliofiscal.Length; pos++)
                        {
                            mensaje = "Cancelado|UUID: " + cancelresponse.cancelResult.Folios[pos].UUID +
                                "|Estatus cancelación: " + cancelresponse.cancelResult.Folios[pos].EstatusCancelacion +
                                "|Estatus UUID: " + cancelresponse.cancelResult.Folios[pos].EstatusUUID;
                        }
                        using (BD_FFEntities db = new BD_FFEntities())
                        {
                            db.Configuration.LazyLoadingEnabled = false;
                            //var valor = db.tbd_Pre_Carta_Porte.ToList<tbd_Pre_Carta_Porte>().Where(u => u.id_prefactura == id).FirstOrDefault();
                            var valorPreFac = db.tbd_Pre_Factura.ToList<tbd_Pre_Factura>().Where(u => u.id_pre_factura == id).FirstOrDefault();
                            tbd_Cancelacion_Factura cancelaFac = new tbd_Cancelacion_Factura
                            {
                                id_pre_fac = id,
                                uuid = ffiscal,
                                folio_sustitucion = cancelresponse.cancelResult.Folios[0].UUID,
                                motivo = motivo,
                                acuse = acuse,
                                rfc_emisor = emisor,
                                fecha = fecha,
                                estatus_camcelacion = cancelresponse.cancelResult.Folios[0].EstatusCancelacion,
                                estatus_uuid = cancelresponse.cancelResult.Folios[0].EstatusUUID
                            };
                            db.tbd_Cancelacion_Factura.Add(cancelaFac);
                            //valor.status = "3";
                            valorPreFac.status = 3;
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    mensaje = "Error: "+cancelresponse.cancelResult.CodEstatus;
                    //MessageBox.Show(estatusUuid);
                }
            }
            catch (Exception ex)
            {

                mensaje = "Error: "+ex;
            }
            return Json(mensaje, JsonRequestBehavior.AllowGet);
        }

        void FabricaPEM(String cer, String key, String pass, String passCSDoFIEL)
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
            url = Server.MapPath("~")+"\\"; 
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

        public byte[] stringToBase64ByteArray(String input)
        {
            Byte[] ret = Encoding.UTF8.GetBytes(input);
            String s = Convert.ToBase64String(ret);
            ret = Convert.FromBase64String(s);
            return ret;
        }

        public JsonResult getStatusXML(int id, string uuid)
        {
            com.finkok.demo.CancelSOAP selloSOAP = new com.finkok.demo.CancelSOAP();
            com.finkok.demo.get_sat_status consulta = new com.finkok.demo.get_sat_status();
            com.finkok.demo.get_sat_statusResponse getResponse = new com.finkok.demo.get_sat_statusResponse();

            consulta.username = "programador1@consultoriacastelan.com";
            consulta.password = "Programador1*";
            consulta.taxpayer_id = "EKU9003173C9";
            consulta.rtaxpayer_id = "MASO451221PM4";
            consulta.uuid = "61FC01D1-7EE9-52BC-A679-D7415DFDA135";
            consulta.total = "0.00";

            getResponse = selloSOAP.get_sat_status(consulta);

            String Escancelable = getResponse.get_sat_statusResult.sat.EsCancelable;
            String CodigoEstatus = getResponse.get_sat_statusResult.sat.CodigoEstatus;
            String Estado = getResponse.get_sat_statusResult.sat.Estado;
            String estatusUuid = getResponse.get_sat_statusResult.error;
            string mensaje = "";
            try
            {
                mensaje = "S|"+Escancelable + "|" + CodigoEstatus + "|" + Estado;
            }
            catch (Exception)
            {
                mensaje = "Error:"+estatusUuid;
            }
            return Json(mensaje, JsonRequestBehavior.AllowGet);
        }
        //---------------------------------------------------------------------------------------
    }
}