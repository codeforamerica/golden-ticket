using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace GoldenTicket.Models
{
    public class Applicant
    {
        private const string LETTERS_SPACES_DASHES_REGEX = @"\A(\p{L}|\s|-)+\z";
        private const string STREET_ADDRESS_REGEX = @"/\A[\p{L}#\d\s\.-]+\z/";
        private const string ZIP_CODE_REGEX = @"\A\d{5}(-\d{4})?\z";
        private const string STATE_REGEX = @"\A[a-z]{2}\z";

        public int ID { get; set; }

        [RegularExpression(LETTERS_SPACES_DASHES_REGEX)]
        public string StudentFirstName { get; set; }

        [RegularExpression(LETTERS_SPACES_DASHES_REGEX)]
        public string StudentMiddleName { get; set; }

        [RegularExpression(LETTERS_SPACES_DASHES_REGEX)]
        public string StudentLastName { get; set; }

        [RegularExpression(STREET_ADDRESS_REGEX)]
        public string StudentStreetAddress1 { get; set; }
        
        [RegularExpression(STREET_ADDRESS_REGEX)]
        public string StudentStreetAddress2 { get; set; }
        
        [RegularExpression(LETTERS_SPACES_DASHES_REGEX)]
        public string StudentCity { get; set; }
        
        [RegularExpression(STATE_REGEX)]
        public string StudentState { get; set; }

        [RegularExpression(ZIP_CODE_REGEX)
        public string StudentZipCode { get; set; }
        public Gender? StudentGender { get; set; }
        public DateTime? StudentBirthday { get; set; }
        
        // Household information
        [Range(1,Int32.MaxValue)]
        public int HouseholdMembers { get; set; }
        
        [Range(0, Int32.MaxValue)]
        public int HouseholdMonthlyIncome { get; set; }

        // Contact person 1
        [RegularExpression(LETTERS_SPACES_DASHES_REGEX)]
        public string Contact1FirstName { get; set; }

        [RegularExpression(LETTERS_SPACES_DASHES_REGEX)]
        public string Contact1LastName { get; set; }
        
        [Phone]
        public string Contact1Phone { get; set; }
        
        [EmailAddress]
        public string Contact1Email { get; set; }
        
        [RegularExpression(LETTERS_SPACES_DASHES_REGEX)]
        public string Contact1Relationship { get; set; }

        // Contact person 2
        [RegularExpression(LETTERS_SPACES_DASHES_REGEX)]
        public string Contact2FirstName { get; set; }
        
        [RegularExpression(LETTERS_SPACES_DASHES_REGEX)]
        public string Contact2LastName { get; set; }
        
        [Phone]
        public string Contact2Phone { get; set; }
        
        [EmailAddress]
        public string Contact2Email { get; set; }
        
        [RegularExpression(LETTERS_SPACES_DASHES_REGEX)]
        public string Contact2Relationship { get; set; }

        // Application information
        public string ConfirmationCode { get; set; }

    }

    // ---- Enums ----
    public enum Gender
    {
        Male, Female
    }

}