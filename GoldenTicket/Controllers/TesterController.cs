using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GoldenTicket.Models;
using GoldenTicket.DAL;

namespace GoldenTicket.Controllers
{
    public class TesterController : Controller
    {
        private readonly GoldenTicketDbContext database = new GoldenTicketDbContext();

        // GET: Tester
        public ActionResult Index()
        {
            ViewBag.Applicants = database.Applicants.Where(a => a.ConfirmationCode != null).ToList();

            return View();
        }
    }
}