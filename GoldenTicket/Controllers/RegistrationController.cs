using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GoldenTicket.Models;
using GoldenTicket.DAL;

namespace GoldenTicket.Controllers
{
    public class RegistrationController : Controller
    {
        private GoldenTicketDbContext database = new GoldenTicketDbContext();

        // GET: Registration
        public ActionResult Index()
        {
            return View();
        }

        private void StudentInformationViewSetup()
        {
            ViewBag.DistrictNames = GetDistrictNames();
        }

        public ActionResult StudentInformation()
        {
            // If this is an application in progress, get the applicant. Otherwise, make a new one
            Applicant applicant = (Applicant) Session["applicant_id"];
            if (applicant == null)
            {
                applicant = new Applicant();
                database.Applicants.Add(applicant);
                database.SaveChanges();

                Session["applicant_id"] = applicant.ID;
            }

            // Options for display
            StudentInformationViewSetup();

            return View(applicant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StudentInformation(Applicant applicant)
        {
            // Valid fields
            if(ModelState.IsValid)
            {
                Save(applicant);
                return RedirectToAction("GuardianInformation");
            }

            // Invalid fields
            StudentInformationViewSetup();
            return View(applicant);
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

        // ---- Helper Fields ----
        private string[] GetDistrictNames()
        {
            ISet<string> districtNames = new HashSet<string>();

            foreach(Program program in database.Programs)
            {
                districtNames.Add(program.City);
            }

            return districtNames.OrderBy(s=>s).ToArray();
        }

        private void Save(Applicant applicant)
        {
            database.Entry(applicant).State = EntityState.Modified;
            database.SaveChanges();
        }
    }
}