using System.Web.WebPages;
using CsvHelper;
using GoldenTicket.DAL;
using GoldenTicket.Models;
using GoldenTicket.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldenTicket.Csv
{
    public class ApplicantCsvReader : ApplicantReader
    {
        private readonly string csvFilePath;
        private readonly Dictionary<string,School> schools = new Dictionary<string, School>();

        private readonly GoldenTicketDbContext db = new GoldenTicketDbContext();

        public ApplicantCsvReader(string csvFilePath, List<School> schoolList)
        {
            this.csvFilePath = csvFilePath;

            schoolList.ForEach(s => schools[s.Name] = s);
        }

        public List<School> ReadApplicants()
        {
            // Read the CSV
            using (var textReader = new StreamReader(csvFilePath))
            {
                var csvReader = new CsvReader(textReader);
                csvReader.Configuration.SkipEmptyRecords = true;
                csvReader.Configuration.TrimFields = true;

                var applicants = new List<Applicant>();
                var applieds = new List<Applied>();

                while (csvReader.Read())
                {
                    var applicant = ParseApplicant(csvReader);
                    applicants.Add(applicant);

                    // Add student to each school
                    var schoolNames = csvReader.GetField<string>("Schools Applied").Split(';');
                    foreach (var rawSchoolName in schoolNames)
                    {
                        var schoolName = rawSchoolName.Trim();
                        if (schoolName.IsEmpty())
                        {
                            continue;
                        }

                        var school = db.Schools.First(s => s.Name.Equals(schoolName, StringComparison.CurrentCultureIgnoreCase));
                        
                        var applied = new Applied {Applicant = applicant, School = school};

                        applieds.Add(applied);
                    }
                }

                db.Applicants.AddRange(applicants);
                db.Applieds.AddRange(applieds);
                db.SaveChanges();
            }

            return schools.Values.ToList();
        }

        private Applicant ParseApplicant(CsvReader csvReader)
        {
            var a = new Applicant
            {
                ConfirmationCode = ValueOrNull(csvReader.GetField<string>("Confirmation Code")),

                StudentFirstName = csvReader.GetField<string>("Student First Name"),
                StudentMiddleName = csvReader.GetField<string>("Student Middle Name"),
                StudentLastName = csvReader.GetField<string>("Student Last Name"),
                StudentBirthday = Convert.ToDateTime(csvReader.GetField<string>("Student Birthday")),
                StudentGender = ParseGender(csvReader.GetField<string>("Student Gender")),

                StudentStreetAddress1 = csvReader.GetField<string>("Student Street Address 1"),
                StudentStreetAddress2 = csvReader.GetField<string>("Student Street Address 2"),
                StudentCity = csvReader.GetField<string>("Student City"),
                StudentZipCode = csvReader.GetField<string>("Student ZIP Code"),

                Contact1FirstName = csvReader.GetField<string>("Contact 1 First Name"),
                Contact1LastName = csvReader.GetField<string>("Contact 1 Last Name"),
                Contact1Phone = csvReader.GetField<string>("Contact 1 Phone"),
                Contact1Email = csvReader.GetField<string>("Contact 1 Email"),
                Contact1Relationship = csvReader.GetField<string>("Contact 1 Relationship"),

                Contact2FirstName = ValueOrNull(csvReader.GetField<string>("Contact 2 First Name")),
                Contact2LastName = ValueOrNull(csvReader.GetField<string>("Contact 2 Last Name")),
                Contact2Phone = ValueOrNull(csvReader.GetField<string>("Contact 2 Phone")),
                Contact2Email = ValueOrNull(csvReader.GetField<string>("Contact 2 Email")),
                Contact2Relationship = ValueOrNull(csvReader.GetField<string>("Contact 2 Relationship")),

                HouseholdMembers = csvReader.GetField<int>("Household Members"),
                HouseholdMonthlyIncome = csvReader.GetField<int>("Household Monthly Income")
            };

            return a;
        }

        private static Gender ParseGender(string genderStr)
        {
            if (genderStr.Equals("MALE", StringComparison.InvariantCultureIgnoreCase))
            {
                return Gender.Male;
            }

            return Gender.Female;
        }

        private static string ValueOrNull(string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}
