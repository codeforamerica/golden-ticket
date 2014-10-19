using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoldenTicket.Models
{
    public class Shuffled
    {
        public int ID { get; set; }
        public int Rank { get; set; }

        // Foreign items
        public int StudentID { get; set; }
        public virtual Student Student { get; set; }

        public int ProgramID { get; set; }
        public virtual Program Program { get; set; }
    }
}