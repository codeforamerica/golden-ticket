using CsvHelper;
using GoldenTicket.Calc;
using GoldenTicket.Domain;
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
        string csvFilePath;
        Dictionary<string,School> schools;
        IncomeCalculator incomeCalc;

        public ApplicantCsvReader(string csvFilePath, List<School> schoolList, IncomeCalculator incomeCalc)
        {
            this.csvFilePath = csvFilePath;
            this.incomeCalc = incomeCalc;

            schools = new Dictionary<string,School>();
            schoolList.ForEach(s => schools[s.Name] = s);
        }

        public List<School> ReadApplicants()
        {
            // Clear the schools
            foreach(string key in schools.Keys)
            {
                schools[key].ClearApplicants();
            }

            // Read the CSV
            using (StreamReader textReader = new StreamReader(csvFilePath))
            {
                CsvReader csvReader = new CsvReader(textReader);
                csvReader.Configuration.SkipEmptyRecords = true;
                csvReader.Configuration.TrimFields = true;

                while (csvReader.Read())
                {
                    Applicant applicant = ParseApplicant(csvReader);

                    // Add student to each school
                    string schoolNames = csvReader.GetField<string>("Schools");
                    foreach (string rawSchoolName in schoolNames.Split(','))
                    {
                        string schoolName = EscapeSchoolName(rawSchoolName.Trim());
                        schools[schoolName].Applicants.Add(applicant);
                    }
                }
            }

            return schools.Values.ToList();
        }

        private Applicant ParseApplicant(CsvReader csvReader)
        {
            Applicant a = new Applicant();

            a.StudentFirstName = csvReader.GetField<string>("Student First Name");
            a.StudentMiddleName = csvReader.GetField<string>("Student Middle Name");
            a.StudentLastName = csvReader.GetField<string>("Student Last Name");
            a.StudentBirthday = Convert.ToDateTime(csvReader.GetField<string>("Student Birthday"));
            a.StudentGender = ParseGender(csvReader.GetField<string>("Student Gender"));

            a.GuardianFirstName = csvReader.GetField<string>("Guardian First Name");
            a.GuardianLastName = csvReader.GetField<string>("Guardian Last Name");
            a.GuardianPhoneNumber = csvReader.GetField<string>("Guardian Phone Number");
            a.GuardianEmailAddress = csvReader.GetField<string>("Guardian E-mail Address");
            a.GuardianRelationshipToStudent = csvReader.GetField<string>("Guardian Relationship to Student");

            a.StreetAddress = csvReader.GetField<string>("Street Address");
            a.ZipCode = csvReader.GetField<string>("Zip Code");
            a.District = csvReader.GetField<string>("District of Residency");
            a.NumHouseholdMembers = Math.Abs(int.Parse(csvReader.GetField<string>("Household Members")));

            // Income calculation (below or above poverty line?)
            int incomeAmount = ParseIncome(csvReader.GetField<string>("Annual Income"));
            if(a.NumHouseholdMembers > 10 && incomeAmount >= 89190) //TODO change statically defined number
            {
                string writeInIncomeAmountStr = csvReader.GetField<string>("Household Income Amount").Replace("$","").Replace(",","");

                if(!string.IsNullOrWhiteSpace(writeInIncomeAmountStr))
                {
                    incomeAmount = int.Parse(writeInIncomeAmountStr);
                }
                else
                {
                    incomeAmount = 1000000; // assume family is above poverty line if income was not entered
                }
            }
            a.IsBelowPovertyLevel = incomeCalc.IsBelowPovertyLine(a.NumHouseholdMembers, incomeAmount);

            return a;
        }

        private static int ParseIncome(string incomeRange)
        {
            int incomeAmount = 0;
            
            //TODO change statically defined numbers
            switch(incomeRange)
            {
                case "$29,101 or below":
                    incomeAmount = 29101;
                    break;
                case "$29,102 - $36,612":
                    incomeAmount = 29102;
                    break;
                case "$36,613 - $44,123":
                    incomeAmount = 36613;
                    break;
                case "$44,124 - $51,634":
                    incomeAmount = 44124;
                    break;
                case "$51,635 - $59,145":
                    incomeAmount = 51635;
                    break;
                case "$59,146 - $66,656":
                    incomeAmount = 59146;
                    break;
                case "$66,657 - $74,167":
                    incomeAmount = 66657;
                    break;
                case "$74,168 - $81,678":
                    incomeAmount = 74168;
                    break;
                case "$81,679 - $89,189":
                    incomeAmount = 81679;
                    break;
                case "$89,190 or above":
                    incomeAmount = 89190;
                    break;
            }

            return incomeAmount;
        }

        private static string EscapeSchoolName(string schoolName)
        {
            return schoolName.Replace("@", "at").Replace('/', '-');
        }

        private static Applicant.Gender ParseGender(string genderStr)
        {
            if (genderStr.Equals("MALE", StringComparison.InvariantCultureIgnoreCase))
            {
                return Applicant.Gender.MALE;
            }

            return Applicant.Gender.FEMALE;
        }
    }
}
