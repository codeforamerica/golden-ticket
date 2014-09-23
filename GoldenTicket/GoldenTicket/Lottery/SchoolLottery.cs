using GoldenTicket.Domain;
using System;
using System.Collections.Generic;
using GoldenTicket.Lottery; //for List shuffle extensions

namespace GoldenTicket.Lottery
{
    public class SchoolLottery
    {
        int studentsPerClassroom;
        double percentMale;
        DateTime age4ByDate;

        public SchoolLottery(int studentsPerClassroom, double percentMale, DateTime age4ByDate)
        {
            this.studentsPerClassroom = studentsPerClassroom;
            this.percentMale = percentMale;
            this.age4ByDate = age4ByDate;
        }

        public School Run(School school)
        {
            return Run(school, school.Applicants, true, true);
        }

        public School Run(School school, List<Applicant> applicantList)
        {
            return Run(school, applicantList, true, true);
        }

        //TODO Optimize this later. There are a lot of repeated loops.
        public School Run(School school, List<Applicant> applicantList, bool shuffleApplicantList, bool filterApplicantList)
        {
            // Counts
            int countMale = 0;
            int countFemale = 0;
            int countBelowPovertyLine = 0;
            int countAbovePovertyLine = 0;
            foreach(Applicant a in school.SelectedApplicants)
            {
                // Gender counts
                if(a.StudentGender == Applicant.Gender.MALE)
                {
                    countMale++;
                }
                else
                {
                    countFemale++;
                }

                // Poverty counts
                if(a.IsBelowPovertyLevel)
                {
                    countBelowPovertyLine++;
                }
                else
                {
                    countAbovePovertyLine++;
                }
            }
            
            // Initial calculations
            int numStudents = school.NumClassrooms * studentsPerClassroom;
            int numMale = (int)Math.Round(numStudents * percentMale);
            int numFemale = numStudents - numMale;
            int numBelowPovertyLine = (int)Math.Round(numStudents*school.PercentBelowPovertyLine);
            int numAbovePovertyLine = numStudents - numBelowPovertyLine;

            // Copy the list to preserve the passed in list
            List<Applicant> applicants = new List<Applicant>(applicantList);

            // Filter applicants
            if(filterApplicantList)
            {
                // Remove duplicates
                applicants = FilterDuplicates(applicants);

                // Remove those that don't live in the distrct
                applicants = FilterByDistrict(applicants,school.District);

                // Remove applicants who are too old or young
                applicants = FilterByAge(applicants);

                // Record the filtering
                school.FilteredApplicants.Clear();
                school.FilteredApplicants.AddRange(applicants);

            }

            // Randomly sort the list
            if(shuffleApplicantList)
            {
                // Randomly shuffle the applicants
                applicants.Shuffle(new Random());

                // Record the shuffling
                school.ShuffledApplicants.Clear();
                school.ShuffledApplicants.AddRange(applicants);
            }

            // Select low income students
            List<Applicant> lowIncomeApplicants = GetByPovertyStatus(applicants, true);
            foreach(Applicant a in lowIncomeApplicants)
            {
                // If the low income quota has been met, move on
                if(countBelowPovertyLine >= numBelowPovertyLine || school.SelectedApplicants.Count >= numStudents)
                {
                    break;
                }

                // Add the student if the male/female ratio hasn't been violated
                if(a.StudentGender == Applicant.Gender.MALE && countMale < numMale)
                {
                    school.SelectedApplicants.Add(a);
                    applicants.Remove(a);

                    countBelowPovertyLine++;
                    countMale++;
                }
                else if(a.StudentGender == Applicant.Gender.FEMALE && countFemale < numFemale)
                {
                    school.SelectedApplicants.Add(a);
                    applicants.Remove(a);

                    countBelowPovertyLine++;
                    countFemale++;
                }
            }

            // Do a second pass on the below poverty line students in case gender balance prevented it from getting fulfilled
            // RIDE: Income balance takes priority over male to female ratio
            // Gender agnostic, income checked pass
            if (countBelowPovertyLine < numBelowPovertyLine)
            {
                lowIncomeApplicants = GetByPovertyStatus(applicants, true);
                foreach (Applicant a in lowIncomeApplicants)
                {
                    // If the low income quota has been met, move on
                    if (countBelowPovertyLine >= numBelowPovertyLine || school.SelectedApplicants.Count >= numStudents)
                    {
                        break;
                    }

                    // Add the student
                    if (a.StudentGender == Applicant.Gender.MALE)
                    {
                        school.SelectedApplicants.Add(a);
                        applicants.Remove(a);

                        countBelowPovertyLine++;
                        countMale++;
                    }
                    else if (a.StudentGender == Applicant.Gender.FEMALE)
                    {
                        school.SelectedApplicants.Add(a);
                        applicants.Remove(a);

                        countBelowPovertyLine++;
                        countFemale++;
                    }

                }
            }

            // Select higher income students
            // TODO refactor -- almost the same as the above loop
            List<Applicant> higherIncomeApplicants = GetByPovertyStatus(applicants, false);
            foreach (Applicant a in higherIncomeApplicants)
            {
                // If the higher income quota has been met, move on
                if (countAbovePovertyLine >= numAbovePovertyLine || school.SelectedApplicants.Count >= numStudents)
                {
                    break;
                }

                // Add the student if the male/female ratio hasn't been violated
                if (a.StudentGender == Applicant.Gender.MALE && countMale < numMale)
                {
                    school.SelectedApplicants.Add(a);
                    applicants.Remove(a);

                    countAbovePovertyLine++;
                    countMale++;
                }
                else if (a.StudentGender == Applicant.Gender.FEMALE && countFemale < numFemale)
                {
                    school.SelectedApplicants.Add(a);
                    applicants.Remove(a);

                    countAbovePovertyLine++;
                    countFemale++;
                }
            }

            // Do a second pass on the above poverty line students in case gender balance prevented it from getting fulfilled
            // RIDE: Income balance takes priority over male to female ratio
            // Gender agnostic, income checked pass
            if (countAbovePovertyLine < numAbovePovertyLine)
            {
                higherIncomeApplicants = GetByPovertyStatus(applicants, false);
                foreach (Applicant a in higherIncomeApplicants)
                {
                    // If the low income quota has been met, move on
                    if (countAbovePovertyLine >= numAbovePovertyLine || school.SelectedApplicants.Count >= numStudents)
                    {
                        break;
                    }

                    // Add the student
                    if (a.StudentGender == Applicant.Gender.MALE)
                    {
                        school.SelectedApplicants.Add(a);
                        applicants.Remove(a);

                        countBelowPovertyLine++;
                        countMale++;
                    }
                    else if (a.StudentGender == Applicant.Gender.FEMALE)
                    {
                        school.SelectedApplicants.Add(a);
                        applicants.Remove(a);

                        countBelowPovertyLine++;
                        countFemale++;
                    }

                }
            }

            // Are there still openings? (income agnostic, gender checked selection)
            foreach(Applicant a in new List<Applicant>(applicants)) // prevents modification during iteration
            {
                if(school.SelectedApplicants.Count >= numStudents)
                {
                    break;
                }

                //TODO refactor -- this chunk of code is similar to other male/female selections
                // Add the student if the male/female ratio hasn't been violated
                if (a.StudentGender == Applicant.Gender.MALE && countMale < numMale)
                {
                    school.SelectedApplicants.Add(a);
                    applicants.Remove(a);

                    countMale++;
                }
                else if (a.StudentGender == Applicant.Gender.FEMALE && countFemale < numFemale)
                {
                    school.SelectedApplicants.Add(a);
                    applicants.Remove(a);

                    countFemale++;
                }
            }

            // Are there still openings? (income and gender agnostic selection)
            foreach (Applicant a in new List<Applicant>(applicants))
            {
                if (school.SelectedApplicants.Count >= numStudents)
                {
                    break;
                }

                school.SelectedApplicants.Add(a);
                applicants.Remove(a);
            }

            // Wait list the rest
            school.WaitlistedApplicants.Clear();
            school.WaitlistedApplicants.AddRange(applicants);

            return school;
        }

