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

        

        private SharedViewHelper viewHelper;

        public RegistrationController()
        {
            config = database.GlobalConfigs.First();
            viewHelper = new SharedViewHelper(database);
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
            viewHelper.PrepareStudentInformationView(ViewBag);

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
            viewHelper.EmptyCheckStudentInformation(ModelState, applicant);

            // Valid fields
            if(ModelState.IsValid)
            {
                SaveStudentInformation(applicant);

                return RedirectToAction("GuardianInformation");
            }

            // Invalid fields
            viewHelper.PrepareStudentInformationView(ViewBag);
            return View(applicant);
        }

        public ActionResult GuardianInformation()
        {
            if (!IsActiveSession()) //TODO Do this with AOP/Annotations instead
            {
                return RedirectToAction("Index");
            }

            viewHelper.PrepareGuardianInformationView(ViewBag);

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
            viewHelper.EmptyCheckGuardianInformation(ModelState, applicant);

            // Validate model
            if(ModelState.IsValid)
            {
                SaveGuardianInformation(applicant);

                return RedirectToAction("SchoolSelection");
            }

            // Invalid model
            viewHelper.PrepareGuardianInformationView(ViewBag);
            return View(applicant);
        }

        public ActionResult SchoolSelection()
        {
            if (!IsActiveSession()) //TODO Do this with AOP/Annotations instead
            {
                return RedirectToAction("Index");
            }

            var applicant = GetSessionApplicant();
            viewHelper.PrepareSchoolSelectionView(ViewBag, applicant);

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
            if(formCollection["programs"] == null || !formCollection["programs"].Any())
            {
                ModelState.AddModelError("programs", GoldenTicketText.NoSchoolSelected);
                viewHelper.PrepareSchoolSelectionView(ViewBag, applicant);
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
                applied.SchoolID = programId;

                // Confirm that the program ID is within the city lived in (no sneakers into other districts)
                var program = database.Schools.Find(programId);
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
            
            PrepareReviewView(applicant);

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

        private void PrepareReviewView(Applicant applicant)
        {
            var applieds = database.Applieds.Where(a => a.ApplicantID == applicant.ID).ToList();
            var programs = new List<School>();

            applieds.ForEach(a => programs.Add(a.School));

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