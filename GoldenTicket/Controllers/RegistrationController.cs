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

        private static readonly DateTime AGE_4_BY_DATE = new DateTime(DateTime.Today.Year, 9, 1);

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

            return View(applicant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StudentInformation(Applicant applicant)
        {
            // Make sure someone isn't playing with the ID from the form
            if(!IsAuthorizedApplicant(applicant))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Conflict, "Applicant submitted is not in the session");
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

            if (applicant.StudentBirthday != null && !IsAgeEligible(applicant.StudentBirthday.Value))
            {
                ModelState.AddModelError("StudentBirthday", "Student is not old enough for pre-kindergarten. Try again next year or fix the birthday if it was entered wrong.");
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

            return View(applicant); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardianInformation(Applicant applicant)
        {
            // Check required fields


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
                applicant = database.Applicants.Find((int) Session["applicantID"]);
            }
            else
            {
                applicant = new Applicant();
                Save(applicant);
            }

            return applicant;
        }

        private bool IsAuthorizedApplicant(Applicant applicant)
        {
            // Make sure that the student is the one the user is authorized to make (i.e. if an ID is given, it should be the same one in the session)
            bool isApplicantNew = applicant.ID == 0;
            bool isActiveSession = Session["applicantID"] != null;

            // If a new sessions
            if (isApplicantNew && !isActiveSession)
            {
                return true;
            }

            // If existing session, check to make sure session applicant ID matches the one submitted
            bool isActiveApplicantSameAsSubmitted = applicant.ID.Equals(Session["applicantID"]);
            return !isApplicantNew && isActiveSession && isActiveApplicantSameAsSubmitted;
        }

        private static bool IsAgeEligible(DateTime birthday)
        {
            int ageByCutoff = AGE_4_BY_DATE.Year - birthday.Year;
            DateTime adjustedDate = AGE_4_BY_DATE.AddYears(-ageByCutoff);
            if(birthday > adjustedDate)
            {
                ageByCutoff--;
            }

            return (ageByCutoff == 4);
        }
    }
}