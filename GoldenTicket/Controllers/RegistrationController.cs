using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GoldenTicket.Models;
using GoldenTicket.DAL;

namespace GoldenTicket.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly GoldenTicketDbContext database = new GoldenTicketDbContext();

        // GET: Registration
        public ActionResult Index()
        {
            Session.Clear();

            return View();
        }

        public ActionResult StudentInformation()
        {
            StudentInformationViewSetup();

            var applicant = GetSessionApplicant();

            return applicant != null ? View(applicant) : View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StudentInformation(Applicant applicant)
        {
            if (!IsAuthorizedApplicant(applicant))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

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

            var applicant = GetSessionApplicant();

            if (applicant != null)
            {
                return View(applicant);
            }

            // TODO check to make sure the user isn't too far in the process

            return new HttpStatusCodeResult(HttpStatusCode.Conflict); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardianInformation(Applicant applicant)
        {
            if (!IsAuthorizedApplicant(applicant))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            // Change for required fields

            // Valid model
            if(ModelState.IsValid)
            {
                Save(applicant);
                return RedirectToAction("SchoolSelection");
            }

            // Invalid model
            return View(applicant);
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
            var incomeRanges = new List<SelectListItem>();

            int previousIncomeLine = 0;
            foreach( int householdMembers in Enumerable.Range(2,9))
            {
                var povertyConfig = database.PovertyConfigs.First(p => p.HouseholdMembers == householdMembers);
                var item = new SelectListItem
                {
                    Text = previousIncomeLine.ToString("C") + " to " + povertyConfig.MinimumIncome.ToString("C"),
                    Value = povertyConfig.MinimumIncome.ToString()
                };

                previousIncomeLine = povertyConfig.MinimumIncome;
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

        private Applicant GetSessionApplicant()
        {
            Applicant applicant = null;
            if (Session["applicantID"] != null)
            {
                applicant = database.Applicants.First(a => a.ID.Equals(Session["applicantID"]));
            }

            return applicant;
        }

        private bool IsAuthorizedApplicant(Applicant applicant)
        {
            // Make sure that the student is the one the user is authorized to make (i.e. if an ID is given, it should be the same one in the session)
            return applicant.ID != 0 && Session["applicantID"] != null && !applicant.ID.Equals(Session["applicantID"]);
        }
    }
}