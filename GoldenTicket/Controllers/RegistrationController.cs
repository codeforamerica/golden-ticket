using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GoldenTicket.Controllers
{
    public class RegistrationController : Controller
    {
        // GET: Registration
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult StudentInformation()
        {
            return View();
        }

        public ActionResult GuardianInformation()
        {
            return View();
        }

        public ActionResult SchoolSelection()
        {
            return View();
        }
        public ActionResult Review()
        {
            return View();
        }

        public ActionResult Confirmation()
        {
            return View();
        }
    }
}