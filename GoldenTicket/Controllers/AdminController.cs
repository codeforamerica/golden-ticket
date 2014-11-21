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
        private static School allSchoolSchool = GetAllSchoolSchool();

        // All Applications
        // GET: Admin
        public ActionResult Index()
        {
            return Redirect("AllApplications");
        }

        public ActionResult AllApplications()
        {
            PrepareApplicationsView();

            ViewBag.Applicants = db.Applicants.Where( a=>a.ConfirmationCode != null ).OrderBy(a => a.StudentLastName).ToList();
            
            return View();
        }

        /*
         * ---------- HELPER METHODS ------------
         */

        private static School GetAllSchoolSchool()
        {
            School school = new School();
            school.ID = 0;
            school.Name = "All Schools";

            return school;
        }

        private void PrepareApplicationsView()
        {
            var globalConfig = db.GlobalConfigs.First();
            ViewBag.LotteryRunDate = globalConfig.LotteryRunDate;
            ViewBag.IsLotteryClosed = globalConfig.CloseDate <= DateTime.Now;

            var schools = db.Schools.OrderBy(s => s.Name).ToList();
            schools.Insert(0, GetAllSchoolSchool());
            ViewBag.Schools = schools;
        }

    }
}