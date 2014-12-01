using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GoldenTicket.Models
{
    public class PovertyConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int HouseholdMembers { get; set; }

        [Range(0,int.MaxValue)]
        [Required]
        public int MinimumIncome { get; set; }
    }
}