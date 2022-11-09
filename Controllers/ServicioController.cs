using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Facturafast.Models;
using Microsoft.Web.WebView2.WinForms;

namespace Facturafast.Controllers
{
    public class ServicioController : Controller
    {
        // GET: Servicio
        public ActionResult GenerarNotaVentaRecurrente(Int32? id_nota_venta, String token)
        {
            String validaToken = "QWERT12345";
            if (token == validaToken)
            {
                BD_FFEntities db = new BD_FFEntities();
                tbd_Notas_Venta nota = db.tbd_Notas_Venta.Where(s => s.id_nota_venta == id_nota_venta).SingleOrDefault();
                if (nota == null)
                {
                    return View(new tbd_Notas_Venta());
                }                
                return View(nota);
            }
            return View(new tbd_Notas_Venta());
        }       
    }
}
