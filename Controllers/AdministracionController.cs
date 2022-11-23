using Facturafast.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;

namespace Facturafast.Controllers
{
    public class AdministracionController : Controller
    {
        BD_FFEntities db;

        #region Usuarios

        public ActionResult Usuarios()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            db = new BD_FFEntities();
            var lista = db.tbc_Usuarios.Where(s => s.id_perfil != 3 && s.id_perfil != 4).ToList();
            return View(lista);
        }

        [HttpPost]
        public ActionResult GuardarUsuario(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            int _idUsuario = Convert.ToInt32(formCollection["txtIdUsuario"]);
            string _usuario = formCollection["txtUsuario"];
            string _nombreRazon = formCollection["txtNombreUsuario"];
            int _idPerfil = Convert.ToInt32(formCollection["cmbPerfil"]);
            string _telefono = formCollection["txtTelefonoUsuario"];
            string _correoElectronico = formCollection["txtCorreoElectronicoUsuario"];
            int _idEstatus = Convert.ToInt32(formCollection["cmbEstatus"]);
            string _password = formCollection["txtPasswordUsuario"];

            db = new BD_FFEntities();

            if (_idUsuario == 0)
            {
                tbc_Usuarios nuevo = new tbc_Usuarios
                {
                    calle = "",
                    colonia = "",
                    correo_electronico = _correoElectronico,
                    cp = "",
                    estado = "",
                    fecha_creacion = DateTime.Now,
                    id_estatus = _idEstatus,
                    id_perfil = _idPerfil,
                    id_regimen_fiscal = 1,
                    id_tipo_persona = 1,
                    localidad = "",
                    municipio = "",
                    nombre_razon = _nombreRazon,
                    num_ext = "",
                    num_int = "",
                    password = _password,
                    registro_patronal = "",
                    rfc = "",
                    telefono = _telefono,
                    url_imagen = "img-default.png",
                    usuario = _usuario
                };

                db.tbc_Usuarios.Add(nuevo);
                db.SaveChanges();                

                TempData["Mensaje"] = "Los datos del usuario se almacenaron correctamente.";
                TempData["TMensaje"] = "success";

            }
            else
            {
                tbc_Usuarios actualizar = db.tbc_Usuarios.Where(s => s.id_usuario == _idUsuario).Single();

                actualizar.usuario = _usuario;
                actualizar.telefono = _telefono;
                actualizar.correo_electronico = _correoElectronico;
                actualizar.nombre_razon = _nombreRazon;
                actualizar.id_perfil = _idPerfil;
                actualizar.id_estatus = _idEstatus;
                actualizar.password = _password == "" ? actualizar.password : _password;

                db.SaveChanges();                

                TempData["Mensaje"] = "Los datos del usuario se actualizaron correctamente.";
                TempData["TMensaje"] = "success";
            }

            return RedirectToAction("Usuarios", "Administracion");
        }

        #endregion

