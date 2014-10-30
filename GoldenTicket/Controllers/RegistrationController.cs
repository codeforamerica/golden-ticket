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
            StudentInformationViewSetup();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StudentInformation(Applicant applicant)
        {
            // Check for required fields
            if(string.IsNullOrEmpty(applicant.StudentFirstName))
            {
                ModelState.AddModelError("StudentFirstName", "Student first name must be entered");
            }
            if (string.IsNullOrEmpty(applicant.StudentLastName))
            {
                ModelState.AddModelError("StudentLastName", "Student last name must be entered");
            }
            if (string.IsNullOrEmpty(applicant.StudentStreetAddress1))
            {
                ModelState.AddModelError("StudentStreetAddress1", "Student street address (line 1) must be entered");
            }
            if (string.IsNullOrEmpty(applicant.StudentCity))
            {
                ModelState.AddModelError("StudentCity", "Student city must be entered");
            }
            if (string.IsNullOrEmpty(applicant.StudentZipCode))
            {
                ModelState.AddModelError("StudentZipCode", "Student ZIP code must be entered");
            }
            if (applicant.StudentBirthday == null)
            {
                ModelState.AddModelError("StudentBirthday", "Student birthday must be entered");
            }
            if (applicant.StudentGender == null)
            {
                ModelState.AddModelError("StudentGender", "Student gender must be entered");
            }

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
            GuardianInformationViewSetup();

            Applicant applicant = database.Applicants.Find(Session["applicantID"]);

            
            return View(applicant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardianInformation(Applicant applicant)
        {
            // Valid model
            if(ModelState.IsValid)
            {
                Save(applicant);
                return RedirectToAction("SchoolSelection");
            }

            // Invalid model
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
        private void StudentInformationViewSetup()
        {
            ViewBag.DistrictNames = GetDistrictNames();   
        }

        private void GuardianInformationViewSetup()
        {
            ViewBag.IncomeRanges = GetIncomeRanges();
        }

        private string[] GetDistrictNames()
        {
            ISet<string> districtNames = new HashSet<string>();
            districtNames.Add("");

            foreach(Program program in database.Programs)
            {
                districtNames.Add(program.City);
            }

            return districtNames.OrderBy(s=>s).ToArray();
        }

        private IEnumerable<SelectListItem> GetIncomeRanges()
        {
            List<SelectListItem> incomeRanges = new List<SelectListItem>();

            int previousIncomeLine = 0;
            foreach( int householdMembers in Enumerable.Range(2,10))
            {
                PovertyConfig povertyConfig = database.PovertyConfigs.Where(p => p.HouseholdMembers == householdMembers).First();
                SelectListItem item = new SelectListItem
                {
                    Text = previousIncomeLine + " to " + povertyConfig.MinimumIncome,
                    Value = povertyConfig.MinimumIncome.ToString()
                };
                
                incomeRanges.Add(item);
            }

            return incomeRanges;
        }

        private void Save(Applicant applicant)
        {
            // Add a new applicant
            if(applicant.ID == 0)
            {
                database.Applicants.Add(applicant);
            }
            // Modify an existing applicant
            else
            {
                database.Entry(applicant).State = EntityState.Modified;
            }

            database.SaveChanges();
            Session["applicantID"] = applicant.ID;
        }
    }
}