using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using GoldenTicket.Misc;
using GoldenTicket.Models;
using GoldenTicket.DAL;
using GoldenTicket.Resources;

namespace GoldenTicket.Controllers
{
    /**
     * <summary>
     * This controller handles all views and submissions related to the parent or applicant
     * side of Golden Ticket. They methods contain all the functionality in the registration
     * process.
     * </summary>
     **/
    public class RegistrationController : Controller
    {
        // Database connection for interacting with model objects
        private readonly GoldenTicketDbContext database = new GoldenTicketDbContext();

        // Contains helper functions shared by other controllers
        private readonly SharedViewHelper viewHelper;

        /**
         * <summary>
         * Configures any instance variables that can't be
         * initialized earlier.
         * </summary>
         **/ 
        public RegistrationController()
        {
            viewHelper = new SharedViewHelper(database);
        }

        /**
         * <summary>
         * Clears the session and presents a welcome message to applicants
         * </summary>
         * */
        public ActionResult Index()
        {
            Session.Clear();

            ViewBag.GlobalConfig = GetGlobalConfig();

            return View();
        }

        /**
         * <summary>
         * Shows a form for student information.
         * </summary>
         * */
        public ActionResult StudentInformation()
        {
            // Add display variables to the ViewBag
            viewHelper.PrepareStudentInformationView(ViewBag);

            // Create an applicant in the database
            var applicant = GetSessionApplicant();

            return View(applicant);
        }

        /**
         * <summary>
         * Saves data entered in the student information form.
         * </summary>
         * 
         * <param name="applicant">Applicant object containing student information fields</param>
         * */
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

            // Validate fields
            if(ModelState.IsValid)
            {
                SaveStudentInformation(applicant);

                return RedirectToAction("GuardianInformation");
            }

            // Invalid fields
            viewHelper.PrepareStudentInformationView(ViewBag);
            return View(applicant);
        }

        /**
         * <summary>
         * Display the form to collect guardian and alternate contact person information.
         * </summary>
         * */
        public ActionResult GuardianInformation()
        {
            // Validate the session
            if (!IsActiveSession()) //TODO Do this with AOP/Annotations instead
            {
                return RedirectToAction("Index");
            }

            // Get data for view variables
            viewHelper.PrepareGuardianInformationView(ViewBag);
            var applicant = GetSessionApplicant();


            return View(applicant); 
        }

        /**
         * <summary>
         * Save data collected from the guardian information form
         * </summary>
         * 
         * <param name="applicant">Applicant object with guardian and contact person fields</param>
         * */
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

        /**
         * <summary>
         * Display a form asking for which schools the applicant wants to enter their child.
         * </summary>
         * */
        public ActionResult SchoolSelection()
        {
            // Validate the session
            if (!IsActiveSession()) //TODO Do this with AOP/Annotations instead
            {
                return RedirectToAction("Index");
            }

            // Setup view variables
            var applicant = GetSessionApplicant();
            viewHelper.PrepareSchoolSelectionView(ViewBag, applicant);

            // Display the form
            return View(applicant); 
        }

        /**
         * <summary>
         * Save school selections for applicant
         * </summary>
         * 
         * <param name="applicant">Applicant object with applicant ID</param>
         * <param name="formCollection">Contains checkbox selections for which schools to register the applicant for</param>
         * */
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

        /**
         * <summary>
         * Shows the application review page.
         * </summary>
         * */
        public ActionResult Review()
        {
            // Validate the session
            if(!IsActiveSession()) //TODO Do this with AOP/Annotations instead
            {
                return RedirectToAction("Index");
            }

            // Populate variables for view
            var applicant = GetSessionApplicant();
            PrepareReviewView(applicant);

            // Display stuff
            return View(applicant);
        }

        /**
         * <summary>
         * Generate a confirmation code and save the language the application was filled out in
         * (helps later when sending a potentially translated email).
         * </summary>
         * */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Review(Applicant applicant)
        {
            // Make sure someone isn't playing with the ID from the form
            if (!IsAuthorizedApplicant(applicant) || !IsActiveSession()) // TODO Use AOP/Annotations to do this instead
            {
                return RedirectToAction("Index");
            }

            // Generate a confirmation code and save the language
            applicant.ConfirmationCode = Guid.NewGuid().ToString().Split('-')[0].ToUpper();
            applicant.Language = CultureInfo.CurrentUICulture.Name;
            SaveReview(applicant);

            return RedirectToAction("Confirmation");
        }

