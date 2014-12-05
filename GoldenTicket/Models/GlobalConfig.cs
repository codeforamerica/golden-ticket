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

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Required]
        public DateTime OpenDate {get; set;}

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Required]
        public DateTime CloseDate {get;set;}

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Required]
        public DateTime NotificationDate { get; set; }

        [Range(0,int.MaxValue)]
        [Required]
        public int IncomeMultiplier { get; set; }

        [Required]
        public string ContactPersonName { get; set; }

        [RegularExpression(ValidationConstants.PHONE_REGEX)]
        [Phone]
        [Required]
        public string ContactPhone { get; set; }

        [EmailAddress]
        [Required]
        public string ContactEmail { get; set; }

        /*
         * This is the day that the lottery was run. 
         * If null or not set, the lottery has not yet been run.
         **/
        public DateTime? LotteryRunDate { get; set; }

        public bool WereNotificationsSent { get; set; }
    }
}