using System.Linq;
using GoldenTicket.Calc;
using GoldenTicket.DAL;
using GoldenTicket.Models;
using System;
using System.Collections.Generic;
using GoldenTicket.Models;
using GoldenTicket.Lottery; //for List shuffle extensions

namespace GoldenTicket.Lottery
{
    public class SchoolLottery
    {
        private readonly double percentMale;
        private GoldenTicketDbContext db = new GoldenTicketDbContext();

        public SchoolLottery(double percentMale)
        {
            this.percentMale = percentMale;
        }

        public School Run(School school, List<Applicant> applicantList)
        {
            return Run(school, applicantList, true);
        }

        //TODO Optimize this later. There are a lot of repeated loops.
        public School Run(School school, List<Applicant> applicantList, bool shuffleApplicantList)
        {
            var shuffledApplicants = Utils.GetApplicants(db.Shuffleds.OrderBy(s=>s.Rank).ToList());
            var selectedApplicants = Utils.GetApplicants(db.Selecteds.OrderBy(s=>s.Rank).ToList());
            var waitlistedApplicants = Utils.GetApplicants(db.Waitlisteds.OrderBy(w=>w.Rank).ToList());

            // Counts
            int countMale = 0;
            int countFemale = 0;
            int countBelowPovertyLine = 0;
            int countAbovePovertyLine = 0;
            foreach(var applicant in selectedApplicants)
            {
                // Gender counts
                if(applicant.StudentGender == Gender.Male)
                {
                    countMale++;
                }
                else
                {
                    countFemale++;
                }

                // Poverty counts
                if(IncomeCalculator.IsBelowPovertyLine(applicant))
                {
                    countBelowPovertyLine++;
                }
                else
                {
                    countAbovePovertyLine++;
                }
            }
            
            // Initial calculations
            int numStudents = school.Seats;
            int numMale = (int)Math.Round(numStudents * percentMale);
            int numFemale = numStudents - numMale;
            int numBelowPovertyLine = (int)Math.Round(numStudents*school.PovertyRate);
            int numAbovePovertyLine = numStudents - numBelowPovertyLine;

            // Copy the list to preserve the passed in list
            List<Applicant> applicants = new List<Applicant>(applicantList);


            // Randomly sort the list
            if(shuffleApplicantList)
            {
                // Clear existing shuffled // TODO make a utility method for this
                ClearTable("Shuffleds");

                // Randomly shuffle the applicants
                applicants.Shuffle(new Random());

                // Record the shuffling
                shuffledApplicants.AddRange(applicants);

                // Preserve the shuffle order
                for (int i = 0; i < shuffledApplicants.Count; i++)
                {
                    var shuffled = new Shuffled
                    {
                        Applicant = shuffledApplicants[i],
                        School = school,
                        Rank = i
                    };

                    db.Shuffleds.Add(shuffled);
                }
                db.SaveChanges();

            }

            // Select low income students
            List<Applicant> lowIncomeApplicants = GetByPovertyStatus(applicants, true);
            foreach(Applicant a in lowIncomeApplicants)
            {
                // If the low income quota has been met, move on
                if(countBelowPovertyLine >= numBelowPovertyLine || selectedApplicants.Count >= numStudents)
                {
                    break;
                }

                // Add the student if the male/female ratio hasn't been violated
                if(a.StudentGender == Gender.Male && countMale < numMale)
                {
                    selectedApplicants.Add(a);
                    applicants.Remove(a);

                    countBelowPovertyLine++;
                    countMale++;
                }
                else if(a.StudentGender == Gender.Female && countFemale < numFemale)
                {
                    selectedApplicants.Add(a);
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
                    if (countBelowPovertyLine >= numBelowPovertyLine || selectedApplicants.Count >= numStudents)
                    {
                        break;
                    }

                    // Add the student
                    if (a.StudentGender == Gender.Male)
                    {
                        selectedApplicants.Add(a);
                        applicants.Remove(a);

                        countBelowPovertyLine++;
                        countMale++;
                    }
                    else if (a.StudentGender == Gender.Female)
                    {
                        selectedApplicants.Add(a);
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
                if (countAbovePovertyLine >= numAbovePovertyLine || selectedApplicants.Count >= numStudents)
                {
                    break;
                }

                // Add the student if the male/female ratio hasn't been violated
                if (a.StudentGender == Gender.Male && countMale < numMale)
                {
                    selectedApplicants.Add(a);
                    applicants.Remove(a);

                    countAbovePovertyLine++;
                    countMale++;
                }
                else if (a.StudentGender == Gender.Female && countFemale < numFemale)
                {
                    selectedApplicants.Add(a);
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
                    if (countAbovePovertyLine >= numAbovePovertyLine || selectedApplicants.Count >= numStudents)
                    {
                        break;
                    }

                    // Add the student
                    if (a.StudentGender == Gender.Male)
                    {
                        selectedApplicants.Add(a);
                        applicants.Remove(a);

                        countBelowPovertyLine++;
                        countMale++;
                    }
                    else if (a.StudentGender == Gender.Female)
                    {
                        selectedApplicants.Add(a);
                        applicants.Remove(a);

                        countBelowPovertyLine++;
                        countFemale++;
                    }

                }
            }

            // Are there still openings? (income agnostic, gender checked selection)
            foreach(Applicant a in new List<Applicant>(applicants)) // prevents modification during iteration
            {
                if(selectedApplicants.Count >= numStudents)
                {
                    break;
                }

                //TODO refactor -- this chunk of code is similar to other male/female selections
                // Add the student if the male/female ratio hasn't been violated
                if (a.StudentGender == Gender.Male && countMale < numMale)
                {
                    selectedApplicants.Add(a);
                    applicants.Remove(a);

                    countMale++;
                }
                else if (a.StudentGender == Gender.Female && countFemale < numFemale)
                {
                    selectedApplicants.Add(a);
                    applicants.Remove(a);

                    countFemale++;
                }
            }

            // Are there still openings? (income and gender agnostic selection)
            foreach (Applicant a in new List<Applicant>(applicants))
            {
                if (selectedApplicants.Count >= numStudents)
                {
                    break;
                }

                selectedApplicants.Add(a);
                applicants.Remove(a);
            }

            // Wait list the rest
            waitlistedApplicants.Clear();
            waitlistedApplicants.AddRange(applicants);

            // Preserve the ranks
            ClearTable("Selecteds");
            ClearTable("Waitlisteds");
            
            for (int i = 0; i < selectedApplicants.Count; i++)
            {
                var selected = new Selected
                {
                    Applicant = selectedApplicants[i],
                    School = school,
                    Rank = i
                };
                db.Selecteds.Add(selected);
            }

            for (int i = 0; i < waitlistedApplicants.Count; i++)
            {
                var waitlisted = new Selected
                {
                    Applicant = waitlistedApplicants[i],
                    School = school,
                    Rank = i
                };
                db.Selecteds.Add(waitlisted);
            }

            db.SaveChanges();


            return school;
        }

        private List<Applicant> GetByPovertyStatus(List<Applicant> applicants, bool isBelowPovertyLine)
        {
            List<Applicant> filteredApplicants = new List<Applicant>();
            foreach(Applicant a in applicants)
            {
                if(IncomeCalculator.IsBelowPovertyLine(a) == isBelowPovertyLine)
                {
                    filteredApplicants.Add(a);
                }
            }

            return filteredApplicants;
        }

        private void ClearTable(string tableName)
        {
            var objCtx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db).ObjectContext;
            objCtx.ExecuteStoreCommand("TRUNCATE TABLE [" + tableName + "]");
        }
    }
}
