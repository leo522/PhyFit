using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhysicalFit.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Error404()
        {
            ViewBag.ErrorMessage = TempData["ErrorMsg"];
            return View("Error404");
        }

        public ActionResult Maintenance()
        {
            return View("Maintenance");
        }
    }
}