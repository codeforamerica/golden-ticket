using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GoldenTicket.DAL;
using GoldenTicket.Models;
using System.Data.Entity; // used for Include(lambda) method

namespace GoldenTicket.Controllers
{
    public class StudentViewController : Controller
    {
        GoldenTicketDbContext dbContext = new GoldenTicketDbContext();

        // GET: StudentView
        public ActionResult Index()
        {
            var students = dbContext.Students.Include(s => s.Contacts);
            ViewBag.Students = students;

            var contacts = dbContext.Contacts;
            ViewBag.Contacts = contacts;

            return View();
        }

        public ActionResult Applied()
        {
            var school = dbContext.Programs.Where(p => p.Name.Equals("Willy's School")).Include(p=>p.Applieds).First();
            ViewBag.School = school;

            return View();
        }
    }
}