        #region Usuarios-Vendedores
        public ActionResult Vendedores()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            db = new BD_FFEntities();
            var lista = db.tbc_Usuarios.Where(s => s.id_perfil == 4).ToList();
            return View(lista);
        }


        [HttpPost]
        public ActionResult GuardarVendedor(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            int _idUsuario = Convert.ToInt32(formCollection["txtIdUsuario"]);
            string _usuario = formCollection["txtUsuario"];
            string _nombreRazon = formCollection["txtNombreUsuario"];
            string _telefono = formCollection["txtTelefonoUsuario"];
            string _correoElectronico = formCollection["txtCorreoElectronicoUsuario"];
            int _idEstatus = Convert.ToInt32(formCollection["cmbEstatus"]);
            string _password = formCollection["txtPasswordUsuario"];

            db = new BD_FFEntities();

            if (_idUsuario == 0)
            {
                tbc_Usuarios nuevo = new tbc_Usuarios
                {
                    calle = "",
                    colonia = "",
                    correo_electronico = _correoElectronico,
                    cp = "",
                    estado = "",
                    fecha_creacion = DateTime.Now,
                    id_estatus = _idEstatus,
                    id_perfil = 4,
                    id_regimen_fiscal = 1,
                    id_tipo_persona = 1,
                    localidad = "",
                    municipio = "",
                    nombre_razon = _nombreRazon,
                    num_ext = "",
                    num_int = "",
                    password = _password,
                    registro_patronal = "",
                    rfc = "",
                    telefono = _telefono,
                    url_imagen = "img-default.png",
                    usuario = _usuario
                };

                db.tbc_Usuarios.Add(nuevo);
                db.SaveChanges();


                tbc_Vendedores nuevo_vendedor = new tbc_Vendedores
                {
                    id_usuario = nuevo.id_usuario,
                    clientes_asignados = 0,
                    comision_total = 0,
                    paquetes_vendidos = 0,
                    total_vendido = 0
                };

                db.tbc_Vendedores.Add(nuevo_vendedor);
                db.SaveChanges();

                TempData["Mensaje"] = "Los datos del vendedor se almacenaron correctamente.";
                TempData["TMensaje"] = "success";

            }
            else
            {
                tbc_Usuarios actualizar = db.tbc_Usuarios.Where(s => s.id_usuario == _idUsuario).Single();

                actualizar.usuario = _usuario;
                actualizar.telefono = _telefono;
                actualizar.correo_electronico = _correoElectronico;
                actualizar.nombre_razon = _nombreRazon;
                actualizar.id_estatus = _idEstatus;
                actualizar.password = _password == "" ? actualizar.password : _password;

                db.SaveChanges();

                TempData["Mensaje"] = "Los datos del vendedor se actualizaron correctamente.";
                TempData["TMensaje"] = "success";
            }

            return RedirectToAction("Vendedores", "Administracion");
        }


        public String ObtenerClientesVendedor(Int32? id)
        {
            if (Session["tbc_Usuarios"] == null)
                return "";

            if (id == null)
                return "";

            db = new BD_FFEntities();

            var lista = db.tbr_Vendedor_Cliente.Where(s => s.id_vendedor_usuario == id)
                .Join(
                db.tbc_Usuarios,
                cliente => cliente.id_cliente_usuario,
                datos => datos.id_usuario,
                (cliente, datos) => new
                {
                    nombre_razon = datos.nombre_razon,
                    rfc = datos.rfc,
                    fecha = cliente.fecha_creacion
                }
                ).ToList();

            String list = "";
            foreach (var s in lista)
            {
                list += "<tr><td></td> <td></td> <td>" + s.nombre_razon + "</td> <td>" + s.rfc + "</td> <td>" + s.fecha.ToString("dd-MM-yyyy") + "</td></tr>";
            }

            return list;
        }


        public string ObtenerComisionesVendedor(Int32? id)
        {
            if (Session["tbc_Usuarios"] == null)
                return "";

            if (id == null)
                return "";

            db = new BD_FFEntities();

            

            var lista = db.tbd_Cobros.Where(s => s.id_vendedor_usuario == id && s.status == "approved")
                .Join(
                db.tbc_Paquetes,
                cobro => cobro.id_paquete,
                paquete => paquete.id_paquete,
                (cobro, paquete) => new
                {
                    concepto = paquete.nombre_paquete,
                    total = cobro.total,
                    comision = cobro.comision,
                    fecha = cobro.fecha_cobro
                }
                ).ToList();

            String list = "";
            foreach (var s in lista)
            {
                list += "<tr><td></td> <td></td> <td>" + s.concepto + "</td> <td>" + s.total.ToString("C") + "</td><td>" + s.comision.ToString("C") + "</td> <td>" + s.fecha.ToString("yyyy-MM-dd HH:mm") + "</td></tr>";
            }

            return list;
        }
        #endregion

        #region Usuario-Clientes

        public ActionResult Clientes()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            db = new BD_FFEntities();
            var lista = db.tbc_Usuarios.Where(s => s.id_perfil == 3).ToList();
            return View(lista);
        }


        [HttpPost]
        public ActionResult GuardarCliente(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            int _idUsuario = Convert.ToInt32(formCollection["txtIdUsuario"]);
            string _usuario = formCollection["txtUsuario"];
            string _nombreRazon = formCollection["txtNombreUsuario"];
            string _telefono = formCollection["txtTelefonoUsuario"];
            string _correoElectronico = formCollection["txtCorreoElectronicoUsuario"];
            string _rfc = formCollection["txtRFCUsuario"];
            int _idEstatus = Convert.ToInt32(formCollection["cmbEstatus"]);
            int _idVendedor = Convert.ToInt32(formCollection["cmbVendedor"]);
            string _password = formCollection["txtPasswordUsuario"];

            db = new BD_FFEntities();

            if (_idUsuario == 0)
            {
                tbc_Usuarios nuevo = new tbc_Usuarios
                {
                    calle = "",
                    colonia = "",
                    correo_electronico = _correoElectronico,
                    cp = "",
                    estado = "",
                    fecha_creacion = DateTime.Now,
                    id_estatus = _idEstatus,
                    id_perfil = 3,
                    id_regimen_fiscal = 1,
                    id_tipo_persona = 1,
                    localidad = "",
                    municipio = "",
                    nombre_razon = _nombreRazon.ToUpper(),
                    num_ext = "",
                    num_int = "",
                    password = _password,
                    registro_patronal = "",
                    rfc = _rfc.ToUpper(),
                    telefono = _telefono,
                    url_imagen = "img-default.png",
                    usuario = _usuario.ToUpper()
                };

                db.tbc_Usuarios.Add(nuevo);
                db.SaveChanges();

                tbr_Vendedor_Cliente relacion = new tbr_Vendedor_Cliente
                {
                    fecha_creacion = DateTime.Now,
                    id_cliente_usuario = nuevo.id_usuario,
                    id_vendedor_usuario = _idVendedor
                };

                tbc_Vendedores sumarCliente = db.tbc_Vendedores.Where(s => s.id_usuario == _idVendedor).Single();
                sumarCliente.clientes_asignados++;

                db.tbr_Vendedor_Cliente.Add(relacion);
                db.SaveChanges();


                tbc_Timbres timbres = new tbc_Timbres
                {
                    id_usuario = nuevo.id_usuario,
                    fecha_vigencia = DateTime.Now.AddMonths(1),
                    timbres_disponibles = 10,
                    timbres_totales = 10,
                    timbres_usados = 0,
                    rfc_usuario = nuevo.rfc
                    
                };

                tbc_Clientes cliente = new tbc_Clientes
                {
                    correo = "",
                    direccion_fiscal = "",
                    fecha_creacion = DateTime.Now,
                    id_uso_cdfi = 13,
                    id_usuario = nuevo.id_usuario,
                    nombre_razon = "PUBLICO EN GENERAL",
                    rfc = "XAXX010101000",
                    telefono = "",
                    rfc_usuario = nuevo.rfc,
                    id_estatus = 1
                };

                db.tbc_Timbres.Add(timbres);
                db.tbc_Clientes.Add(cliente);
                //Send Correo Bienvenida
                emailBienvenida(_nombreRazon, _correoElectronico);
                db.SaveChanges();

                
                PanelProductivo.RegistrationSOAP SoapAdd = new PanelProductivo.RegistrationSOAP();


                PanelProductivo.add can = new PanelProductivo.add();
                PanelProductivo.addResponse respuesta = new PanelProductivo.addResponse();
                can.reseller_username = "cfdi@facturafast.mx";
                can.reseller_password = "F4ctur4f4st_C@st3l4n";
                can.taxpayer_id = _rfc.ToUpper();//RFC EMISOR PARA AGREGAR TIMBRAR
                can.type_user = "P";

                respuesta = SoapAdd.add(can);

                if (respuesta.addResult.success.Value)
                {
                    PanelProductivo.assign asign = new PanelProductivo.assign();
                    PanelProductivo.assignResponse resp = new PanelProductivo.assignResponse();

                    asign.credit = "10";
                    asign.username = "cfdi@facturafast.mx";
                    asign.password = "F4ctur4f4st_C@st3l4n";
                    asign.taxpayer_id = _rfc.ToUpper();

                    resp = SoapAdd.assign(asign);
                }


                TempData["Mensaje"] = "Los datos del cliente se almacenaron correctamente.";
                TempData["TMensaje"] = "success";

            }
            else
            {
                tbc_Usuarios actualizar = db.tbc_Usuarios.Where(s => s.id_usuario == _idUsuario).Single();

                actualizar.rfc = _rfc.ToUpper();
                actualizar.usuario = _usuario.ToUpper();
                actualizar.telefono = _telefono;
                actualizar.correo_electronico = _correoElectronico;
                actualizar.nombre_razon = _nombreRazon.ToUpper();
                actualizar.id_estatus = _idEstatus;
                actualizar.password = _password == "" ? actualizar.password : _password;

                db.SaveChanges();

                tbr_Vendedor_Cliente relacionExiste = db.tbr_Vendedor_Cliente.Where(s => s.id_cliente_usuario == actualizar.id_usuario).SingleOrDefault();
                if (relacionExiste != null)
                {
                    tbc_Vendedores restarCliente = db.tbc_Vendedores.Where(s => s.id_usuario == relacionExiste.id_vendedor_usuario).Single();
                    restarCliente.clientes_asignados--;

                    db.tbr_Vendedor_Cliente.Remove(relacionExiste);
                    db.SaveChanges();
                }

                tbr_Vendedor_Cliente relacion = new tbr_Vendedor_Cliente
                {
                    fecha_creacion = DateTime.Now,
                    id_cliente_usuario = actualizar.id_usuario,
                    id_vendedor_usuario = _idVendedor
                };

                tbc_Vendedores sumarCliente = db.tbc_Vendedores.Where(s => s.id_usuario == _idVendedor).Single();
                sumarCliente.clientes_asignados++;

                db.tbr_Vendedor_Cliente.Add(relacion);
                db.SaveChanges();


                TempData["Mensaje"] = "Los datos del cliente se actualizaron correctamente.";
                TempData["TMensaje"] = "success";
            }

            return RedirectToAction("Clientes", "Administracion");
        }

        #endregion

        #region Configuraciones-Paquetes

        public ActionResult Paquetes()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            db = new BD_FFEntities();
            var lista = db.tbc_Paquetes.ToList();
            return View(lista);
        }

        [HttpPost]
        public ActionResult GuardarPaquete(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            int _idPaquete = Convert.ToInt32(formCollection["txtIdPaquete"]);
            string _nombrePaquete = formCollection["txtNombrePaquete"];
            string _descripcionPaquete = formCollection["txtDescripcionPaquete"];
            int _folios = Convert.ToInt32(formCollection["txtFolios"]);
            decimal _importe = Convert.ToDecimal(formCollection["txtImporte"]);
            decimal _iva = Convert.ToDecimal(formCollection["txtIVA"]);
            int _idEstatus = Convert.ToInt32(formCollection["cmbEstatus"]);
            int _Comision = Convert.ToInt32(formCollection["txtComision"]);
            db = new BD_FFEntities();

            if (_idPaquete == 0)
            {
                tbc_Paquetes nuevo = new tbc_Paquetes
                {
                    costo = _importe + _iva,
                    descripcion_paquete = _descripcionPaquete,
                    fecha_creacion = DateTime.Now,
                    folios = _folios,
                    nombre_paquete = _nombrePaquete,
                    id_estatus = _idEstatus,
                    iva = _iva,
                    importe = _importe,
                    comision = _Comision
                };
                db.tbc_Paquetes.Add(nuevo);
                db.SaveChanges();

                TempData["Mensaje"] = "Los datos del paquete se almacenaron correctamente.";
                TempData["TMensaje"] = "success";
            }
            else
            {
                tbc_Paquetes actualizar = db.tbc_Paquetes.Where(s => s.id_paquete == _idPaquete).Single();

                actualizar.nombre_paquete = _nombrePaquete;
                actualizar.descripcion_paquete = _descripcionPaquete;
                actualizar.folios = _folios;
                actualizar.costo = _importe + _iva;
                actualizar.id_estatus = _idEstatus;
                actualizar.iva = _iva;
                actualizar.importe = _importe;
                actualizar.comision = _Comision;

                db.SaveChanges();

                TempData["Mensaje"] = "Los datos del paquete se actualizaron correctamente.";
                TempData["TMensaje"] = "success";
            }
            return RedirectToAction("Paquetes", "Administracion");
        }
        #endregion

        #region Configuraciones-Variables de Cálculo

        public ActionResult Variables()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            db = new BD_FFEntities();
            var variables = db.tbc_Variables_Calculo.Single();
            return View(variables);
        }


        #endregion

        #region Validaciones

        public String existeUsuario(String usuario, Int32 id)
        {
            db = new BD_FFEntities();
            tbc_Usuarios tbc_Usuarios = db.tbc_Usuarios.Where(s => s.usuario == usuario && s.id_usuario != id).SingleOrDefault();
            if (tbc_Usuarios != null)
                return "is-invalid";
            return "is-valid";
        }              

        #endregion

        #region Utilidades

        public String obtenerVendedores()
        {
            db = new BD_FFEntities();
            var vendedores = db.tbc_Usuarios.Where(s => s.id_perfil == 4 && s.id_estatus == 1).ToList();
            String str = "";
            foreach (var item in vendedores)
            {
                str += "<option value='" + item.id_usuario + "'>" + item.nombre_razon + "</option>";
            }
            return str;
        }

        #endregion
        public void emailBienvenida(string nombre, string correo_send)
        {
            String correo = @"<!DOCTYPE html><html xmlns='http://www.w3.org/1999/xhtml' xmlns:o='urn:schemas-microsoft-com:office:office' style='width: 100%; text-size-adjust: 100%; padding: 0px; margin: 0px; overflow-x: visible !important;'><head><meta http-equiv='Content-Security-Policy' content='script-src 'none'; connect-src 'none'; object-src 'none'; form-action 'none';'><meta charset='UTF-8'><meta content='width=device-width, initial-scale=1' name='viewport'><meta name='x-apple-disable-message-reformatting'><meta http-equiv='X-UA-Compatible' content='IE=edge'><meta content='telephone=no' name='format-detection'><title></title><!--[if (mso 16)]>
                <style type='text/css'>
                    a {text-decoration: none;}
                </style>
                <![endif]--><!--[if gte mso 9]><style>sup { font-size: 100% !important; }</style><![endif]--><!--[if gte mso 9]>

                <![endif]--><style type='text/css'>#outlook a {	padding:0;}.ExternalClass {	width:100%;}.ExternalClass,.ExternalClass p,.ExternalClass span,.ExternalClass font,.ExternalClass td,.ExternalClass div {	line-height:100%;}.es-button {	mso-style-priority:100!important;	text-decoration:none!important;}a[x-apple-data-detectors] {	color:inherit!important;	text-decoration:none!important;	font-size:inherit!important;	font-family:inherit!important;	font-weight:inherit!important;	line-height:inherit!important;}.es-desk-hidden {	display:none;	float:left;	overflow:hidden;	width:0;	max-height:0;	line-height:0;	mso-hide:all;}[data-ogsb] .es-button {	border-width:0!important;	padding:15px 25px 15px 25px!important;}[data-ogsb] .es-button.es-button-1 {	padding:15px 30px!important;}@media only screen and (max-width:600px) {p, ul li, ol li, a { line-height:150%!important } h1, h2, h3, h1 a, h2 a, h3 a { line-height:120%!important } h1 { font-size:30px!important; text-align:center } h2 { font-size:26px!important; text-align:center } h3 { font-size:20px!important; text-align:center } .es-header-body h1 a, .es-content-body h1 a, .es-footer-body h1 a { font-size:30px!important } .es-header-body h2 a, .es-content-body h2 a, .es-footer-body h2 a { font-size:26px!important } .es-header-body h3 a, .es-content-body h3 a, .es-footer-body h3 a { font-size:20px!important } .es-menu td a { font-size:16px!important } .es-header-body p, .es-header-body ul li, .es-header-body ol li, .es-header-body a { font-size:16px!important } .es-content-body p, .es-content-body ul li, .es-content-body ol li, .es-content-body a { font-size:16px!important } .es-footer-body p, .es-footer-body ul li, .es-footer-body ol li, .es-footer-body a { font-size:16px!important } .es-infoblock p, .es-infoblock ul li, .es-infoblock ol li, .es-infoblock a { font-size:12px!important } *[class='gmail-fix'] { display:none!important } .es-m-txt-c, .es-m-txt-c h1, .es-m-txt-c h2, .es-m-txt-c h3 { text-align:center!important } .es-m-txt-r, .es-m-txt-r h1, .es-m-txt-r h2, .es-m-txt-r h3 { text-align:right!important } .es-m-txt-l, .es-m-txt-l h1, .es-m-txt-l h2, .es-m-txt-l h3 { text-align:left!important } .es-m-txt-r img, .es-m-txt-c img, .es-m-txt-l img { display:inline!important } .es-button-border { display:block!important } a.es-button, button.es-button { font-size:20px!important; display:block!important; border-width:15px 25px 15px 25px!important } .es-btn-fw { border-width:10px 0px!important; text-align:center!important } .es-adaptive table, .es-btn-fw, .es-btn-fw-brdr, .es-left, .es-right { width:100%!important } .es-content table, .es-header table, .es-footer table, .es-content, .es-footer, .es-header { width:100%!important; max-width:600px!important } .es-adapt-td { display:block!important; width:100%!important } .adapt-img { width:100%!important; height:auto!important } .es-m-p0 { padding:0px!important } .es-m-p0r { padding-right:0px!important } .es-m-p0l { padding-left:0px!important } .es-m-p0t { padding-top:0px!important } .es-m-p0b { padding-bottom:0!important } .es-m-p20b { padding-bottom:20px!important } .es-mobile-hidden, .es-hidden { display:none!important } tr.es-desk-hidden, td.es-desk-hidden, table.es-desk-hidden { width:auto!important; overflow:visible!important; float:none!important; max-height:inherit!important; line-height:inherit!important } tr.es-desk-hidden { display:table-row!important } table.es-desk-hidden { display:table!important } td.es-desk-menu-hidden { display:table-cell!important } .es-menu td { width:1%!important } table.es-table-not-adapt, .esd-block-html table { width:auto!important } table.es-social { display:inline-block!important } table.es-social td { display:inline-block!important } .es-desk-hidden { display:table-row!important; width:auto!important; overflow:visible!important; max-height:inherit!important } .h-auto { height:auto!important } }</style><link href='assets/css/dev-custom-scroll.css' rel='stylesheet' type='text/css'><base target='_blank'></head><body style='width: 100%; text-size-adjust: 100%; font-family: lato, &quot;helvetica neue&quot;, helvetica, arial, sans-serif; padding: 0px; margin: 0px; overflow-y: scroll !important; visibility: visible !important;'><div class='es-wrapper-color' style='background-color:#F4F4F4'><!--[if gte mso 9]>
			        <v:background xmlns:v='urn:schemas-microsoft-com:vml' fill='t'>
				        <v:fill type='tile' color='#f4f4f4'></v:fill>
			            </v:background>
		            <![endif]--><table class='es-wrapper' width='100%' cellspacing='0' cellpadding='0' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;padding:0;Margin:0;width:100%;height:100%;background-repeat:repeat;background-position:center top;background-color:#F4F4F4'><tbody><tr class='gmail-fix' height='0' style='border-collapse:collapse'><td style='padding:0;Margin:0'><table cellspacing='0' cellpadding='0' border='0' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;width:600px'><tbody><tr style='border-collapse:collapse'><td cellpadding='0' cellspacing='0' border='0' style='padding:0;Margin:0;line-height:1px;min-width:600px' height='0'><img src='https://tlr.stripocdn.email/content/guids/CABINET_837dc1d79e3a5eca5eb1609bfe9fd374/images/41521605538834349.png' style='display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;max-height:0px;min-height:0px;min-width:600px;width:600px' alt='' width='600' height='1'></td></tr></tbody></table></td></tr><tr style='border-collapse:collapse'><td valign='top' style='padding:0;Margin:0'><table class='es-header' cellspacing='0' cellpadding='0' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%;background-color:#FFA73B;background-repeat:repeat;background-position:center top'><tbody><tr style='border-collapse:collapse'><td align='center' bgcolor='#398133' style='padding:0;Margin:0;background-color:#398133'><table class='es-header-body' cellspacing='0' cellpadding='0' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px'><tbody><tr style='border-collapse:collapse'><td align='left' style='Margin:0;padding-bottom:10px;padding-left:10px;padding-right:10px;padding-top:20px'><table width='100%' cellspacing='0' cellpadding='0' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'><tbody><tr style='border-collapse:collapse'><td valign='top' align='center' style='padding:0;Margin:0;width:580px'><table width='100%' cellspacing='0' cellpadding='0' role='presentation' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'><tbody><tr style='border-collapse:collapse'><td align='center' style='padding:10px;Margin:0;font-size:0px'><img src='https://demo.stripocdn.email/content/guids/74fb2e94-2b08-4559-8852-765086bd0c94/images/outputonlinepngtools.png' alt='' style='display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic' width='560' class='adapt-img'></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table><table class='es-content' cellspacing='0' cellpadding='0' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%'><tbody><tr style='border-collapse:collapse'><td style='padding:0;Margin:0;background-color:#398133' bgcolor='#398133' align='center'><table class='es-content-body' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px' cellspacing='0' cellpadding='0' align='center'><tbody><tr style='border-collapse:collapse'><td align='left' style='padding:0;Margin:0'><table width='100%' cellspacing='0' cellpadding='0' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'><tbody><tr style='border-collapse:collapse'><td valign='top' align='center' style='padding:0;Margin:0;width:600px'><table style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;background-color:#ffffff;border-radius:4px' width='100%' cellspacing='0' cellpadding='0' bgcolor='#ffffff' role='presentation'><tbody><tr style='border-collapse:collapse'><td align='center' style='Margin:0;padding-bottom:5px;padding-left:30px;padding-right:30px;padding-top:35px'><h1 style='Margin:0;line-height:58px;mso-line-height-rule:exactly;font-family:lato, 'helvetica neue', helvetica, arial, sans-serif;font-size:48px;font-style:normal;font-weight:normal;color:#111111'>Bienvenid@!</h1>
                    <h2 style='Margin:0;line-height:58px;mso-line-height-rule:exactly;font-family:lato, 'helvetica neue', helvetica, arial, sans-serif;font-size:48px;font-style:normal;font-weight:normal;color:#111111'>" + nombre + @"</h2>
                </td></tr><tr style='border-collapse:collapse'><td bgcolor='#ffffff' align='center' style='Margin:0;padding-top:5px;padding-bottom:5px;padding-left:20px;padding-right:20px;font-size:0'><table width='100%' height='100%' cellspacing='0' cellpadding='0' border='0' role='presentation' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'><tbody><tr style='border-collapse:collapse'><td style='padding:0;Margin:0;border-bottom:1px solid #ffffff;background:#FFFFFF none repeat scroll 0% 0%;height:1px;width:100%;margin:0px'></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table><table class='es-content' cellspacing='0' cellpadding='0' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%'><tbody><tr style='border-collapse:collapse'><td align='center' style='padding:0;Margin:0'><table class='es-content-body' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px' cellspacing='0' cellpadding='0' align='center'><tbody><tr style='border-collapse:collapse'><td align='left' style='padding:0;Margin:0'><table width='100%' cellspacing='0' cellpadding='0' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'><tbody><tr style='border-collapse:collapse'><td valign='top' align='center' style='padding:0;Margin:0;width:600px'><table style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;border-radius:4px;background-color:#ffffff' width='100%' cellspacing='0' cellpadding='0' bgcolor='#ffffff' role='presentation'><tbody><tr style='border-collapse:collapse'><td class='es-m-txt-l' bgcolor='#ffffff' align='left' style='Margin:0;padding-top:20px;padding-bottom:20px;padding-left:30px;padding-right:30px'><p style='Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:lato, 'helvetica neue', helvetica, arial, sans-serif;line-height:27px;color:#666666;font-size:18px'>Tu cuenta ha sido creada satisfactoriamente. Ahora podras iniciar sesión dando clic en el siguiente botón:</p></td></tr><tr style='border-collapse:collapse'><td align='center' style='Margin:0;padding-left:10px;padding-right:10px;padding-top:35px;padding-bottom:35px'><span class='es-button-border' style='border-style:solid;border-color:#398133;background:#398133;border-width:1px;display:inline-block;border-radius:2px;width:auto'><a href='https://cfdi.facturafast.mx/' class='es-button es-button-1' target='_blank' style='mso-style-priority:100 !important;text-decoration:none;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;color:#FFFFFF;font-size:20px;border-style:solid;border-color:#398133;border-width:15px 30px;display:inline-block;background:#398133;border-radius:2px;font-family:helvetica, 'helvetica neue', arial, verdana, sans-serif;font-weight:normal;font-style:normal;line-height:24px;width:auto;text-align:center'>Iniciar sesión</a></span></td></tr><tr style='border-collapse:collapse'><td class='es-m-txt-l' align='center' style='padding:0;Margin:0;padding-top:20px;padding-left:30px;padding-right:30px'><p style='Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:lato, 'helvetica neue', helvetica, arial, sans-serif;line-height:27px;color:#666666;font-size:18px'>Revisa nuestro Aviso de privacidad y Términos y condiciones:</p></td></tr><tr style='border-collapse:collapse'><td class='es-m-txt-l' align='center' style='padding:0;Margin:0;padding-top:20px;padding-left:30px;padding-right:30px'><a target='_blank' href='https://viewstripo.email/' style='-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:underline;color:#FFA73B;font-size:18px'>Aviso de privacidad</a></td></tr><tr style='border-collapse:collapse'><td class='es-m-txt-l' align='center' style='padding:0;Margin:0;padding-top:20px;padding-left:30px;padding-right:30px'><a target='_blank' href='https://viewstripo.email/' style='-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:underline;color:#FFA73B;font-size:18px'>Términos y condiciones</a></td></tr><tr style='border-collapse:collapse'><td class='es-m-txt-l' align='left' style='padding:0;Margin:0;padding-top:20px;padding-left:30px;padding-right:30px'><p style='Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:lato, 'helvetica neue', helvetica, arial, sans-serif;line-height:27px;color:#666666;font-size:18px'>Si tiene alguna duda o pregunta,&nbsp;contáctenos al correo: ventas@facturafast.mx<br>siempre estaremos encantados de ayudarle.</p></td></tr><tr style='border-collapse:collapse'><td class='es-m-txt-l' align='left' style='Margin:0;padding-top:20px;padding-left:30px;padding-right:30px;padding-bottom:40px'><p style='Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:lato, 'helvetica neue', helvetica, arial, sans-serif;line-height:27px;color:#666666;font-size:18px'>Ventas,</p><p style='Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:lato, 'helvetica neue', helvetica, arial, sans-serif;line-height:27px;color:#666666;font-size:18px'>Facturafast</p></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table><table class='es-content' cellspacing='0' cellpadding='0' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%'><tbody><tr style='border-collapse:collapse'><td align='center' style='padding:0;Margin:0'><table class='es-content-body' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px' cellspacing='0' cellpadding='0' align='center'><tbody><tr style='border-collapse:collapse'><td align='left' style='padding:0;Margin:0'><table width='100%' cellspacing='0' cellpadding='0' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'><tbody><tr style='border-collapse:collapse'><td valign='top' align='center' style='padding:0;Margin:0;width:600px'><table width='100%' cellspacing='0' cellpadding='0' role='presentation' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'><tbody><tr style='border-collapse:collapse'><td align='center' style='Margin:0;padding-top:10px;padding-bottom:20px;padding-left:20px;padding-right:20px;font-size:0'><table width='100%' height='100%' cellspacing='0' cellpadding='0' border='0' role='presentation' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'><tbody><tr style='border-collapse:collapse'><td style='padding:0;Margin:0;border-bottom:1px solid #f4f4f4;background:#FFFFFF none repeat scroll 0% 0%;height:1px;width:100%;margin:0px'></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table><table class='es-content' cellspacing='0' cellpadding='0' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%'><tbody><tr style='border-collapse:collapse'><td align='center' style='padding:0;Margin:0'><table class='es-content-body' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px' cellspacing='0' cellpadding='0' align='center'><tbody><tr style='border-collapse:collapse'><td align='left' style='padding:0;Margin:0'><table width='100%' cellspacing='0' cellpadding='0' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'><tbody><tr style='border-collapse:collapse'><td valign='top' align='center' style='padding:0;Margin:0;width:600px'><table style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:separate;border-spacing:0px;background-color:#ffecd1;border-radius:4px' width='100%' cellspacing='0' cellpadding='0' bgcolor='#ffecd1' role='presentation'><tbody><tr style='border-collapse:collapse'><td align='center' style='padding:0;Margin:0;padding-top:30px;padding-left:30px;padding-right:30px'><h3 style='Margin:0;line-height:24px;mso-line-height-rule:exactly;font-family:lato, 'helvetica neue', helvetica, arial, sans-serif;font-size:20px;font-style:normal;font-weight:normal;color:#111111'>¿Necesitas más ayuda?</h3></td></tr><tr style='border-collapse:collapse'><td esdev-links-color='#ffa73b' align='center' style='padding:0;Margin:0;padding-bottom:30px;padding-left:30px;padding-right:30px'><p style='Margin:0;-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;font-family:lato, 'helvetica neue', helvetica, arial, sans-serif;line-height:27px;color:#666666;font-size:18px'>Clic aquí</p></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table><table class='es-content' cellspacing='0' cellpadding='0' align='center' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;table-layout:fixed !important;width:100%'><tbody><tr style='border-collapse:collapse'><td align='center' style='padding:0;Margin:0'><table class='es-content-body' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px;background-color:transparent;width:600px' cellspacing='0' cellpadding='0' align='center'><tbody><tr style='border-collapse:collapse'><td align='left' style='Margin:0;padding-left:20px;padding-right:20px;padding-top:30px;padding-bottom:30px'><table width='100%' cellspacing='0' cellpadding='0' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'><tbody><tr style='border-collapse:collapse'><td valign='top' align='center' style='padding:0;Margin:0;width:560px'><table width='100%' cellspacing='0' cellpadding='0' role='presentation' style='mso-table-lspace:0pt;mso-table-rspace:0pt;border-collapse:collapse;border-spacing:0px'><tbody><tr style='border-collapse:collapse'><td class='es-infoblock made_with' align='center' style='padding:0;Margin:0;line-height:0px;font-size:0px;color:#CCCCCC'><a target='_blank' href='https://viewstripo.email/?utm_source=templates&amp;utm_medium=email&amp;utm_campaign=software2&amp;utm_content=trigger_newsletter' style='-webkit-text-size-adjust:none;-ms-text-size-adjust:none;mso-line-height-rule:exactly;text-decoration:underline;color:#CCCCCC;font-size:12px'><img src='https://demo.stripocdn.email/content/guids/74fb2e94-2b08-4559-8852-765086bd0c94/images/outputonlinepngtools.png' alt='' width='125' style='display:block;border:0;outline:none;text-decoration:none;-ms-interpolation-mode:bicubic'></a></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></div></body></html>";
            try
            {
                string email = "contabilidad@consultoriacastelan.com";
                //string email = "cobranza@consultoriacastelan.com";

                MailMessage msg = new MailMessage();
                string DireccionaEnviar = correo_send;//"rene_slipk@hotmail.com";
                msg.To.Add(DireccionaEnviar);
                msg.From = new MailAddress("ventas@facturafast.mx", "Facturafast", System.Text.Encoding.UTF8);
                //msg.From = new MailAddress("comunicados@facturafast.mx", "FACTURAFAST ", System.Text.Encoding.UTF8);
                
                msg.Subject = "Bienvenid@";
                msg.SubjectEncoding = System.Text.Encoding.UTF8;
                msg.Body = correo;
                /* Archivo adjunto */
                //string fullPath = variables.url_pdf + "\\" + nota.url_pdf;
                //Attachment data = new Attachment(fullPath, MediaTypeNames.Application.Pdf);
                //msg.Attachments.Add(data);
                /*******/
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                msg.IsBodyHtml = true;

                SmtpClient client = new SmtpClient();
                client.Credentials = new NetworkCredential("contabilidad@consultoriacastelan.com", "29tR#+54thfq");
                //client.Credentials = new NetworkCredential(email, "C0nsultor1a*128");

                client.Port = 587;
                client.Host = "mail.consultoriacastelan.com";
                client.EnableSsl = false;
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chai, SslPolicyErrors sslPolicyErrors)
                { return true; };

                client.Send(msg);

            }
            catch (Exception ex)
            {

            }
            finally
            {
                GC.Collect();
            }


        }

        public ActionResult test() {
            string _nombreRazon = "Alexis Sanchez";
            string _correoElectronico = "miguel.angel.dominguez.serrano@gmail.com";
            emailBienvenida(_nombreRazon, _correoElectronico);
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

    }
}