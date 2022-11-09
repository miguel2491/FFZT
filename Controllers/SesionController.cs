using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Facturafast.Models;
using Facturafast.Plantillas;

namespace Facturafast.Controllers
{
    public class SesionController : Controller
    {
        BD_FFEntities db;
        public ActionResult Inicio()
        {
            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }
            if (TempData["Clear"] != null)
            {
                ViewBag.Clear = "Limpiar";
            }


            //var path = Server.MapPath("~");
            //PDF pDF = new PDF(path);
            //pDF.GenerarDocA(@"D:\XML\Facturas\20200102\2E26D423-2A18-184B-BE16-0F5EDD78CAD9.xml");

            //Convertir Plantilla DOCX a PDF
            //Requiere instalacion C:\Users\rene_\Downloads\LibreOfficePortablePrevious\App\libreoffice\program\soffice.exe
            //db = new BD_FFEntities();
            //tbc_Variables_Calculo variables = db.tbc_Variables_Calculo.Single();
            //String nombreWord = "PDFA.docx";
            //var pdfProcess = new Process();
            //pdfProcess.StartInfo.FileName = variables.url_libreoffice;
            //pdfProcess.StartInfo.Arguments = "--headless --convert-to pdf \"" + (variables.url_docx + nombreWord) + "\" --outdir  \"" + variables.url_pdf + "\"";
            //pdfProcess.Start();


            return View();
        }

        [HttpPost]
        public ActionResult InicioSesion(FormCollection formCollection)
        {
            string _Usuario = formCollection["txtUsuario"];
            string _Password = formCollection["txtPassword"];
            Boolean _remmember = formCollection["remember"] != null ? true : false;

            db = new BD_FFEntities();

            tbc_Usuarios usuario = db.tbc_Usuarios.Where(s => s.usuario == _Usuario && s.password == _Password).SingleOrDefault();
            if (usuario != null)
            {
                if (_remmember)
                {
                    TempData["user"] = _Usuario;
                    TempData["pass"] = _Password;
                }

                Session["tbc_Usuarios"] = usuario;
                return RedirectToAction("Inicio", "Panel");
            }

            TempData["Mensaje"] = "No se encontro registro del usuario.";
            TempData["TMensaje"] = "danger";
            return RedirectToAction("Inicio", "Sesion");
        }

        public String SesionAutomatica(String user, String pass)
        {
            String msj = "";
            db = new BD_FFEntities();

            tbc_Usuarios tbc_Usuarios = db.tbc_Usuarios.Where(s => s.usuario == user && s.password == pass).SingleOrDefault();
            if (tbc_Usuarios == null)
            {
                msj = "No se encontro el usuario, verifique sus credenciales de acceso.";
                return msj;
            }
            Session["tbc_Usuarios"] = tbc_Usuarios;
            return "Correcto";
        }
        public ActionResult RecuperarCredenciales()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RecuperarCredenciales(FormCollection formCollection)
        {
            string _Usuario = formCollection["txtUsuario"];

            db = new BD_FFEntities();

            tbc_Usuarios usuario = db.tbc_Usuarios.Where(s => s.usuario == _Usuario).SingleOrDefault();
            if (usuario != null)
            {
                /* Enviar Correo con Credenciales*/


                ViewBag.Mensaje = "Sus credenciales de acceso fueron enviados al correo registrado: " + usuario.correo_electronico + ".";
                ViewBag.TMensaje = "success";
                return View();
            }

            ViewBag.Mensaje = "No se encontro registro del usuario.";
            ViewBag.TMensaje = "danger";
            return View();
        }
        public ActionResult CerrarSesion()
        {
            Session.Clear();
            TempData["Mensaje"] = "Su sesión finalizó correctamente.";
            TempData["TMensaje"] = "success";
            TempData["Clear"] = "Limpiar";
            return RedirectToAction("Inicio", "Sesion");
        }
    }
}