        /**
         * <summary>
         * Display a confirmation page for the applicant, along with their confirmation code.
         * Also, send a confirmation email.
         * </summary>
         * */
        public ActionResult Confirmation()
        {
            // Validate the session
            if (!IsActiveSession()) //TODO Do this with AOP/Annotations instead
            {
                return RedirectToAction("Index");
            }

            // Get the applicant
            var applicant = GetSessionApplicant();

            // Clear the session
            Session.Clear();

            // Send an email with confirmation code and messages
            var globalConfig = GetGlobalConfig();
            ViewBag.GlobalConfig = globalConfig;

            var messageBody = string.Format(GoldenTicketText.ParentConfirmationEmail,
                applicant.StudentFirstName,
                applicant.ConfirmationCode,
                globalConfig.NotificationDate.ToString("MM/dd/yyyy"),
                globalConfig.ContactPersonName, 
                globalConfig.ContactEmail, 
                globalConfig.ContactPhone);
            EmailHelper.SendEmail(applicant.Contact1Email, "RI Pre-K Lottery Confirmation", messageBody);
            if (!string.IsNullOrEmpty(applicant.Contact2Email))
            {
                EmailHelper.SendEmail(applicant.Contact2Email, "RI Pre-K Lottery Confirmation", messageBody);    
            }

            // Display confirmation page
            return View(applicant);
        }

        // ---- Helper Fields ----

        /**
         * <summary>
         * Saves student informatin fields to an existing applicant in the database.
         * Different from the out-of-the-box AddOrUpdate in that fields not populated during
         * this step will not get overwritten with null in the database. In other words, it
         * only saves the relevant fields with student information without touching the other fields.
         * </summary>
         * */
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

            // Save and put the applicant ID into the session
            database.SaveChanges();
            Session["applicantID"] = applicant.ID;
        }

        /**
         * <summary>
         * Gets the applicant associated with the session. If there is no applicant associated
         * with the session, creates a new one so that an ID can be associated with the session.
         * </summary>
         * 
         * <returns>The applicant associated with the current session</returns>
         */
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

        /**
         * <summary>
         * Verifies that data coming in from a form is associated with the applicant that the session was
         * started with. This prevents a malicious user from manually overriding the hidden ID field on
         * most pages with that of a different applicant's ID. This not only prevents them from getting back
         * information about applicants already in the database, but from modifying applications that aren't their
         * own.
         * </summmary>
         * 
         * <param name="applicant">Applicant with ID field that is incoming from a submission</param>
         * <returns>True if valid/authorized, false otherwise</returns>
         */
        private bool IsAuthorizedApplicant(Applicant applicant)
        {
            // Make sure that the student is the one the user is authorized to make (i.e. if an ID is given, it should be the same one in the session)
            var isApplicantNew = applicant.ID == 0;
            var isActiveSession = Session["applicantID"] != null;

            // If a new sessions
            if (isApplicantNew && !isActiveSession)
            {
                return true;
            }

            // If existing session, check to make sure session applicant ID matches the one submitted
            var isActiveApplicantSameAsSubmitted = applicant.ID.Equals(Session["applicantID"]);
            return !isApplicantNew && isActiveSession && isActiveApplicantSameAsSubmitted;
        }
        /**
          * <summary>
          * Saves guardian information fields to an existing applicant in the database.
          * Different from the out-of-the-box AddOrUpdate in that fields not populated during
          * this step will not get overwritten with null in the database. In other words, it
          * only saves the relevant fields with student information without touching the other fields.
          * </summary>
          * */
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

        /**
         * <summary>
         * Prepares the ViewBag for display the application review page. (List of schools and notification date)
         * </summary>
         */
        private void PrepareReviewView(Applicant applicant)
        {
            var applieds = database.Applieds.Where(a => a.ApplicantID == applicant.ID).ToList();
            var programs = new List<School>();

            applieds.ForEach(a => programs.Add(a.School));

            ViewBag.Programs = programs;

            ViewBag.NotificationDate = database.GlobalConfigs.First();
        }

        /**
        * <summary>
        * Saves confirmation and language fields to an existing applicant in the database.
        * Different from the out-of-the-box AddOrUpdate in that fields not populated during
        * this step will not get overwritten with null in the database. In other words, it
        * only saves the relevant fields with confirmation and languages fields without touching the other fields.
        * </summary>
        * */
        private void SaveReview(Applicant applicant)
        {
            database.Applicants.Attach(applicant);
            var applicantEntry = database.Entry(applicant);

            applicantEntry.Property(a => a.ConfirmationCode).IsModified = true;
            applicantEntry.Property(a => a.Language).IsModified = true;

            database.SaveChanges();
        }

        /**
         * <summary>
         * Is the view associated with an active session?
         * </summary>
         * <returns>True if a session is active, false otherwise</returns>
         */
        private bool IsActiveSession()
        {
            return Session["applicantID"] != null;
        }

        /**
         * <summary>
         * Gets the global configuration.
         * </summary>
         * 
         * <returns>The global configuration for the application</returns>
         */
        private GlobalConfig GetGlobalConfig()
        {
            return database.GlobalConfigs.First();
        }
    }
}