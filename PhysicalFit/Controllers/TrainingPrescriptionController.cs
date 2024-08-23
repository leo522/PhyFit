using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhysicalFit.Controllers
{
    public class TrainingPrescriptionController : Controller
    {
        private PhFitnessEntities _db = new PhFitnessEntities();

        public ActionResult Prescription()
        {
            return PartialView("_TreadmillPrescription");
        }

    }
}