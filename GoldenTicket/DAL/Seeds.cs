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
            Program willysSchool = new Program { Name = "Willy's School", StreetAddress1 = "1000 Chocolate Factory Rd", City = "Providence", State = "RI", ZipCode = "02904", Email = "willy@wonka.org", Phone = "401-000-0000", Seats = 18, PovertyRate = 0.87 };
            context.Programs.Add(willysSchool);

            Program arthurAcademy = new Program { Name = "Arthur Academy", StreetAddress1 = "999 Gobstopper Blvd", City = "Providence", State = "RI", ZipCode = "02904", Email = "art@slugworth.com", Phone = "401-555-1234", Seats = 36, PovertyRate = 0.87 };
            context.Programs.Add(arthurAcademy);

            // Selected student with 3 contacts
            Student charlieBucket = new Student { FirstName = "Charlie", MiddleName = "C", LastName = "Bucket", City = "Providence", State = "RI", ZipCode="02903", Gender = Gender.Male, HouseholdMembers=4, HouseholdMonthlyIncome=30000 };
            
            Contact charlieBucketMom = new Contact { FirstName = "Mrs", LastName = "Bucket", Phone = "401-111-1111", Email = "mrsbucket@choco.org", Relationship = "Mom", Student = charlieBucket };
            Contact charlieBucketDad = new Contact { FirstName = "Mister", LastName = "Bucket", Phone = "401-222-2222", Email = "mrbucket@choco.org", Relationship = "Dad" };
            Contact charlieBucketGrandpa = new Contact { FirstName = "Joe", LastName = "Bucket", Phone = "401-333-3333", Email = "grandpajoe@choco.org", Relationship = "Grandpa" };
            charlieBucket.Contacts.Add(charlieBucketMom);
            charlieBucket.Contacts.Add(charlieBucketDad);
            charlieBucket.Contacts.Add(charlieBucketGrandpa);

            Applied charlieBucketApplied = new Applied();
            charlieBucketApplied.Student = charlieBucket;
            charlieBucketApplied.Program = willysSchool;
            willysSchool.Applieds.Add(charlieBucketApplied);
            context.Applieds.Add(charlieBucketApplied);
            
            context.Students.Add(charlieBucket);
            
            
            context.SaveChanges();
        }
    }
}