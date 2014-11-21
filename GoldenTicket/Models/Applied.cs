using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoldenTicket.Models
{
    public class Applied
    {
        public Applied()
        {
            AddedDateTime = DateTime.Now;
        }

        public int ID { get; set; }
        public DateTime AddedDateTime { get; set; }

        // Foreign items
        public int ApplicantID { get; set; }
        public virtual Applicant Applicant { get; set; }

        public int ProgramID { get; set; }
        public virtual School Program { get; set; }
    }
}