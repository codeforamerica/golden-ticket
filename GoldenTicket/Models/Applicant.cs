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
        public Applicant()
        {
            HouseholdMembers = 2; // default value, to get past minimum validation value
        }

        public int ID { get; set; }

        [RegularExpression(ValidationConstants.LETTERS_SPACES_DASHES_REGEX)]
        public string StudentFirstName { get; set; }

        [RegularExpression(ValidationConstants.LETTERS_SPACES_DASHES_REGEX)]
        public string StudentMiddleName { get; set; }

        [RegularExpression(ValidationConstants.LETTERS_SPACES_DASHES_REGEX)]
        public string StudentLastName { get; set; }

        [RegularExpression(ValidationConstants.STREET_ADDRESS_REGEX)]
        public string StudentStreetAddress1 { get; set; }

        [RegularExpression(ValidationConstants.STREET_ADDRESS_REGEX)]
        public string StudentStreetAddress2 { get; set; }

        [RegularExpression(ValidationConstants.LETTERS_SPACES_DASHES_REGEX)]
        public string StudentCity { get; set; }

        [RegularExpression(ValidationConstants.ZIP_CODE_REGEX)]
        public string StudentZipCode { get; set; }

        public Gender? StudentGender { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? StudentBirthday { get; set; }
        
        // Household information
        [Range(2, int.MaxValue)]
        public int HouseholdMembers { get; set; }
        
        [Range(0, int.MaxValue)]
        public int HouseholdMonthlyIncome { get; set; }

        // Contact person 1
        [RegularExpression(ValidationConstants.LETTERS_SPACES_DASHES_REGEX)]
        public string Contact1FirstName { get; set; }

        [RegularExpression(ValidationConstants.LETTERS_SPACES_DASHES_REGEX)]
        public string Contact1LastName { get; set; }
        
        [Phone]
        public string Contact1Phone { get; set; }
        
        [EmailAddress]
        public string Contact1Email { get; set; }

        [RegularExpression(ValidationConstants.LETTERS_SPACES_DASHES_REGEX)]
        public string Contact1Relationship { get; set; }

        // Contact person 2
        [RegularExpression(ValidationConstants.LETTERS_SPACES_DASHES_REGEX)]
        public string Contact2FirstName { get; set; }

        [RegularExpression(ValidationConstants.LETTERS_SPACES_DASHES_REGEX)]
        public string Contact2LastName { get; set; }
        
        [Phone]
        public string Contact2Phone { get; set; }
        
        [EmailAddress]
        public string Contact2Email { get; set; }

        [RegularExpression(ValidationConstants.LETTERS_SPACES_DASHES_REGEX)]
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