        private List<Applicant> FilterDuplicates(List<Applicant> applicants)
        {
            List<Applicant> dedupedApplicants = new List<Applicant>();
            HashSet<string> applicantCodes = new HashSet<string>();

            foreach(Applicant a in applicants)
            {
                string checksum = a.Checksum();
                if(!applicantCodes.Contains(checksum))
                {
                    dedupedApplicants.Add(a);
                    applicantCodes.Add(checksum);
                }
            }

            return dedupedApplicants;
        }

        private static List<Applicant> FilterByDistrict(List<Applicant> applicants, string district)
        {
            List<Applicant> filteredApplicants = new List<Applicant>();

            foreach (Applicant a in applicants)
            {
                if(district.Equals(a.District))
                {
                    filteredApplicants.Add(a);
                }
            }

            return filteredApplicants;
        }

        private List<Applicant> FilterByAge(List<Applicant> applicants)
        {
            List<Applicant> filteredApplicants = new List<Applicant>();

            foreach (Applicant a in applicants)
            {
                int ageByCutoff = age4ByDate.Year - a.StudentBirthday.Year;
                DateTime adjustedDate = age4ByDate.AddYears(-ageByCutoff);
                if(a.StudentBirthday > adjustedDate)
                {
                    ageByCutoff--;
                }

                if (ageByCutoff == 4)
                {
                    filteredApplicants.Add(a);
                }
            }

            return filteredApplicants;
        }

        private List<Applicant> GetByPovertyStatus(List<Applicant> applicants, bool isBelowPovertyLine)
        {
            List<Applicant> filteredApplicants = new List<Applicant>();
            foreach(Applicant a in applicants)
            {
                if(a.IsBelowPovertyLevel == isBelowPovertyLine)
                {
                    filteredApplicants.Add(a);
                }
            }

            return filteredApplicants;
        }
    }
}
