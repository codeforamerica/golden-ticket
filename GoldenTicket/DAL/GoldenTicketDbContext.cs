using System;
using System.Data.Entity;

using GoldenTicket.Models;

namespace GoldenTicket.DAL
{
    public class GoldenTicketDbContext : DbContext
    {
        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<Applied> Applieds { get; set; }
        public DbSet<Shuffled> Shuffleds { get; set; }
        public DbSet<Selected> Selecteds { get; set; }
        public DbSet<Waitlisted> Waitlisteds { get; set; }

        public DbSet<GlobalConfig> GlobalConfigs { get; set; }

        public DbSet<PovertyConfig> PovertyConfigs { get; set; }
    }
}