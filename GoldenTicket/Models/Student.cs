using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;


namespace GoldenTicket.Models
{
    public class Student
    {
        public Student()
        {
            Contacts = new List<Contact>();
        }
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public Gender? Gender { get; set; }
        public int HouseholdMembers { get; set; }
        public int HouseholdMonthlyIncome { get; set; }
        
        // Foreign items
        public virtual ICollection<Contact> Contacts { get; set; }
    }

    // ---- Enums ----
    public enum Gender
    {
        Male, Female
    }

}