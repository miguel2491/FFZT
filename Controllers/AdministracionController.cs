using Facturafast.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}