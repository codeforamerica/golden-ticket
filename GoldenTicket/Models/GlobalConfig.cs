using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoldenTicket.Models
{
    public class GlobalConfig
    {
        public int ID { get; set; }
        public DateTime OpenDate {get; set;}
        public DateTime CloseDate {get;set;}
        public DateTime NotificationDate { get; set; }
        public double MinimumIncomeMultiplier { get; set; }
    }
}