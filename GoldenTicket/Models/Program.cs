using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoldenTicket.Models
{
    public class Program
    {
        public Program()
        {
            Applieds = new List<Applied>();
            Shuffleds = new List<Shuffled>();
            Selecteds = new List<Selected>();
            Waitlisteds = new List<Waitlisted>();
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int Seats { get; set; }
        public double PovertyRate { get; set; }

        // Foreign items
        public virtual ICollection<Applied> Applieds { get; set; }
        public virtual ICollection<Shuffled> Shuffleds { get; set; }
        public virtual ICollection<Selected> Selecteds {get; set;}
        public virtual ICollection<Waitlisted> Waitlisteds {get; set;}

    }
}