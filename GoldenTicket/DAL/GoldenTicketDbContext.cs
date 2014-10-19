using System;
using System.Data.Entity;

using GoldenTicket.Models;

namespace GoldenTicket.DAL
{
    public class GoldenTicketDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        public DbSet<Program> Programs { get; set; }
        public DbSet<Applied> Applieds { get; set; }
        public DbSet<Shuffled> Shuffleds { get; set; }
        public DbSet<Selected> Selecteds { get; set; }
        public DbSet<Waitlisted> Waitlisteds { get; set; }
    }
}