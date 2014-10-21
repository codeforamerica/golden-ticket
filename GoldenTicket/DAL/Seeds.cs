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
            // Programs
            Program willysSchool = new Program { 
                Name = "Willy's School", 
                StreetAddress1 = "1000 Chocolate Factory Rd", 
                City = "Providence", 
                State = "RI", 
                ZipCode = "02904", 
                Email = "willy@wonka.org", 
                Phone = "401-000-0000", 
                Seats = 18, 
                PovertyRate = 0.87 
            };
            context.Programs.Add(willysSchool);

            Program arthurAcademy = new Program { 
                Name = "Arthur Academy", 
                StreetAddress1 = "999 Gobstopper Blvd", 
                City = "Providence", 
                State = "RI", 
                ZipCode = "02904", 
                Email = "art@slugworth.com", 
                Phone = "401-555-1234", 
                Seats = 36, 
                PovertyRate = 0.87 
            };
            context.Programs.Add(arthurAcademy);

            // Student with 1 contact, applied to 1 program
            Applicant charlieBucket = new Applicant { 
                StudentFirstName = "Charlie", 
                StudentMiddleName = "C", 
                StudentLastName = "Bucket", 
                StudentCity = "Providence", 
                StudentState = "RI", 
                StudentZipCode="02903", 
                StudentGender = Gender.Male, 
                HouseholdMembers=4, 
                HouseholdMonthlyIncome=30000,
                Contact1FirstName= "Joe",
                Contact1LastName= "Bucket",
                Contact1Phone="401-111-1111",
                Contact1Email="grandpajoe@dahl.net"
            };

            Applied charlieBucketApplied = new Applied();
            charlieBucketApplied.Applicant = charlieBucket;
            charlieBucketApplied.Program = willysSchool;
            willysSchool.Applieds.Add(charlieBucketApplied);
            context.Applieds.Add(charlieBucketApplied);
            
            context.Applicants.Add(charlieBucket);
            
            context.SaveChanges();
        }
    }
}