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
            TempData["InitPoint"] = preference.SandboxInitPoint;
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

                Decimal comision = (paquete.costo * variable.comision) / 100;

                sumarVendedor.paquetes_vendidos++;
                sumarVendedor.comision_total += comision;
                sumarVendedor.total_vendido += paquete.costo;

                actualizar.comision = comision;
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

                /*

                */



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
                item.importe = Decimal.Round(item.precio_unitario * item.cantidad, 2);
                item.total_descuento = item.tipo_descuento == 1 ? Decimal.Round((item.importe * item.descuento) / 100, 2) : Decimal.Round(item.importe * item.descuento, 2);

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
                    item.total_iva = Decimal.Round(item.importe * (db.tbc_IVA.Where(s => s.id_iva == item.id_iva).Single().@decimal), 2);
                    item.iva_tasa_impuesto = Convert.ToString(Decimal.Round(item.importe * (db.tbc_IVA.Where(s => s.id_iva == item.id_iva).Single().@decimal), 2));
                }
                if (item.id_isr != 0)
                {
                    item.total_isr = Decimal.Round(item.importe * (db.tbc_ISR.Where(s => s.id_isr == item.id_isr).Single().@decimal), 2);
                    item.isr_ret_impuesto = Convert.ToString(Decimal.Round(item.importe * (db.tbc_ISR.Where(s => s.id_isr == item.id_isr).Single().@decimal), 2));
                }
                if (item.id_iva_ret != 0)
                {
                    item.total_iva_ret = Decimal.Round(item.importe * (db.tbc_IVA_Ret.Where(s => s.id_iva_ret == item.id_iva_ret).Single().@decimal), 2);
                    item.iva_ret_impuesto = Convert.ToString(Decimal.Round(item.importe * (db.tbc_IVA_Ret.Where(s => s.id_iva_ret == item.id_iva_ret).Single().@decimal), 2));
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
                item.importe = Decimal.Round(item.precio_unitario * item.cantidad, 2);
                item.total_iva = Decimal.Round(item.importe * (db.tbc_IVA.Where(s => s.id_iva == item.id_iva).Single().@decimal), 2);
                item.total_isr = Decimal.Round(item.importe * (db.tbc_ISR.Where(s => s.id_isr == item.id_isr).Single().@decimal), 2);
                item.total_iva_ret = Decimal.Round(item.importe * (db.tbc_IVA_Ret.Where(s => s.id_iva_ret == item.id_iva_ret).Single().@decimal), 2);
                item.total_descuento = item.tipo_descuento == 1 ? Decimal.Round((item.importe * item.descuento) / 100, 2) : Decimal.Round(item.importe * item.descuento, 2);

                item.total = item.importe + item.total_iva - item.total_iva_ret - item.total_isr - item.total_descuento;

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

                }
                db.SaveChanges();
                return "{\"Estatus\":1, \"Mensaje\":\"\"}";
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

        [HttpPost]
        public String AlmacenarNota(FormCollection formCollection)
        {
            Int32 _idNota = Convert.ToInt32(formCollection["idNotaVenta"]);
            db = new BD_FFEntities();
            tbc_Variables_Calculo variables = db.tbc_Variables_Calculo.Single();
            foreach (string file in Request.Files)
            {
                if (file == "docx" && Request.Files[file].ContentLength > 0)
                {
                    tbd_Notas_Venta nota = db.tbd_Notas_Venta.Where(s => s.id_nota_venta == _idNota).Single();
                    if (nota.url_pdf == "")
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

                        nota.url_pdf = nombre + ".PDF";
                        db.SaveChanges();
                    }
                }
            }
            return "";
        }

        public ActionResult DescargarNotaVenta(Int32? idNotaVenta)
        {
            db = new BD_FFEntities();
            tbc_Variables_Calculo variables = db.tbc_Variables_Calculo.Single();
            tbd_Notas_Venta nota = db.tbd_Notas_Venta.Where(s => s.id_nota_venta == idNotaVenta).Single();
            tbc_Clientes cliente = db.tbc_Clientes.Where(s => s.id_cliente == nota.id_cliente).Single();
            string fullPath = variables.url_pdf + "\\" + nota.url_pdf;
            int i = 0;
            while (i < 30)
            {
                Thread.Sleep(1000);
                if (System.IO.File.Exists(fullPath))
                    break;
            }
            return File(fullPath, "application/pdf", "Nota de Venta " + nota.clave_nota + "_" + cliente.rfc + ".PDF");
        }


        public String enviarCorreosNota(List<ListaCorreos> correos, Int32 txtIdNotaVenta)
        {
            if (Session["tbc_Usuarios"] == null)
                return "{\"Estatus\":0, \"Mensaje\":\"Su sesión a caducado. Vuelva a iniciar sesión nuevamente.\"}";

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();

            foreach (var item in correos)
            {
                Correos enviar = new Correos();
                enviar.emailEnvioNotaVenta(item, txtIdNotaVenta, usuario);
            }

            return "{\"Estatus\":1, \"Mensaje\":\"Los correos electrónicos fueron enviados. Puede checar el estatus de envío desde el registro de la Nota de Venta.\"}";

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

    }
}