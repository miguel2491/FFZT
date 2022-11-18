using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Facturafast.Models;

namespace Facturafast.Controllers
{
    public class VendedorController : Controller
    {
        BD_FFEntities db;
        public ActionResult Clientes()
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }
            db = new BD_FFEntities();
            var lista = db.tbr_Vendedor_Cliente.Where(s => s.id_vendedor_usuario == usuario.id_usuario).ToList();
            
            return View(lista);
        }

        public ActionResult PaquetesCliente() 
        {
            if (Session["tbc_Usuarios"] == null)
                return RedirectToAction("Inicio", "Sesion");

            tbc_Usuarios usuario = Session["tbc_Usuarios"] as tbc_Usuarios;
            if (TempData["Mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["Mensaje"];
                ViewBag.TMensaje = TempData["TMensaje"];
            }
            db = new BD_FFEntities();

            var lista = db.tbd_Cobros.Where(s => s.id_vendedor_usuario == usuario.id_usuario && s.status == "approved").ToList();
            return View(lista);
        }
        // GET: Vendedor
        public ActionResult Inicio()
        {
            return View();
        }

        // GET: Vendedor/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Vendedor/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Vendedor/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Vendedor/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Vendedor/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Vendedor/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Vendedor/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
