using System;
using System.Data.Entity;

using GoldenTicket.Models;

namespace GoldenTicket.DAL
{
    /**
     * <summary>
     * Stores model objects in a database.
     * </summary>
     */
    public class GoldenTicketDbContext : DbContext
    {
        // <summary>Applications put into the system<summary>
        public DbSet<Applicant> Applicants { get; set; }

        // <summary>Schools that applicants can apply for<summary>
        public DbSet<School> Schools { get; set; }

        // <summary>Association table linking an applicant to a school</summary>
        public DbSet<Applied> Applieds { get; set; }

        // <summary>Association table linking an applicant to a school, while preserving the order they were randomly shuffled for the lottery<summary>
        public DbSet<Shuffled> Shuffleds { get; set; }

        // <summary>Association table linking an applicant to a school, while preserving the order they were selected for the lottery<summary>
        public DbSet<Selected> Selecteds { get; set; }

        // <summary>Association table linking an applicant to a school, while preserving the order they were waitlisted for the lottery<summary>
        public DbSet<Waitlisted> Waitlisteds { get; set; }

        // <summary>Global configuration of the system. A one row table, able to always use .First()</summary>
        public DbSet<GlobalConfig> GlobalConfigs { get; set; }

        // <summary>Configurations linking household income to the poverty line, based on the number of people in a household.</summary>
        public DbSet<PovertyConfig> PovertyConfigs { get; set; }
    }
}