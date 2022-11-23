using Facturafast.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Mvc;

using System.Threading.Tasks;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;
using Word = Microsoft.Office.Interop.Word;
using Facturafast.CLS40;
using System.Xml.Serialization;
using System.Xml;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Net.Security;

namespace Facturafast.Controllers
{
    public class PanelController : Controller
    {
        BD_FFEntities db;
        public ActionResult Inicio()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            if (TempData["pass"] != null && TempData["user"] != null)
            {
                ViewBag.pass = TempData["pass"];
                ViewBag.user = TempData["user"];
            }
            return View();
        }

        #region Perfil
        public ActionResult Perfil()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            if (TempData["TabPassword"] != null)
            {
                ViewBag.TabPassword = TempData["TabPassword"];
            }

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            return View(usuario);
        }

        [HttpPost]
        public ActionResult ActualizarPerfil(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            //string _rfc = formCollection["txtRFC"];
            string _nombreRazon = formCollection["txtNombreRazon"];
            int _tipoPersona = Convert.ToInt32(formCollection["cmbTipoPersona"]);
            int _regimenFiscal = Convert.ToInt32(formCollection["cmbRegimenFiscal"]);
            string _telefono = formCollection["txtTelefono"];
            string _correoElectronico = formCollection["txtCorreoElectronico"];
            string _registroPatronal = formCollection["txtRegistroPatronal"];
            string _cp = formCollection["txtCP"];
            string _calle = formCollection["txtCalle"];
            string _numExt = formCollection["txtNumExt"];
            string _numInt = formCollection["txtNumInt"];
            string _colonia = formCollection["txtColonia"];
            string _localidad = formCollection["txtLocalidad"];
            string _municipio = formCollection["txtMunicipio"];
            string _estado = formCollection["cmbEstado"];

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            String url_imagen = "";
            foreach (string file in Request.Files)
            {
                if (file == "imgImagen" && Request.Files[file].ContentLength > 0)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(Request.Files[file].FileName);
                    //! Carpeta
                    var path = Server.MapPath("~/img/logos");
                    var ruta = Path.Combine(path, fileName);
                    //! Ruta completa
                    Request.Files[file].SaveAs(ruta);
                    url_imagen = fileName;
                }
            }

            tbc_Usuarios actualizar = db.tbc_Usuarios.Where(s => s.id_usuario == usuario.id_usuario).Single();

            //actualizar.rfc = _rfc;
            actualizar.nombre_razon = _nombreRazon;
            actualizar.id_tipo_persona = _tipoPersona;
            actualizar.id_regimen_fiscal = _regimenFiscal;
            actualizar.telefono = _telefono;
            actualizar.correo_electronico = _correoElectronico;
            actualizar.registro_patronal = _registroPatronal;
            actualizar.cp = _cp;
            actualizar.calle = _calle;
            actualizar.num_ext = _numExt;
            actualizar.num_int = _numInt;
            actualizar.colonia = _colonia;
            actualizar.localidad = _localidad;
            actualizar.municipio = _municipio;
            actualizar.estado = _estado;
            if (url_imagen != "")
            {
                if (actualizar.url_imagen != "")
                {
                    var path = Server.MapPath("~/img/logos");
                    var ruta = Path.Combine(path, actualizar.url_imagen);
                    System.IO.File.Delete(ruta);
                }
                actualizar.url_imagen = url_imagen;
            }

            db.SaveChanges();

            Session["tbc_Usuarios"] = actualizar;

            TempData["Mensaje"] = "Los datos fueron actualizados correctamente.";
            TempData["TMensaje"] = "success";

            return RedirectToAction("Perfil", "Panel");
        }

        [HttpPost]
        public ActionResult CambiarPassword(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            string _passwordActual = formCollection["txtPasswordActual"];
            string _passwordNueva = formCollection["txtPasswordNueva"];
            string _passwordVerificar = formCollection["txtPasswordVerificar"];

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();

            tbc_Usuarios actualizar = db.tbc_Usuarios.Where(s => s.id_usuario == usuario.id_usuario).Single();

            actualizar.password = _passwordVerificar;

            db.SaveChanges();

            TempData["Mensaje"] = "La contraseña se actualizó correctamente.";
            TempData["TMensaje"] = "success";

            TempData["TabPassword"] = true;

            return RedirectToAction("Perfil", "Panel");
        }
        #endregion

        #region Firmas
        public ActionResult Firmas()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;

            tbd_Firmas tbd_Firmas = db.tbd_Firmas.Where(s => s.id_usuario == usuario.id_usuario).SingleOrDefault();
            if (tbd_Firmas == null)
                tbd_Firmas = new tbd_Firmas
                {
                    certificado_fiel = "",
                    certificado_sello = "",
                    password_fiel = "",
                    password_sello = "",
                    url_cer_fiel = "",
                    url_cer_sello = "",
                    url_key_fiel = "",
                    url_key_sello = ""
                };

            return View(tbd_Firmas);
        }

        [HttpPost]
        public ActionResult CargarSellos(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            int _idFirma = Convert.ToInt32(formCollection["txtIdFirmaSello"]);
            string _password = formCollection["txtPasswordSellos"];

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            String url_cer = "", url_key = "";
            String certificado = "";
            DateTime vigencia = DateTime.Now;

            foreach (string file in Request.Files)
            {
                if (file == "fileCerSello" && Request.Files[file].ContentLength > 0)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(Request.Files[file].FileName);
                    //! Carpeta
                    var path = Server.MapPath("/Plantillas/FIRMAS/"+usuario.rfc);
                    //Crear Directorio
                    DirectoryInfo di = Directory.CreateDirectory(path);
                    var ruta = Path.Combine(path, fileName);
                    
                    //! Ruta completa
                    Request.Files[file].SaveAs(ruta);

                    if (System.IO.File.Exists(ruta))
                    {
                        Certificado cer = compruebaCertificado(ruta);
                        System.IO.File.Delete(ruta);
                        if (cer.Estatus == 2)
                        {
                            if (cer.RFC != usuario.rfc)
                            {
                                TempData["Mensaje"] = "El certificado cargado no corresponde a su RFC [" + cer.RFC + "]";
                                TempData["TMensaje"] = "danger";
                                return RedirectToAction("Firmas", "Panel");
                            }
                            if (cer.FechaVigencia < DateTime.Now)
                            {
                                TempData["Mensaje"] = "El certificado cargado ya esta caducado [" + cer.FechaVigencia.ToString("dd/MM/yyyy") + "]";
                                TempData["TMensaje"] = "danger";
                                return RedirectToAction("Firmas", "Panel");
                            }
                            break;
                        }
                        else if (cer.Estatus == 1)
                        {
                            TempData["Mensaje"] = "El certificado cargado no es un Sello Digital (CSD).";
                            TempData["TMensaje"] = "danger";
                            return RedirectToAction("Firmas", "Panel");
                        }
                        else
                        {
                            TempData["Mensaje"] = cer.Mensaje;
                            TempData["TMensaje"] = "danger";
                            return RedirectToAction("Firmas", "Panel");
                        }
                    }
                }
            }

            foreach (string file in Request.Files)
            {
                if (file == "fileCerSello" && Request.Files[file].ContentLength > 0)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(Request.Files[file].FileName);
                    //! Carpeta
                    var path = Server.MapPath("/Plantillas/Firmas/"+usuario.rfc);
                    var ruta = Path.Combine(path, fileName);
                    //! Ruta completa
                    Request.Files[file].SaveAs(ruta);
                    if (System.IO.File.Exists(ruta))
                    {
                        Certificado cer = compruebaCertificado(ruta);
                        vigencia = cer.FechaVigencia;
                        certificado = cer.NoCertificado;
                    }
                    url_cer = fileName;
                }
                if (file == "fileKeySello" && Request.Files[file].ContentLength > 0)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(Request.Files[file].FileName);
                    //! Carpeta
                    var path = Server.MapPath("/Plantillas/Firmas/"+usuario.rfc);
                    var ruta = Path.Combine(path, fileName);
                    //! Ruta completa
                    Request.Files[file].SaveAs(ruta);
                    url_key = fileName;
                }
            }

            if (_idFirma == 0)
            {
                tbd_Firmas nuevo = new tbd_Firmas
                {
                    id_usuario = usuario.id_usuario,
                    certificado_fiel = "",
                    certificado_sello = certificado,
                    fecha_creacion = DateTime.Now,
                    fecha_fiel = null,
                    fecha_sello = vigencia,
                    password_fiel = "",
                    password_sello = _password,
                    rfc = usuario.rfc,
                    url_cer_fiel = "",
                    url_key_fiel = "",
                    url_cer_sello = url_cer,
                    url_key_sello = url_key,
                    url_pfx_fiel = "",
                    url_pfx_sello = "",
                    es_carga_inicial = true,
                    password_ciec = ""
                };
                db.tbd_Firmas.Add(nuevo);
                db.SaveChanges();

                TempData["Mensaje"] = "Los Sellos Digitales (CSD) fueron cargados correctamente.";
                TempData["TMensaje"] = "success";
                return RedirectToAction("Firmas", "Panel");
            }
            else
            {
                tbd_Firmas actualizar = db.tbd_Firmas.Where(s => s.id_firma == _idFirma).Single();

                var path = Server.MapPath("/Plantillas/Firmas/"+usuario.rfc);
                var ruta_cer = Path.Combine(path, actualizar.url_cer_sello);
                var ruta_key = Path.Combine(path, actualizar.url_key_sello);
                if (System.IO.File.Exists(ruta_cer))
                    System.IO.File.Delete(ruta_cer);
                if (System.IO.File.Exists(ruta_key))
                    System.IO.File.Delete(ruta_key);

                actualizar.url_key_sello = url_key;
                actualizar.url_cer_sello = url_cer;
                actualizar.fecha_sello = vigencia;
                actualizar.password_sello = _password;
                actualizar.certificado_sello = certificado;
                db.SaveChanges();

                TempData["Mensaje"] = "Los Sellos Digitales (CSD) fueron cargados correctamente.";
                TempData["TMensaje"] = "success";
                return RedirectToAction("Firmas", "Panel");
            }
        }

        [HttpPost]
        public ActionResult CargarFIEL(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            int _idFirma = Convert.ToInt32(formCollection["txtIdFirmaFIEL"]);
            string _password = formCollection["txtPasswordFIEL"];

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            String url_cer = "", url_key = "";
            String certificado = "";
            DateTime vigencia = DateTime.Now;

            String url_pfx = "";

            foreach (string file in Request.Files)
            {
                if (file == "fileCerFIEL" && Request.Files[file].ContentLength > 0)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(Request.Files[file].FileName);
                    //! Carpeta
                    var path = Server.MapPath("~/doc/validacion");
                    var ruta = Path.Combine(path, fileName);
                    //! Ruta completa
                    Request.Files[file].SaveAs(ruta);

                    if (System.IO.File.Exists(ruta))
                    {
                        Certificado cer = compruebaCertificado(ruta);
                        System.IO.File.Delete(ruta);
                        if (cer.Estatus == 1)
                        {
                            if (cer.RFC != usuario.rfc)
                            {
                                TempData["Mensaje"] = "El certificado cargado no corresponde a su RFC [" + cer.RFC + "]";
                                TempData["TMensaje"] = "danger";
                                return RedirectToAction("Firmas", "Panel");
                            }
                            if (cer.FechaVigencia < DateTime.Now)
                            {
                                TempData["Mensaje"] = "El certificado cargado ya esta caducado [" + cer.FechaVigencia.ToString("dd/MM/yyyy") + "]";
                                TempData["TMensaje"] = "danger";
                                return RedirectToAction("Firmas", "Panel");
                            }
                            break;
                        }
                        else if (cer.Estatus == 2)
                        {
                            TempData["Mensaje"] = "El certificado cargado no es una Firma Electrónica (FIEL).";
                            TempData["TMensaje"] = "danger";
                            return RedirectToAction("Firmas", "Panel");
                        }
                        else
                        {
                            TempData["Mensaje"] = cer.Mensaje;
                            TempData["TMensaje"] = "danger";
                            return RedirectToAction("Firmas", "Panel");
                        }
                    }
                }
            }

            foreach (string file in Request.Files)
            {
                if (file == "fileCerFIEL" && Request.Files[file].ContentLength > 0)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(Request.Files[file].FileName);
                    //! Carpeta
                    var path = Server.MapPath("~/doc/firmas");
                    var ruta = Path.Combine(path, fileName);
                    //! Ruta completa
                    Request.Files[file].SaveAs(ruta);
                    if (System.IO.File.Exists(ruta))
                    {
                        Certificado cer = compruebaCertificado(ruta);
                        vigencia = cer.FechaVigencia;
                        certificado = cer.NoCertificado;
                    }
                    url_cer = fileName;
                }
                if (file == "fileKeyFIEL" && Request.Files[file].ContentLength > 0)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(Request.Files[file].FileName);
                    //! Carpeta
                    var path = Server.MapPath("~/doc/firmas");
                    var ruta = Path.Combine(path, fileName);
                    //! Ruta completa
                    Request.Files[file].SaveAs(ruta);
                    url_key = fileName;
                }
            }

            var pathPFX = Server.MapPath("~/doc/firmas");
            String _cer = Path.Combine(pathPFX, url_cer);
            String _key = Path.Combine(pathPFX, url_key);
            String _clavePrivada = _password;
            String _urlPEM = Server.MapPath("~/doc/validacion");
            String _urlPFX = Server.MapPath("~/doc/firmas");
            PFX pfx = new PFX();

            PFXResponse pFXResponse = pfx.CreaPFX(_cer, _key, _clavePrivada, _urlPEM, _urlPFX);
            if (pFXResponse.Estatus == 0)
            {
                var path = Server.MapPath("~/doc/firmas");
                var ruta_cer = Path.Combine(path, url_cer);
                var ruta_key = Path.Combine(path, url_key);
                if (System.IO.File.Exists(ruta_cer))
                    System.IO.File.Delete(ruta_cer);
                if (System.IO.File.Exists(ruta_key))
                    System.IO.File.Delete(ruta_key);

                TempData["Mensaje"] = pFXResponse.Mensaje;
                TempData["TMensaje"] = "danger";
                return RedirectToAction("Firmas", "Panel");
            }
            else
            {
                url_pfx = pFXResponse.URL;
            }

            if (_idFirma == 0)
            {
                tbd_Firmas nuevo = new tbd_Firmas
                {
                    id_usuario = usuario.id_usuario,
                    certificado_fiel = certificado,
                    certificado_sello = "",
                    fecha_creacion = DateTime.Now,
                    fecha_fiel = vigencia,
                    fecha_sello = null,
                    password_fiel = _password,
                    password_sello = "",
                    rfc = usuario.rfc,
                    url_cer_fiel = url_cer,
                    url_key_fiel = url_key,
                    url_cer_sello = "",
                    url_key_sello = "",
                    url_pfx_fiel = url_pfx,
                    url_pfx_sello = "",
                    es_carga_inicial = true,
                    password_ciec = ""
                };
                db.tbd_Firmas.Add(nuevo);
                db.SaveChanges();

                TempData["Mensaje"] = "La Firma Electrónica (FIEL) fue cargada correctamente.";
                TempData["TMensaje"] = "success";
                return RedirectToAction("Firmas", "Panel");
            }
            else
            {
                tbd_Firmas actualizar = db.tbd_Firmas.Where(s => s.id_firma == _idFirma).Single();

                var path = Server.MapPath("~/doc/firmas");
                var ruta_cer = Path.Combine(path, actualizar.url_cer_fiel);
                var ruta_key = Path.Combine(path, actualizar.url_key_fiel);
                var ruta_pfx = Path.Combine(path, actualizar.url_pfx_fiel);

                if (System.IO.File.Exists(ruta_cer))
                    System.IO.File.Delete(ruta_cer);
                if (System.IO.File.Exists(ruta_key))
                    System.IO.File.Delete(ruta_key);
                if (System.IO.File.Exists(ruta_pfx))
                    System.IO.File.Delete(ruta_pfx);



                actualizar.url_key_fiel = url_key;
                actualizar.url_cer_fiel = url_cer;
                actualizar.fecha_fiel = vigencia;
                actualizar.password_fiel = _password;
                actualizar.certificado_fiel = certificado;
                actualizar.url_pfx_fiel = url_pfx;
                db.SaveChanges();

                TempData["Mensaje"] = "La Firma Electrónica (FIEL) fue cargada correctamente.";
                TempData["TMensaje"] = "success";
                return RedirectToAction("Firmas", "Panel");
            }

        }
        #endregion

        #region Comprar Timbres
        public ActionResult ComprarTimbres()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            db = new BD_FFEntities();
            var lista = db.tbc_Paquetes.Where(s => s.id_estatus == 1).ToList().OrderBy(s => s.folios);
            return View(lista);
        }


        public async Task<ActionResult> SeleccionarPlan(Int32? id)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (id == null)
            {
                return RedirectToAction("ComprarTimbres", "Panel");
            }

            db = new BD_FFEntities();

            tbc_Paquetes tbc_Paquetes = db.tbc_Paquetes.Where(s => s.id_paquete == id).Single();
            tbc_Variables_Calculo tbc_Variables_Calculo = db.tbc_Variables_Calculo.Single();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;

            //APP_USR-2450731942541262-111615-57009eebf5483eee453c627feb453593-802731391
            //TEST-4135746951028339-022423-4b46685dfe6e02f9c2ad531a96e4ddf4-39121160
            MercadoPagoConfig.AccessToken = tbc_Variables_Calculo.access_token_mp;
            // Crea el objeto de request de la preference
            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = tbc_Paquetes.nombre_paquete,
                        Quantity = 1,
                        CurrencyId = "MXN",
                        UnitPrice = tbc_Paquetes.costo,

                    },
                },
                Payer = new PreferencePayerRequest
                {
                    Name = usuario.nombre_razon,
                    Email = usuario.correo_electronico
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = tbc_Variables_Calculo.url_back_mp,
                    Failure = tbc_Variables_Calculo.url_back_mp
                },
                BinaryMode = true,
                AutoReturn = "approved",
                ExternalReference = "",

            };

            // Crea la preferencia usando el client
            var client = new PreferenceClient();
            Preference preference = await client.CreateAsync(request);

            TempData["PREFERENCE_ID"] = preference.Id;
            TempData["InitPoint"] = preference.InitPoint;
            TempData["tbc_Paquetes"] = tbc_Paquetes;
            return RedirectToAction("NuevaCompra", "Panel");
        }

        public ActionResult NuevaCompra()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (TempData["PREFERENCE_ID"] == null ||
                TempData["tbc_Paquetes"] == null ||
                TempData["InitPoint"] == null)
            {
                return RedirectToAction("ComprarTimbres", "Panel");
            }

            ViewBag.PREFERENCE_ID = TempData["PREFERENCE_ID"];
            ViewBag.InitPoint = TempData["InitPoint"];
            tbc_Paquetes tbc_Paquetes = TempData["tbc_Paquetes"] as tbc_Paquetes;
            return View(tbc_Paquetes);
        }

        [HttpPost]
        public String AlmacenarCobro(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return "";


            string _idPreference = formCollection["txtIdPreference"];
            string _correo = formCollection["txtCorreo"];
            int _idUsoFactura = Convert.ToInt32(formCollection["cmbUsoFactura"]);
            int _idMetodo = Convert.ToInt32(formCollection["cmbFormaPago"]);
            int _idPaquete = Convert.ToInt32(formCollection["txtIdPaquete"]);
            string _initPoint = formCollection["txtInitPoint"];


            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;

            db = new BD_FFEntities();
            tbc_Paquetes paquete = db.tbc_Paquetes.Where(s => s.id_paquete == _idPaquete).Single();

            tbd_Cobros buscarCobro = db.tbd_Cobros.Where(s => s.id_preference == _idPreference && s.id_usuario == usuario.id_usuario).SingleOrDefault();
            if (buscarCobro == null)
            {
                tbd_Cobros nuevo = new tbd_Cobros
                {
                    fecha_cobro = DateTime.Now,
                    id_estatus = 1,
                    id_forma_pago = _idMetodo,
                    id_paquete = _idPaquete,
                    id_preference = _idPreference,
                    id_uso_cfdi = _idUsoFactura,
                    id_usuario = usuario.id_usuario,
                    importe = paquete.importe,
                    iva = paquete.iva,
                    total = paquete.costo,
                    correo_electronico = _correo,
                    payment_id = "",
                    payment_method = "",
                    status = "",
                    id_vendedor_usuario = 0,
                    timbres = paquete.folios,
                    comision = 0,
                    init_point = _initPoint,
                    rfc_usuario = usuario.rfc
                };

                db.tbd_Cobros.Add(nuevo);
                db.SaveChanges();
                return "success";
            }
            else
            {
                if (buscarCobro.id_estatus == 1)
                {
                    buscarCobro.fecha_cobro = DateTime.Now;
                    buscarCobro.id_forma_pago = _idMetodo;
                    buscarCobro.id_paquete = _idPaquete;
                    buscarCobro.id_preference = _idPreference;
                    buscarCobro.id_uso_cfdi = _idUsoFactura;
                    buscarCobro.id_forma_pago = _idMetodo;
                    buscarCobro.importe = paquete.importe;
                    buscarCobro.iva = paquete.iva;
                    buscarCobro.total = paquete.costo;
                    buscarCobro.correo_electronico = _correo;
                    buscarCobro.timbres = paquete.folios;
                    buscarCobro.init_point = _initPoint;

                    db.SaveChanges();
                    return "success";
                }
                else
                {
                    return "";
                }
            }
        }
        static string p = "";
        static string p_xml = "";
        public ActionResult PagoFinalizadoTest()
        {
            db = new BD_FFEntities();
            tbd_Cobros actualizar = db.tbd_Cobros.Where(s => s.id_preference == "802731391-bc9600af-12f7-4b84-9c76-754c08e8d538").Single();
            var res = genXMLPagosServicio(actualizar.id_cobro);
            if (res == "Success") 
            {
                enviarCorreoPago(actualizar.id_cobro, "Pago");
                //TimbrarXMLPago(actualizar.id_cobro, "FacturaC", "FacturaC");
            }

            return View();
        }
        
        public ActionResult PagoFinalizado()
        {
            String _idPreference = Request["preference_id"];
            String _estatus = Request["status"];
            String _metodo = Request["payment_type"];
            String _idPayment = Request["payment_id"];


            if (_estatus == "approved")
            {
                db = new BD_FFEntities();
                tbd_Cobros actualizar = db.tbd_Cobros.Where(s => s.id_preference == _idPreference).Single();
                actualizar.status = _estatus;
                actualizar.payment_method = _metodo;
                actualizar.payment_id = _idPayment;
                actualizar.id_estatus = 3;

                tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;

                //Sumar Timbres
                tbc_Timbres sumarTimbres = db.tbc_Timbres.Where(s => s.id_usuario == usuario.id_usuario).Single();
                tbc_Paquetes paquete = db.tbc_Paquetes.Where(s => s.id_paquete == actualizar.id_paquete).Single();

                sumarTimbres.fecha_vigencia = DateTime.Now.AddYears(1);
                sumarTimbres.timbres_totales += paquete.folios;
                sumarTimbres.timbres_disponibles += paquete.folios;

                //Sumar Comisión
                tbr_Vendedor_Cliente tbr_Vendedor_Cliente = db.tbr_Vendedor_Cliente.Where(s => s.id_cliente_usuario == usuario.id_usuario).Single();
                tbc_Vendedores sumarVendedor = db.tbc_Vendedores.Where(s => s.id_usuario == tbr_Vendedor_Cliente.id_vendedor_usuario).Single();
                tbc_Variables_Calculo variable = db.tbc_Variables_Calculo.Single();

                //Decimal comision = (paquete.costo * variable.comision) / 100;

                sumarVendedor.paquetes_vendidos++;
                sumarVendedor.comision_total += paquete.comision;
                sumarVendedor.total_vendido += paquete.costo;

                actualizar.comision = paquete.comision;
                actualizar.id_vendedor_usuario = sumarVendedor.id_usuario;

                db.SaveChanges();

                PanelProductivo.RegistrationSOAP SoapAdd = new PanelProductivo.RegistrationSOAP();
                PanelProductivo.assign asign = new PanelProductivo.assign();
                PanelProductivo.assignResponse resp = new PanelProductivo.assignResponse();

                asign.credit = paquete.folios.ToString();
                asign.username = "cfdi@facturafast.mx";
                asign.password = "F4ctur4f4st_C@st3l4n";
                asign.taxpayer_id = usuario.rfc.ToUpper();

                resp = SoapAdd.assign(asign);

                //Generar Factura
                db = new BD_FFEntities();
                var res = genXMLPagosServicio(actualizar.id_cobro);
                if (res == "Success")
                {
                    TimbrarXMLPago(actualizar.id_cobro, "FacturaC", "FacturaC");
                }

                ViewBag.MensajePago = "El pago se acredito correctamente.";
                ViewBag.Icono = "check";
                ViewBag.Comentario = "Su sompra fue registrda con éxito y los timbres fueron sumados a su cuenta.";
                ViewBag.Tipo = "success";
                return View();
            }
            else
            {
                db = new BD_FFEntities();
                tbd_Cobros actualizar = db.tbd_Cobros.Where(s => s.id_preference == _idPreference).Single();
                actualizar.status = _estatus != null ? _estatus : "rejected";
                actualizar.payment_method = _metodo != null ? _metodo : "";
                actualizar.payment_id = _idPayment != null ? _idPayment : "";
                actualizar.id_estatus = 4;
            }

            ViewBag.MensajePago = "Hubo un problema con el pago, vuelve a intentarlo más tarde.";
            ViewBag.Icono = "times";
            ViewBag.Comentario = "Puede ir a su lista de Pagos para volver a intentar su compra.";
            ViewBag.Tipo = "danger";

            return View();
        }

        #endregion

        #region Pagos

        public ActionResult Pagos()
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
            var lista = db.tbd_Cobros.Where(s => s.status != null && s.rfc_usuario == usuario.rfc).ToList();
            return View(lista);
        }

        public ActionResult ReintentarPago(Int32? id)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            db = new BD_FFEntities();
            var tbd_Cobros = db.tbd_Cobros.Where(s => s.id_cobro == id).Single();
            var tbc_Paquetes = db.tbc_Paquetes.Where(s => s.id_paquete == tbd_Cobros.id_paquete).Single();
            TempData["PREFERENCE_ID"] = tbd_Cobros.id_preference;
            TempData["InitPoint"] = tbd_Cobros.init_point;
            TempData["tbc_Paquetes"] = tbc_Paquetes;
            return RedirectToAction("NuevaCompra", "Panel");
        }

        public ActionResult EliminarCobro(Int32? id_cobro_eliminar)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            db = new BD_FFEntities();
            var tbd_Cobros = db.tbd_Cobros.Where(s => s.id_cobro == id_cobro_eliminar).Single();
            if (tbd_Cobros.id_estatus != 3)
            {
                db.tbd_Cobros.Remove(tbd_Cobros);
                db.SaveChanges();

                TempData["Mensaje"] = "El registro de cobro fue eliminado correctamente.";
                TempData["TMensaje"] = "success";

                return RedirectToAction("Pagos", "Panel");
            }

            TempData["Mensaje"] = "El registro de pago no se puede eliminar.";
            TempData["TMensaje"] = "danger";

            return RedirectToAction("Pagos", "Panel");

        }

        #endregion

        #region Notas de Venta

        public ActionResult NotasVentas()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");



            DateTime Final = DateTime.Now;
            DateTime Inicio = new DateTime(Final.Year, Final.Month, 1);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var lista = db.tbd_Notas_Venta.Where(s => s.rfc_usuario == usuario.rfc && s.fecha_creacion >= Inicio && s.fecha_creacion <= Final).ToList();

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
        public ActionResult NotasVentas(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");


            DateTime Final = Convert.ToDateTime(formCollection["txtFechaFinal"]).AddDays(1).AddMinutes(-1);
            DateTime Inicio = Convert.ToDateTime(formCollection["txtFechaInicial"]);

            ViewBag.Inicio = Inicio;
            ViewBag.Final = Final;

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var lista = db.tbd_Notas_Venta.Where(s => s.rfc_usuario == usuario.rfc && s.fecha_creacion >= Inicio && s.fecha_creacion <= Final).ToList();

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

        public String calcularImportes(List<ConceptosNota> conceptos)
        {
            if (Session["tbc_Usuarios"] == null)
                return "";

            db = new BD_FFEntities();
            Decimal Total = 0;
            Decimal Total_IVA = 0;
            Decimal Total_ISRRet = 0;
            Decimal Total_IVARet = 0;
            Decimal Total_Descuento = 0;
            Decimal Total_Importe = 0;
            Decimal Total_Impuesto = 0;

            foreach (var item in conceptos)
            {
                item.seleccion = item.seleccion == null ? "" : item.seleccion;
                item.clave = item.clave == null ? "" : item.clave;
                item.importe = Decimal.Round(item.precio_unitario * item.cantidad, 4);
                item.total_descuento = item.tipo_descuento == 1 ? Decimal.Round((item.importe * item.descuento) / 100, 4) : Decimal.Round(item.importe * item.descuento, 4);

                if (item.id_detalle_nota_venta != 0)
                {
                    tbc_Productos_Servicios prod = db.tbc_Productos_Servicios.Where(s => s.id_producto_servicio == item.id_detalle_nota_venta).Single();
                    tbc_ProdServ sat = db.tbc_ProdServ.Where(s => s.id_sat == prod.id_sat).Single();
                    tbc_Unidades_Medida med = db.tbc_Unidades_Medida.Where(s => s.id_unidad_medida == prod.id_unidad_medida).Single();

                    item.id_sat = sat.id_sat;
                    item.clave_sat = sat.c_pord_serv;
                    item.id_unidad_medida = med.id_unidad_medida;
                    item.unidad = med.clave;

                    item.id_isr = prod.id_isr;
                    item.id_iva = prod.id_iva;
                    item.id_iva_ret = prod.id_iva_ret;

                    item.clave = prod.clave;
                }
                if (item.id_iva != 0)
                {
                    item.total_iva = Decimal.Round(item.importe * (db.tbc_IVA.Where(s => s.id_iva == item.id_iva).Single().@decimal), 4);
                    item.iva_tasa_impuesto = Convert.ToString(Decimal.Round(item.importe * (db.tbc_IVA.Where(s => s.id_iva == item.id_iva).Single().@decimal), 4));
                }
                if (item.id_isr != 0)
                {
                    item.total_isr = Decimal.Round(item.importe * (db.tbc_ISR.Where(s => s.id_isr == item.id_isr).Single().@decimal), 4);
                    item.isr_ret_impuesto = Convert.ToString(Decimal.Round(item.importe * (db.tbc_ISR.Where(s => s.id_isr == item.id_isr).Single().@decimal), 4));
                }
                if (item.id_iva_ret != 0)
                {
                    item.total_iva_ret = Decimal.Round(item.importe * (db.tbc_IVA_Ret.Where(s => s.id_iva_ret == item.id_iva_ret).Single().@decimal), 4);
                    item.iva_ret_impuesto = Convert.ToString(Decimal.Round(item.importe * (db.tbc_IVA_Ret.Where(s => s.id_iva_ret == item.id_iva_ret).Single().@decimal), 4));
                }
                if (item.importe != 0 || item.total_iva != 0 || item.total_iva_ret != 0 || item.total_isr != 0 || item.total_descuento != 0) 
                {
                    item.total = Decimal.Round((item.importe + item.total_iva - item.total_iva_ret - item.total_isr - item.total_descuento), 2);
                    Total_Impuesto = item.total_iva_ret + item.total_isr;
                    item.total_imp_retenido = Total_Impuesto.ToString();
                    Total += item.total;
                    Total_ISRRet += item.total_isr;
                    Total_IVA += item.total_iva;
                    Total_IVARet += item.total_iva_ret;
                    Total_Descuento += item.total_descuento;
                    Total_Importe += item.importe;
                }
            }

            return "{\"Total_Importe\":" + Total_Importe + ",\"Total\":" + Total + ",\"Total_ISRRet\":" + Total_ISRRet + ",\"Total_IVA\":" + Total_IVA + ",\"Total_IVARet\":" + Total_IVARet + ",\"Total_Descuento\":" + Total_Descuento + ",\"Data\":" + JsonConvert.SerializeObject(conceptos) + "}";
        }

        public String guardarNotaVenta(List<ConceptosNota> conceptos, Int32 txtIdCliente, Int32 txtIdNotaVenta, String txtSerie, Int32 txtIdCuenta)
        {
            int idnota = 0;
            if (Session["tbc_Usuarios"] == null)
                return "{\"Estatus\":0, \"Mensaje\":\"Su sesión a caducado. Vuelva a iniciar sesión nuevamente.\"}";

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();

            Decimal Total = 0;
            Decimal Total_IVA = 0;
            Decimal Total_ISRRet = 0;
            Decimal Total_IVARet = 0;
            Decimal Total_Descuento = 0;
            Decimal Total_Importe = 0;
            foreach (var item in conceptos)
            {
                item.seleccion = item.seleccion == null ? "" : item.seleccion;
                item.clave = item.clave == null ? "" : item.clave;
                item.importe = Decimal.Round(item.precio_unitario * item.cantidad, 4);
                item.total_iva = Decimal.Round(item.importe * (db.tbc_IVA.Where(s => s.id_iva == item.id_iva).Single().@decimal), 4);
                item.total_isr = Decimal.Round(item.importe * (db.tbc_ISR.Where(s => s.id_isr == item.id_isr).Single().@decimal), 4);
                item.total_iva_ret = Decimal.Round(item.importe * (db.tbc_IVA_Ret.Where(s => s.id_iva_ret == item.id_iva_ret).Single().@decimal), 4);
                item.total_descuento = item.tipo_descuento == 1 ? Decimal.Round((item.importe * item.descuento) / 100, 4) : Decimal.Round(item.importe * item.descuento, 4);

                item.total = Decimal.Round(item.importe,4) + Decimal.Round(item.total_iva,4) - item.total_iva_ret - item.total_isr - item.total_descuento;

                Total += item.total;
                Total_ISRRet += item.total_isr;
                Total_IVA += item.total_iva;
                Total_IVARet += item.total_iva_ret;
                Total_Descuento += item.total_descuento;
                Total_Importe += item.importe;
            }

            if (txtIdNotaVenta == 0)
            {
                Int32 nextFolio = 0;
                var maxFolio = db.tbd_Notas_Venta.Where(s => s.serie == txtSerie && s.rfc_usuario == usuario.rfc).OrderByDescending(s=> s.fecha_creacion).FirstOrDefault();
                if (maxFolio != null)
                {
                    nextFolio = Convert.ToInt32(maxFolio.folio);
                }

                tbd_Notas_Venta nueva = new tbd_Notas_Venta
                {
                    clave_nota = RandomString(8),
                    descuento = Total_Descuento,
                    fecha_creacion = DateTime.Now,
                    id_cliente = txtIdCliente,
                    id_estatus = 1,
                    id_usuario = usuario.id_usuario,
                    ieps = 0,
                    url_pdf = "",
                    isr_ret = Total_ISRRet,
                    iva = Total_IVA,
                    iva_ret = Total_IVARet,
                    subtotal = Total_Importe,
                    total = Total,
                    serie = txtSerie,
                    folio = (nextFolio + 1).ToString(),
                    rfc_usuario = usuario.rfc,
                    id_cuenta_bancaria = txtIdCuenta
                };
                db.tbd_Notas_Venta.Add(nueva);
                db.SaveChanges();

                foreach (var item in conceptos)
                {
                    tbd_Conceptos_Nota_Venta nuevoConcepto = new tbd_Conceptos_Nota_Venta
                    {
                        cantidad = item.cantidad,
                        clave = item.clave,
                        cuota_ieps = 0,
                        descuento = item.descuento,
                        es_tasa_ieps = 1,
                        fecha_creacion = DateTime.Now,
                        id_ieps = 18,
                        id_isr = item.id_isr,
                        id_iva = item.id_iva,
                        id_iva_ret = item.id_iva_ret,
                        id_nota_venta = nueva.id_nota_venta,
                        id_sat = item.id_sat,
                        id_unidad_medida = item.id_unidad_medida,
                        id_usuario = usuario.id_usuario,
                        importe = item.importe,
                        precio_unitario = item.precio_unitario,
                        tipo_descuento = item.tipo_descuento,
                        total = item.total,
                        total_descuento = item.total_descuento,
                        total_isr_ret = item.total_isr,
                        total_iva = item.total_iva,
                        total_iva_ret = item.total_iva_ret,
                        concepto = item.concepto,
                        rfc_usuario = usuario.rfc
                    };
                    db.tbd_Conceptos_Nota_Venta.Add(nuevoConcepto);
                    idnota = nuevoConcepto.id_nota_venta;
                }
                db.SaveChanges();
                return "{\"Estatus\":1, \"Id\":"+idnota+", \"Mensaje\":\"\"}";
            }


            return "{\"Estatus\":0, \"Mensaje\":\"Ocurrio un error al procesar su petición, inténtelo más tarde.\"}";
        }

        public ActionResult CancelarNotaVenta(Int32? id_nota_venta)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (id_nota_venta != null)
            {
                db = new BD_FFEntities();
                tbd_Notas_Venta cancelar = db.tbd_Notas_Venta.Where(s => s.id_nota_venta == id_nota_venta).SingleOrDefault();
                cancelar.id_estatus = 6;
                db.SaveChanges();
                TempData["Mensaje"] = "La nota de venta se cancelo correctamente.";
                TempData["TMensaje"] = "success";
            }
            return RedirectToAction("NotasVentas", "Panel");
        }

        [HttpPost]
        public ActionResult PagarNotaVenta(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            Int32 idNota = Convert.ToInt32(formCollection["id_nota_venta_pago"]);
            Int32 idFormaPago = Convert.ToInt32(formCollection["cmbFormaPago"]);
            Decimal total = Convert.ToDecimal(formCollection["txtTotalPagoNota"]);
            DateTime fechaPago = Convert.ToDateTime(formCollection["txtFechaPago"]);
            Int32 idUsoCFDI = Convert.ToInt32(formCollection["cmbUsoCFDI"]);

            string fileName = "";
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();

            tbd_Notas_Venta nota = db.tbd_Notas_Venta.Where(s => s.id_nota_venta == idNota).Single();

            //!Identificar si la nota esta facturado o no
            if (nota.id_estatus == 6)
            {
                TempData["Mensaje"] = "No se pueden pagar las Notas de Ventas canceladas [" + nota.serie + "-" + nota.folio + "].";
                TempData["TMensaje"] = "danger";
                return RedirectToAction("NotasVentas", "Panel");
            }


            if (nota.id_estatus == 7) //! Facturado
            {
                //!Se hará un complemento de pago por cada pago
            }
            else
            {
                //!Se hará un anticipo por cada pago, el ultimo pago que cubra el total, se generará una factura descontando el total de todos los anticipos
                Decimal totalPagado = db.tbd_Pagos_Nota_Venta.Where(s => s.id_nota_venta == idNota).Select(s => s.total_pagado).DefaultIfEmpty(0).Sum();
                Decimal restanteNota = nota.total - totalPagado;

                if (totalPagado == 0 && total == nota.total)
                {
                    //!Se hace solo una factura con el total pagado
                    foreach (string file in Request.Files)
                    {
                        if (file == "fileComprobantePago" && Request.Files[file].ContentLength > 0)
                        {
                            fileName = Guid.NewGuid() + Path.GetExtension(Request.Files[file].FileName);
                            //! Carpeta
                            var path = Server.MapPath("~/doc/comprobantes");
                            var ruta = Path.Combine(path, fileName);
                            //! Ruta completa
                            Request.Files[file].SaveAs(ruta);
                        }
                    }

                    tbd_Pagos_Nota_Venta pago = new tbd_Pagos_Nota_Venta
                    {
                        fecha_creacion = DateTime.Now,
                        id_forma_pago = idFormaPago,
                        id_nota_venta = idNota,
                        id_usuario = usuario.id_usuario,
                        rfc_usuario = usuario.rfc,
                        tipo_pago = "Total",
                        total_pagado = total,
                        url_comprobante = fileName,
                        id_uso_cfdi = idUsoCFDI,
                        fecha_pago = fechaPago,
                        id_estatus = 5

                    };
                    db.tbd_Pagos_Nota_Venta.Add(pago);

                    //nota.id_estatus = 7;

                    db.SaveChanges();
                }
                else
                {
                    if (restanteNota > 0)
                    {
                        if (total < restanteNota)
                        {
                            //!Se hace el anticipo con el total pagado
                            foreach (string file in Request.Files)
                            {
                                if (file == "fileComprobantePago" && Request.Files[file].ContentLength > 0)
                                {
                                    fileName = Guid.NewGuid() + Path.GetExtension(Request.Files[file].FileName);
                                    //! Carpeta
                                    var path = Server.MapPath("~/doc/comprobantes");
                                    var ruta = Path.Combine(path, fileName);
                                    //! Ruta completa
                                    Request.Files[file].SaveAs(ruta);
                                }
                            }

                            tbd_Pagos_Nota_Venta pago = new tbd_Pagos_Nota_Venta
                            {
                                fecha_creacion = DateTime.Now,
                                id_forma_pago = idFormaPago,
                                id_nota_venta = idNota,
                                id_usuario = usuario.id_usuario,
                                rfc_usuario = usuario.rfc,
                                tipo_pago = "Anticipo",
                                total_pagado = total,
                                url_comprobante = fileName,
                                id_uso_cfdi = idUsoCFDI,
                                fecha_pago = fechaPago,
                                id_estatus = 5
                            };
                            db.tbd_Pagos_Nota_Venta.Add(pago);

                            //nota.id_estatus = 7;

                            db.SaveChanges();
                        }
                        else if (total == restanteNota)
                        {
                            //!Se hace la factura con el total pagado
                            foreach (string file in Request.Files)
                            {
                                if (file == "fileComprobantePago" && Request.Files[file].ContentLength > 0)
                                {
                                    fileName = Guid.NewGuid() + Path.GetExtension(Request.Files[file].FileName);
                                    //! Carpeta
                                    var path = Server.MapPath("~/doc/comprobantes");
                                    var ruta = Path.Combine(path, fileName);
                                    //! Ruta completa
                                    Request.Files[file].SaveAs(ruta);
                                }
                            }

                            tbd_Pagos_Nota_Venta pago = new tbd_Pagos_Nota_Venta
                            {
                                fecha_creacion = DateTime.Now,
                                id_forma_pago = idFormaPago,
                                id_nota_venta = idNota,
                                id_usuario = usuario.id_usuario,
                                rfc_usuario = usuario.rfc,
                                tipo_pago = "Final",
                                total_pagado = total,
                                url_comprobante = fileName,
                                id_uso_cfdi = idUsoCFDI,
                                fecha_pago = fechaPago,
                                id_estatus = 5
                            };
                            db.tbd_Pagos_Nota_Venta.Add(pago);


                            //!Se genera la factura con el total de la nota con el descuento de los anticipos anteriores

                            Decimal TotalNota = nota.total;
                            Decimal Descuento = nota.total - total;


                            //nota.id_estatus = 7;

                            db.SaveChanges();
                        }
                        else
                        {
                            TempData["Mensaje"] = "El pago excede al restante de la Nota de Venta [" + nota.serie + "-" + nota.folio + "]. Saldo restante: " + restanteNota.ToString("c") + ".";
                            TempData["TMensaje"] = "danger";
                            return RedirectToAction("NotasVentas", "Panel");
                        }
                    }
                    else
                    {
                        TempData["Mensaje"] = "La Nota de Venta [" + nota.serie + "-" + nota.folio + "] ya fué pagada.";
                        TempData["TMensaje"] = "danger";
                        return RedirectToAction("NotasVentas", "Panel");
                    }
                }
            }

            TempData["Mensaje"] = "El pago de la nota de venta [" + nota.serie + "-" + nota.folio + "] se registro correctamente.";
            TempData["TMensaje"] = "success";

            return RedirectToAction("NotasVentas", "Panel");
        }

        public String obtenerHistoricoPagos(Int32? id)
        {
            if (Session["tbc_Usuarios"] == null)
                return "";

            if (id == null)
                return "";

            db = new BD_FFEntities();            

            var lista = db.tbd_Pagos_Nota_Venta.Where(s => s.id_nota_venta == id).OrderBy(s => s.fecha_creacion).ToList();
            Decimal total = 0;
            String list = "";
            foreach (var item in lista)
            {
                if (item.id_estatus == 5)
                {
                    total += item.total_pagado;
                    list += "<tr><td></td><td>" + (item.url_comprobante != "" ? "<a href='/doc/comprobantes/" + item.url_comprobante + "' class='btn btn-success btn-sm' title='Descargar Comprobante de Pago' target='_blank'><i class='fas fa-download'></i></a>" : "") + "</td> <td>" + item.fecha_pago.ToString("yyyy/MM/dd HH:mm") + "</td> <td>" + item.tipo_pago + "</td> <td>" + item.total_pagado.ToString("c") + "</td> <td>" + total.ToString("c") + "</td></tr>";
                }
                else
                {
                    list += "<tr class='table-danger'><td></td><td>" + (item.url_comprobante != "" ? "<a href='/doc/comprobantes/" + item.url_comprobante + "' class='btn btn-success btn-sm' title='Descargar Comprobante de Pago' target='_blank'><i class='fas fa-download'></i></a>" : "") + "</td> <td>" + item.fecha_pago.ToString("yyyy/MM/dd HH:mm") + "</td> <td>" + "Cancelado" + "</td> <td>" + item.total_pagado.ToString("c") + "</td> <td>" + total.ToString("c") + "</td></tr>";
                }
                
            }

            return list;
        }

        
        public JsonResult AlmacenarNota(Int32? idNotaVenta)
        {
            //Int32 _idNota = Convert.ToInt32(formCollection["idNotaVenta"]);
            //db = new BD_FFEntities();
            //tbc_Variables_Calculo variables = db.tbc_Variables_Calculo.Single();
            //foreach (string file in Request.Files)
            //{
            //    if (file == "docx" && Request.Files[file].ContentLength > 0)
            //    {
            //        tbd_Notas_Venta nota = db.tbd_Notas_Venta.Where(s => s.id_nota_venta == _idNota).Single();
            //        if (nota.url_pdf == "")
            //        {
            //            string nombre = Guid.NewGuid().ToString();
            //            string nombreWord = nombre + ".DOCX";
            //            //! Ruta completa
            //            Request.Files[file].SaveAs((variables.url_docx + nombreWord));

            //            //! Creamos PDF
            //            var pdfProcess = new Process();
            //            pdfProcess.StartInfo.FileName = variables.url_libreoffice;
            //            pdfProcess.StartInfo.Arguments = "--headless --convert-to pdf \"" + (variables.url_docx + nombreWord) + "\" --outdir  \"" + variables.url_pdf + "\"";
            //            pdfProcess.Start();

            //            nota.url_pdf = nombre + ".PDF";
            //            db.SaveChanges();
            //        }
            //    }
            //}
            //return "";
            if (Session["tbc_Usuarios"] == null)
                return Json("Error", JsonRequestBehavior.AllowGet);
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            //--------------------------------------------------------------------------------------------------------------------------------------
            db = new BD_FFEntities();
            tbd_Notas_Venta NotaVenta = db.tbd_Notas_Venta.Where(s => s.id_nota_venta == idNotaVenta).Single();
            tbc_Clientes cliente = db.tbc_Clientes.Where(s => s.id_cliente == NotaVenta.id_cliente).Single();
            tbc_Cuentas_Bancarias banco = db.tbc_Cuentas_Bancarias.Where(s => s.id_cuenta_bancaria == NotaVenta.id_cuenta_bancaria).Single();
            var ruta = db.tbc_Variables_Calculo.Where(s => s.id_variable == 1).ToList().First();
            //--------------------------------------------------------------------------------------------------------------------------------------
            string namefile = "";
            string DirPrg = Server.MapPath("~");
            string fca_pago = NotaVenta.fecha_creacion.ToString("d");
            String[] fechaE = fca_pago.Split(' ');
            string aux_fc_emi = fechaE[0];
            String[] auxfechaE = aux_fc_emi.Split('/');
            string ax_fc_emi = auxfechaE[0] + auxfechaE[1] + auxfechaE[2];
            namefile = NotaVenta.clave_nota + "-" + cliente.rfc + "-" + usuario.rfc;
            string path = DirPrg + "/Plantillas/NotasVenta/PDF/" + usuario.rfc + "/" + ax_fc_emi + "/" + namefile + ".pdf";
            //--------------------------------------------------------------------------------------------------------------------------------------

            string auxpath = DirPrg + "Plantillas\\NotasVenta\\PDF\\" + usuario.rfc + "\\" + ax_fc_emi;
            DirectoryInfo di = Directory.CreateDirectory(auxpath);
            string nombreArchivo = "NotaVenta_v12";
            string direcArchivo = DirPrg + "/Plantillas/" + nombreArchivo + ".docx";
            string direcDestino = DirPrg + "Plantillas/NotasVenta/PDF/" + usuario.rfc + "/" + ax_fc_emi + "/" + namefile + ".docx";

            System.IO.File.Copy(direcArchivo, direcDestino, true);

            object ObjMiss = System.Reflection.Missing.Value;
            Word.Application ObjWord = new Word.Application();
            Word.Document ObjDoc = ObjWord.Documents.Open(direcDestino, ObjMiss);

            //Definir Marcadores
            object nombre_razon = "nombre_razon"; //Definir marcador
            Word.Range nombrerazon = ObjDoc.Bookmarks.get_Item(ref nombre_razon).Range; //Busqueda de marcador en la plantilla
            nombrerazon.Text = usuario.nombre_razon; //Agregar texto al marcador

            object direccion_fiscal = "direccion_fiscal"; //Definir marcador
            Word.Range direccionfiscal = ObjDoc.Bookmarks.get_Item(ref direccion_fiscal).Range; //Busqueda de marcador en la plantilla
            direccionfiscal.Text = usuario.cp; //Agregar texto al marcador

            object rfc_emisor = "rfc_emisor"; //Definir marcador
            Word.Range rfcemisor = ObjDoc.Bookmarks.get_Item(ref rfc_emisor).Range; //Busqueda de marcador en la plantilla
            rfcemisor.Text = usuario.rfc; //Agregar texto al marcador

            object serie = "serie"; //Definir marcador
            Word.Range serie_ = ObjDoc.Bookmarks.get_Item(ref serie).Range; //Busqueda de marcador en la plantilla
            serie_.Text = NotaVenta.serie; //Agregar texto al marcador

            object folio = "folio"; //Definir marcador
            Word.Range folio_ = ObjDoc.Bookmarks.get_Item(ref folio).Range; //Busqueda de marcador en la plantilla
            folio_.Text = NotaVenta.folio; //Agregar texto al marcador

            object expedicion = "expedicion"; //Definir marcador
            Word.Range expedicion_ = ObjDoc.Bookmarks.get_Item(ref expedicion).Range; //Busqueda de marcador en la plantilla
            expedicion_.Text = usuario.cp; //Agregar texto al marcador

            object rfc_cliente = "rfc"; //Definir marcador
            Word.Range rfccliente = ObjDoc.Bookmarks.get_Item(ref rfc_cliente).Range; //Busqueda de marcador en la plantilla
            rfccliente.Text = cliente.rfc; //Agregar texto al marcador

            object razon_cliente = "razon_cliente"; //Definir marcador
            Word.Range razoncliente = ObjDoc.Bookmarks.get_Item(ref razon_cliente).Range; //Busqueda de marcador en la plantilla
            razoncliente.Text = cliente.nombre_razon; //Agregar texto al marcador

            object direccion_cliente = "direccion_cliente"; //Definir marcador
            Word.Range direccioncliente = ObjDoc.Bookmarks.get_Item(ref direccion_cliente).Range; //Busqueda de marcador en la plantilla
            direccioncliente.Text = cliente.direccion_fiscal; //Agregar texto al marcador

            object fecha_ = "fecha"; //Definir marcador
            Word.Range fecha = ObjDoc.Bookmarks.get_Item(ref fecha_).Range; //Busqueda de marcador en la plantilla
            
            string auxfca = String.Format("{0:yyyy-MM-ddTHH:mm:ss}", NotaVenta.fecha_creacion);//
            fecha.Text = auxfca;//NotaVenta.fecha_creacion.ToString(); //Agregar texto al marcador

            var conceptos = db.tbd_Conceptos_Nota_Venta.Where(s => s.id_nota_venta == idNotaVenta).ToList();
            conceptos.Count();
            int i = 1;
            object Tabla_Conceptos = "Tabla_Conceptos";
            Word.Range TablaConceptos = ObjDoc.Bookmarks.get_Item(ref Tabla_Conceptos).Range;
            Word.Table TableConceptos = ObjDoc.Tables.Add(TablaConceptos, conceptos.Count, 6);
            for (int z = 0; z < conceptos.Count; z++)
            {
                var unidad = db.tbc_Unidades_Medida.ToList<tbc_Unidades_Medida>().Where(s => s.id_unidad_medida == conceptos[z].id_unidad_medida).Single();
                var producto = db.tbc_ProdServ.ToList<tbc_ProdServ>().Where(s => s.id_sat == conceptos[z].id_sat).Single();

                TableConceptos.Cell(i, 1).Range.Text = conceptos[z].cantidad.ToString();
                TableConceptos.Cell(i, 2).Range.Text = "[" + unidad.clave + "] " + unidad.descripcion;
                TableConceptos.Cell(i, 3).Range.Text = producto.c_pord_serv;
                TableConceptos.Cell(i, 4).Range.Text = conceptos[z].concepto.ToString();
                TableConceptos.Cell(i, 5).Range.Text = conceptos[z].precio_unitario.ToString("C");
                TableConceptos.Cell(i, 6).Range.Text = conceptos[z].importe.ToString("C");
            }

            TablaConceptos.Columns[1].SetWidth(55, 0);
            TablaConceptos.Columns[2].SetWidth(55, 0);
            TablaConceptos.Columns[3].SetWidth(70, 0);
            TablaConceptos.Columns[4].SetWidth(230, 0);
            TablaConceptos.Columns[5].SetWidth(85, 0);
            TablaConceptos.Columns[6].SetWidth(80, 0);


            //
            object total_letra = "total_letra"; //Definir marcador
            Word.Range totalletra = ObjDoc.Bookmarks.get_Item(ref total_letra).Range; //Busqueda de marcador en la plantilla
            totalletra.Text = Convert.ToDecimal(NotaVenta.total).NumeroALetras(); //Agregar texto al marcador

            object subtotal_ = "subtotal"; //Definir marcador
            Word.Range subtotal = ObjDoc.Bookmarks.get_Item(ref subtotal_).Range; //Busqueda de marcador en la plantilla
            subtotal.Text = NotaVenta.subtotal.ToString("C"); //Agregar texto al marcador

            object iva_ = "iva"; //Definir marcador
            Word.Range iva = ObjDoc.Bookmarks.get_Item(ref iva_).Range; //Busqueda de marcador en la plantilla
            iva.Text = NotaVenta.iva.ToString("C"); //Agregar texto al marcador

            object iva_ret = "iva_ret"; //Definir marcador
            Word.Range ivaret = ObjDoc.Bookmarks.get_Item(ref iva_ret).Range; //Busqueda de marcador en la plantilla
            ivaret.Text = NotaVenta.iva_ret.ToString("C"); //Agregar texto al marcador

            object isr_ret = "isr_ret"; //Definir marcador
            Word.Range isrret = ObjDoc.Bookmarks.get_Item(ref isr_ret).Range; //Busqueda de marcador en la plantilla
            isrret.Text = NotaVenta.isr_ret.ToString("C"); //Agregar texto al marcador

            object total_ = "total"; //Definir marcador
            Word.Range total = ObjDoc.Bookmarks.get_Item(ref total_).Range; //Busqueda de marcador en la plantilla
            total.Text = NotaVenta.total.ToString("C"); //Agregar texto al marcador

            object banco_ = "banco"; //Definir marcador
            Word.Range _banco = ObjDoc.Bookmarks.get_Item(ref banco_).Range; //Busqueda de marcador en la plantilla
            _banco.Text = banco.banco; //Agregar texto al marcador

            object clabe_ = "clabe"; //Definir marcador
            Word.Range clabe = ObjDoc.Bookmarks.get_Item(ref clabe_).Range; //Busqueda de marcador en la plantilla
            clabe.Text = banco.clabe.ToString(); //Agregar texto al marcador

            object propietario_ = "propietario"; //Definir marcador
            Word.Range propietario = ObjDoc.Bookmarks.get_Item(ref propietario_).Range; //Busqueda de marcador en la plantilla
            propietario.Text = banco.propietario; //Agregar texto al marcador

            ObjDoc.SaveAs2(direcDestino);
            ObjDoc.Close();
            ObjWord.Quit();

            //Crear PDF
            var pdfProcess = new Process();
            pdfProcess.StartInfo.FileName = "" + ruta.url_libreoffice;
            pdfProcess.StartInfo.Arguments = "--headless --convert-to pdf " + direcDestino + " --outdir  " + DirPrg + "Plantillas\\NotasVenta\\PDF\\" + usuario.rfc + "\\" + ax_fc_emi + "\\";
            pdfProcess.Start();

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                if (System.IO.File.Exists(path))
                {
                    NotaVenta.url_pdf = "NotasVenta/PDF/" + usuario.rfc + "/" + ax_fc_emi + "/" + namefile + ".pdf";
                    break;
                }
            }
            db.SaveChanges();
            System.IO.File.Delete(direcDestino);
            //Int32 _idNota = Convert.ToInt32(formCollection["idNotaVenta"]);
            //db = new BD_FFEntities();
            //tbc_Variables_Calculo variables = db.tbc_Variables_Calculo.Single();
            //foreach (string file in Request.Files)
            //{
            //    if (file == "docx" && Request.Files[file].ContentLength > 0)
            //    {
            //        tbd_Notas_Venta nota = db.tbd_Notas_Venta.Where(s => s.id_nota_venta == _idNota).Single();
            //        if (nota.url_pdf == "")
            //        {
            //            string nombre = Guid.NewGuid().ToString();
            //            string nombreWord = nombre + ".DOCX";
            //            //! Ruta completa
            //            Request.Files[file].SaveAs((variables.url_docx + nombreWord));

            //            //! Creamos PDF
            //            var pdfProcess = new Process();
            //            pdfProcess.StartInfo.FileName = variables.url_libreoffice;
            //            pdfProcess.StartInfo.Arguments = "--headless --convert-to pdf \"" + (variables.url_docx + nombreWord) + "\" --outdir  \"" + variables.url_pdf + "\"";
            //            pdfProcess.Start();

            //            nota.url_pdf = nombre + ".PDF";
            //            db.SaveChanges();
            //        }
            //    }
            //}
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        public ActionResult DescargarNotaVenta(Int32? idNotaVenta)
        {
            //db = new BD_FFEntities();
            //tbc_Variables_Calculo variables = db.tbc_Variables_Calculo.Single();
            //tbd_Notas_Venta nota = db.tbd_Notas_Venta.Where(s => s.id_nota_venta == idNotaVenta).Single();
            //tbc_Clientes cliente = db.tbc_Clientes.Where(s => s.id_cliente == nota.id_cliente).Single();
            //string fullPath = variables.url_pdf + "\\" + nota.url_pdf;
            //int i = 0;
            //while (i < 30)
            //{
            //    Thread.Sleep(1000);
            //    if (System.IO.File.Exists(fullPath))
            //        break;
            //}
            //return File(fullPath, "application/pdf", "Nota de Venta " + nota.clave_nota + "_" + cliente.rfc + ".PDF");
            if (Session["tbc_Usuarios"] == null)
                return Json("Error", JsonRequestBehavior.AllowGet);
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            tbd_Notas_Venta NotaVenta = db.tbd_Notas_Venta.Where(s => s.id_nota_venta == idNotaVenta).Single();
            tbc_Clientes cliente = db.tbc_Clientes.Where(s => s.id_cliente == NotaVenta.id_cliente).Single();
            string DirPrg = Server.MapPath("~");
            string path = DirPrg + "Plantillas/" + NotaVenta.url_pdf;

            return File(path, "application/pdf", "Nota de Venta " + NotaVenta.clave_nota + "_" + cliente.rfc + ".PDF");
        }

        public ActionResult DescargarNotaVentaXML(Int32? idNotaVenta)
        {
            if (Session["tbc_Usuarios"] == null)
                return Json("Error", JsonRequestBehavior.AllowGet);
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            tbd_Notas_Venta NotaVenta = db.tbd_Notas_Venta.Where(s => s.id_nota_venta == idNotaVenta).Single();
            tbc_Clientes cliente = db.tbc_Clientes.Where(s => s.id_cliente == NotaVenta.id_cliente).Single();
            string DirPrg = Server.MapPath("~");
            string path = DirPrg + "Plantillas/" + NotaVenta.url_xml;

            return File(path, "application/xml", "Nota de Venta " + NotaVenta.clave_nota + "_" + cliente.rfc + ".xml");
        }
        public ActionResult DescargarFacturaFacturafastXML(Int32? idcobro)
        {
            if (Session["tbc_Usuarios"] == null)
                return Json("Error", JsonRequestBehavior.AllowGet);
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            tbd_Cobros cobro = db.tbd_Cobros.Where(s => s.id_cobro == idcobro).Single();
            string DirPrg = Server.MapPath("~");
            string path = DirPrg + "Plantillas/" + cobro.url_xml;

            return File(path, "application/xml", "Cobro " + cobro.uuid + "_" + usuario.rfc + ".xml");
        }
        public String enviarCorreosNota(List<ListaCorreos> correos, Int32 txtIdNotaVenta)
        {
            if (Session["tbc_Usuarios"] == null)
                return "{\"Estatus\":0, \"Mensaje\":\"Su sesión a caducado. Vuelva a iniciar sesión nuevamente.\"}";

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            string ruta = Server.MapPath("~");
            foreach (var item in correos)
            {

                Correos enviar = new Correos();
                enviar.emailEnvioNotaVenta(item, txtIdNotaVenta, usuario, ruta);
            }

            return "{\"Estatus\":1, \"Mensaje\":\"Los correos electrónicos fueron enviados. Puede checar el estatus de envío desde el registro de la Nota de Venta.\"}";

        }

        public JsonResult UpdTimbre(List<tbd_Notas_Venta> nota, Int32 id) 
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                foreach (var prefac in nota)
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    var valor = db.tbd_Notas_Venta.ToList<tbd_Notas_Venta>().Where(u => u.id_nota_venta == id).FirstOrDefault();
                    valor.id_uso_cfdi = prefac.id_uso_cfdi;
                    valor.id_forma_pago = prefac.id_forma_pago;
                    valor.id_metodo_pago = prefac.id_metodo_pago;
                    valor.lugar_expedicion = prefac.lugar_expedicion;
                }
                db.SaveChanges();
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult conNotaVenta(Int32? id) 
        {
            using (BD_FFEntities db = new BD_FFEntities())
            {
                var nota_venta = from nv in db.tbd_Notas_Venta
                                    where nv.id_nota_venta == id
                                    select new
                                    {
                                        forma_pago = nv.id_forma_pago,
                                        metodo_pago = nv.id_metodo_pago,
                                        lugar_expedicion = nv.lugar_expedicion,
                                        uso_cfdi = nv.id_uso_cfdi
                                    };
                return Json(nota_venta.FirstOrDefault(), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Utilidades

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
        public bool isCSD(List<X509KeyUsageExtension> extension)
        {
            X509KeyUsageFlags flag = X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation;
            return extension[0].KeyUsages.HasFlag(flag);
        }

        public bool isFIEL(List<X509KeyUsageExtension> extension)
        {
            X509KeyUsageFlags flag = X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation | X509KeyUsageFlags.KeyAgreement | X509KeyUsageFlags.DataEncipherment;
            return extension[0].KeyUsages.HasFlag(flag);
        }

        public Certificado compruebaCertificado(string path)
        {
            String NoCertificado = "";
            DateTime FechaVigencia = DateTime.Now;
            Int32 Estatus = 0;
            String Mensaje = "";
            String RFC = "";
            X509Certificate2 cert = new X509Certificate2(path);
            if (cert.Version != 3)
            {
                Estatus = 0;
                Mensaje = String.Format("El certificado con nombre {0}, no es versión 3 y no puede ser comprobado.", cert.GetNameInfo(X509NameType.DnsName, false));
            }
            List<X509KeyUsageExtension> extension = cert.Extensions.OfType<X509KeyUsageExtension>().ToList();
            if (isFIEL(extension))
            {
                NoCertificado = Encoding.ASCII.GetString(cert.GetSerialNumber().Reverse().ToArray());
                FechaVigencia = cert.NotAfter;
                String[] Datos = cert.Subject.Split(',');
                foreach (var item in Datos)
                {
                    if (item.Contains("OID.2.5.4.45="))
                    {
                        int pFrom = item.IndexOf(" OID.2.5.4.45=") + " OID.2.5.4.45=".Length;
                        String[] OID = item.Substring(pFrom).Split('/');
                        RFC = OID[0].Trim();
                    }
                }

                Estatus = 1;
            }
            else
            {
                if (isCSD(extension))
                {
                    NoCertificado = Encoding.ASCII.GetString(cert.GetSerialNumber().Reverse().ToArray());
                    FechaVigencia = cert.NotAfter;
                    String[] Datos = cert.Subject.Split(',');
                    foreach (var item in Datos)
                    {
                        if (item.Contains("OID.2.5.4.45="))
                        {
                            int pFrom = item.IndexOf(" OID.2.5.4.45=") + " OID.2.5.4.45=".Length;
                            String[] OID = item.Substring(pFrom).Split('/');
                            RFC = OID[0].Trim();
                        }
                    }
                    Estatus = 2;
                }
                else
                {
                    Estatus = 3;
                    Mensaje = "El certificado cargado no es CSD ni FIEL.";
                }
            }

            return new Certificado { NoCertificado = NoCertificado, FechaVigencia = FechaVigencia, Estatus = Estatus, Mensaje = Mensaje, RFC = RFC }; ;
        }

        #endregion
        #region Facturacion
        public String genXMLPagosServicio(Int32? id_)
        {
            XMLController xml_ = new XMLController();
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
            bool fileExist = System.IO.File.Exists(DirPrg_ + "Plantillas/" + ruta_xml);
            FileInfo file = new FileInfo(DirPrg_ + "Plantillas/" + ruta_xml);
            try
            {
                file.Delete();
                fileExist = System.IO.File.Exists(DirPrg_ + "Plantillas/" + ruta_xml);
            }
            catch (Exception e)
            {

            }
            if (!fileExist)
            {
                DirectoryInfo didoc = Directory.CreateDirectory(DirPrg_ + "Plantillas/" + ruta_xml);
            }
            string aux_path = ruta_xml + @"\" + namefile + ".xml";
            string path = Server.MapPath("~");
            p = path;
            string pathXML = path + @"Plantillas\" + ruta_xml + "\\" + namefile + ".xml";

            string pathCer = path + @"\Plantillas\Firmas\Facturafast\00001000000506285169.cer";
            string pathKey = path + @"\Plantillas\Firmas\Facturafast\CSD_FACTURAFAST_SA_DE_CV_FAC201027H66_20210129_131834.key";
            string clavePrivada = "HUEXOTITLA2021";

            p_xml = pathXML;
            //Obtenemos el Número de Certificado
            string numeroCertificado, aa, b, c;
            CLS40.SelloDigital.leerCER(pathCer, out aa, out b, out c, out numeroCertificado);
            //----------------Llenamos la clase COMPROBANTE ---------------------------
            Comprobante oComprobante = new Comprobante();
            oComprobante.Version = "4.0";
            oComprobante.Serie = "A";
            oComprobante.Folio = "1";
            oComprobante.Fecha = DateTime.Now.AddMinutes(-2).ToString("yyyy-MM-ddTHH:mm:ss");
            //oComprobante.Sello = ""; 
            oComprobante.FormaPago = "03";
            oComprobante.NoCertificado = numeroCertificado;
            //oComprobante.Certificado = "";
            oComprobante.SubTotal = s_iva;
            oComprobante.Moneda = "MXN";
            oComprobante.Total = Math.Round(Convert.ToDecimal(factura.total), 2);
            oComprobante.TipoDeComprobante = "I";
            oComprobante.MetodoPago = "PUE";
            oComprobante.LugarExpedicion = "72534";
            oComprobante.Descuento = 0;
            oComprobante.Exportacion = "01";

            oComprobante.FormaPagoSpecified = true;
            oComprobante.MetodoPagoSpecified = true;
            //Emisor
            ComprobanteEmisor oEmisor = new ComprobanteEmisor();
            oEmisor.Rfc = "FAC201027H66";
            oEmisor.Nombre = "FACTURAFAST";
            oEmisor.RegimenFiscal = "601";
            //Receptor
            ComprobanteReceptor oReceptor = new ComprobanteReceptor();
            oReceptor.Nombre = usuario.nombre_razon;
            oReceptor.Rfc = usuario.rfc;
            oReceptor.UsoCFDI = "G03";
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
            oConcepto.Descripcion = db.tbc_Paquetes.Where(u => u.id_paquete == factura.id_paquete).Select(u => u.nombre_paquete).First(); ;
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
            return "Success";
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
        public JsonResult TimbrarXMLPago(int id_, string n_doc, string tipo)
        {
            string DirPrg = Server.MapPath("~");
            //Get Info PreFac en DB
            db = new BD_FFEntities();
            var ruta_xml = "";
            string namefile = "";
            string ruta_pdf = "";
            tbc_Usuarios usuario_ = Session["tbc_Usuarios"] as tbc_Usuarios;
            
            var factura = db.tbd_Cobros.ToList<tbd_Cobros>().Where(u => u.id_cobro == id_).Single();
            //----------
            var fca_emision = factura.fecha_cobro.ToString();
            String[] fechaE = fca_emision.Split(' ');
            string aux_fc_emi = fechaE[0];
            String[] auxfechaE = aux_fc_emi.Split('/');
            string ax_fc_emi = auxfechaE[0] + auxfechaE[1] + auxfechaE[2];
            //--------
            namefile = "tempXML";
            ruta_xml = "XML\\PDF\\Facturafast\\" + ax_fc_emi;
            
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
            xmlDocument.Load(DirPrg + "Plantillas\\" + ruta_xml + "\\" + n_doc);
            string rutaO = DirPrg + ruta_pdf;// + "\\" + n_doc;
            string rutaM = "";
            //Conviertes el archivo en Byte
            byte[] byteXmlDocument = Encoding.UTF8.GetBytes(xmlDocument.OuterXml);
            string stringByteXmlDocument = Convert.ToBase64String(byteXmlDocument);
            byteXmlDocument = Convert.FromBase64String(stringByteXmlDocument);

            //Timbras el Archivo
            stamp.xml = byteXmlDocument;
            stamp.username = "cfdi@facturafast.mx";
            stamp.password = "F4ctur4f4st_C@st3l4n";


            //Generamos Request
            String usuario;
            usuario = Environment.UserName;
            String url = DirPrg + "Plantillas\\" + ruta_xml;
            StreamWriter XML = new StreamWriter(url + "SOAP_Request.xml");
            //Direccion donde guardaremos el SOAP Envelope
            XmlSerializer soap = new XmlSerializer(stamp.GetType());
            //Obtenemos los datos del objeto oStamp que contiene los parámetros de envió y es de tipo stamp()
            soap.Serialize(XML, stamp);
            XML.Close();

            //Recibes la respuesta de Timbrado
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
                //Actualizar Estado XML
                string root_xml = DirPrg + "Plantillas\\" + ruta_xml + "\\" + uuidR + ".xml";
                using (BD_FFEntities db = new BD_FFEntities())
                {   
                    //Guardar a PreFactura
                    db.Configuration.LazyLoadingEnabled = false;
                    var valorCobro = db.tbd_Cobros.ToList<tbd_Cobros>().Where(u => u.id_cobro == id_).FirstOrDefault();
                    valorCobro.uuid = uuidR;
                    valorCobro.url_xml = ruta_xml + "\\" + uuidR + ".xml";
                    valorCobro.url_pdf = ruta_xml + "\\" + uuidR + ".pdf";
                    valorCobro.id_estatus = 7;
                    db.SaveChanges();
                }
                //Restar timbre
                //tbc_Timbres timbres = db.tbc_Timbres.Where(s => s.rfc_usuario == usuario_.rfc).Single();
                //Enviar Correo
                enviarCorreoPago(id_,"NV");
                //timbres.timbres_usados++;
                //timbres.timbres_disponibles--;

                db.SaveChanges();
                //---------Agregar a base Facturas-------------------
                LeerArchivo(root_xml, usuario_.rfc, id_, tipo);
                enviarCorreoPago(id_, "Pago");
            }
            //FacturafacturaFast(id_);
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
                                var r_xml = "";
                                var r_pdf = "";
                                r_xml = db.tbd_Cobros.Where(s => s.id_cobro == id_prefac).Select(u => u.url_xml).SingleOrDefault();
                                r_pdf = db.tbd_Cobros.Where(s => s.id_cobro == id_prefac).Select(u => u.url_pdf).SingleOrDefault();
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
                                        tbd_Pre_Factura preFac = new tbd_Pre_Factura
                                        {
                                            id_usuario = 14,
                                            rfc_usuario = "FAC201027H66",
                                            uuid = nuevaFactura.uuid,
                                            nombre_usuario_rfc = nuevaFactura.nombre_emisor,
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

                                        //-----------------------------------
                                        db.Configuration.LazyLoadingEnabled = false;
                                        var prefac = db.tbd_Cobros.ToList<tbd_Cobros>().Where(u => u.id_cobro == id_prefac).FirstOrDefault();
                                        //----------------------------------------------------------------------------------------------------
                                        db.SaveChanges();

                                        foreach (var item in conceptos)
                                        {
                                            item.id_factura = nuevaFactura.id_factura;
                                        }

                                        db.tbd_Conceptos_Factura.AddRange(conceptos);
                                        db.SaveChanges();
                                        FacturaFacturafast(preFac.id_pre_factura);
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
            }
        }
        public JsonResult FacturaFacturafast(Int32? id)
        {
            if (Session["tbc_Usuarios"] == null)
                return Json("Error", JsonRequestBehavior.AllowGet);
            CultureInfo ci = new CultureInfo("en-us");
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            db = new BD_FFEntities();
            tbd_Pre_Factura prefactura_ = db.tbd_Pre_Factura.Where(s => s.id_pre_factura == id).Single();
            tbc_Metodos_Pago mpago_ = db.tbc_Metodos_Pago.Where(u => u.id_metodo_pago == prefactura_.metodo_pago).Single();
            tbc_Formas_Pago fpago_ = db.tbc_Formas_Pago.Where(u => u.id_forma_pago == prefactura_.forma_pago).Single();
            tbc_Usos_CFDI ucfdi_ = db.tbc_Usos_CFDI.Where(u => u.id_uso_cfdi == prefactura_.clave_uso_cfdi).Single();
            var valorCFDI = db.tbd_Cfdi_Uuid.ToList<tbd_Cfdi_Uuid>().Where(u => u.id_pre_factura == id).ToList();
            var valorConc = db.tbd_Cobros.Where(u => u.uuid == prefactura_.uuid).Single() /*db.tbd_Conceptos_Pre_Factura.ToList<tbd_Conceptos_Pre_Factura>().Where(u => u.id_pre_factura == id).ToList()*/;
            //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            var ruta = db.tbc_Variables_Calculo.Where(s => s.id_variable == 1).ToList().First();
            var fca_emision = prefactura_.fecha_emision.ToString();

            String[] fechaE = fca_emision.Split(' ');
            string aux_fc_emi = fechaE[0];
            String[] auxfechaE = aux_fc_emi.Split('/');
            string ax_fc_emi = auxfechaE[0] + auxfechaE[1] + auxfechaE[2];
            string DirPrg = Server.MapPath("~");
            string namefile = prefactura_.uuid;


            //string path = "Plantillas/XML/PDF/Facturafast" + "/" + ax_fc_emi + "/" + namefile + ".pdf";
            //-----------------------------------------------------------------------------------------------------------------------------

            //string auxpath = DirPrg + "Plantillas\\PREPAGO\\XML\\PDF\\Facturafast" + "\\" + ax_fc_emi;
            //DirectoryInfo di = Directory.CreateDirectory(auxpath);
            //string auxpathdoc = DirPrg + "Plantillas\\PREPAGO\\XML\\DOCX\\" + usuario.rfc + "\\" + ax_fc_emi;
            //DirectoryInfo didoc = Directory.CreateDirectory(auxpathdoc);
            string nombrearchivo = "";
            object ObjMiss = System.Reflection.Missing.Value;
            Word.Application ObjWord = new Word.Application();

            nombrearchivo = "CFDI40Facturafast.docx";
            string rutaorigen = DirPrg + "Plantillas\\" + nombrearchivo;
            string rutai = prefactura_.url_pdf;
            int cont = prefactura_.url_pdf.Length - (prefactura_.uuid.Length + 4);
            rutai = rutai.Remove(cont);
            string rutadestino = DirPrg + "Plantillas\\" + rutai + namefile + ".docx";



            //Process[] proceso = Process.GetProcessesByName("word.exe");
            //proceso[0].Kill();

            //System.IO.File.Delete(rutadestino);
            System.IO.File.Copy(rutaorigen, rutadestino, true);

            Word.Document ObjDoc = ObjWord.Documents.Open(rutadestino, ObjMiss);



            //Definir Marcadores
            //object nombre_razon = "RFC_Emisor";
            //object razon_social_emisor = "Razon_Social_Emisor";
            object tipo_comprobante = "Tipo_Comprobante";
            //object lugar_expedicion = "Lugar_Expedicion";
            //object regimen_fiscal = "Regimen_Fiscal";
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
            object uuid = "UUID_";
            object no_certificado_sat = "No_Cetificado_SAT";
            object fecha_timbrado = "Fecha_Timbrado";

            object sello_cfd = "Sello_CFD";
            object sello_sat = "Sello_SAT";
            object complemento_certificacion = "Complemento_Certificacion";
            //Busqueda de marcadores en la plantilla
            //Word.Range nombrerazon = ObjDoc.Bookmarks.get_Item(ref nombre_razon).Range;
            //Word.Range razonsocialemisor = ObjDoc.Bookmarks.get_Item(ref razon_social_emisor).Range;
            Word.Range tipocomprobante = ObjDoc.Bookmarks.get_Item(ref tipo_comprobante).Range;
            //Word.Range lugarexpedicion = ObjDoc.Bookmarks.get_Item(ref lugar_expedicion).Range;
            //Word.Range regimenfiscal = ObjDoc.Bookmarks.get_Item(ref regimen_fiscal).Range;
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



            object Logo_Emisor = "Logo_Emisor";

            ObjDoc.Bookmarks.get_Item(ref Logo_Emisor).Range.InlineShapes.AddPicture((DirPrg + "/img/logos/ff4ee1c2-ed6b-44e3-b449-2905f71b44b4.png"), false, true);



            //Crear Codigo QR
            string fileName = prefactura_.uuid + "-" + prefactura_.rfc_cliente;
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            ASCIIEncoding ASSCII = new ASCIIEncoding();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(ASSCII.GetBytes("https://verificacfdi.facturaelectronica.sat.gob.mx/default.aspx?&id=" + prefactura_.uuid + "&re=" + usuario.rfc + "&rr=" + prefactura_.rfc_cliente + "&tt=" + prefactura_.total + "&fe=" + prefactura_.selloCFDI.Substring(prefactura_.selloCFDI.Length - 8, 8)), QRCodeGenerator.ECCLevel.H);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(2);
            qrCodeImage.Save(DirPrg + "/Plantillas/" + rutai + fileName + ".jpg", ImageFormat.Jpeg);
            //qrCodeImage.Save(@"D:\VS\Formatos\Documentos\Codigos QR\" + archivo + tipoComprobante + "" + fileName + ".jpg", ImageFormat.Jpeg);

            object Imagen_QR = "Imagen_QR";

            ObjDoc.Bookmarks.get_Item(ref Imagen_QR).Range.InlineShapes.AddPicture((DirPrg + "/Plantillas/" + rutai + fileName + ".jpg"), false, true);
            //Fin Crear Codigo QR


            //Agregar texto al marcador
            string tc = prefactura_.tipo_comprobante;
            string auxcad = tc == "I" ? "Ingreso" : tc == "E" ? "Egreso" : tc == "T" ? "Traslado" : tc == "N" ? "Nómina" : tc == "P" ? "Pago" : "Pago";

            //nombrerazon.Text = usuario.rfc;
            //razonsocialemisor.Text = prefactura_.nombre_usuario_rfc;//db.tbc_Clientes.Where(u => u.rfc_usuario == prefactura_.rfc_usuario).Select(u => u.nombre_razon).First();
            tipocomprobante.Text = auxcad;
            //lugarexpedicion.Text = prefactura_.lugar_expedicion;
            //regimenfiscal.Text = prefactura_.reg_fiscal_usuario + "-" + db.tbc_Regimenes.Where(u => u.clave == prefactura_.reg_fiscal_usuario).Select(u => u.regimen).First();
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
            string auxfca = String.Format("{0:yyyy-MM-ddTHH:mm:ss}",prefactura_.fecha_emision);
            fechaemision.Text = auxfca;

            cliente_.Text = prefactura_.nombre_rfc;
            rfcreceptor.Text = prefactura_.rfc_cliente;
            domiciliofiscal.Text = usuario.cp;
            usoCFDI.Text = ucfdi_.clave + "-" + ucfdi_.uso_cfdi;
            regimenfiscalreceptor.Text = prefactura_.clave_reg_fiscal + "-" + db.tbc_Regimenes.Where(u => u.clave == prefactura_.clave_reg_fiscal).Select(u => u.regimen).First();
            //Creacion y definicion de tabla
            var cantProductos = db.tbd_Conceptos_Pre_Factura.Where(s => s.id_pre_factura == prefactura_.id_pre_factura).ToList();
            //valorConc.Count();

            Word.Table TablaProd;
            TablaProd = ObjDoc.Tables.Add(Tablaproductos, 1, 8);

            //int i = 1;
            //for (int z = 0; z <= valorConc.Count - 1; z++)
            //{
            tbc_Paquetes paquete = db.tbc_Paquetes.Where(s => s.id_paquete == valorConc.id_paquete).Single();
            var aux = ""/*valorConc[z].c_unidad_medida*/;
            Decimal canti = 1;
            Decimal total_ = paquete.costo;
            Decimal importet_ = paquete.importe;
            Decimal impuesto_ = paquete.iva;
            //var query = db.tbc_ProdServ.ToList<tbc_ProdServ>().Where(s => s.c_pord_serv == valorConc[z].c_producto).Select(s => s.descripcion).First();
            //int cu = valorConc[z].c_unidad_medida;
            TablaProd.Cell(1, 1).Range.Text = "1.00";
            TablaProd.Cell(1, 2).Range.Text = "E48"/*"[" + valorConc[z].c_unidad_medida + "]" + valorConc[z].unidad*/;
            TablaProd.Cell(1, 3).Range.Text = " "/*valorConc[z].c_prod_serv*/;
            TablaProd.Cell(1, 4).Range.Text = "E48"/*valorConc[z].c_producto*/;
            TablaProd.Cell(1, 5).Range.Text = paquete.descripcion_paquete;
            TablaProd.Cell(1, 6).Range.Text = importet_.ToString("C");
            TablaProd.Cell(1, 6).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
            TablaProd.Cell(1, 7).Range.Text = impuesto_.ToString("C");
            TablaProd.Cell(1, 7).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
            TablaProd.Cell(1, 8).Range.Text = total_.ToString("C");
            TablaProd.Cell(1, 8).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            //    i++;
            //}

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
            decimal descuento_ = Convert.ToDecimal(prefactura_.descuento2 == "" ? "0" : prefactura_.descuento2);
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
            fechatimbrado.Text = String.Format("{0:yyyy-MM-ddTHH:mm:ss}", prefactura_.fca_timbrado);
            
            //Si ya timbro
            string selloCFDI = prefactura_.status != 1 ? db.tbd_Facturas.Where(u => u.sello_sat == prefactura_.selloSAT).Select(u => u.sello_cfdi).First() : "";
            SelloCFD.Text = selloCFDI;
            SelloSAT.Text = prefactura_.selloSAT;


            CCertificacion.Text = "||" + prefactura_.version_timbrado + "|" + prefactura_.uuid + "|" + prefactura_.fca_timbrado + "|" + prefactura_.selloCFDI + "|" + prefactura_.ccertificacion + "||";
            //Cerrar word

            ObjDoc.SaveAs2(DirPrg + "/Plantillas/" + rutai + namefile + ".docx");
            ObjDoc.Close();
            ObjWord.Quit();

            //Crear PDF
            var pdfProcess = new Process();
            pdfProcess.StartInfo.FileName = "" + ruta.url_libreoffice; //*@"C:\Users\Desarrollo Duala\Downloads\LibreOfficePortable\App\libreoffice\program\soffice.exe";
            var rutaii = rutai.Replace("/", @"\");
            pdfProcess.StartInfo.Arguments = "--headless --convert-to pdf " + DirPrg + "Plantillas\\" + rutaii + namefile + ".docx --outdir  " + DirPrg + "Plantillas\\" + rutaii;
            pdfProcess.Start();

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                if (System.IO.File.Exists(DirPrg + "Plantillas\\" + prefactura_.url_pdf))
                {
                    break;
                }
            }


            System.IO.File.Delete(rutadestino);
            System.IO.File.Delete(DirPrg + "Plantillas\\" + rutai + fileName + ".jpg");
            return Json("ok", JsonRequestBehavior.AllowGet);
        }

        public ActionResult DescargarFacturaFacturafast(Int32? idcobro)
        {
            if (Session["tbc_Usuarios"] == null)
                return Json("Error", JsonRequestBehavior.AllowGet);
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            //tbd_Pre_Factura factura = db.tbd_Pre_Factura.Where(s => s.id_pre_factura == idFactura).Single();
            tbd_Cobros cobro = db.tbd_Cobros.Where(s => s.id_cobro == idcobro).Single();
            tbc_Paquetes paquete = db.tbc_Paquetes.Where(s => s.id_paquete == cobro.id_paquete).Single();
            string DirPrg_ = Server.MapPath("~");
            string path = cobro.url_pdf;

            return File(DirPrg_+"Plantillas\\" + path, "application/pdf", "Facturafast " + paquete.descripcion_paquete + ".PDF");
        }
        #endregion
        public ActionResult enviarCorreoPago(int id_, string tipo)
        {
            String mensaje;
            String url = "https://castelanauditores.com/FFDemo/img/cuentas/";
            db = new BD_FFEntities();
            string DirPrg = Server.MapPath("~");
            string fullPath = "";
            string fullPathXML = "";
            string nombre_rfc = ""; string rfc = ""; string url_pdf = ""; string url_xml = ""; string title_ = "";
            string correo_ = "";
            String cuerpo = "";
            
                tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
                tbd_Cobros pago = db.tbd_Cobros.Where(s => s.id_cobro == id_).Single();
                url_pdf = pago.url_pdf;
                url_xml = pago.url_xml;
                fullPath = DirPrg + @"Plantillas\" + url_pdf;
                fullPathXML = DirPrg + @"Plantillas\" + url_xml;
                rfc = usuario.rfc;
                nombre_rfc = usuario.nombre_razon;
                title_ = "Archivos de pagos";
                correo_ = usuario.correo_electronico;            
                cuerpo =
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
                string DireccionaEnviar = correo_; //"programador1@consultoriacastelan.com"
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
    }
}