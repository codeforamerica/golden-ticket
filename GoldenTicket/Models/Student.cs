using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;


namespace GoldenTicket.Models
{
    public class Applicant
    {
        public Applicant()
        {
        }

        public int ID { get; set; }
        public string StudentFirstName { get; set; }
        public string StudentMiddleName { get; set; }
        public string StudentLastName { get; set; }
        public string StudentStreetAddress1 { get; set; }
        public string StudentStreetAddress2 { get; set; }
        public string StudentCity { get; set; }
        public string StudentState { get; set; }
        public string StudentZipCode { get; set; }
        public Gender? StudentGender { get; set; }
        public int HouseholdMembers { get; set; }
        public int HouseholdMonthlyIncome { get; set; }

        // Contact person 1
        public string Contact1FirstName { get; set; }
        public string Contact1LastName { get; set; }
        public string Contact1Phone { get; set; }
        public string Contact1Email { get; set; }
        public string Contact1Relationship { get; set; }

        // Contact person 2
        public string Contact2FirstName { get; set; }
        public string Contact2LastName { get; set; }
        public string Contact2Phone { get; set; }
        public string Contact2Email { get; set; }
        public string Contact2Relationship { get; set; }

    }

    // ---- Enums ----
    public enum Gender
    {
        Male, Female
    }

}