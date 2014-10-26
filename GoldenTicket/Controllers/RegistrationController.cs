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
            ViewBag.DistrictNames = GetDistrictNames();

            return View(applicant);
        }

        [HttpPost]
        public ActionResult StudentInformation(Applicant applicant)
        {
            /**
             *  Required Fields:
             *  
             *  StudentFirstName
             *  StudentLastName
             *  StudentStreetAddress1
             *  StudentStreetCity
             *  StudentState
             *  StudentZipCode
             *  StudentBirthday
             *  StudentGender
             *  
             * TODO Use Model level validation
             * TODO Learn how to do partial model level validation
             */



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
    }
}