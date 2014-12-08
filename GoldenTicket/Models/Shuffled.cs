using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoldenTicket.Models
{
    public class Shuffled
    {
        public int ID { get; set; }

        // Lower number is better for applicant
        public int Rank { get; set; }

        // Foreign items
        public int ApplicantID { get; set; }
        public virtual Applicant Applicant { get; set; }

        public int SchoolID { get; set; }
        public virtual School School { get; set; }
    }
}