using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using GoldenTicket.Resources;


namespace GoldenTicket.Models
{
    public class Applicant
    {
        public Applicant()
        {
            HouseholdMembers = 2; // default value, to get past minimum validation value
        }

        public int ID { get; set; }

        [RegularExpression(ValidationConstants.SAFE_TEXT, ErrorMessageResourceName="LettersSpacesAndApostrophesOnlyError", ErrorMessageResourceType=typeof(GoldenTicketText))]
        [Display(Name = "StudentFirstName", ResourceType = typeof(GoldenTicketText))]
        public string StudentFirstName { get; set; }

        [RegularExpression(ValidationConstants.SAFE_TEXT, ErrorMessageResourceName = "LettersSpacesAndApostrophesOnlyError", ErrorMessageResourceType = typeof(GoldenTicketText))]
        [Display(Name = "StudentMiddleName", ResourceType = typeof(GoldenTicketText))]
        public string StudentMiddleName { get; set; }

        [RegularExpression(ValidationConstants.SAFE_TEXT, ErrorMessageResourceName = "LettersSpacesAndApostrophesOnlyError", ErrorMessageResourceType = typeof(GoldenTicketText))]
        [Display(Name = "StudentLastName", ResourceType = typeof(GoldenTicketText))]
        public string StudentLastName { get; set; }

        [RegularExpression(ValidationConstants.STREET_ADDRESS_REGEX, ErrorMessageResourceName = "StreetAddressValidationError", ErrorMessageResourceType = typeof(GoldenTicketText))]
        [Display(Name = "StudentStreetAddress1", ResourceType = typeof(GoldenTicketText))]
        public string StudentStreetAddress1 { get; set; }

        [RegularExpression(ValidationConstants.STREET_ADDRESS_REGEX, ErrorMessageResourceName = "StreetAddressValidationError", ErrorMessageResourceType = typeof(GoldenTicketText))]
        [Display(Name = "StudentStreetAddress2", ResourceType = typeof(GoldenTicketText))]
        public string StudentStreetAddress2 { get; set; }

        [RegularExpression(ValidationConstants.SAFE_TEXT, ErrorMessageResourceName = "LettersSpacesAndApostrophesOnlyError", ErrorMessageResourceType = typeof(GoldenTicketText))]
        [Display(Name = "StudentCity", ResourceType = typeof(GoldenTicketText))]
        public string StudentCity { get; set; }

        [RegularExpression(ValidationConstants.ZIP_CODE_REGEX, ErrorMessageResourceName = "ZipCodeValidationError", ErrorMessageResourceType = typeof(GoldenTicketText))]
        [Display(Name = "StudentZipCode", ResourceType = typeof(GoldenTicketText))]
        public string StudentZipCode { get; set; }

        [Display(Name = "StudentGender", ResourceType = typeof(GoldenTicketText))]
        public Gender? StudentGender { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Display(Name = "StudentBirthday", ResourceType = typeof(GoldenTicketText))]
        public DateTime? StudentBirthday { get; set; }
        
        // Household information
        [Range(2, int.MaxValue, ErrorMessageResourceName = "HouseholdMemberRange", ErrorMessageResourceType = typeof(GoldenTicketText))]
        [Display(Name = "HouseholdMembers", ResourceType = typeof(GoldenTicketText))]
        public int HouseholdMembers { get; set; }
        
        [Range(0, int.MaxValue)]
        [Display(Name = "HouseholdMonthlyIncome", ResourceType = typeof(GoldenTicketText))]
        public int HouseholdMonthlyIncome { get; set; }

        // Contact person 1
        [RegularExpression(ValidationConstants.SAFE_TEXT, ErrorMessageResourceName = "LettersSpacesAndApostrophesOnlyError", ErrorMessageResourceType = typeof(GoldenTicketText))]
        [Display(Name = "Contact1FirstName", ResourceType = typeof(GoldenTicketText))]
        public string Contact1FirstName { get; set; }

