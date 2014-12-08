using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using GoldenTicket.Lottery;
using GoldenTicket.Misc;
using GoldenTicket.Models;
using GoldenTicket.DAL;
using GoldenTicket.Resources;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GoldenTicket.Controllers
{
    /**
     * <summary>
     * <para>
     *  Handles all views associated with Golden Ticket administrative functions.
     *  Includes:
     * </para>
     *  
     * <list type="bullet">
     *  <item><description>Viewing / editing / deleting applications</description></item>
     *  <item><description>Adding / removing administrators</description></item>
     *  <item><description>Adding / viewing / editing / deleting schools</description></item>
     *  <item><description>Updating global settings (poverty limits, setting dates)</description></item>
     * </list>
     * 
     * <para>
     * All actions/methods in this class require a user to be authenticated.
     * </para>
     * </summary>
     **/
    [Authorize]
    public class AdminController : Controller
    {
        // Adds "All Schools" to the school navigation for ViewApplicants action
        private static readonly School ALL_SCHOOL_SCHOOL = GetAllSchoolSchool();

        // Subject line for selected and waitlisted emails
        private const string NOTIFICATION_E_MAIL_SUBJECT = "RI Pre-K Lottery Results";
        
        // Database access for model objects
        private readonly GoldenTicketDbContext db = new GoldenTicketDbContext();
        
        // Database access for authentication and authorization
        private readonly ApplicationDbContext identityContext = new ApplicationDbContext();
        private readonly ApplicationUserManager userManager;
        
        // Helper methods shared between Admin and Registration controllers
        private readonly SharedViewHelper viewHelper;

        /**
         * <summary>
         * Sets up instance variables that could not be set at declaration
         * </summary>
         **/
        public AdminController()
        {
            viewHelper = new SharedViewHelper(db);
            userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(identityContext));
        }


        /**
         * <summary>
         * Default view, simply redirects to ViewApplicants action
         * </summary>
         * <returns>ViewApplicants page (first page)</returns>
         **/
        public ActionResult Index()
        {
            return RedirectToAction("ViewApplicants");
        }

        /**
         * <summary>
         * Helper method for showing all applicants
         * </summary>
         **/
        private void PrepareApplicantsView()
        {
            AddSchoolsToViewBag();
        }

        /**
         * <summary>
         * Show all applicants for all schools. Shows 100 per page.
         * The id argument is the page number. If an invalid page number
         * is given, it will show the first page of applicants by default.
         * Applicants are ordered in ascending order by last name.
         * </summary>
         * <param name="id">Page of applicants</param>
         **/
        public ActionResult ViewApplicants(int? id)
        {
            // id is the page number, default to the first one if invalid
            // URL's first page is 1, but for the controller the first page is 0
            if (id == null || id <= 0)
            {
                id = 0;
            }
            else
            {
                id = id - 1;
            }

            // Setup view variables
            PrepareApplicantsView();
            ViewBag.School = ALL_SCHOOL_SCHOOL;
            ViewBag.GlobalConfig = db.GlobalConfigs.First();

            // Get the applicants based on the page
            var numApplicants = db.Applicants.Count();
            var skipCount = id.Value*100;

            if (numApplicants < skipCount)
            {
                skipCount = 0;
                id = 0;
            }

            var applicants =
                db.Applicants.Where(a => a.ConfirmationCode != null)
                    .OrderBy(a => a.StudentLastName)
                    .Skip(skipCount)
                    .Take(100)
                    .ToList();
            ViewBag.Applicants = applicants;

            // Pagination
            var numPages = numApplicants/100;
            if (numApplicants%100 >= 1)
            {
                numPages += 1;
            }
            ViewBag.NumPages = numPages;
            ViewBag.PageNum = id + 1;

            return View();
        }

        /**
         * <summary>
         * Exports all applicants to a CSV file.
         * 
         * <remarks>
         * The file exported by this function can be used to setup new test
         * data for import in Seeds.cs. To load the current system's data into
         * a new one, export using this view, then swap this file with `GoldenTicket/TestData/data.csv`.
         * You'll have to enable the loading of test data too and setup the schools with the exact same names.
         * Alternately, you can write code to import this file elsewhere using `GoldenTicket.Csv.ApplicantCsvReader`.
         * </remarks>
         * 
         * </summary>
         * 
         * <returns>CSV file name: applicants_all.csv</returns>
         **/
        public ActionResult ExportApplicants()
        {
            var applicants = db.Applicants.Where(a => a.ConfirmationCode != null).OrderBy(a=>a.StudentLastName).ToList();

            return ExportApplicantsCsvFile(applicants, "applicants_all.csv", true);
        }

        /**
         * <summary>
         * Exports all applicants for a specific school to a CSV file. 
         * </summary>
         * 
         * <param name="id">School's database ID</param>
         * <returns>CSV file name: applicants_(school name).csv</returns>
         **/
        public ActionResult ExportApplicantsForSchool(int id)
        {
            // Verify the school exists
            var school = db.Schools.Find(id);
            if (school == null)
            {
                return HttpNotFound();
            }

            // Export applicants to CSV
            var applieds = db.Applieds.Where(a => a.SchoolID == id && a.Applicant.ConfirmationCode != null).OrderBy(a=>a.Applicant.StudentLastName).ToList();
            var applicants = Utils.GetApplicants(applieds);

            var schoolName = school.Name.Replace(' ', '_');
            return ExportApplicantsCsvFile(applicants, "applicants_" + schoolName + ".csv");
        }

        /**
         * <summary>
         * Exports the shuffle list from a lottery (i.e. the random ordering of students prior to performing selection)
         * </summary>
         * 
         * <param name="id">School's database ID</param>
         * <returns>CSV file name: shuffle_(school name).csv</returns>
         **/
        public ActionResult ExportShuffleForSchool(int id)
        {
            // Verify that the school exists
            var school = db.Schools.Find(id);
            if (school == null)
            {
                return HttpNotFound();
            }

            // Get the shuffled applicants
            var shuffleds = db.Shuffleds.Where(s => s.SchoolID == id).OrderBy(s => s.Rank).ToList();
            var applicants = Utils.GetApplicants(shuffleds);

            // Export to file
            var schoolName = school.Name.Replace(' ', '_');
            return ExportApplicantsCsvFile(applicants, "shuffle_" + schoolName + ".csv");
        }

        /**
         * <summary>
         * Exports the selected list for a school from the lottery run.
         * </summary>
         * 
         * <param name="id">School's database ID</param>
         * <returns>CSV file name: selected_(school name).csv</returns>
         **/
        public ActionResult ExportSelectedForSchool(int id)
        {
            // Verify that the school exists
            var school = db.Schools.Find(id);
            if (school == null)
            {
                return HttpNotFound();
            }

            // Get the selected applicants (ordered by selected rank)
            var selecteds = db.Selecteds.Where(s => s.SchoolID == id).OrderBy(s => s.Rank).ToList();
            var applicants = Utils.GetApplicants(selecteds);

            // Export to file
            var schoolName = school.Name.Replace(' ', '_');
            return ExportApplicantsCsvFile(applicants, "selected_" + schoolName + ".csv");
        }

        /**
         * <summary>
         * Exports the wait list for a school from a lottery run.
         * </summary>
         * 
         * <param name="id">School's database ID</param>
         * <returns>CSV file name: waitlisted_(school name).csv</returns>
         **/
        public ActionResult ExportWaitlistedForSchool(int id)
        {
            // Verify that the school exists
            var school = db.Schools.Find(id);
            if (school == null)
            {
                return HttpNotFound();
            }

            // Get all the waitlisted students (ordered by wait list rank)
            var waitlisteds = db.Waitlisteds.Where(w => w.SchoolID == id).OrderBy(w => w.Rank).ToList();
            var applicants = Utils.GetApplicants(waitlisteds);

            // Export to file
            var schoolName = school.Name.Replace(' ', '_');
            return ExportApplicantsCsvFile(applicants, "waitlisted_" + schoolName + ".csv");
        }

        /**
         * <summary>
         * View all applicants for a specified school. If the lottery has not been run, it
         * displays the applicants sorted by last name (ascending). If the lottery has been run,
         * applicants are separated by selected and waitlisted status and ordered by rank within each.
         * </summary>
         * 
         * <param name="id">School's database ID</param>
         * 
         * <returns>Applicants for a given school, or all applicants for all schools if `id` is not specified</returns>
         **/
        public ActionResult ViewApplicantsForSchool(int? id)
        {
            // If no id, show all applicants for all schools
            if (id == null)
            {
                return RedirectToAction("ViewApplicants");
            }

            // If the school ID is not valid, show all the applicants
            ViewBag.School = db.Schools.Find(id);
            if (ViewBag.School == null)
            {
                return RedirectToAction("ViewApplicants", 0);
            }

            // If the lottery was run, get the selected and waitlisted applicants
            ViewBag.WasLotteryRun = WasLotteryRun();
            if (ViewBag.WasLotteryRun)
            {
                // Selected applicants
                var selecteds = db.Selecteds.Where(s => s.SchoolID == id).OrderBy(s => s.Rank).ToList();
                var selectedApplicants = new List<Applicant>();
                foreach (var selected in selecteds) // don't convert to LINQ -- needs to preserve order
                {
                    selectedApplicants.Add(selected.Applicant);
                }
                ViewBag.SelectedApplicants = selectedApplicants;

                // Waitlisted applicants
                var waitlisteds = db.Waitlisteds.Where(w => w.SchoolID == id).OrderBy(w => w.Rank).ToList();
                var waitlistedApplicants = new List<Applicant>();
                foreach (var waitlisted in waitlisteds) // don't convert to LINQ -- needs to preserve order
                {
                   waitlistedApplicants.Add(waitlisted.Applicant);
                }
                ViewBag.WaitlistedApplicants = waitlistedApplicants;
            }
            // Show all applicants for the school if the lottery has NOT been run
            else
            {
                var applieds = db.Applieds.Where(a => a.SchoolID == id && a.Applicant.ConfirmationCode != null).OrderBy(a => a.Applicant.StudentLastName).ToList();
                var applicants = new List<Applicant>();
                foreach (var applied in applieds) // don't convert to LINQ -- needs to preserve order
                {
                    applicants.Add(applied.Applicant);
                }
                ViewBag.Applicants = applicants;
            }

            // Other things needed for display
            AddSchoolsToViewBag();

            return View();
        }
        
        /**
         * <summary>
         * Helper method for display any action that uses the "_ApplicantDetails.cshtml" partial view.
         * </summary>
         **/
        private void PrepareApplicantDetailView(Applicant applicant)
        {
            // Applied schools
            var applieds = db.Applieds.Where(a => a.ApplicantID == applicant.ID).OrderBy(a => a.School.Name).ToList();
            ViewBag.AppliedSchools = Utils.GetSchools(applieds);
         
            // Selected school
            var selected = db.Selecteds.FirstOrDefault(s => s.ApplicantID == applicant.ID);
            if (selected != null)
            {
                ViewBag.SelectedSchool = selected.School;
            }
            
            // Waitlisted schools
            ViewBag.WaitlistedSchools =
                Utils.GetSchools(db.Waitlisteds.Where(a => a.ApplicantID == applicant.ID).OrderBy(a => a.School.Name).ToList());
            ViewBag.WasLotteryRun = GetLotteryRunDate() != null;

            // Income status
            var incomeCalc = new IncomeCalculator(db);
            ViewBag.IsBelowPoverty = incomeCalc.IsBelowPovertyLine(applicant);
        }

        /**
         * <summary>
         * View details for a specific applicant. Shows all the information they submitted via the
         * parent form.
         * </summary>
         * 
         * <param name="id">Applicant's database ID</param>
         **/
        public ActionResult ViewApplicant(int id)
        {
            var applicant = db.Applicants.Find(id);

            // Send back to view all applicants if an incorrect ID is specified
            if(applicant == null)
            {
                return RedirectToAction("ViewApplicants");
            }

            // Variables for display
            PrepareApplicantDetailView(applicant);

            return View(applicant);
        }

        /**
         * <summary>
         * Show a form with all same fields that the applicants viewed in the parent side of the application,
         * but in a single page view for ease of editing.
         * </summary>
         * 
         * <param name="id">Applicant's database ID</param>
         **/
        public ActionResult EditApplicant(int id)
        {
            // Verify that the applicant actually exists
            var applicant = db.Applicants.Find(id);
            if (applicant == null)
            {
                return HttpNotFound();
            }

            // Present form
            PrepareEditApplicantView(applicant);
            return View(applicant);
        }

        /**
         * <summary>
         * Receives all information about an applicant via a form (from the action of the same name). Performs
         * validation on fields. 
         * </summary>
         * 
         * <param name="applicant">All applicant fields</param>
         * <param name="formCollection">Used for the school checkboxes</param>
         * 
         * <returns>
         * If the applicant does not exist already, a 404 error. If the submission is valid, the user
         * is sent back to the ViewApplicant(id) page/action. If there are validation errors, the user
         * is returned to the form in EditApplicant(id).
         * </returns>
         **/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditApplicant(Applicant applicant, FormCollection formCollection)
        {
            // Verify that the applicant exists
            var queriedApplicant = db.Applicants.Find(applicant.ID);
            if (queriedApplicant == null)
            {
                return HttpNotFound();
            }

            // Empty check student and guardian information
            viewHelper.EmptyCheckStudentInformation(ModelState, applicant);
            viewHelper.EmptyCheckGuardianInformation(ModelState, applicant);

            // School selection check //TODO Make this code shareable with the parent side
            var schoolIds = new List<int>();
            if (formCollection["programs"] == null || !formCollection["programs"].Any())
            {
                ModelState.AddModelError("programs", GoldenTicketText.NoSchoolSelected);
                PrepareEditApplicantView(applicant);
                return View(applicant);
            }
            else
            {
                var programIdStrs = formCollection["programs"].Split(',').ToList();
                programIdStrs.ForEach(idStr => schoolIds.Add(int.Parse(idStr)));
            }

            if (!ModelState.IsValid)
            {
                PrepareEditApplicantView(applicant);
                return View(applicant);
            }

            // Remove existing applications for this user
            var applieds = db.Applieds.Where(applied => applied.ApplicantID == applicant.ID).ToList();
            applieds.ForEach(a => db.Applieds.Remove(a));

            // Add new Applied associations (between program and program)
            var populatedApplicant = db.Applicants.Find(applicant.ID);
            foreach (var programId in schoolIds)
            {
                var applied = new Applied();
                applied.ApplicantID = applicant.ID;
                applied.SchoolID = programId;

                // Confirm that the program ID is within the city lived in (no sneakers into other districts)
                var program = db.Schools.Find(programId);
                if (program != null && program.City.Equals(populatedApplicant.StudentCity, StringComparison.CurrentCultureIgnoreCase))
                {
                    db.Applieds.Add(applied);
                }
            }

            // Save changes to the database
            db.Applicants.AddOrUpdate(applicant);
            db.SaveChanges();

            return RedirectToAction("ViewApplicant", new{id=applicant.ID});
        }

        /**
         * <summary>
         * Shows potential duplicate applications by school.
         * 
         * The controller delivers a dictionary keyed by school and valued with applicant lists of
         * possible duplicates. From this view, an admin user can choose to delete applications that
         * are definitively duplicates.
         * </summary>
         * 
         * <returns>A page showing possible duplicate applicants</returns>
         **/
        public ActionResult ViewDuplicateApplicants()
        {
            // Get all the potential duplicate applicants
            // Key: School, Value: Duplicate applicants
            var schoolDuplicates = new Dictionary<School, List<Applicant>>();
            
            var schools = db.Schools.OrderBy(s => s.Name).ToList();
            foreach (var s in schools)
            {
                var applieds =
                    db.Applieds.Where(a => a.SchoolID == s.ID && a.Applicant.ConfirmationCode != null)
                        .OrderBy(a => a.Applicant.StudentLastName)
                        .ThenBy(a => a.Applicant.StudentFirstName)
                        .ToList();
                var duplicates = Utils.GetPossibleDuplicateApplicants(Utils.GetApplicants(applieds));

                schoolDuplicates.Add(s,duplicates);
            }

            // Show page
            return View(schoolDuplicates);
        }

        /**
         * <summary>
         * Presents a confirmation page where an admin needs to confirm deleting an
         * applicant from the pool.
         * </summary>
         * 
         * <param name="id">An applicant's database ID</param>
         **/
        public ActionResult DeleteApplicant(int id)
        {
            // Verify that the applica
            var applicant = db.Applicants.Find(id);
            if (applicant == null)
            {
                return HttpNotFound();
            }

            // Get the applicant's details for display
            PrepareApplicantDetailView(applicant);

            // View stuff
            return View(applicant);
        }

        /**
         * <summary>
         * Actually deletes an applicant and unassociates them from the schools
         * they applied for, were selected for, or was waitlisted for. If a selected
         * student is removed, then the lottery algorithm is run again to find a replacement for their slot. When done
         * return the user to the school's applicant list afterwards
         * </summary>
         * 
         * <param name="applicant">The applicant to delete</param>
         **/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteApplicant(Applicant applicant)
        {
            // Confirm that an applicant with the same ID exists in the database
            var queriedApplicant = db.Applicants.Find(applicant.ID);
            if (queriedApplicant == null)
            {
                return HttpNotFound();
            }

            // Remove from lists (and run lottery to place waitlisted applicant if the student was selected)
            if (WasLotteryRun())
            {
                var waitlisteds = db.Waitlisteds.Where(w => w.ApplicantID == applicant.ID).ToList();
                db.Waitlisteds.RemoveRange(waitlisteds);

                var selected = db.Selecteds.FirstOrDefault(s => s.ApplicantID == applicant.ID);
                if (selected != null)
                {
                    var school = selected.School;

                    // Remove the selected applicant
                    db.Selecteds.Remove(selected);

                    // Get the waitlist to pass into the lottery algorithm
                    var waitlistedApplicants = Utils.GetApplicants(db.Waitlisteds.Where(w => w.SchoolID == school.ID).OrderBy(w => w.Rank).ToList());

                    // Run the lottery to fill the slot
                    var lottery = new SchoolLottery(db);
                    lottery.Run(school, waitlistedApplicants, false);

                    // Remove the newly selected applicants from the selected school from other waitlists 
                    // (does it for all selecteds in the school since we don't know which student was the one filled in at this point)
                    var newSelecteds = school.Selecteds;
                    var removeWaitlisteds = new List<Waitlisted>();
                    foreach (var newSelected in newSelecteds)
                    {
                        var otherApplicantWaitlisteds =
                            db.Waitlisteds.Where(w => w.ApplicantID == newSelected.ApplicantID).ToList();
                        removeWaitlisteds.AddRange(otherApplicantWaitlisteds);
                    }
                    db.Waitlisteds.RemoveRange(removeWaitlisteds);
                }                
            }


            // Remove the applicant entirely from the system
            db.Applicants.Remove(queriedApplicant);
            db.SaveChanges();

            return RedirectToAction("ViewApplicants");
        }

        /**
         * <summary>
         * View all the schools that the system has been configured with.
         * </summary>
         **/
        public ActionResult ViewSchools()
        {
            return View(db.Schools.OrderBy(s=>s.Name).ToList());
        }

        /**
         * <summary>Presents a form for adding a new school to the system</summary>
         **/
        public ActionResult AddSchool()
        {
            return View();
        }

        /**
         * <summary>
         * Adds a school to the system. Send the user to the ViewSchools page afterwards.
         * </summary>
         * 
         * <param name="school">A school to be added to the system</param>
         **/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddSchool(School school)
        {
            // Convert rates to multipliers (i.e. .5 instead of 50%)
            school.GenderBalance /= 100;
            school.PovertyRate /= 100;

            // Validate
            ModelState.Clear();
            TryValidateModel(school);
            if (!ModelState.IsValid)
            {
                // Convert rates to percents (for display)
                school.GenderBalance *= 100;
                school.PovertyRate *= 100;

                return View(school);
            }

            // Save stuff
            db.Schools.Add(school);
            db.SaveChanges();

            return RedirectToAction("ViewSchools");
        }

        /**
         * <summary>
         * Presents a form that allows an admin to update a school.
         * The form is pre-populated with the school's existing information.
         * </summary>
         * 
         * <param name="id">The school's database ID</param>
         **/
        public ActionResult EditSchool(int id)
        {
            // Verify that the school actually exists
            var school = db.Schools.Find(id);
            if (school == null)
            {
                return HttpNotFound();
            }

            // Convert rates to percents (example: from .5 to 50%)
            school.GenderBalance *= 100;
            school.PovertyRate *= 100;

            return View(school);
        }

        /**
         * <summary>
         * Modifies an existing school's information as specified by an admin. Return the user to the ViewSchools page afterwards.
         * </summary>
         * 
         * <param name="school">The school to modify</param>
         **/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSchool(School school)
        {
            // Verify that the school given exists in the database
            var queriedSchool = db.Schools.Find(school.ID);
            if (queriedSchool == null)
            {
                return HttpNotFound();
            }

            //TODO Reuse this code with AddSchool's POST
            // Convert rates to multipliers
            school.GenderBalance /= 100;
            school.PovertyRate /= 100;

            // Validate
            ModelState.Clear();
            TryValidateModel(school);
            if (!ModelState.IsValid)
            {
                // Convert rates to percents
                school.GenderBalance *= 100;
                school.PovertyRate *= 100;

                return View(school);
            }

            // Save stuff
            db.Schools.AddOrUpdate(school);
            db.SaveChanges();

            return RedirectToAction("ViewSchools");
        }

        /**
         * <summary>
         * Presents a page confirming that the admin actually wants to delete a specified school.
         * </summary>
         * 
         * <param name="id">The school's database ID</param>
         **/
        public ActionResult DeleteSchool(int id)
        {
            // Verify that the school exists in the database
            var school = db.Schools.Find(id);
            if (school == null)
            {
                return HttpNotFound();
            }

            // Display stuff
            return View(school);
        }

        /**
         * <summary>
         * Actually deletes a school from the system. Return the user to the ViewSchools action afterwards.
         * </summary>
         * 
         * <param name="school">The school to delete from the database</param>
         **/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSchool(School school)
        {
            // Verifies that the school exists in the database
            var queriedSchool = db.Schools.Find(school.ID);
            if (queriedSchool == null)
            {
                return HttpNotFound();
            }

            // Remove associated data
            // TODO The EntityFramework may do some of this automatically. See what actually needs to be done.
            db.Applieds.RemoveRange(queriedSchool.Applieds);
            db.Selecteds.RemoveRange(queriedSchool.Selecteds);
            db.Waitlisteds.RemoveRange(queriedSchool.Waitlisteds);
            db.Shuffleds.RemoveRange(queriedSchool.Shuffleds);
            db.Schools.Remove(queriedSchool);
            db.SaveChanges();

            return RedirectToAction("ViewSchools");
        }

        /**
         * <summary>
         * Presents a form that allows an admin to change global settings for the application.
         * </summary>
         **/
        public ActionResult EditSettings()
        {
            ViewBag.PovertyConfigs = db.PovertyConfigs.ToList();
            return View(db.GlobalConfigs.First());
        }

        /**
         * <summary>
         * Modifies the global settings in the system. Return the user to the same page afterwards.
         * </summary>
         * 
         * <param name="globalConfig">The new global configuration settings</param>
         * <param name="formCollection">Used to collect the poverty configuration data (since each comprises a separate object)</param>
         **/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSettings(GlobalConfig globalConfig, FormCollection formCollection)
        {
            // Verify that the global configuration exists in the system (basically, the ID should be 1)
            var queriedGlobalConfig = db.GlobalConfigs.Find(globalConfig.ID);
            if (queriedGlobalConfig == null)
            {
                return HttpNotFound();
            }

            // Validate that poverty fields are complete and valid
            var updatedPovertyConfigs = new List<PovertyConfig>();
            var previousMinIncome = 0;
            foreach (var householdMembers in Enumerable.Range(2, 9))
            {
                var povertyConfig = db.PovertyConfigs.First(p => p.HouseholdMembers == householdMembers);

                var key = "poverty_config_" + householdMembers;
                var fieldName = "Minimum income (" + householdMembers + ')';

                // Empty check
                if (string.IsNullOrEmpty(formCollection[key]))
                {
                    ModelState.AddModelError("", fieldName + " must be completed");
                    break;
                }

                // Is it a number?
                int minIncome;
                if (!int.TryParse(formCollection[key], out minIncome))
                {
                    ModelState.AddModelError("", fieldName + " must be a number (no decimal places)");
                    break;
                }

                // Is it more than the previous one
                if (previousMinIncome >= minIncome)
                {
                    ModelState.AddModelError("", fieldName + " must be greater than minimum income (" + (householdMembers+1) + ')');
                    break;
                }

                //Set values
                previousMinIncome = minIncome;
                povertyConfig.MinimumIncome = minIncome;
                updatedPovertyConfigs.Add(povertyConfig);
            }

            // Validate some more
            if (!ModelState.IsValid)
            {
                ViewBag.PovertyConfigs = db.PovertyConfigs.ToList();
                return View(db.GlobalConfigs.First());
            }

            // Update poverty configs
            updatedPovertyConfigs.ForEach(p => db.PovertyConfigs.AddOrUpdate(p));

            // Update global config
            db.GlobalConfigs.AddOrUpdate(globalConfig);

            db.SaveChanges();

            ViewBag.PovertyConfigs = updatedPovertyConfigs;

            return View(db.GlobalConfigs.First());
        }

        /**
         * <summary>
         * Asks the admin to confirm if they want to reset the lottery.
         * </summary>
         **/
        public ActionResult ResetLottery()
        {
            return View();
        }

        /**
         * <summary>
         * Resets the lottery
         * </summary>
         * 
         * <remarks>The id param can be anything, it doesn't matter</remarks>
         * 
         * <param name="id">id is just dummy text, to differentiate from the GET-based ResetLottery() ... comes in as a static-valued hidden field</param>
         **/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetLottery(string id)
        {
            // Remove all applicants
            var applicants = db.Applicants.ToList();
            db.Applicants.RemoveRange(applicants);

            // Reset global configuration values
            var globalConfig = db.GlobalConfigs.First();
            globalConfig.LotteryRunDate = null;
            globalConfig.WereNotificationsSent = false;
            db.GlobalConfigs.Add(globalConfig);

            db.SaveChanges();

            return RedirectToAction("EditSettings");
        }

        /**
         * <summary>
         * Runs the lottery! Woot woot!
         * </summary>
         * 
         * <remarks>TODO this should be a POST request</remarks>
         **/
        public ActionResult RunLottery()
        {
            if (!WasLotteryRun())
            {
                // Run the selection algorithm for each school
                var schoolLottery = new SchoolLottery(db);
                foreach (var school in db.Schools.ToList())
                {
                    schoolLottery.Run(school);
                }

                // Make sure applicants were selected for more than one school (or waitlisted on any others if they were selected)
                //TODO this performs a little slowly ... probably too many database roundtrips. Optimize later.
                var reconciler = new CrossSchoolReconciler(db);
                reconciler.Reconcile();

                // Save a record that the lottery was run
                var globalConfig = db.GlobalConfigs.First();
                globalConfig.LotteryRunDate = DateTime.Now;
                db.GlobalConfigs.AddOrUpdate(globalConfig);
                db.SaveChanges();
            }

            return RedirectToAction("ViewApplicants");
        }

        /**
         * <summary>
         * View all the system administrators. Feel the power.
         * </summary>
         **/
        public ActionResult ViewAdmins()
        {
            // Prevent the current user from being deleting by signaling who the current user is
            var currentUser = userManager.FindById(User.Identity.GetUserId());
            ViewBag.CurrentUser = currentUser;

            return View(identityContext.Users.ToList());
        }

        /**
         * <summary>
         * Presents a form that allows an admin to be added to the system.
         * </summary>
         **/
        public ActionResult AddAdmin()
        {
            return View();
        }

        /**
         * <summary>
         * Actually adds a new admin user to the system. Return the user to the ViewAdmins page afterwards.
         * </summary>
         * 
         * <param name="registerViewModel">Object with the information for the new admin user (email, password)</param>
         **/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAdmin(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                // Fill the user object with information from the form
                var user = new ApplicationUser()
                {
                    UserName = registerViewModel.Email,
                    Email = registerViewModel.Email,
                    EmailConfirmed = true // we don't confirm email, so just switch this to true so that password resets can happen
                };

                // Save the new user to the database
                userManager.Create(user, registerViewModel.Password);
                identityContext.SaveChanges();

                return RedirectToAction(actionName: "ViewAdmins");
            }

            return View(registerViewModel);
        }

        /**
         * <summary>
         * Shows a form that asks the admin to confirm deleting an admin user from the system.
         * </summary>
         * 
         * <param name="email">Email address of the admin user that might be deleted</param>
         **/
        public ActionResult DeleteAdmin(string email)
        {
            // Verify that an admin with the email address actually exists in the system
            var user = userManager.FindByEmail(email);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Display the confirmation page
            return View(user);
        }

        /**
         * <summary>
         * Actually deletes an admin user from the system. Return the user to the ViewAdmins
         * action afterwords.
         * </summary>
         * 
         * <param name="applicationUser">The admin to delete</param>
         **/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAdmin(ApplicationUser applicationUser)
        {
            // Verify that the admin specified exists in the database
            var user = userManager.FindByEmail(applicationUser.Email);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Delete them
            userManager.Delete(user);
            identityContext.SaveChanges();

            // Return to view admins page
            return RedirectToAction(actionName: "ViewAdmins");
        }

        /**
         * <summary>
         * Send emails to all applicants notifying them if they were selected or waitlisted.
         * </summary>
         **/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NotifyApplicants()
        {
            // Don't send emails if they were already sent
            var globalConfig = db.GlobalConfigs.First();
            if (globalConfig.WereNotificationsSent)
            {
                return RedirectToAction(actionName: "ViewApplicants");
            }

            // Notify selected applicants
            var selecteds = db.Selecteds.ToList();
            foreach (var selected in selecteds)
            {
               SendSelectedEmail(selected);
            }

            // Notify waitlisted students
            foreach (var applicant in db.Applicants.ToList())
            {
                var waitlisteds = db.Waitlisteds.Where(w => w.ApplicantID == applicant.ID).ToList();
                SendWaitlistedEmail(waitlisteds);
            }

            // Mark notifications as having been sent
            globalConfig.WereNotificationsSent = true;
            db.GlobalConfigs.AddOrUpdate(globalConfig);
            db.SaveChanges();

            return RedirectToAction(actionName:"ViewApplicants");
        }

        /*
         * ---------- HELPER METHODS ------------
         */


        /**
         * <summary>
         * Create a school used to represent the view for seeing applications for all schools.
         * </summary>
         * 
         * <returns>School with index 0 and name "All Schools"</returns>
         **/
        private static School GetAllSchoolSchool()
        {
            var school = new School();
            school.ID = 0;
            school.Name = "All Schools";

            return school;
        }

        /**
         * <summary>Get the date that the lottery was run</summary>
         * <returns>Date the lottery was run</returns>
         **/
        private DateTime? GetLotteryRunDate()
        {
            return db.GlobalConfigs.First().LotteryRunDate;
        }

        /**
         * <summary>Was the lottery run?</summary>
         * <returns>True if it was, false if not</returns>
         **/
        private bool WasLotteryRun()
        {
            return db.GlobalConfigs.First().LotteryRunDate != null;
        }

        /**
         * <summary>Fills the ViewBag with all the schools for navigation on the ViewApplicants* pages</summary>
         **/
        private void AddSchoolsToViewBag()
        {
            var schools = db.Schools.OrderBy(s => s.Name).ToList();
            schools.Insert(0, ALL_SCHOOL_SCHOOL);
            ViewBag.Schools = schools;
        }

        /**
         * <summary>Exports an applicant list to a CSV file</summary>
         * <param name="applicants">The applicants to export</param>
         * <param name="fileName">The name of the exported file</param>
         * <param name="printSchoolList">True if there should be a column listing all the schools the applicant applied for (false by default)</param>
         **/
        private FileStreamResult ExportApplicantsCsvFile(IEnumerable<Applicant> applicants, string fileName, bool printSchoolList = false)
        {
            // Generate the CSV text itself in string format
            var csvText = Utils.ApplicantsToCsv(applicants);

            // Configure the output method and copy the data into the file
            var byteArray = Encoding.UTF8.GetBytes(csvText);
            var stream = new MemoryStream(byteArray);

            // Return the file with the data
            return File(stream, "text/plain", fileName);
        }

        /**
         * <summary>
         * Prepares the view for editing an applicant by populating variables
         * in the ViewBag.
         * </summary>
         * 
         * <param name="applicant">The applicant who's data should be populated into the view</param>
         **/
        private void PrepareEditApplicantView(Applicant applicant)
        {
            viewHelper.PrepareStudentInformationView(ViewBag, false);
            viewHelper.PrepareGuardianInformationView(ViewBag);
            viewHelper.PrepareSchoolSelectionView(ViewBag, applicant);
        }

        /**
         * <summary>
         * Send an email to a selected applicant.
         * </summary>
         * 
         * <param name="seleted">The selected person to receive the email</param>
         **/
        private void SendSelectedEmail(Selected selected)
        {
            // Get objects relevant to the applicant
            var applicant = selected.Applicant;
            var school = selected.School;
            var globalConfig = db.GlobalConfigs.First();

            // Get the language they filled out the form in
            var cultureInfo = CultureInfo.InvariantCulture;
            if (applicant.Language != null)
            {
                cultureInfo = new CultureInfo(applicant.Language);
            }

            // Construct the email message
            var messageBody = GoldenTicketText.ResourceManager.GetString("SelectedNotificationEmail", cultureInfo);
            messageBody = string.Format(messageBody,
                applicant.StudentFirstName,
                school.Name,
                school.Email,
                school.Phone,
                globalConfig.ContactPersonName,
                globalConfig.ContactEmail,
                globalConfig.ContactPhone
                );


            // Send it!
            EmailHelper.SendEmail(applicant.Contact1Email, NOTIFICATION_E_MAIL_SUBJECT, messageBody);
            if (!string.IsNullOrEmpty(applicant.Contact2Email))
            {
                EmailHelper.SendEmail(applicant.Contact2Email, NOTIFICATION_E_MAIL_SUBJECT, messageBody);
            }            
        }

        /**
         * <summary>
         * Sends ONE email to an applicant with ALL the schools they were waitlisted for.
         * Must stress that all applicants in the waitlisted enumerable should be the same one.
         * Don't use this method for Waitlisted objects for different applicants.
         * </summary>
         * 
         * <param name="waitlisteds">List of waitlisteds for a single applicant to send emails to</param>
         **/
        private void SendWaitlistedEmail(IEnumerable<Waitlisted> waitlisteds)
        {
            // Get information needed for the email
            var globalConfig = db.GlobalConfigs.First();

            if (waitlisteds.Any())
            {
                // Get the applicant
                var applicant = waitlisteds.First().Applicant; // First() is okay, since all elements in the IEnumerable will have the same applicant
                
                // Build an HTML list (<ul>) of the schools waitlisted for
                var waitlistedSchools = "";
                foreach (var waitlisted in waitlisteds)
                {
                    var schoolName = waitlisted.School.Name;
                    waitlistedSchools += "<li>" + schoolName + "</li>";
                }

                // Determine the language the applicant filled out the forms in
                var cultureInfo = CultureInfo.InvariantCulture;
                if (applicant.Language != null)
                {
                    cultureInfo = new CultureInfo(applicant.Language);
                }

                // Construct the messsage
                var messageBody = GoldenTicketText.ResourceManager.GetString("WaitlistedNotificationEmail", cultureInfo);
                messageBody = string.Format(messageBody,
                    applicant.StudentFirstName,
                    waitlistedSchools,
                    globalConfig.ContactPersonName,
                    globalConfig.ContactEmail,
                    globalConfig.ContactPhone
                    );

                // Send it
                EmailHelper.SendEmail(applicant.Contact1Email, NOTIFICATION_E_MAIL_SUBJECT, messageBody);
                if (!string.IsNullOrEmpty(applicant.Contact2Email))
                {
                    EmailHelper.SendEmail(applicant.Contact2Email, NOTIFICATION_E_MAIL_SUBJECT, messageBody);
                }
            }
   
        }
    }
}