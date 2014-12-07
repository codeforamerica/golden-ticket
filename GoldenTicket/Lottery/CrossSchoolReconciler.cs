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
     * Prevents applicants from being selected at more than one school. For applicants selected
     * at more than one school, they are removed from the school in which they had the lower rank (see
     * Selected object). Selected students are also removed from all schools they were waitlisted for.
     * </summary>
     */
    public class CrossSchoolReconciler
    {
        // Database connection
        private GoldenTicketDbContext db = new GoldenTicketDbContext();
        
        // School lottery algorithm
        private SchoolLottery schoolLottery;

        /**
         * <summary>
         * Creates a new reconciliation algorithm object.
         * </summary>
         * 
         * <param name="db">Database connection</param>
         */
        public CrossSchoolReconciler(GoldenTicketDbContext db)
        {
            this.db = db;
            this.schoolLottery = new SchoolLottery(db);
        }

        /**
         * <summary>
         * Prevent applicants from being selected more than once. Selected applicants will remain
         * in the school that they had the best rank (i.e. lowest number) and will be removed from other
         * selected and waitlisted schools.
         * </summary>
         * 
         * <remarks>
         * There's a lot that can be done to optimize this method. Currently takes too long to run (still
         * faster than the old paper method, but in computer time, it's still slow). Database roundtrips
         * can be minimized, as well as repeat functions eliminated.
         * </remarks>
         */
        public void Reconcile()
        {
            var schools = db.Schools.ToList();

            // Remove selected students from all school waitlists
            RemoveSelectedFromWaitlists(schools);

            // Reconcile students selected for multiple schools
            // i.e. Leave the student in the school in which they are highest on the selected list
            var reconciledApplicants = new List<Applicant>();
            var remainingSchools = new List<School>(schools);
            foreach(var s in schools)
            {
                schoolLoop: { }
                var selectedApplicants = Utils.GetApplicants(s.Selecteds.OrderBy(selected=>selected.Rank).ToList());
                foreach(var a in selectedApplicants)
                {
                    // Skip applicant if already reconciled - done to optimize in the event the schoolLoop is reset (see bottom of loop)
                    if(reconciledApplicants.Contains(a))
                    {
                        continue;
                    }

                    // Reconcile the applicant between schools he/she was selected 
                    var selectedSchools = GetSchoolsApplicantWasSelectedAt(a, remainingSchools);
                    var currentSchoolEffected = ReconcileApplicant(a, s, selectedSchools);

                    RemoveSelectedFromWaitlists(schools, selectedApplicants); //TODO This step is very inefficient. Optimize later.

                    // Adjust the control structures
                    reconciledApplicants.Add(a);

                    // Reset the loop if the current school's selected list has been updated/impacted 
                    // (i.e. the current applicant has been removed and someone new has been added to the selected list)
                    if (currentSchoolEffected)
                    {
                        goto schoolLoop; //TODO Is there a better way than a goto? Optimize later.
                    }
                }
                remainingSchools.Remove(s);
            }
        }

        /**
         * <summary>Gets a list of applicants that have been selected at the indicated schools</summary>
         * <param name="schools">The schools to get selected applicants for</param>
         * <returns>Selected applicants</returns>
         */
        private List<Applicant> GetAllSelectedApplicants(IEnumerable<School> schools)
        {
            var selectedApplicants = new List<Applicant>();
            foreach(var s in schools)
            {
                var currentSelectedApplicants =
                    Utils.GetApplicants(s.Selecteds.OrderBy(selected => selected.Rank).ToList());
                selectedApplicants.AddRange(currentSelectedApplicants);
            }
            return selectedApplicants;
        }

        /**
         * <summary>Global removal of all selected applicants from specified schools' waitlists</summary>
         * <param name="schools">Schools to remove selected applicants from</param>
         * <remarks>TODO rename this</remarks>
         */
        private void RemoveSelectedFromWaitlists(IEnumerable<School> schools)
        {
            RemoveSelectedFromWaitlists(schools, GetAllSelectedApplicants(schools));
        }

        /**
         * <summary>Removal of specified selected applicants from specified schools' waitlists</summary>
         * <param name="schools">Schools to remove selected applicants from</param>
         * <param name="applicants">Applicants to remove from waitlists</param>
         * <remarks>TODO rename this</remarks>
         */
        private void RemoveSelectedFromWaitlists(IEnumerable<School> schools, IEnumerable<Applicant> applicants)
        {
            var removeWaitlisteds = new List<Waitlisted>();
            foreach (var a in applicants)
            {
                foreach (var s in schools)
                {
                    var waitlisted = s.Waitlisteds.FirstOrDefault( w=> w.ApplicantID == a.ID);
                    if (waitlisted != null)
                    {
                        removeWaitlisteds.Add(waitlisted);    
                    }
                }
            }

            db.Waitlisteds.RemoveRange(removeWaitlisteds);
            db.SaveChanges();
        }

        /**
         * <summary>
         * Identify the schools (from the list passed in) that an applicant was selected at
         * </summary>
         * 
         * <param name="applicant">Applicant</param>
         * <param name="schools">Schools to see if the applicant was selected at</param>
         * <returns>List of schools that the applicant was selected at</returns>
         */
        private List<School> GetSchoolsApplicantWasSelectedAt(Applicant applicant, IEnumerable<School> schools)
        {
            var selectedSchools = new List<School>();
            
            foreach(var s in schools)
            {
                var selectedApplicants =
                    Utils.GetApplicants(db.Selecteds.Where(selected => selected.SchoolID == s.ID).OrderBy(selected => selected.Rank).ToList());
                if(selectedApplicants.Contains(applicant))
                {
                    selectedSchools.Add(s);
                }
            }

            return selectedSchools;
        }

        /**
         * <summary>
         * Makes sure that an applicant is only selected once. If the applicant is removed from currentSchool because
         * they had a better selected rank at another school, returns true. Otherwise returns false.
         * </summary>
         * 
         * <param name="applicant">Applicant to reconcile across schools</param>
         * <param name="currentSchool">The school that is currently being iterated through by the larger algorithm</param>
         * <param name="selectedSchools">Schools the applicant was selected at</param>
         * <returns>True if removed from the current school, false otherwise</returns>
         */
        private bool ReconcileApplicant(Applicant applicant, School currentSchool, List<School> selectedSchools)
        {
            // If applicant is only in one school, no reconciliation needed
            if(selectedSchools.Count <= 1)
            {
                return false;
            }

            // Determine which school the applicant is the lowest (number-wise) on the list
            School lowestSchool = null;
            var lowestIndex = 10000; //start very high
            
            List<School> shuffledSchools = new List<School>(selectedSchools);
            shuffledSchools.Shuffle(new Random());

            foreach(School s in shuffledSchools)
            {
                int index = db.Selecteds.First(selected=>selected.ApplicantID == applicant.ID && selected.SchoolID == s.ID).Rank;
                if(index < lowestIndex)
                {
                    lowestSchool = s;
                    lowestIndex = index;
                }
            }

            // Remove the student from the higher (number-wise) on the list schools and run lotteries for those schools
            var effectsCurrentSchool = false;
            foreach(School s in selectedSchools)
            {
                if(s != lowestSchool)
                {
                    var selected = s.Selecteds.First(se => se.ApplicantID == applicant.ID);
                    db.Selecteds.Remove(selected);
                    db.SaveChanges();

                    var waitlisteds = Utils.GetApplicants(s.Waitlisteds.OrderBy(w => w.Rank).ToList());

                    schoolLottery.Run(s, waitlisteds);
                    if(s == currentSchool)
                    {
                        effectsCurrentSchool = true;
                    }
                }
            }

            return effectsCurrentSchool;
        }
    }
}