        [RegularExpression(ValidationConstants.SAFE_TEXT, ErrorMessageResourceName = "LettersSpacesAndApostrophesOnlyError", ErrorMessageResourceType = typeof(GoldenTicketText))]
        [Display(Name = "Contact1LastName", ResourceType = typeof(GoldenTicketText))]
        public string Contact1LastName { get; set; }
        
        [RegularExpression(ValidationConstants.PHONE_REGEX, ErrorMessageResourceName="PhoneValidationError", ErrorMessageResourceType=typeof(GoldenTicketText))] //Phone annotation accepted internal numbers, using regex instead
        [Display(Name = "Contact1Phone", ResourceType = typeof(GoldenTicketText))]
        public string Contact1Phone { get; set; }

        [EmailAddress(ErrorMessageResourceName = "IsNotAValidEmailAddress", ErrorMessageResourceType = typeof(GoldenTicketText), ErrorMessage = null)] // ErrorMessage = Null is for .NET 4.5 Bug: https://connect.microsoft.com/VisualStudio/feedback/details/757298/emailaddress-attribute-is-unable-to-load-error-message-from-resource-mvc
        [Display(Name = "Contact1Email", ResourceType = typeof(GoldenTicketText))]
        public string Contact1Email { get; set; }

        [RegularExpression(ValidationConstants.SAFE_TEXT, ErrorMessageResourceName = "LettersSpacesAndApostrophesOnlyError", ErrorMessageResourceType = typeof(GoldenTicketText))]
        [Display(Name = "Contact1Relationship", ResourceType = typeof(GoldenTicketText))]
        public string Contact1Relationship { get; set; }

        // Contact person 2
        [RegularExpression(ValidationConstants.SAFE_TEXT, ErrorMessageResourceName = "LettersSpacesAndApostrophesOnlyError", ErrorMessageResourceType = typeof(GoldenTicketText))]
        [Display(Name = "Contact2FirstName", ResourceType = typeof(GoldenTicketText))]
        public string Contact2FirstName { get; set; }

        [RegularExpression(ValidationConstants.SAFE_TEXT, ErrorMessageResourceName = "LettersSpacesAndApostrophesOnlyError", ErrorMessageResourceType = typeof(GoldenTicketText))]
        [Display(Name = "Contact2LastName", ResourceType = typeof(GoldenTicketText))]
        public string Contact2LastName { get; set; }

        [RegularExpression(ValidationConstants.PHONE_REGEX, ErrorMessageResourceName = "PhoneValidationError", ErrorMessageResourceType = typeof(GoldenTicketText))] //Phone annotation accepted internal numbers, using regex instead
        [Display(Name = "Contact2Phone", ResourceType = typeof(GoldenTicketText))]
        public string Contact2Phone { get; set; }

        [EmailAddress(ErrorMessageResourceName = "IsNotAValidEmailAddress", ErrorMessageResourceType = typeof(GoldenTicketText), ErrorMessage = null)] // ErrorMessage = Null is for .NET 4.5 Bug: https://connect.microsoft.com/VisualStudio/feedback/details/757298/emailaddress-attribute-is-unable-to-load-error-message-from-resource-mvc
        [Display(Name = "Contact2Email", ResourceType = typeof(GoldenTicketText))]
        public string Contact2Email { get; set; }

        [RegularExpression(ValidationConstants.SAFE_TEXT, ErrorMessageResourceName = "LettersSpacesAndApostrophesOnlyError", ErrorMessageResourceType = typeof(GoldenTicketText))]
        [Display(Name = "Contact2Relationship", ResourceType = typeof(GoldenTicketText))]
        public string Contact2Relationship { get; set; }

        // Application information
        [Display(Name = "ConfirmationCode", ResourceType = typeof(GoldenTicketText))]
        public string ConfirmationCode { get; set; }

        // This is the language the applicant filled out the application in
        public string Language { get; set; }


        // ---- Utility Methods ----
        public int Checksum()
        {
            // hash code of first name, last name, and city (no spaces and all lowercase)  
            return (StudentLastName.Replace(" ", "").ToLower() + StudentCity.Replace(" ", "").ToLower() + StudentBirthday.ToString()).GetHashCode();
        }
    }
}