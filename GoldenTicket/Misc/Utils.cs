using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoldenTicket.DAL;
using GoldenTicket.Models;

namespace GoldenTicket.Misc
{
    public class Utils
    {
        private static GoldenTicketDbContext db = new GoldenTicketDbContext();

        public static List<Applicant> GetApplicants(IEnumerable<Selected> selecteds)
        {
            var applicants = new List<Applicant>();
            foreach (var s in selecteds)
            {
                applicants.Add(s.Applicant);
            }

            return applicants;
        }

        public static List<Applicant> GetApplicants(IEnumerable<Shuffled> shuffleds)
        {
            var applicants = new List<Applicant>();
            foreach (var s in shuffleds)
            {
                applicants.Add(s.Applicant);
            }

            return applicants;
        }

        public static List<Applicant> GetApplicants(IEnumerable<Waitlisted> waitlisteds)
        {
            var applicants = new List<Applicant>();
            foreach (var w in waitlisteds)
            {
                applicants.Add(w.Applicant);
            }

            return applicants;
        }

        public static List<Applicant> GetApplicants(IEnumerable<Applied> applieds)
        {
            var applicants = new List<Applicant>();
            foreach (var a in applieds)
            {
                applicants.Add(a.Applicant);
            }

            return applicants;
        }

        public static List<School> GetSchools(IEnumerable<Applied> applieds)
        {
            var schools = new List<School>();
            foreach(var a in applieds)
            {
                schools.Add(a.School);
            }

            return schools;
        }

        public static List<School> GetSchools(IEnumerable<Waitlisted> waitlisteds)
        {
            var schools = new List<School>();
            foreach (var a in waitlisteds)
            {
                schools.Add(a.School);
            }

            return schools;
        }

        public static string ApplicantsToCsv(IEnumerable<Applicant> applicants)
        {
            return ApplicantsToCsv(applicants, true);
        }

        public static string ApplicantsToCsv(IEnumerable<Applicant> applicants, bool printSchoolList)
        {
            var csvText = new StringBuilder();

            csvText.Append("Confirmation Code,");
            csvText.Append("Student First Name,");
            csvText.Append("Student Middle Name,");
            csvText.Append("Student Last Name,");
            csvText.Append("Student Birthday,");
            csvText.Append("Student Gender,");
            csvText.Append("Student Street Address 1,");
            csvText.Append("Student Street Address 2,");
            csvText.Append("Student City,");
            csvText.Append("Student ZIP Code,");

            csvText.Append("Contact 1 First Name,");
            csvText.Append("Contact 1 Last Name,");
            csvText.Append("Contact 1 Phone,");
            csvText.Append("Contact 1 Email,");
            csvText.Append("Contact 1 Relationship,");

            csvText.Append("Contact 2 First Name,");
            csvText.Append("Contact 2 Last Name,");
            csvText.Append("Contact 2 Phone,");
            csvText.Append("Contact 2 Email,");
            csvText.Append("Contact 2 Relationship,");

            csvText.Append("Household Members,");
            csvText.Append("Household Monthly Income,");

            if (printSchoolList)
            {
                csvText.Append("Schools Applied");
            }

            csvText.Append('\n');
                
            foreach (var a in applicants)
            {
                csvText.Append(a.ConfirmationCode); csvText.Append(',');
                csvText.Append(a.StudentFirstName); csvText.Append(',');
                csvText.Append(a.StudentMiddleName); csvText.Append(',');
                csvText.Append(a.StudentLastName); csvText.Append(',');
                csvText.Append(a.StudentBirthday.Value.ToString("MM/dd/yyyy")); csvText.Append(',');
                csvText.Append(a.StudentGender); csvText.Append(',');
                csvText.Append(a.StudentStreetAddress1); csvText.Append(',');
                csvText.Append(a.StudentStreetAddress2); csvText.Append(',');
                csvText.Append(a.StudentCity); csvText.Append(',');
                csvText.Append(a.StudentZipCode); csvText.Append(',');

                csvText.Append(a.Contact1FirstName); csvText.Append(',');
                csvText.Append(a.Contact1LastName); csvText.Append(',');
                csvText.Append(a.Contact1Phone); csvText.Append(',');
                csvText.Append(a.Contact1Email); csvText.Append(',');
                csvText.Append(a.Contact1Relationship); csvText.Append(',');

                csvText.Append(a.Contact2FirstName); csvText.Append(',');
                csvText.Append(a.Contact2LastName); csvText.Append(',');
                csvText.Append(a.Contact2Phone); csvText.Append(',');
                csvText.Append(a.Contact2Email); csvText.Append(',');
                csvText.Append(a.Contact2Relationship); csvText.Append(',');

                csvText.Append(a.HouseholdMembers); csvText.Append(',');
                csvText.Append(a.HouseholdMonthlyIncome); csvText.Append(',');

                if (printSchoolList)
                {
                    var applieds =
                        db.Applieds.Where(applied => applied.ApplicantID == a.ID)
                            .OrderBy(applied => applied.School.Name)
                            .ToList();

                    foreach (var applied in applieds)
                    {
                        csvText.Append(applied.School.Name);
                        csvText.Append(';');
                    }
                    csvText.Append(',');
                }

                csvText.Append('\n');
            }

            return csvText.ToString();
        }

        public static bool AreApplicantsEqual(Applicant a1, Applicant a2)
        {
            return a1.Checksum() == a2.Checksum();
        }

        public static List<Applicant> GetDuplicateApplicants(IEnumerable<Applicant> applicants)
        {
            var applicantsCopy = new List<Applicant>(applicants); // needed to prevent concurrent iteration of the same list during count
            var duplicates = new List<Applicant>();
            foreach (var a in applicants)
            {
                var checksum = a.Checksum();
                var count = applicantsCopy.Count(applicant => applicant.Checksum() == checksum);

                if (count > 1)
                {
                    duplicates.Add(a);
                }
            }

            return duplicates;            
        }

    }
}