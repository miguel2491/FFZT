using Facturafast.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Facturafast.Controllers
{
    public class CatalogosController : Controller
    {
        BD_FFEntities db;

        public ActionResult Clientes()
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
            var lista = db.tbc_Clientes.Where(s => s.rfc_usuario == usuario.rfc).ToList();
            return View(lista);
        }

        [HttpPost]
        public ActionResult GuardarCliente(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            int _idCliente = Convert.ToInt32(formCollection["txtIdCliente"]);
            string _rfc = formCollection["txtRFCCliente"];
            string _nombreRazon = formCollection["txtNombreRazonCliente"];
            string _telefono = formCollection["txtTelefonoCliente"];
            string _correoElectronico = formCollection["txtCorreoElectronicoCliente"];
            string _direccionFiscal = formCollection["txtDireccionFiscalCliente"];
            int _idUsoCFDI = Convert.ToInt32(formCollection["cmbUsoCFDICliente"]);
            int _idRegimenFiscal = Convert.ToInt32(formCollection["cmbRegimenFiscal"]);
            int _codigoPostal= Convert.ToInt32(formCollection["txtCodigoPostal"]);

            _nombreRazon = _nombreRazon.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();


            if (_rfc == usuario.rfc)
            {
                TempData["Mensaje"] = "El RFC del cliente debe ser diferente al RFC del usuario.";
                TempData["TMensaje"] = "danger";
                return RedirectToAction("Clientes", "Catalogos");
            }


            if (_idCliente == 0)
            {
                tbc_Clientes nuevo = new tbc_Clientes
                {
                    id_usuario = usuario.id_usuario,
                    rfc = _rfc.ToUpper(),
                    nombre_razon = _nombreRazon.ToUpper(),
                    telefono = _telefono,
                    correo = _correoElectronico,
                    direccion_fiscal = _direccionFiscal,
                    fecha_creacion = DateTime.Now,
                    id_uso_cdfi = _idUsoCFDI,
                    id_regimen_fiscal = _idRegimenFiscal,
                    rfc_usuario = usuario.rfc,
                    codigo_postal = _codigoPostal,
                    id_estatus = 1
                };

                db.tbc_Clientes.Add(nuevo);
                db.SaveChanges();

                TempData["Mensaje"] = "Los datos del cliente fueron almacenados correctamente.";
                TempData["TMensaje"] = "success";

            }
            else
            {
                tbc_Clientes actualizar = db.tbc_Clientes.Where(s => s.id_cliente == _idCliente).Single();
                actualizar.id_uso_cdfi = _idUsoCFDI;
                actualizar.id_regimen_fiscal = _idRegimenFiscal;
                actualizar.nombre_razon = _nombreRazon.ToUpper();
                actualizar.rfc = _rfc.ToUpper();
                actualizar.direccion_fiscal = _direccionFiscal;
                actualizar.correo = _correoElectronico;
                actualizar.telefono = _telefono;
                actualizar.codigo_postal = _codigoPostal;

                TempData["Mensaje"] = "Los datos del cliente fueron actualizados correctamente.";
                TempData["TMensaje"] = "success";

                db.SaveChanges();
            }

            return RedirectToAction("Clientes", "Catalogos");

        }

        public ActionResult ServiciosRecurrentes(Int32? id_cliente)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (id_cliente == null)
                return RedirectToAction("Clientes", "Catalogos");

            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var cliente = db.tbc_Clientes.Where(s => s.id_cliente == id_cliente).Single();

            if (cliente.rfc_usuario != usuario.rfc)
            {
                TempData["Mensaje"] = "No tiene acceso a ese cliente.";
                TempData["TMensaje"] = "danger";
                return RedirectToAction("Clientes", "Catalogos");
            }

            var lista = db.tbd_Servicios_Recurrentes.Where(s => s.id_cliente == id_cliente).ToList();
            ViewBag.Cliente = cliente;
            return View(lista);
        }

        public String guardarServicioRecurrente(List<ConceptosNota> conceptos, Int32 txtIdCliente, Int32 txtIdServicioRecurrente, String txtSerie, String txtFechaInicio, Int32 cmbPeriodicidadPago, Int32 txtIdCuenta)
        {
            if (Session["tbc_Usuarios"] == null)
                return "";

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

            DateTime fecha_inicio = Convert.ToDateTime(txtFechaInicio);
            DateTime fecha_proxima = fecha_inicio;
            if (fecha_inicio.Date < DateTime.Now.Date)
            {
                fecha_proxima = fecha_inicio.AddMonths(1);
            }

            if (txtIdServicioRecurrente == 0)
            {
                tbd_Servicios_Recurrentes nueva = new tbd_Servicios_Recurrentes
                {

                    fecha_creacion = DateTime.Now,
                    id_cliente = txtIdCliente,
                    id_estatus = 1,
                    id_usuario = usuario.id_usuario,
                    total = Total,
                    serie = txtSerie,
                    rfc_usuario = usuario.rfc,
                    fecha_inicio = Convert.ToDateTime(txtFechaInicio),
                    fecha_proxima = fecha_proxima,
                    fecha_ultima = fecha_proxima,
                    id_periodicidad = cmbPeriodicidadPago,
                    id_cuenta_bancaria = txtIdCuenta
                };
                db.tbd_Servicios_Recurrentes.Add(nueva);
                db.SaveChanges();

                foreach (var item in conceptos)
                {
                    tbd_Detalles_Servicio_Recurrentes nuevoConcepto = new tbd_Detalles_Servicio_Recurrentes
                    {
                        cantidad = item.cantidad,
                        clave = item.clave,
                        cuota_ieps = 0,
                        descuento = item.descuento,
                        es_tasa_ieps = 1,
                        fecha_creacion = DateTime.Now,
                        id_ieps = 18,
                        id_isr_ret = item.id_isr,
                        id_iva = item.id_iva,
                        id_iva_ret = item.id_iva_ret,
                        id_servicio_recurrente = nueva.id_servicio_recurrente,
                        id_sat = item.id_sat,
                        id_unidad_medida = item.id_unidad_medida,
                        id_usuario = usuario.id_usuario,
                        precio_unitario = item.precio_unitario,
                        tipo_descuento = item.tipo_descuento,
                        concepto = item.concepto,
                        rfc_usuario = usuario.rfc
                    };
                    db.tbd_Detalles_Servicio_Recurrentes.Add(nuevoConcepto);

                }
                db.SaveChanges();
                return "{\"Estatus\":1, \"Mensaje\":\"\"}";
            }


            return "{\"Estatus\":0, \"Mensaje\":\"Ocurrio un error al procesar su petición, inténtelo más tarde.\"}";
        }

        public ActionResult CancelarRecurrencia(Int32? id_recurrencia, Int32? id_cliente_recurrencia)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            if (id_recurrencia != null && id_cliente_recurrencia != null)
            {
                db = new BD_FFEntities();
                tbd_Servicios_Recurrentes cancelar = db.tbd_Servicios_Recurrentes.Where(s => s.id_servicio_recurrente == id_recurrencia).SingleOrDefault();
                cancelar.id_estatus = 2;
                db.SaveChanges();
                TempData["Mensaje"] = "El servicio de recurrencia se inactivo correctamente.";
                TempData["TMensaje"] = "success";
            }
            return RedirectToAction("ServiciosRecurrentes", "Catalogos", new { id_cliente = id_cliente_recurrencia });
        }

        public ActionResult Productos()
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
            var lista = db.tbc_Productos_Servicios.Where(s => s.rfc_usuario == usuario.rfc).ToList();
            return View(lista);
        }

        [HttpPost]
        public ActionResult GuardarProducto(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            int _idProducto = Convert.ToInt32(formCollection["txtIdProducto"]);
            int _idUnidad = Convert.ToInt32(formCollection["txtIdUnidad"]);
            int _idSAT = Convert.ToInt32(formCollection["txtIdSAT"]);
            int _idIVA = Convert.ToInt32(formCollection["cmbIVA"]);
            int _idIVARet = Convert.ToInt32(formCollection["cmbIVARet"]);
            int _idISR = Convert.ToInt32(formCollection["cmbISR"]);
            int _idTipoIEPS = Convert.ToInt32(formCollection["cmbTipoIEPS"]);
            int _idIEPS = Convert.ToInt32(formCollection["cmbIEPS"]);
            decimal _ieps = Convert.ToDecimal(formCollection["txtIEPS"]);
            string _concepto = formCollection["txtConcepto"];
            string _clave = formCollection["txtClave"];
            decimal _precioUnitario = Convert.ToDecimal(formCollection["txtPrecioUnitario"]);


            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();

            if (_idProducto == 0)
            {
                tbc_Productos_Servicios nuevo = new tbc_Productos_Servicios
                {
                    id_usuario = usuario.id_usuario,
                    id_isr = _idISR,
                    clave = _clave,
                    concepto = _concepto,
                    cuota_ieps = _ieps,
                    es_tasa_ieps = _idTipoIEPS,
                    fecha_creacion = DateTime.Now,
                    id_ieps = _idIEPS,
                    id_iva = _idIVA,
                    id_iva_ret = _idIVARet,
                    id_sat = _idSAT,
                    id_unidad_medida = _idUnidad,
                    precio_unitario = _precioUnitario,
                    rfc_usuario = usuario.rfc
                };

                db.tbc_Productos_Servicios.Add(nuevo);
                db.SaveChanges();

                TempData["Mensaje"] = "Los datos del producto o servicio fueron almacenados correctamente.";
                TempData["TMensaje"] = "success";

            }
            else
            {
                tbc_Productos_Servicios actualizar = db.tbc_Productos_Servicios.Where(s => s.id_producto_servicio == _idProducto).Single();
                actualizar.id_isr = _idISR;
                actualizar.clave = _clave;
                actualizar.concepto = _concepto;
                actualizar.cuota_ieps = _ieps;
                actualizar.es_tasa_ieps = _idTipoIEPS;
                actualizar.id_ieps = _idIEPS;
                actualizar.id_iva = _idIVA;
                actualizar.id_iva_ret = _idIVARet;
                actualizar.id_sat = _idSAT;
                actualizar.id_unidad_medida = _idUnidad;
                actualizar.precio_unitario = _precioUnitario;

                TempData["Mensaje"] = "Los datos del producto o servicio fueron actualizados correctamente.";
                TempData["TMensaje"] = "success";

                db.SaveChanges();
            }

            return RedirectToAction("Productos", "Catalogos");

        }

        public ActionResult CuentasBancarias()
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
            var lista = db.tbc_Cuentas_Bancarias.Where(s => s.rfc_usuario == usuario.rfc).ToList();
            return View(lista);
        }

        [HttpPost]
        public ActionResult GuardarCuenta(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            int _idCuenta = Convert.ToInt32(formCollection["txtIdCuenta"]);

            string _banco = formCollection["txtBanco"];
            string _clabe = formCollection["txtCLABE"];
            string _propietario = formCollection["txtPropietario"];



            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();

            if (_idCuenta == 0)
            {
                tbc_Cuentas_Bancarias nuevo = new tbc_Cuentas_Bancarias
                {
                    id_usuario = usuario.id_usuario,
                    banco = _banco,
                    clabe = _clabe,
                    propietario = _propietario,
                    fecha_creacion = DateTime.Now,
                    rfc_usuario = usuario.rfc
                };

                db.tbc_Cuentas_Bancarias.Add(nuevo);
                db.SaveChanges();

                TempData["Mensaje"] = "Los datos de la cuenta bancaria fueron almacenados correctamente.";
                TempData["TMensaje"] = "success";

            }
            else
            {
                tbc_Cuentas_Bancarias actualizar = db.tbc_Cuentas_Bancarias.Where(s => s.id_cuenta_bancaria == _idCuenta).Single();
                actualizar.banco = _banco;
                actualizar.clabe = _clabe;
                actualizar.propietario = _propietario;

                TempData["Mensaje"] = "Los datos de la cuenta bancaria fueron actualizados correctamente.";
                TempData["TMensaje"] = "success";

                db.SaveChanges();
            }

            return RedirectToAction("CuentasBancarias", "Catalogos");
        }

        public ActionResult Empleados()
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
            var lista = db.tbc_Empleados.Where(s => s.rfc_usuario == usuario.rfc).ToList();
            return View(lista);
        }


        [HttpPost]
        public ActionResult GuardarEmpleado(FormCollection formCollection)
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            int _idEmpleado = Convert.ToInt32(formCollection["txtIdEmpleado"]);
            string _nombre = formCollection["txtNombre"];
            string _apePaterno = formCollection["txtApellidoPaterno"];
            string _apeMaterno = formCollection["txtApellidoMaterno"];
            string _curp = formCollection["txtCURP"];
            string _rfc = formCollection["txtRFC"];
            string _nss = formCollection["txtNSS"];
            string _telefono = formCollection["txtTelefono"];
            string _correo = formCollection["txtCorreo"];
            string _numEmpleado = formCollection["txtNumEmpleado"];
            string _puesto = formCollection["txtPuesto"];
            string _departamento = formCollection["txtDepartamento"];
            string _cp = formCollection["txtCP"];
            string _calle = formCollection["txtCalle"];
            string _numExt = formCollection["txtNumExt"];
            string _numInt = formCollection["txtNumInt"];
            string _colonia = formCollection["txtColonia"];
            string _localidad = formCollection["txtLocalidad"];
            string _municipio = formCollection["txtMunicipio"];
            string _estado = formCollection["cmbEstado"];
            int _idGrupo = Convert.ToInt32(formCollection["cmbGrupo"]);
            int _idEstatus = Convert.ToInt32(formCollection["cmbEstatus"]);
            int _idTipoContrato = Convert.ToInt32(formCollection["cmbTipoContrato"]);
            DateTime _iniciorelacionLaboral = Convert.ToDateTime(formCollection["txtInicioRelacionLaboral"]);
            int _idPeriodicidadPago = Convert.ToInt32(formCollection["cmbPeriodicidadPago"]);
            int _idRegimenContratacion = Convert.ToInt32(formCollection["cmbRegimenContratacion"]);
            int _idRiesgoPuesto = Convert.ToInt32(formCollection["cmbRiesgoPuesto"]);
            int _idTipoJornada = Convert.ToInt32(formCollection["cmbTipoJornada"]);
            decimal _salarioDiarioIntegrado = Convert.ToDecimal(formCollection["txtSalarioDiarioIntegrado"]);
            decimal _salarioDiarioCuotasAportaciones = Convert.ToDecimal(formCollection["txtSalarioDiarioCuotasAportaciones"]);


            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();



            if (_idEmpleado == 0)
            {
                tbc_Empleados nuevo = new tbc_Empleados
                {
                    id_usuario = usuario.id_usuario,
                    apellido_materno = _apeMaterno,
                    apellido_paterno = _apePaterno,
                    salario_diario_cuotas_apoprtaciones = _salarioDiarioCuotasAportaciones,
                    calle = _calle,
                    colonia = _colonia,
                    correo_electronico = _correo,
                    cp = _cp,
                    curp = _curp.ToUpper(),
                    departamento = _departamento,
                    estado = _estado,
                    fecha_creacion = DateTime.Now,
                    id_grupo_empleados = _idGrupo,
                    id_periodicidad_pago = _idPeriodicidadPago,
                    id_regimen_contratacion = _idRegimenContratacion,
                    id_riesgo_puesto = _idRiesgoPuesto,
                    id_tipo_contrato = _idTipoContrato,
                    id_tipo_jornada = _idTipoJornada,
                    inicio_relacion_laboral = _iniciorelacionLaboral,
                    localidad = _localidad,
                    municipio = _municipio,
                    nombre = _nombre,
                    nss = _nss,
                    num_empleado = _numEmpleado,
                    num_ext = _numExt,
                    num_int = _numInt,
                    puesto = _puesto,
                    rfc = _rfc.ToUpper(),
                    salario_diario_integrado = _salarioDiarioIntegrado,
                    telefono = _telefono,
                    id_estatus = _idEstatus,
                    rfc_usuario = usuario.rfc
                };

                db.tbc_Empleados.Add(nuevo);
                db.SaveChanges();
                TempData["Mensaje"] = "Los datos del empleado fueron almacenados correctamente.";
                TempData["TMensaje"] = "success";
            }
            else
            {
                tbc_Empleados actualizar = db.tbc_Empleados.Where(s => s.id_empleado == _idEmpleado).Single();
                actualizar.apellido_materno = _apeMaterno;
                actualizar.apellido_paterno = _apePaterno;
                actualizar.salario_diario_cuotas_apoprtaciones = _salarioDiarioCuotasAportaciones;
                actualizar.calle = _calle;
                actualizar.colonia = _colonia;
                actualizar.correo_electronico = _correo;
                actualizar.cp = _cp;
                actualizar.curp = _curp.ToUpper();
                actualizar.departamento = _departamento;
                actualizar.estado = _estado;
                actualizar.id_grupo_empleados = _idGrupo;
                actualizar.id_periodicidad_pago = _idPeriodicidadPago;
                actualizar.id_regimen_contratacion = _idRegimenContratacion;
                actualizar.id_riesgo_puesto = _idRiesgoPuesto;
                actualizar.id_tipo_contrato = _idTipoContrato;
                actualizar.id_tipo_jornada = _idTipoJornada;
                actualizar.inicio_relacion_laboral = _iniciorelacionLaboral;
                actualizar.localidad = _localidad;
                actualizar.municipio = _municipio;
                actualizar.nombre = _nombre;
                actualizar.nss = _nss;
                actualizar.num_empleado = _numEmpleado;
                actualizar.num_ext = _numExt;
                actualizar.num_int = _numInt;
                actualizar.puesto = _puesto;
                actualizar.rfc = _rfc.ToUpper();
                actualizar.salario_diario_integrado = _salarioDiarioIntegrado;
                actualizar.telefono = _telefono;
                actualizar.id_estatus = _idEstatus;

                TempData["Mensaje"] = "Los datos del empleado fueron actualizados correctamente.";
                TempData["TMensaje"] = "success";

                db.SaveChanges();
            }

            return RedirectToAction("Empleados", "Catalogos");

        }
        public ActionResult Impuestos()
        {
            return View();
        }

        #region Utilidades

        public String obtenerIVA()
        {
            db = new BD_FFEntities();
            var iva = db.tbc_IVA.ToList();
            String str = "";
            foreach (var item in iva)
            {
                str += "<option data-factor='" + item.@decimal + "' value='" + item.id_iva + "'>" + item.porcentaje_letra + "</option>";
            }
            return str;
        }

        public String obtenerIVARet()
        {
            db = new BD_FFEntities();
            var iva = db.tbc_IVA_Ret.ToList();
            String str = "";
            foreach (var item in iva)
            {
                str += "<option data-factor='" + item.@decimal + "' value='" + item.id_iva_ret + "'>" + item.porcentaje_letra + "</option>";
            }
            return str;
        }

        public String obtenerIEPS()
        {
            db = new BD_FFEntities();
            var ieps = db.tbc_IEPS.ToList();
            String str = "";
            foreach (var item in ieps)
            {
                str += "<option data-factor='" + item.@decimal + "' value='" + item.id_ieps + "'>" + item.porcentaje_letra + "</option>";
            }
            return str;
        }

        public String obtenerISR()
        {
            db = new BD_FFEntities();
            var isr = db.tbc_ISR.ToList();
            String str = "";
            foreach (var item in isr)
            {
                str += "<option data-factor='" + item.@decimal + "' value='" + item.id_isr + "'>" + item.porcentaje_letra + "</option>";
            }
            return str;
        }

        public String obtenerUnidadesMedida(String term)
        {
            db = new BD_FFEntities();
            var unidades = db.tbc_Unidades_Medida.Where(s => ("[" + s.clave + "] " + s.descripcion).Contains(term)).ToList();
            if (unidades.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in unidades)
            {
                str += "{\"label\": \"" + ("[" + item.clave + "] " + item.descripcion) + "\", \"value\":" + item.id_unidad_medida + "}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerEmbalaje(String term)
        {
            db = new BD_FFEntities();
            var unidades = db.tbc_Tipos_Embalaje.Where(s => ("[" + s.clave_designacion + "] " + s.descripcion).Contains(term)).ToList();
            if (unidades.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in unidades)
            {
                str += "{\"label\": \"" + ("[" + item.clave_designacion + "] " + item.descripcion) + "\", \"value\":" + item.id_tipo_embalaje + "}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerMoneda(String term)
        {
            db = new BD_FFEntities();
            var unidades = db.tbc_Tipos_Moneda.Where(s => ("[" + s.clave_moneda + "] " + s.descripcion).Contains(term)).ToList();
            if (unidades.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in unidades)
            {
                str += "{\"label\": \"" + ("[" + item.clave_moneda + "] " + item.descripcion) + "\", \"value\":" + item.id_moneda + "}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerFraccionesArancelarias(String term)
        {
            db = new BD_FFEntities();
            var unidades = db.tbc_Fracciones_Arancelaria.Where(s => ("[" + s.c_fraccion_arancelaria + "] " + s.descripcion).Contains(term)).ToList();
            if (unidades.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in unidades)
            {
                str += "{\"label\": \"" + ("[" + item.c_fraccion_arancelaria + "] " + item.descripcion) + "\", \"value\":" + item.id_fraccion_arancelaria + "}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerUnidadesPeso(String term)
        {
            db = new BD_FFEntities();
            var unidades = db.tbc_Unidades_Peso.Where(s => ("[" + s.clave_unidad + "] " + s.nombre__unidad_peso).Contains(term)).ToList();
            if (unidades.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in unidades)
            {
                str += "{\"label\": \"" + ("[" + item.clave_unidad + "] " + item.nombre__unidad_peso) + "\", \"value\":" + item.id_unidad_peso + "}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerMaterialPeli(String term)
        {
            db = new BD_FFEntities();
            var unidades = db.tbc_Materiales_Peligrosos.Where(s => ("[" + s.clave_material_peligroso + "] " + s.descripcion).Contains(term)).ToList();
            if (unidades.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in unidades)
            {
                str += "{\"label\": \"" + ("[" + item.clave_material_peligroso + "] " + item.descripcion) + "\", \"value\":" + item.id_material_peligroso + "}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obteneridSAT(String term)
        {
            db = new BD_FFEntities();
            var unidades = db.tbc_ProdServ.Where(s => ("[" + s.c_pord_serv + "] " + s.descripcion).Contains(term)).ToList();
            if (unidades.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in unidades)
            {
                str += "{\"label\": \"" + ("[" + item.c_pord_serv + "] " + item.descripcion) + "\", \"value\":" + item.id_sat + ", \"clave\":" + item.c_pord_serv + " }, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerClaveSTCC(String term)
        {
            db = new BD_FFEntities();
            var unidades = db.tbc_Clave_STCC.Where(s => ("[" + s.clave_stcc + "] " + s.descripcion).Contains(term)).ToList();
            if (unidades.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in unidades)
            {
                str += "{\"label\": \"" + ("[" + item.clave_stcc + "] " + item.descripcion) + "\", \"value\":" + item.id_clave_stcc + "}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerRFCCliente(String term)
        {
            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var clientes = db.tbc_Clientes.Where(s => ("[" + s.rfc + "] " + s.nombre_razon).Contains(term) && s.rfc_usuario == usuario.rfc).ToList();
            if (clientes.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in clientes)
            {
                str += "{\"label\": \"[" + item.rfc + "] " + item.nombre_razon + "\", \"value\":" + item.id_cliente + ", \"name\":\"" + item.nombre_razon + "\", \"rfc\":\"" + item.rfc + "\"}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerTransporte(String term)
        {
            db = new BD_FFEntities();
            var unidades = db.tbc_Tipos_Transporte.Where(s => ("[" + s.clave_transporte + "] " + s.descripcion_tipo_transporte).Contains(term)).ToList();
            if (unidades.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in unidades)
            {
                str += "{\"label\": \"" + ("[" + item.clave_transporte + "] " + item.descripcion_tipo_transporte) + "\", \"value\":" + item.id_tipo_transporte + "}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public string obtenerProductos(String term)
        {
            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var productos = db.tbc_Productos_Servicios.Where(s => s.concepto.Contains(term) && s.rfc_usuario == usuario.rfc).ToList();
            if (productos.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in productos)
            {
                var sat = db.tbc_ProdServ.Where(s => s.id_sat == item.id_sat).Single();
                var unidad = db.tbc_Unidades_Medida.Where(s => s.id_unidad_medida == item.id_unidad_medida).Single();
                str += "{\"label\": \"" + item.concepto +
                    "\", \"value\":\"" + item.concepto +
                    "\", \"clave\":\"" + item.clave +
                    "\", \"id_sat\":" + item.id_sat +
                    ", \"id_producto\":" + item.id_producto_servicio +
                    ", \"c_sat\":\"" + "[" + sat.c_pord_serv + "] " + sat.descripcion +
                    "\", \"id_unidad\":" + unidad.id_unidad_medida +
                    ", \"unidad\":\"" + "[" + unidad.clave + "] " + unidad.descripcion +
                    "\", \"precio\":\"" + item.precio_unitario +
                    "\", \"id_iva\":" + item.id_iva +
                    ", \"id_iva_ret\":" + item.id_iva_ret +
                    ", \"id_isr_ret\":" + item.id_isr + "}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerGrupos()
        {
            if (Session["tbc_Usuarios"] == null)
                return "";

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var grupo = db.tbc_Grupos_Empleados.Where(s => s.rfc_usuario == usuario.rfc).ToList();
            String str = "";
            foreach (var item in grupo)
            {
                str += "<option value='" + item.id_grupo_empleados + "'>" + item.grupo + "</option>";
            }
            return str;
        }
        public String guardarGrupo(String grupo, String reg, String estado)
        {
            if (Session["tbc_Usuarios"] == null)
                return "{\"estatus\":\"0\",\"mensaje\":\"Sesión caducada, vuelva a iniciar sesión.\"}";

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();

            var existe = db.tbc_Grupos_Empleados.Where(s => s.grupo == grupo && s.id_usuario == usuario.id_usuario).SingleOrDefault();
            if (existe != null)
                return "{\"estatus\":\"0\",\"mensaje\":\"El Grupo Empresarial " + grupo + " ya existe.\"}";

            var nuevo = new tbc_Grupos_Empleados
            {
                grupo = grupo,
                fecha_creacion = DateTime.Now,
                id_usuario = usuario.id_usuario,
                estado = estado,
                registro_patronal = reg,
                rfc_usuario = usuario.rfc
            };
            db.tbc_Grupos_Empleados.Add(nuevo);
            db.SaveChanges();

            var lista = db.tbc_Grupos_Empleados.Where(s => s.rfc_usuario == usuario.rfc).ToList();
            String str = "";
            foreach (var item in lista)
            {
                str += "<option value='" + item.id_grupo_empleados + "'>" + item.grupo + "</option>";
            }
            return "{\"estatus\":\"1\",\"mensaje\":\"El Grupo Empresarial se agrego correctamente.\", \"data\":\"" + str + "\"}";
        }
        public String obtenerTipoContrato()
        {
            db = new BD_FFEntities();
            var lista = db.tbc_Tipos_Contratos.ToList();
            String str = "";
            foreach (var item in lista)
            {
                str += "<option value='" + item.id_tipo_contrato + "'>" + item.clave + " - " + item.tipo_contrato + "</option>";
            }
            return str;
        }

        public String obtenerPeriodicidad()
        {
            db = new BD_FFEntities();
            var lista = db.tbc_Periodicidades_Pago.ToList();
            String str = "";
            foreach (var item in lista)
            {
                str += "<option value='" + item.id_periodicidad_pago + "'>" + item.clave + " - " + item.periodicidad_pago + "</option>";
            }
            return str;
        }

        public String obtenerTipoPermiso(string term)
        {
            db = new BD_FFEntities();
            var paises = db.tbc_Tipos_Permiso.Where(s => ("[" + s.clave + "] " + s.descripcion).Contains(term)).ToList();
            if (paises.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in paises)
            {
                str += "{\"label\": \"[" + item.clave + "] " + item.descripcion + "\", \"value\":" + item.id_tipo_permiso + ", \"name\":\"" + item.clave + "\", \"des\":\"" + item.descripcion + "\"}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }

        public String obtenerCodigoTransporteAereo(string term)
        {
            db = new BD_FFEntities();
            var paises = db.tbc_Codigo_Transporte_Aereo.Where(s => ("[" + s.clave_identificacion + "] " + s.nombre_aerolinea).Contains(term)).ToList();
            if (paises.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in paises)
            {
                str += "{\"label\": \"[" + item.clave_identificacion + "] " + item.nombre_aerolinea + "\", \"value\":" + item.id_codigo_transporte_aereo + ", \"name\":\"" + item.clave_identificacion + "\", \"des\":\"" + item.nombre_aerolinea + "\"}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }

        public String obtenerTipoEmbarcacion(string term)
        {
            db = new BD_FFEntities();
            var paises = db.tbc_Config_Maritima.Where(s => ("[" + s.clave_configuracion_maritima + "] " + s.descripcion).Contains(term)).ToList();
            if (paises.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in paises)
            {
                str += "{\"label\": \"[" + item.clave_configuracion_maritima + "] " + item.descripcion + "\", \"value\":" + item.id_config_maritima + ", \"name\":\"" + item.clave_configuracion_maritima + "\", \"des\":\"" + item.clave_configuracion_maritima + "\"}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }

        public String obtenerTipoCarga(string term)
        {
            db = new BD_FFEntities();
            var paises = db.tbc_Clave_Tipo_Carga.Where(s => ("[" + s.clave_tipo_carga + "] " + s.descripcion).Contains(term)).ToList();
            if (paises.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in paises)
            {
                str += "{\"label\": \"[" + item.clave_tipo_carga + "] " + item.descripcion + "\", \"value\":" + item.id_clave_tipo_carga + ", \"name\":\"" + item.clave_tipo_carga + "\", \"des\":\"" + item.clave_tipo_carga + "\"}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerSubRemolque(string term)
        {
            db = new BD_FFEntities();
            var paises = db.tbc_Sub_Tipo_Rem.Where(s => ("[" + s.clave_remolque + "] " + s.remolque).Contains(term)).ToList();
            if (paises.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in paises)
            {
                str += "{\"label\": \"[" + item.clave_remolque + "] " + item.remolque + "\", \"value\":" + item.id_remolque + ", \"name\":\"" + item.clave_remolque + "\"}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerNumAutorizacionNaviera(string term)
        {
            db = new BD_FFEntities();
            var paises = db.tbc_Num_Autorizacion_Naviero.Where(s => (s.numero_autorizacion).Contains(term)).ToList();
            if (paises.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in paises)
            {
                str += "{\"label\": \"[" + item.numero_autorizacion + "] " + item.numero_autorizacion + "\", \"value\":" + item.id_num_autorizacion_naviero + ", \"name\":\"" + item.numero_autorizacion + "\", \"des\":\"" + item.numero_autorizacion + "\"}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerTipoContenedor(string term)
        {
            db = new BD_FFEntities();
            var paises = db.tbc_Contenedor_Maritimo.Where(s => ("[" + s.clave_contenedor_maritimo + "] " + s.descripcion).Contains(term)).ToList();
            if (paises.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in paises)
            {
                str += "{\"label\": \"[" + item.clave_contenedor_maritimo + "] " + item.descripcion + "\", \"value\":" + item.id_contenedor_maritimo + ", \"name\":\"" + item.clave_contenedor_maritimo + "\", \"des\":\"" + item.clave_contenedor_maritimo + "\"}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerConfigVe(string term)
        {
            db = new BD_FFEntities();
            var paises = db.tbc_Config_AutoTransporte.Where(s => ("[" + s.clave + "] " + s.descripcion).Contains(term)).ToList();
            if (paises.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in paises)
            {
                str += "{\"label\": \"[" + item.clave + "] " + item.descripcion + "\", \"value\":" + item.id_conf_autotrans + ", \"name\":\"" + item.clave + "\", \"des\":\"" + item.clave + "\"}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerRegimenContratacion()
        {
            db = new BD_FFEntities();
            var lista = db.tbc_Regimenes_Contratacion.ToList();
            String str = "";
            foreach (var item in lista)
            {
                str += "<option value='" + item.id_regimen_contratacion + "'>" + item.clave + " - " + item.regimen_contratacion + "</option>";
            }
            return str;
        }

        public String obtenerRiesgoPuesto()
        {
            db = new BD_FFEntities();
            var lista = db.tbc_Riesgos_Puesto.ToList();
            String str = "";
            foreach (var item in lista)
            {
                str += "<option value='" + item.id_riesgo_puesto + "'>" + item.clave + " - " + item.riesgo_puesto + "</option>";
            }
            return str;
        }

        public String obtenerTipoJornada()
        {
            db = new BD_FFEntities();
            var lista = db.tbc_Tipos_Jornada.ToList();
            String str = "";
            foreach (var item in lista)
            {
                str += "<option value='" + item.id_tipo_jornada + "'>" + item.clave + " - " + item.tipo_jornada + "</option>";
            }
            return str;
        }

        public String obtenerFormaPago()
        {
            db = new BD_FFEntities();
            var lista = db.tbc_Formas_Pago.ToList();
            String str = "";
            foreach (var item in lista)
            {
                str += "<option value='" + item.id_forma_pago + "'>" + item.clave + " - " + item.forma_pago + "</option>";
            }
            return str;
        }

        public String obtenerMetodoPago()
        {
            db = new BD_FFEntities();
            var lista = db.tbc_Metodos_Pago.ToList();
            String str = "";
            foreach (var item in lista)
            {
                str += "<option value='" + item.id_metodo_pago + "'>" + item.clave + " - " + item.metodo_pago + "</option>";
            }
            return str;
        }

        public String obtenerUsoCFDI()
        {
            db = new BD_FFEntities();
            var lista = db.tbc_Usos_CFDI.ToList();
            String str = "";
            foreach (var item in lista)
            {
                str += "<option value='" + item.id_uso_cfdi + "'>" + item.clave + " - " + item.uso_cfdi + "</option>";
            }
            return str;
        }

        public String cmbRegimenFiscal()
        {
            db = new BD_FFEntities();
            var lista = db.tbc_Regimenes.ToList();
            String str = "";
            foreach (var item in lista)
            {
                str += "<option value='" + item.id_regimen_fiscal + "'>" + item.clave + " - " + item.regimen + "</option>";
            }
            return str;
        }
        public String obtenerNumPermiso(String term)
        {
            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var autotrans = db.tbd_Autotransporte.Where(s => ("[" + s.num_permiso_sct + "] " + s.num_permiso_sct).Contains(term) && s.id_usuario == usuario.id_usuario).ToList();
            if (autotrans.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in autotrans)
            {
                str += "{\"label\": \"[" + item.placa_vm + "] " + item.num_permiso_sct + "\", \"value\":" + item.id_autotransporte + ", \"name\":\"" + item.num_permiso_sct + "\"}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String obtenerCuentas()
        {
            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            var lista = db.tbc_Cuentas_Bancarias.Where(s => s.rfc_usuario == usuario.rfc).ToList();
            String str = "";
            foreach (var item in lista)
            {
                str += "<option value='" + item.id_cuenta_bancaria + "'>[" + item.banco + "] " + item.propietario + "</option>";
            }
            return str;
        }

        public String obtenerContactos(Int32? id)
        {
            if (Session["tbc_Usuarios"] == null)
                return "";

            if (id == null)
                return "";
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var lista = db.tbc_Contactos_Cliente.Where(s => s.id_cliente == id).ToList();
            String list = "";
            foreach (var item in lista)
            {
                list += "<tr><td></td><td> " + "<a href='#' onclick='eliminarContacto(" + item.id_contacto_cliente + ");' class='btn btn-danger btn-sm' title='Eliminar Contacto' ><i class='fas fa-trash'></i></a>" + " </td> <td>" + item.nombre + "</td> <td>" + item.puesto + "</td> <td>" + item.correo_electronico + "</td></tr>";
            }
            return list;
        }
        public String obtenerPais(String term)
        {
            db = new BD_FFEntities();
            var paises = db.tbc_Paises.Where(s => ("[" + s.clave_pais + "] " + s.descripcion).Contains(term)).ToList();
            if (paises.Count == 0)
            {
                return "[]";
            }
            String str = "[";
            foreach (var item in paises)
            {
                str += "{\"label\": \"[" + item.clave_pais + "] " + item.descripcion + "\", \"value\":" + item.id_pais + ", \"name\":\"" + item.clave_pais + "\", \"des\":\"" + item.descripcion + "\"}, ";
            }
            return str.Substring(0, str.Length - 2) + "]";
        }
        public String agregarContactos(Int32? id, String nombre, String puesto, String correo)
        {
            if (Session["tbc_Usuarios"] == null)
                return "";

            if (id == null)
                return "";

            db = new BD_FFEntities();
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            tbc_Contactos_Cliente nuevo = new tbc_Contactos_Cliente
            {
                correo_electronico = correo,
                fecha_creacion = DateTime.Now,
                id_cliente = id.Value,
                id_usuario = usuario.id_usuario,
                nombre = nombre,
                puesto = puesto,
                rfc_usuario = usuario.rfc
            };
            db.tbc_Contactos_Cliente.Add(nuevo);
            db.SaveChanges();
            db = new BD_FFEntities();
            var lista = db.tbc_Contactos_Cliente.Where(s => s.id_cliente == id.Value).ToList();
            String list = "";
            foreach (var item in lista)
            {
                list += "<tr><td></td><td> " + "<a href='#' onclick='eliminarContacto(" + item.id_contacto_cliente + ");' class='btn btn-danger btn-sm' title='Eliminar Contacto' ><i class='fas fa-trash'></i></a>" + " </td> <td>" + item.nombre + "</td> <td>" + item.puesto + "</td> <td>" + item.correo_electronico + "</td></tr>";
            }
            return list;
        }

        public string eliminarContactos(Int32? id)
        {
            if (Session["tbc_Usuarios"] == null)
                return "";

            if (id == null)
                return "";
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            Int32 id_cliente = 0;
            tbc_Contactos_Cliente eliminar = db.tbc_Contactos_Cliente.Where(s => s.id_contacto_cliente == id.Value).SingleOrDefault();
            if (eliminar != null)
            {
                id_cliente = eliminar.id_cliente;
                if (eliminar.rfc_usuario == usuario.rfc)
                {
                    db.tbc_Contactos_Cliente.Remove(eliminar);
                    db.SaveChanges();
                }
                else
                {
                    return "0";
                }

            }
            db = new BD_FFEntities();
            var lista = db.tbc_Contactos_Cliente.Where(s => s.id_cliente == id_cliente).ToList();
            String list = "";
            foreach (var item in lista)
            {
                list += "<tr><td></td><td> " + "<a href='#' onclick='eliminarContacto(" + item.id_contacto_cliente + ");' class='btn btn-danger btn-sm' title='Eliminar Contacto' ><i class='fas fa-trash'></i></a>" + " </td> <td>" + item.nombre + "</td> <td>" + item.puesto + "</td> <td>" + item.correo_electronico + "</td></tr>";
            }
            return list;
        }

        public String obtenerContactosEnvio(Int32? id)
        {
            if (Session["tbc_Usuarios"] == null)
                return "";

            if (id == null)
                return "";
            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            db = new BD_FFEntities();
            var lista = db.tbc_Contactos_Cliente.Where(s => s.id_cliente == id).ToList();
            String list = "";

            var cliente = db.tbc_Clientes.Where(s => s.id_cliente == id).Single();

            list += "<tr><td></td><td> " + "<input type='checkbox' />" + " </td> <td>" + cliente.nombre_razon + "</td> <td>" + "Principal" + "</td> <td>" + cliente.correo + "</td></tr>";


            foreach (var item in lista)
            {
                list += "<tr><td></td><td> " + "<input type='checkbox' />" + " </td> <td>" + item.nombre + "</td> <td>" + item.puesto + "</td> <td>" + item.correo_electronico + "</td></tr>";
            }
            return list;
        }

        #endregion

        #region Validaciones

        public String existeRFCCliente(String rfc, Int32 id)
        {
            if (Session["tbc_Usuarios"] == null)
                return "is-invalid";

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;

            db = new BD_FFEntities();
            tbc_Clientes tbc_Clientes = db.tbc_Clientes.Where(s => s.rfc == rfc && s.id_cliente != id && s.id_usuario == usuario.id_usuario).SingleOrDefault();
            if (tbc_Clientes != null)
                return "is-invalid";
            return "is-valid";
        }

        public String existeCURPEmpleado(String curp, Int32 id)
        {
            if (Session["tbc_Usuarios"] == null)
                return "is-invalid";

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;

            db = new BD_FFEntities();
            tbc_Empleados tbc_Empleados = db.tbc_Empleados.Where(s => s.curp == curp && s.id_empleado != id && s.id_usuario == usuario.id_usuario).SingleOrDefault();
            if (tbc_Empleados != null)
                return "is-invalid";
            return "is-valid";
        }

        public String existeRFCEmpleado(String rfc, Int32 id)
        {
            if (Session["tbc_Usuarios"] == null)
                return "is-invalid";

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;

            db = new BD_FFEntities();
            tbc_Empleados tbc_Empleados = db.tbc_Empleados.Where(s => s.rfc == rfc && s.id_empleado != id && s.id_usuario == usuario.id_usuario).SingleOrDefault();
            if (tbc_Empleados != null)
                return "is-invalid";
            return "is-valid";
        }

        public String existeNSSEmpleado(String nss, Int32 id)
        {
            if (Session["tbc_Usuarios"] == null)
                return "is-invalid";

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;

            db = new BD_FFEntities();
            tbc_Empleados tbc_Empleados = db.tbc_Empleados.Where(s => s.nss == nss && s.id_empleado != id && s.id_usuario == usuario.id_usuario).SingleOrDefault();
            if (tbc_Empleados != null)
                return "is-invalid";
            return "is-valid";
        }

        public String existeNumEmpleado(String numEmpleado, Int32 id)
        {
            if (Session["tbc_Usuarios"] == null)
                return "is-invalid";

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;

            db = new BD_FFEntities();
            tbc_Empleados tbc_Empleados = db.tbc_Empleados.Where(s => s.num_empleado == numEmpleado && s.id_empleado != id && s.id_usuario == usuario.id_usuario).SingleOrDefault();
            if (tbc_Empleados != null)
                return "is-invalid";
            return "is-valid";
        }

        #endregion
    }
}