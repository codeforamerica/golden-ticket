using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using GoldenTicket.Models;
using System.Data.Entity.Validation;
using GoldenTicket.Csv;

namespace GoldenTicket.DAL 
{
    public class Seeds : System.Data.Entity.DropCreateDatabaseIfModelChanges<GoldenTicketDbContext>
    {
        private const string FAKE_ADDRESS_1 = "123 Main St";
        private const string FAKE_ADDRESS_2 = "Suite 300";
        private const string FAKE_ZIP_CODE = "02903";
        private const string FAKE_PHONE = "401-123-4567";
        private const string FAKE_EMAIL = "fake@email.com";
        private const double GENDER_BALANCE = 0.5;

        protected override void Seed(GoldenTicketDbContext db)
        {
            // Configurations
            var globalConfig = new GlobalConfig
            {
                ID = 1,
                OpenDate = new DateTime(2014, 10, 20),
                CloseDate = new DateTime(2014, 12, 25),
                NotificationDate = new DateTime(2014, 12, 31),
                IncomeMultiplier = 625,
                ContactPersonName = "Roald Dahl",
                ContactEmail = "roald@gtridetest.org",
                ContactPhone = "401-123-4567"
            };
            db.GlobalConfigs.Add(globalConfig);

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
            db.PovertyConfigs.AddRange(povertyConfigs);

            // Programs
            var captHunt = new School
            {
                Name = "Central Falls - Capt. Hunt Early Learning Center",
                StreetAddress1 = FAKE_ADDRESS_1,
                StreetAddress2 = FAKE_ADDRESS_2,
                City = "Central Falls",
                ZipCode = FAKE_ZIP_CODE,
                Phone = FAKE_PHONE,
                Email = FAKE_EMAIL,
                GenderBalance = GENDER_BALANCE,
                PovertyRate = 0.857,
                Seats = 18
            };
            db.Schools.Add(captHunt);

            var ccap = new School
            {
                Name = "Cranston - Comprehensive Community Action Program (CCAP)",
                StreetAddress1 = FAKE_ADDRESS_1,
                StreetAddress2 = FAKE_ADDRESS_2,
                City = "Cranston",
                ZipCode = FAKE_ZIP_CODE,
                Phone = FAKE_PHONE,
                Email = FAKE_EMAIL,
                GenderBalance = GENDER_BALANCE,
                PovertyRate = 0.432,
                Seats = 18
            };
            db.Schools.Add(ccap);

            var eastbay = new School
            {
                Name = "Newport - East Bay Community Action Head Start",
                StreetAddress1 = FAKE_ADDRESS_1,
                StreetAddress2 = FAKE_ADDRESS_2,
                City = "Newport",
                ZipCode = FAKE_ZIP_CODE,
                Phone = FAKE_PHONE,
                Email = FAKE_EMAIL,
                GenderBalance = GENDER_BALANCE,
                PovertyRate = 0.61,
                Seats = 36
            };
            db.Schools.Add(eastbay);

            var readyPaw = new School
            {
                Name = "Pawtucket - Ready to Learn-Heritage Park YMCA Early Learning Center",
                StreetAddress1 = FAKE_ADDRESS_1,
                StreetAddress2 = FAKE_ADDRESS_2,
                City = "Pawtucket",
                ZipCode = FAKE_ZIP_CODE,
                Phone = FAKE_PHONE,
                Email = FAKE_EMAIL,
                GenderBalance = GENDER_BALANCE,
                PovertyRate = 0.755,
                Seats = 18
            };
            db.Schools.Add(readyPaw);

            var beautiful = new School
            {
                Name = "Providence - Beautiful Beginnings",
                StreetAddress1 = FAKE_ADDRESS_1,
                StreetAddress2 = FAKE_ADDRESS_2,
                City = "Providence",
                ZipCode = FAKE_ZIP_CODE,
                Phone = FAKE_PHONE,
                Email = FAKE_EMAIL,
                GenderBalance = GENDER_BALANCE,
                PovertyRate = 0.867,
                Seats = 18
            };
            db.Schools.Add(beautiful);

            var mariposa = new School
            {
                Name = "Providence - The Mariposa Center",
                StreetAddress1 = FAKE_ADDRESS_1,
                StreetAddress2 = FAKE_ADDRESS_2,
                City = "Providence",
                ZipCode = FAKE_ZIP_CODE,
                Phone = FAKE_PHONE,
                Email = FAKE_EMAIL,
                GenderBalance = GENDER_BALANCE,
                PovertyRate = 0.867,
                Seats = 18
            };
            db.Schools.Add(mariposa);

            var readyProv = new School
            {
                Name = "Providence - Ready to Learn Providence at CCRI Liston Campus",
                StreetAddress1 = FAKE_ADDRESS_1,
                StreetAddress2 = FAKE_ADDRESS_2,
                City = "Providence",
                ZipCode = FAKE_ZIP_CODE,
                Phone = FAKE_PHONE,
                Email = FAKE_EMAIL,
                GenderBalance = GENDER_BALANCE,
                PovertyRate = 0.867,
                Seats = 18
            };
            db.Schools.Add(readyProv);

            var smithHill = new School
            {
                Name = "Providence - Smith Hill Early Childhood Learning Center",
                StreetAddress1 = FAKE_ADDRESS_1,
                StreetAddress2 = FAKE_ADDRESS_2,
                City = "Providence",
                ZipCode = FAKE_ZIP_CODE,
                Phone = FAKE_PHONE,
                Email = FAKE_EMAIL,
                GenderBalance = GENDER_BALANCE,
                PovertyRate = 0.867,
                Seats = 36
            };
            db.Schools.Add(smithHill);

            var childIncWarwick = new School
            {
                Name = "Warwick - CHILD Inc.",
                StreetAddress1 = FAKE_ADDRESS_1,
                StreetAddress2 = FAKE_ADDRESS_2,
                City = "Warwick",
                ZipCode = FAKE_ZIP_CODE,
                Phone = FAKE_PHONE,
                Email = FAKE_EMAIL,
                GenderBalance = GENDER_BALANCE,
                PovertyRate = 0.358,
                Seats = 18
            };
            db.Schools.Add(childIncWarwick);

            var imagine = new School
            {
                Name = "Warwick - Imagine Preschool at CCRI Knight Campus",
                StreetAddress1 = FAKE_ADDRESS_1,
                StreetAddress2 = FAKE_ADDRESS_2,
                City = "Warwick",
                ZipCode = FAKE_ZIP_CODE,
                Phone = FAKE_PHONE,
                Email = FAKE_EMAIL,
                GenderBalance = GENDER_BALANCE,
                PovertyRate = 0.358,
                Seats = 18
            };
            db.Schools.Add(imagine);

            var westbay = new School
            {
                Name = "Warwick - Westbay Community Action Children's Center",
                StreetAddress1 = FAKE_ADDRESS_1,
                StreetAddress2 = FAKE_ADDRESS_2,
                City = "Warwick",
                ZipCode = FAKE_ZIP_CODE,
                Phone = FAKE_PHONE,
                Email = FAKE_EMAIL,
                GenderBalance = GENDER_BALANCE,
                PovertyRate = 0.358,
                Seats = 18
            };
            db.Schools.Add(westbay);

            var childIncWestWarwick = new School
            {
                Name = "West Warwick - CHILD Inc.",
                StreetAddress1 = FAKE_ADDRESS_1,
                StreetAddress2 = FAKE_ADDRESS_2,
                City = "West Warwick",
                ZipCode = FAKE_ZIP_CODE,
                Phone = FAKE_PHONE,
                Email = FAKE_EMAIL,
                GenderBalance = GENDER_BALANCE,
                PovertyRate = 0.536,
                Seats = 36
            };
            db.Schools.Add(childIncWestWarwick);

            var woonsocket = new School
            {
                Name = "Woonsocket - Woonsocket Head Start Child Development Association",
                StreetAddress1 = FAKE_ADDRESS_1,
                StreetAddress2 = FAKE_ADDRESS_2,
                City = "Woonsocket",
                ZipCode = FAKE_ZIP_CODE,
                Phone = FAKE_PHONE,
                Email = FAKE_EMAIL,
                GenderBalance = GENDER_BALANCE,
                PovertyRate = 0.767,
                Seats = 36
            };
            db.Schools.Add(woonsocket);

            db.SaveChanges();


            // Import applicants
            var csvFilePath = HttpContext.Current.Server.MapPath("~/TestData/data.csv");
            var applicantCsvReader = new ApplicantCsvReader(csvFilePath, db.Schools.ToList());
            applicantCsvReader.ReadApplicants();
        }
    }
}