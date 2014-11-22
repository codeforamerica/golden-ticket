using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GoldenTicket.Models;
using GoldenTicket.DAL;

namespace GoldenTicket.Controllers
{
    public class AdminController : Controller
    {

        private GoldenTicketDbContext db = new GoldenTicketDbContext();
        private static School ALL_SCHOOL_SCHOOL = GetAllSchoolSchool();

        // All Applications
        // GET: Admin
        public ActionResult Index()
        {
            return Redirect("AllApplications");
        }


        private void PrepareApplicationsView()
        {
            ViewBag.LotteryRunDate = GetLotteryRunDate();
            ViewBag.IsLotteryClosed = GetLotteryCloseDate() <= DateTime.Now;

            var schools = db.Schools.OrderBy(s => s.Name).ToList();
            schools.Insert(0, ALL_SCHOOL_SCHOOL);
            ViewBag.Schools = schools;
        }

        public ActionResult AllApplicants()
        {
            PrepareApplicationsView();

            ViewBag.Applicants = db.Applicants.Where( a=>a.ConfirmationCode != null ).OrderBy(a => a.StudentLastName).ToList();
            
            return View();
        }

        public ActionResult SchoolApplicants(string id)
        {
            ViewBag.TestID = id;
            return View();
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
            //return new DateTime(2014, 11, 20); // forced lottery closed
            return new DateTime(2014, 11, 30); // forced lottery open
        }
    }
}