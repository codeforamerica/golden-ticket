using System.Web.WebPages;
using CsvHelper;
using GoldenTicket.DAL;
using GoldenTicket.Models;
using GoldenTicket.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GoldenTicket.Csv
{
    /**
     * <summary>
     * Imports an all applicants CSV file (as exported via the app's ViewApplicants for all schools view)
     * into the system's database. Before importing applicants, all the schools must be setup in the system
     * with the exact names used in the all applicants CSV file.
     * </summary>
     */
    public class ApplicantCsvReader : ApplicantReader
    {
        // File system path to the CSV file to import
        private readonly string csvFilePath;

        // Dictionary of school names to School objects
        private readonly Dictionary<string,School> schools = new Dictionary<string, School>();

        // Database connection
        private readonly GoldenTicketDbContext db = new GoldenTicketDbContext();

        /**
         * <summary>Setup a new ApplicantCsvReader object</summary>
         * 
         * <param name="csvFilePath">File system path to an all applicants CSV file</param>
         * <param name="schoolList">List of schools that the applicants might belong to</param>
         */
        public ApplicantCsvReader(string csvFilePath, List<School> schoolList)
        {
            this.csvFilePath = csvFilePath;

            schoolList.ForEach(s => schools[s.Name] = s);
        }

        /**
         * <summary>Import all the applicants from the CSV file into the system.</summary>
         */
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
        
        /**
         * <summary>
         * Parses a row of the CSV file.
         * </summary>
         * 
         * <param name="csvReader">CSV file input stream</param>
         * <returns>A parsed Applicant form the row of data</returns>
         */
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

        /**
         * <summary>Converts gender text to the Gender enum value</summary>
         * <returns>A gender based on the text</returns>
         */
        private static Gender ParseGender(string genderStr)
        {
            if (genderStr.Equals("MALE", StringComparison.InvariantCultureIgnoreCase))
            {
                return Gender.Male;
            }

            return Gender.Female;
        }

        /**
         * <summary>
         * Gets the value of a string object if it's not null. Otherwise, it'll return null.
         * This prevents null pointer exceptions when reading optional fields.
         * </summary>
         * 
         * <param name="value">A string value</param>
         */
        private static string ValueOrNull(string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}
