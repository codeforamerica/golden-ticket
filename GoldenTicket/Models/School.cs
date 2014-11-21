using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoldenTicket.Models
{
    public class School
    {
        public School()
        {
            Applieds = new List<Applied>();
            Shuffleds = new List<Shuffled>();
            Selecteds = new List<Selected>();
            Waitlisteds = new List<Waitlisted>();
        }

        public int ID { get; set; }

        [RegularExpression(ValidationConstants.LETTERS_SPACES_DASHES_APOSTROPHE_REGEX)]
        [Required]
        public string Name { get; set; }

        [RegularExpression(ValidationConstants.STREET_ADDRESS_REGEX)]
        [Required]
        public string StreetAddress1 { get; set; }

        [RegularExpression(ValidationConstants.LETTERS_SPACES_DASHES_APOSTROPHE_REGEX)]
        public string StreetAddress2 { get; set; }
        
        [RegularExpression(ValidationConstants.LETTERS_SPACES_DASHES_APOSTROPHE_REGEX)]
        [Required]
        public string City { get; set; }

        [RegularExpression(ValidationConstants.ZIP_CODE_REGEX)]
        [Required]
        public string ZipCode { get; set; }

        [RegularExpression(ValidationConstants.PHONE_REGEX)] //Phone annotation accepted internal numbers, using regex instead
        [Required]
        public string Phone { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Range(1,int.MaxValue)]
        [Required]
        public int Seats { get; set; }

        [Range(0,double.MaxValue)]
        [Required]
        public double PovertyRate { get; set; }

        [Range(0, double.MaxValue)]
        [Required]
        public double GenderBalance { get; set; }

        // Foreign items
        public virtual ICollection<Applied> Applieds { get; set; }
        public virtual ICollection<Shuffled> Shuffleds { get; set; }
        public virtual ICollection<Selected> Selecteds {get; set;}
        public virtual ICollection<Waitlisted> Waitlisteds {get; set;}

    }
}