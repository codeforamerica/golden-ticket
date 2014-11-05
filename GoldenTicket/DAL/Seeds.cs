using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using GoldenTicket.Models;
using System.Data.Entity.Validation;

namespace GoldenTicket.DAL 
{
    public class Seeds : System.Data.Entity.DropCreateDatabaseIfModelChanges<GoldenTicketDbContext>
    {
        protected override void Seed(GoldenTicketDbContext context)
        {
            // Configurations
            var globalConfig = new GlobalConfig
            {
                ID = 1,
                OpenDate = new DateTime(2014, 10, 20),
                CloseDate = new DateTime(2014, 12, 31),
                MinimumIncomeMultiplier = 625
            };
            context.GlobalConfigs.Add(globalConfig);

            var povertyConfigs = new List<PovertyConfig> {
                new PovertyConfig{ HouseholdMembers = 2, MinimumIncome = 2425 },
                new PovertyConfig{ HouseholdMembers = 3, MinimumIncome = 3051 },
                new PovertyConfig{ HouseholdMembers = 4, MinimumIncome = 3676 },
                new PovertyConfig{ HouseholdMembers = 5, MinimumIncome = 4302 },
                new PovertyConfig{ HouseholdMembers = 6, MinimumIncome = 4928 },
                new PovertyConfig{ HouseholdMembers = 7, MinimumIncome = 5554 },
                new PovertyConfig{ HouseholdMembers = 8, MinimumIncome = 6180 },
                new PovertyConfig{ HouseholdMembers = 9, MinimumIncome = 6806 },
                new PovertyConfig{ HouseholdMembers = 10, MinimumIncome = 7432 },
            };
            povertyConfigs.ForEach(pc => context.PovertyConfigs.Add(pc));


            // Programs
            var willysSchool = new Program
            {
                Name = "Willy Wonka School",
                StreetAddress1 = "1000 Chocolate Factory Rd",
                StreetAddress2 = "Blah",
                City = "Providence",
                ZipCode = "02904",
                Phone = "401-000-0000",
                Email = "willy@wonka.org",
                Seats = 18,
                PovertyRate = 0.87,
                GenderBalance = 0.5
            };
            context.Programs.Add(willysSchool);

            var arthurAcademy = new Program
            {
                Name = "Arthur Academy",
                StreetAddress1 = "999 Gobstopper Blvd",
                City = "Providence",
                ZipCode = "02904",
                Phone = "401-555-1234",
                Email = "art@slugworth.com",
                Seats = 36,
                PovertyRate = 0.87,
                GenderBalance = 0.5
            };
            context.Programs.Add(arthurAcademy);

            var giantPeach = new Program
            {
                Name = "Giant Peach",
                StreetAddress1 = "1 Humongous Fruit Blvd",
                StreetAddress2 = "Pit Suite",
                City = "West Warwick",
                ZipCode = "02911",
                Phone = "401-333-3333",
                Email = "james@giantpeach.edu",
                Seats = 46,
                PovertyRate = 0.35,
                GenderBalance = 0.5
            };
            context.Programs.Add(giantPeach);
            
            context.SaveChanges();
        }
    }
}