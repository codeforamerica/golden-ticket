using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using GoldenTicket.Misc;
using GoldenTicket.Models;
using GoldenTicket.DAL;

namespace GoldenTicket.Controllers
{
    public class AdminController : Controller
    {

        private GoldenTicketDbContext db = new GoldenTicketDbContext();
        private static readonly School ALL_SCHOOL_SCHOOL = GetAllSchoolSchool();

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
            var applicants = db.Applicants.OrderBy(a=>a.StudentLastName).ToList();

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
                var applieds = db.Applieds.Where(a => a.SchoolID == id).OrderBy(a => a.Applicant.StudentLastName).ToList();
                var applicants = new List<Applicant>();
                foreach (var applied in applieds) // don't convert to LINQ -- needs to preserve order
                {
                    applicants.Add(applied.Applicant);
                }
                ViewBag.Applicants = applicants;
            }

            // Other things needed for disply
            AddSchoolsToViewBag();

            return View();
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
            ViewBag.AppliedSchools = Utils.GetSchools(db.Applieds.Where(a => a.ApplicantID == applicant.ID).OrderBy(a=>a.School.Name).ToList());
            var selectedSchool = db.Selecteds.FirstOrDefault(s => s.ApplicantID == applicant.ID);
            if (selectedSchool != null)
            {
                ViewBag.SelectedSchool = selectedSchool;
            }
            ViewBag.WaitlistedSchools = Utils.GetSchools(db.Waitlisteds.Where(a => a.ApplicantID == applicant.ID).OrderBy(a => a.School.Name).ToList());
            ViewBag.WasLotteryRun = GetLotteryRunDate() != null;

            return View(applicant);
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
            //return db.GlobalConfigs.First().LotteryRunDate; // real call
            return null; // forced lottery not run
            //return new DateTime(2014, 11, 21); // forced lottery run already
        }

        private DateTime? GetLotteryCloseDate()
        {
            //return db.GlobalConfigs.First().CloseDate; // real call
            return new DateTime(2014, 11, 20); // forced lottery closed
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
    }
}