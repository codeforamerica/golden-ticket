using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
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
    [Authorize]
    public class AdminController : Controller
    {

        private GoldenTicketDbContext db = new GoldenTicketDbContext();
        private ApplicationDbContext identityContext = new ApplicationDbContext();
        private static readonly School ALL_SCHOOL_SCHOOL = GetAllSchoolSchool();

        private readonly SharedViewHelper viewHelper;

        public AdminController()
        {
            viewHelper = new SharedViewHelper(db);
        }


        // All Applications
        // GET: Admin
        public ActionResult Index()
        {
            return RedirectToAction("ViewApplicants");
        }


        private void PrepareApplicationsView()
        {
            ViewBag.LotteryRunDate = GetLotteryRunDate();
            ViewBag.LotteryCloseDate = GetLotteryCloseDate();
            ViewBag.IsLotteryClosed = ViewBag.LotteryCloseDate <= DateTime.Now;

            AddSchoolsToViewBag();
        }

        public ActionResult ViewApplicants(int? id)
        {

            if (id == null || id <= 0)
            {
                id = 0;
            }
            else
            {
                id = id - 1;
            }

            PrepareApplicationsView();
            ViewBag.School = ALL_SCHOOL_SCHOOL;

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

        public ActionResult ExportApplicants()
        {
            var applicants = db.Applicants.Where(a => a.ConfirmationCode != null).OrderBy(a=>a.StudentLastName).ToList();

            return ExportApplicantsCsvFile(applicants, "applicants_all.csv");
        }

        public ActionResult ExportApplicantsForSchool(int id)
        {
            var school = db.Schools.Find(id);
            if (school == null)
            {
                return HttpNotFound();
            }

            var applieds = db.Applieds.Where(a => a.SchoolID == id).OrderBy(a=>a.Applicant.StudentLastName).ToList();
            var applicants = Utils.GetApplicants(applieds);

            var schoolName = db.Schools.Find(id).Name.Replace(' ', '_');
            return ExportApplicantsCsvFile(applicants, "applicants_" + schoolName + ".csv");
        }


        public ActionResult ViewApplicantsForSchool(int? id)
        {
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
                var selecteds = db.Selecteds.Where(s => s.SchoolID == id).OrderBy(s => s.Rank).ToList();
                var selectedApplicants = new List<Applicant>();
                foreach (var selected in selecteds) // don't convert to LINQ -- needs to preserve order
                {
                    selectedApplicants.Add(selected.Applicant);
                }
                ViewBag.SelectedApplicants = selectedApplicants;

                var waitlisteds = db.Waitlisteds.Where(w => w.SchoolID == id).OrderBy(w => w.Rank).ToList();
                var waitlistedApplicants = new List<Applicant>();
                foreach (var waitlisted in waitlisteds) // don't convert to LINQ -- needs to preserve order
                {
                   waitlistedApplicants.Add(waitlisted.Applicant);
                }
                ViewBag.WaitlistedApplicants = waitlistedApplicants;
            }
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
        
        private void PrepareApplicantDetailView(Applicant applicant)
        {
            // Applied
            var applieds = db.Applieds.Where(a => a.ApplicantID == applicant.ID).OrderBy(a => a.School.Name).ToList();
            ViewBag.AppliedSchools = Utils.GetSchools(applieds);
         
            // Selected
            var selected = db.Selecteds.FirstOrDefault(s => s.ApplicantID == applicant.ID);
            if (selected != null)
            {
                ViewBag.SelectedSchool = selected.School;
            }
            
            // Waitlisted
            ViewBag.WaitlistedSchools =
                Utils.GetSchools(db.Waitlisteds.Where(a => a.ApplicantID == applicant.ID).OrderBy(a => a.School.Name).ToList());
            ViewBag.WasLotteryRun = GetLotteryRunDate() != null;
        }

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

        public ActionResult EditApplicant(int id)
        {
            var applicant = db.Applicants.Find(id);
            if (applicant == null)
            {
                return HttpNotFound();
            }

            PrepareEditApplicantView(applicant);

            return View(applicant);
        }

        [HttpPost]
        public ActionResult EditApplicant(Applicant applicant, FormCollection formCollection)
        {
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

            db.Applicants.AddOrUpdate(applicant);
            db.SaveChanges();

            return RedirectToAction("ViewApplicant", new{id=applicant.ID});
        }


        public ActionResult ViewDuplicateApplicants()
        {
            var schoolDuplicates = new Dictionary<School, List<Applicant>>();
            
            var schools = db.Schools.OrderBy(s => s.Name).ToList();
            foreach (var s in schools)
            {
                var applieds =
                    db.Applieds.Where(a => a.SchoolID == s.ID)
                        .OrderBy(a => a.Applicant.StudentLastName)
                        .ThenBy(a => a.Applicant.StudentFirstName)
                        .ToList();
                var duplicates = Utils.GetDuplicateApplicants(Utils.GetApplicants(applieds));

                schoolDuplicates.Add(s,duplicates);
            }

            return View(schoolDuplicates);
        }

        public ActionResult DeleteApplicant(int id)
        {
            var applicant = db.Applicants.Find(id);
            if (applicant == null)
            {
                return HttpNotFound();
            }

            PrepareApplicantDetailView(applicant);

            return View(applicant);
        }

        [HttpPost]
        public ActionResult DeleteApplicant(Applicant applicant)
        {
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

                    db.Selecteds.Remove(selected);

                    var waitlistedApplicants = Utils.GetApplicants(db.Waitlisteds.Where(w => w.SchoolID == school.ID).OrderBy(w => w.Rank).ToList());

                    var lottery = new SchoolLottery(db);
                    lottery.Run(school, waitlistedApplicants, false);

                    // Remove selected applicants from the selected school from other waitlists (does it for all since we don't know which student was the one filled in at this point)
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



            db.Applicants.Remove(queriedApplicant);
            
            
            db.SaveChanges();

            return RedirectToAction("ViewApplicants");
        }

        public ActionResult ViewSchools()
        {
            return View(db.Schools.OrderBy(s=>s.Name).ToList());
        }

        public ActionResult AddSchool()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddSchool(School school)
        {
            // Convert rates to multipliers
            school.GenderBalance /= 100;
            school.PovertyRate /= 100;

            // Validate
            ModelState.Clear();
            TryValidateModel(school);
            if (!ModelState.IsValid)
            {
                return View(school);
            }

            db.Schools.Add(school);
            db.SaveChanges();

            return RedirectToAction("ViewSchools");
        }

        public ActionResult EditSchool(int id)
        {
            var school = db.Schools.Find(id);
            if (school == null)
            {
                return HttpNotFound();
            }

            // Convert rates to percents
            school.GenderBalance *= 100;
            school.PovertyRate *= 100;

            return View(school);
        }

        [HttpPost]
        public ActionResult EditSchool(School school)
        {

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

            db.Schools.AddOrUpdate(school);
            db.SaveChanges();

            return RedirectToAction("ViewSchools");
        }

        public ActionResult DeleteSchool(int id)
        {
            var school = db.Schools.Find(id);
            if (school == null)
            {
                return HttpNotFound();
            }

            return View(school);
        }

        [HttpPost]
        public ActionResult DeleteSchool(School school)
        {
            var queriedSchool = db.Schools.Find(school.ID);
            if (queriedSchool == null)
            {
                return HttpNotFound();
            }

            db.Applieds.RemoveRange(queriedSchool.Applieds);
            db.Selecteds.RemoveRange(queriedSchool.Selecteds);
            db.Waitlisteds.RemoveRange(queriedSchool.Waitlisteds);
            db.Shuffleds.RemoveRange(queriedSchool.Shuffleds);
            db.Schools.Remove(queriedSchool);
            db.SaveChanges();

            return RedirectToAction("ViewSchools");
        }

        public ActionResult EditSettings()
        {
            ViewBag.PovertyConfigs = db.PovertyConfigs.ToList();
            return View(db.GlobalConfigs.First());
        }

        [HttpPost]
        public ActionResult EditSettings(GlobalConfig globalConfig, FormCollection formCollection)
        {
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

                previousMinIncome = minIncome;
                povertyConfig.MinimumIncome = minIncome;
                updatedPovertyConfigs.Add(povertyConfig);
            }

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

        public ActionResult ResetLottery()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetLottery(string id) // id is just dummy text, to differentiate from the GET-based ResetLottery() ... comes in as a static-valued hidden field
        {
            var applicants = db.Applicants.ToList();
            db.Applicants.RemoveRange(applicants);

            var globalConfig = db.GlobalConfigs.First();
            globalConfig.LotteryRunDate = null;
            db.GlobalConfigs.Add(globalConfig);

            db.SaveChanges();

            return RedirectToAction("EditSettings");
        }

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

        public ActionResult ViewAdmins()
        {
            var userManager= new ApplicationUserManager(new UserStore<ApplicationUser>(identityContext));
            var currentUser = userManager.FindById(User.Identity.GetUserId());
            ViewBag.CurrentUser = currentUser;

            return View(identityContext.Users.ToList());
        }

        public ActionResult AddAdmin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddAdmin(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser()
                {
                    UserName = registerViewModel.Email,
                    Email = registerViewModel.Email,
                    EmailConfirmed = true // we don't confirm email, so just switch this to true so that password resets can happen
                };
                var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(identityContext));
                userManager.Create(user, registerViewModel.Password);

                identityContext.SaveChanges();

                return RedirectToAction(actionName: "ViewAdmins");
            }

            return View(registerViewModel);
        }

        /*
         * ---------- HELPER METHODS ------------
         */

        private static School GetAllSchoolSchool()
        {
            var school = new School();
            school.ID = 0;
            school.Name = "All Schools";

            return school;
        }

        private DateTime? GetLotteryRunDate()
        {
            return db.GlobalConfigs.First().LotteryRunDate; // real call
//            return null; // forced lottery not run
            //return new DateTime(2014, 11, 21); // forced lottery run already
        }

        private DateTime? GetLotteryCloseDate()
        {
            return db.GlobalConfigs.First().CloseDate; // real call
//            return new DateTime(2014, 11, 20); // forced lottery closed
            //return new DateTime(2014, 11, 30); // forced lottery open
        }

        private bool WasLotteryRun()
        {
            return db.GlobalConfigs.First().LotteryRunDate != null;
        }

        private void AddSchoolsToViewBag()
        {
            var schools = db.Schools.OrderBy(s => s.Name).ToList();
            schools.Insert(0, ALL_SCHOOL_SCHOOL);
            ViewBag.Schools = schools;
        }

        private FileStreamResult ExportApplicantsCsvFile(IEnumerable<Applicant> applicants, string fileName)
        {
            var csvText = Utils.ApplicantsToCsv(applicants);

            var byteArray = Encoding.UTF8.GetBytes(csvText);
            var stream = new MemoryStream(byteArray);

            return File(stream, "text/plain", fileName);
        }

        private void PrepareEditApplicantView(Applicant applicant)
        {
            viewHelper.PrepareStudentInformationView(ViewBag, false);
            viewHelper.PrepareGuardianInformationView(ViewBag);
            viewHelper.PrepareSchoolSelectionView(ViewBag, applicant);
        }
    }
}