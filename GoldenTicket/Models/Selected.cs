using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoldenTicket.Models
{
    public class Selected
    {
        public int ID { get; set; }
        public int Rank { get; set; }

        // Foreign items
        public int ApplicantID { get; set; }
        public virtual Applicant Applicant { get; set; }

        public int ProgramID { get; set; }
        public virtual School Program { get; set; }
    }
}