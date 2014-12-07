using System.Linq;
using GoldenTicket.DAL;
using GoldenTicket.Misc;
using GoldenTicket.Models;
using System;
using System.Collections.Generic;
using GoldenTicket.Lottery; //for List shuffle extensions


namespace GoldenTicket.Lottery
{
    public class CrossSchoolReconciler
    {
        private GoldenTicketDbContext db = new GoldenTicketDbContext();
        private SchoolLottery schoolLottery;

        public CrossSchoolReconciler(GoldenTicketDbContext db)
        {
            this.db = db;
            this.schoolLottery = new SchoolLottery(db);
        }

        //TODO This method can be made more efficient. Optimize later.
        public List<School> Reconcile()
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

            return schools;
        }

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

        // Global removal of selected from all schools' waitlists
        private void RemoveSelectedFromWaitlists(IEnumerable<School> schools)
        {
            RemoveSelectedFromWaitlists(schools, GetAllSelectedApplicants(schools));
        }

        //TODO Rename this method
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
