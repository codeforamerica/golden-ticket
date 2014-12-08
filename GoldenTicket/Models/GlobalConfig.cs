using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoldenTicket.Models
{
    public class GlobalConfig
    {
        public int ID { get; set; }

        // <summary>Date that the lottery opens</summary>
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Required]
        public DateTime OpenDate {get; set;}

        // <summary>Date that the lottery closes </summary>
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Required]
        public DateTime CloseDate {get;set;}

        // <summary>Date that applicants should be notified by</summary>
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Required]
        public DateTime NotificationDate { get; set; }

        // <summary>Multiplier in dollars that the poverty line should be increased by per household member over 10</summary>
        [Range(0,int.MaxValue)]
        [Required]
        public int IncomeMultiplier { get; set; }

        // <summary>Lottery administrator to contact for help</summary>
        [Required]
        public string ContactPersonName { get; set; }

        // <summary>Phone number to contact a lottery administrator</summary>
        [RegularExpression(ValidationConstants.PHONE_REGEX)]
        [Phone]
        [Required]
        public string ContactPhone { get; set; }

        // <summary>Email to contact a lottery adminsitrator</summary>
        [EmailAddress]
        [Required]
        public string ContactEmail { get; set; }

        /*
         * This is the day that the lottery was run. 
         * If null or not set, the lottery has not yet been run.
         **/
        public DateTime? LotteryRunDate { get; set; }

        // <summary>True if selected/waitlisted notifications were sent</summary>
        public bool WereNotificationsSent { get; set; }
    }
}