using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GoldenTicket.Models;
using GoldenTicket.DAL;
using GoldenTicket.Resources;

namespace GoldenTicket.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly GoldenTicketDbContext database = new GoldenTicketDbContext();
        private GlobalConfig config;

        private static readonly DateTime AGE_4_BY_DATE = new DateTime(DateTime.Today.Year, 9, 1);

        public RegistrationController()
        {
            config = database.GlobalConfigs.First();
        }

        // GET: Registration
        public ActionResult Index()
        {
            Session.Clear();

            ViewBag.GlobalConfig = GetGlobalConfig();

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
            if(!IsAuthorizedApplicant(applicant) || !IsActiveSession()) // TODO Use AOP/Annotations to do this instead
            {
                return RedirectToAction("Index");
            }

            // Check for required fields
            if(string.IsNullOrEmpty(applicant.StudentFirstName))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentFirstName);
                ModelState.AddModelError("StudentFirstName", message);
            }
            if (string.IsNullOrEmpty(applicant.StudentLastName))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentLastName);
                ModelState.AddModelError("StudentLastName", message);
            }
            if (string.IsNullOrEmpty(applicant.StudentStreetAddress1))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentStreetAddress1);
                ModelState.AddModelError("StudentStreetAddress1", message);
            }
            if (string.IsNullOrEmpty(applicant.StudentCity))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentCity);
                ModelState.AddModelError("StudentCity", message);
            }
            if (string.IsNullOrEmpty(applicant.StudentZipCode))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentZipCode);
                ModelState.AddModelError("StudentZipCode", message);
            }
            if (applicant.StudentBirthday == null)
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentBirthday);
                ModelState.AddModelError("StudentBirthday", message);
            }
            if (applicant.StudentGender == null)
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentGender);
                ModelState.AddModelError("StudentGender", message);
            }

            if (applicant.StudentBirthday != null && !IsAgeEligible(applicant.StudentBirthday.Value))
            {
                var message = string.Format(GoldenTicketText.IneligibleBirthday, DateTime.Today.Year.ToString());
                ModelState.AddModelError("StudentBirthday", message);
            }

            // Valid fields
            if(ModelState.IsValid)
            {
                SaveStudentInformation(applicant);

                return RedirectToAction("GuardianInformation");
            }

            // Invalid fields
            StudentInformationViewSetup();
            return View(applicant);
        }

        public ActionResult GuardianInformation()
        {
            if (!IsActiveSession()) //TODO Do this with AOP/Annotations instead
            {
                return RedirectToAction("Index");
            }

            GuardianInformationViewSetup();

            var applicant = GetSessionApplicant();

            return View(applicant); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardianInformation(Applicant applicant)
        {
            // Make sure someone isn't playing with the ID from the form
            if (!IsAuthorizedApplicant(applicant) || !IsActiveSession()) // TODO Use AOP/Annotations to do this instead
            {
                return RedirectToAction("Index");
            }

            // Check required fields
            if( string.IsNullOrEmpty(applicant.Contact1FirstName) )
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.Contact1FirstName);
                ModelState.AddModelError("Contact1FirstName", message);
            }
            if( string.IsNullOrEmpty(applicant.Contact1LastName) )
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.Contact1LastName);
                ModelState.AddModelError("Contact1FirstName", message);
            }
            if( string.IsNullOrEmpty(applicant.Contact1Phone) )
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.Contact1Phone);
                ModelState.AddModelError("Contact1Phone", message);
            }
            if( string.IsNullOrEmpty(applicant.Contact1Email) )
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.Contact1Email);
                ModelState.AddModelError("Contact1Email", message);
            }
            if( string.IsNullOrEmpty(applicant.Contact1Relationship) )
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.Contact1Relationship);
                ModelState.AddModelError("Contact1Relationship", message);
            }          
            if( applicant.HouseholdMembers == null || applicant.HouseholdMembers < 2)
            {
                ModelState.AddModelError("HouseholdMembers", GoldenTicketText.HouseholdMembersMissingOrInvalid);
            }
            if( applicant.HouseholdMonthlyIncome == null || applicant.HouseholdMonthlyIncome == 0) // 1 is the bottom range, although to users 0 will appear as the minimum. This will help with validation checking, since an empty selection is assigned 0 by MVC framework. This does not impact income calculations for lottery selection.
            {
                ModelState.AddModelError("HouseholdMonthlyIncome", GoldenTicketText.HouseholdIncomeMissing);
            }

            // Validate model
            if(ModelState.IsValid)
            {
                SaveGuardianInformation(applicant);

                return RedirectToAction("SchoolSelection");
            }

            // Invalid model
            GuardianInformationViewSetup();
            return View(applicant);
        }

        public ActionResult SchoolSelection()
        {
            if (!IsActiveSession()) //TODO Do this with AOP/Annotations instead
            {
                return RedirectToAction("Index");
            }

            var applicant = GetSessionApplicant();
            SchoolSelectionViewSetup(applicant);

            return View(applicant); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SchoolSelection(Applicant applicant, FormCollection formCollection)
        {
            // Make sure someone isn't playing with the ID from the form
            if (!IsAuthorizedApplicant(applicant) || !IsActiveSession()) // TODO Use AOP/Annotations to do this instead
            {
                return RedirectToAction("Index");
            }

            // At least one program needs to be selected
            applicant = GetSessionApplicant(); // fills in the other properties, other than just Applicant ID
            var programIds = new List<int>();
            if(formCollection["programs"] == null || formCollection["programs"].Count() <= 0)
            {
                ModelState.AddModelError("programs", GoldenTicketText.NoSchoolSelected);
                SchoolSelectionViewSetup(applicant);
                return View(applicant);
            }
            else
            {
                var programIdStrs = formCollection["programs"].Split(',').ToList();
                programIdStrs.ForEach(idStr => programIds.Add(int.Parse(idStr)));
            }

            // Remove existing applications for this user
            var applieds = database.Applieds.Where(applied => applied.ApplicantID == applicant.ID).ToList();
            applieds.ForEach(a => database.Applieds.Remove(a));

            // Add new Applied associations (between program and program)
            var populatedApplicant = database.Applicants.Find(applicant.ID);
            foreach( var programId in programIds )
            {
                var applied = new Applied();
                applied.ApplicantID = applicant.ID;
                applied.ProgramID = programId;

                // Confirm that the program ID is within the city lived in (no sneakers into other districts)
                var program = database.Programs.Find(programId);
                if(program != null && program.City.Equals(populatedApplicant.StudentCity, StringComparison.CurrentCultureIgnoreCase))
                {
                    database.Applieds.Add(applied);
                }
            }

            database.SaveChanges();
            return RedirectToAction("Review");
        }

        public ActionResult Review()
        {
            if(!IsActiveSession()) //TODO Do this with AOP/Annotations instead
            {
                return RedirectToAction("Index");
            }

            var applicant = GetSessionApplicant();
            
            ReviewViewSetup(applicant);

            return View(applicant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Review(Applicant applicant)
        {
            // Make sure someone isn't playing with the ID from the form
            if (!IsAuthorizedApplicant(applicant) || !IsActiveSession()) // TODO Use AOP/Annotations to do this instead
            {
                return RedirectToAction("Index");
            }

            applicant.ConfirmationCode = Guid.NewGuid().ToString().Split('-')[0].ToUpper();
            SaveReview(applicant);

            return RedirectToAction("Confirmation");
        }

        public ActionResult Confirmation()
        {
            if (!IsActiveSession()) //TODO Do this with AOP/Annotations instead
            {
                return RedirectToAction("Index");
            }

            var applicant = GetSessionApplicant();

            Session.Clear();

            ViewBag.GlobalConfig = GetGlobalConfig();

            return View(applicant);
        }

        // ---- Helper Fields ----
        private void StudentInformationViewSetup()
        {
            ViewBag.DistrictNames = GetDistrictNames();   
        }

        private void GuardianInformationViewSetup()
        {
            var incomeRanges = GetIncomeRanges();
            ViewBag.IncomeRanges = incomeRanges;
            ViewBag.MaxIncome = incomeRanges.Last().Value;
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

            int previousIncomeLine = 1; // 1 is the bottom range, although to users 0 will appear as the minimum. This will help with validation checking.
            foreach( int householdMembers in Enumerable.Range(2,9))
            {
                var povertyConfig = database.PovertyConfigs.First(p => p.HouseholdMembers == householdMembers);
                var item = new SelectListItem
                {
                    Text = previousIncomeLine.ToString("C") + " - " + povertyConfig.MinimumIncome.ToString("C"),
                    Value = povertyConfig.MinimumIncome.ToString()
                };

                previousIncomeLine = povertyConfig.MinimumIncome;
                incomeRanges.Add(item);
            }

            var maxIncome = previousIncomeLine + 1;
            var maxRange = new SelectListItem
            {
                Text =  string.Format(GoldenTicketText.OrMore, maxIncome.ToString("C")),
                Value = maxIncome.ToString()
            };
            incomeRanges.Add(maxRange);

            return incomeRanges;
        }

        private void SaveStudentInformation(Applicant applicant)
        {
            // Add a new applicant
            if(applicant.ID == 0)
            {
                database.Applicants.Add(applicant);
            }
            // Modify an existing applicant
            else
            {
                database.Applicants.Attach(applicant);
                var applicantEntry = database.Entry(applicant);

                applicantEntry.Property(a => a.StudentFirstName).IsModified = true;
                applicantEntry.Property(a => a.StudentMiddleName).IsModified = true;
                applicantEntry.Property(a => a.StudentLastName).IsModified = true;
                applicantEntry.Property(a => a.StudentStreetAddress1).IsModified = true;
                applicantEntry.Property(a => a.StudentStreetAddress2).IsModified = true;
                applicantEntry.Property(a => a.StudentCity).IsModified = true;
                applicantEntry.Property(a => a.StudentZipCode).IsModified = true;
                applicantEntry.Property(a => a.StudentBirthday).IsModified = true;
                applicantEntry.Property(a => a.StudentGender).IsModified = true;
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
                SaveStudentInformation(applicant);
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

        private void SaveGuardianInformation(Applicant applicant)
        {
            database.Applicants.Attach(applicant);
            var applicantEntry = database.Entry(applicant);

            applicantEntry.Property(a => a.Contact1FirstName).IsModified = true;
            applicantEntry.Property(a => a.Contact1LastName).IsModified = true;
            applicantEntry.Property(a => a.Contact1Phone).IsModified = true;
            applicantEntry.Property(a => a.Contact1Email).IsModified = true;
            applicantEntry.Property(a => a.Contact1Relationship).IsModified = true;
            applicantEntry.Property(a => a.Contact2FirstName).IsModified = true;
            applicantEntry.Property(a => a.Contact2LastName).IsModified = true;
            applicantEntry.Property(a => a.Contact2Phone).IsModified = true;
            applicantEntry.Property(a => a.Contact2Email).IsModified = true;
            applicantEntry.Property(a => a.Contact2Relationship).IsModified = true;
            applicantEntry.Property(a => a.HouseholdMembers).IsModified = true;
            applicantEntry.Property(a => a.HouseholdMonthlyIncome).IsModified = true;

            database.SaveChanges();
        }

        private void SchoolSelectionViewSetup(Applicant applicant)
        {
            var eligiblePrograms = database.Programs.Where(p => p.City == applicant.StudentCity).OrderBy(p => p.Name).ToList();
            ViewBag.Programs = eligiblePrograms;

            var applieds = database.Applieds.Where(a => a.ApplicantID == applicant.ID).ToList();
            ViewBag.Applieds = applieds;
        }

        private void ReviewViewSetup(Applicant applicant)
        {
            var applieds = database.Applieds.Where(a => a.ApplicantID == applicant.ID).ToList();
            var programs = new List<Program>();

            applieds.ForEach(a => programs.Add(a.Program));

            ViewBag.Programs = programs;

            ViewBag.NotificationDate = config.NotificationDate;
        }

        private void SaveReview(Applicant applicant)
        {
            database.Applicants.Attach(applicant);
            var applicantEntry = database.Entry(applicant);

            applicantEntry.Property(a => a.ConfirmationCode).IsModified = true;

            database.SaveChanges();
        }

        private bool IsActiveSession()
        {
            return Session["applicantID"] != null;
        }

        private GlobalConfig GetGlobalConfig()
        {
            return database.GlobalConfigs.First();
        }
    }
}