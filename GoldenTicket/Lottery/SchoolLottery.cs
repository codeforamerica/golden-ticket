using System.Linq;
using GoldenTicket.DAL;
using GoldenTicket.Misc;
using GoldenTicket.Models;
using System;
using System.Collections.Generic;
using GoldenTicket.Lottery; //for List shuffle extensions

namespace GoldenTicket.Lottery
{
    /**
     * <summary>
     * Lottery selection algorithm.
     * 
     * <para>
     * The lottery randomly selects applicants for schools, with only weights
     * being income level (above/below poverty line) and gender. While each school may
     * have a different income and gender requirement, income is always considered a more
     * important factor.
     * </para>
     * 
     * </summary>
     */
    public class SchoolLottery
    {
        // Database connection
        private GoldenTicketDbContext db;

        // Calculates whether an applicant is below/above
        private IncomeCalculator incomeCalculator;

        /**
         * <summary>Creates a new lottery algorithm object</summary>
         * <param name="db">Database connection</param>
         */
        public SchoolLottery(GoldenTicketDbContext db)
        {
            this.db = db;
            this.incomeCalculator = new IncomeCalculator(db);
        }

        /**
         * <summary>
         * Runs the lottery algorithm for an individual school.
         * The school's entire applicant list is used for the run.
         * Will shuffle the applicant list during the run.
         * </summary>
         * 
         * <param name="school">School to run the algorithm for</param>
         * <returns>The school passed in</returns>
         */
        public School Run(School school)
        {
            var applicants = Utils.GetApplicants(db.Applieds.Where(a => a.Applicant.ConfirmationCode != null));
            return Run(school, applicants, true);
        }

        /**
         * <summary>
         * Runs the lottery algorithm for an individual school.
         * A specific list of applicants is used for the lottery run.
         * Will shuffle the applicants during the running.
         * </summary>
         * 
         * <param name="school">School to run the algorithm for</param>
         * <param name="applicantList">Applicants to use for selection in the lottery run</param>
         * <returns>The school passed in</returns>
         */
        public School Run(School school, List<Applicant> applicantList)
        {
            return Run(school, applicantList, true);
        }

        /**
         * <summary>
         * Runs the lottery algorithm for an individual school.
         * A specific list of applicants is used for the lottery run.
         * </summary>
         * 
         * <remarks>TODO Optimize this later. There are a lot of repeated loops.</remarks>
         * 
         * <param name="school">School to run the algorithm for</param>
         * <param name="applicantList">Applicants to use for selection in the lottery run</param>
         * <returns>The school passed in</returns>
         */
        public School Run(School school, List<Applicant> applicantList, bool shuffleApplicantList)
        {
            // Order the selected and waitlists by rank
            var selecteds = school.Selecteds.OrderBy(s => s.Rank).ToList();
            var selectedApplicants = Utils.GetApplicants(selecteds);

            var waitlisteds = school.Waitlisteds.OrderBy(w => w.Rank).ToList();
            var waitlistedApplicants = Utils.GetApplicants(waitlisteds);

            // Clear selected and waitlisteds
            db.Selecteds.RemoveRange(selecteds);
            db.Waitlisteds.RemoveRange(waitlisteds);

            // Counts the existing numbers of selected applicants for the school
            var countMale = 0;
            var countFemale = 0;
            var countBelowPovertyLine = 0;
            var countAbovePovertyLine = 0;
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
                if(incomeCalculator.IsBelowPovertyLine(applicant))
                {
                    countBelowPovertyLine++;
                }
                else
                {
                    countAbovePovertyLine++;
                }
            }
            
            // Initial calculations
            var numStudents = school.Seats;
            var numMale = (int)Math.Round(numStudents * school.GenderBalance);
            var numFemale = numStudents - numMale;
            var numBelowPovertyLine = (int)Math.Round(numStudents*school.PovertyRate);
            var numAbovePovertyLine = numStudents - numBelowPovertyLine;

            // Copy the list to preserve the passed in list
            var applicants = new List<Applicant>(applicantList);


            // Randomly sort the list
            if(shuffleApplicantList)
            {
                // Clear existing shuffled (there really shouldn't be any) // TODO make a utility method for this
                var existingShuffleds = db.Shuffleds.Where(s => s.SchoolID == school.ID).ToList();
                db.Shuffleds.RemoveRange(existingShuffleds);

                // Randomly shuffle the applicants
                applicants.Shuffle(new Random());

                // Record the shuffling
                var shuffledApplicants = new List<Applicant>(applicants);

                // Preserve the shuffle order
                var shuffleds = new List<Shuffled>(shuffledApplicants.Count);
                for (var i = 0; i < shuffledApplicants.Count; i++)
                {
                    var shuffled = new Shuffled
                    {
                        Applicant = shuffledApplicants[i],
                        School = school,
                        Rank = i
                    };
                    shuffleds.Add(shuffled);
                }
                db.Shuffleds.AddRange(shuffleds);
                db.SaveChanges();
            }

            // Select low income students
            var lowIncomeApplicants = GetByPovertyStatus(applicants, true);
            foreach(var a in lowIncomeApplicants)
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
                foreach (var a in lowIncomeApplicants)
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
            var higherIncomeApplicants = GetByPovertyStatus(applicants, false);
            foreach (var a in higherIncomeApplicants)
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
                foreach (var a in higherIncomeApplicants)
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
            foreach(var a in new List<Applicant>(applicants)) // prevents modification during iteration
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
            foreach (var a in new List<Applicant>(applicants))
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
            var newSelecteds = new List<Selected>(selectedApplicants.Count);
            for (var i = 0; i < selectedApplicants.Count; i++)
            {
                var selected = new Selected
                {
                    Applicant = selectedApplicants[i],
                    School = school,
                    Rank = i
                };
                newSelecteds.Add(selected);
            }
            db.Selecteds.AddRange(newSelecteds);

            var newWaitlisteds = new List<Waitlisted>(waitlistedApplicants.Count);
            for (var i = 0; i < waitlistedApplicants.Count; i++)
            {
                var waitlisted = new Waitlisted
                {
                    Applicant = waitlistedApplicants[i],
                    School = school,
                    Rank = i
                };
                newWaitlisteds.Add(waitlisted);
            }
            db.Waitlisteds.AddRange(newWaitlisteds);


            db.SaveChanges();


            return school;
        }

        /**
         * <summary>Get all applicants by a specific poverty status</summary>
         * <param name="applicants">Applicants to return for poverty level</param>
         * <param name="isBelowPovertyLine">True if returned students should be below the poverty line, false otherwise</param>
         * <returns>List of applicants</returns>
         */
        private List<Applicant> GetByPovertyStatus(IEnumerable<Applicant> applicants, bool isBelowPovertyLine)
        {
            var filteredApplicants = new List<Applicant>();
            foreach(var a in applicants)
            {
                if(incomeCalculator.IsBelowPovertyLine(a) == isBelowPovertyLine)
                {
                    filteredApplicants.Add(a);
                }
            }

            return filteredApplicants;
        }
    }
}
