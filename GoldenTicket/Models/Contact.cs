using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoldenTicket.Models
{
    public class Contact
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Relationship { get; set; }
        public string Email { get; set; }

        // Foreign items
        public int StudentID { get; set; }
        public virtual Student Student { get; set; }
    }
}