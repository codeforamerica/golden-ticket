using GoldenTicket.Domain;
using System;
using System.Collections.Generic;
using GoldenTicket.Lottery; //for List shuffle extensions


namespace GoldenTicket.Lottery
{
    public class CrossSchoolReconciler
    {
        SchoolLottery schoolLottery;

        public CrossSchoolReconciler(SchoolLottery schoolLottery)
        {
            this.schoolLottery = schoolLottery;
        }

        //TODO This method can be made more efficient. Optimize later.
        public List<School> Reconcile(List<School> schools)
        {
            // Remove selected students from all school waitlists
            RemoveSelectedFromWaitlists(schools);

            // Reconcile students selected for multiple schools
            // i.e. Leave the student in the school in which they are highest on the selected list
            var reconciledApplicants = new List<Applicant>();
            var remainingSchools = new List<School>(schools);
            foreach(School s in schools)
            {
                schoolLoop: { }
                List<Applicant> selectedApplicants = new List<Applicant>(s.SelectedApplicants); // to prevent concurrent modification during iteration
                foreach(Applicant a in selectedApplicants)
                {
                    // Skip applicant if already reconciled - done to optimize in the event the schoolLoop is reset (see bottom of loop)
                    if(reconciledApplicants.Contains(a))
                    {
                        continue;
                    }

                    // Reconcile the applicant between schools he/she was selected 
                    List<School> selectedSchools = GetSchoolsApplicantWasSelectedAt(a, remainingSchools);
                    bool currentSchoolEffected = ReconcileApplicant(a, s, selectedSchools);
                    RemoveSelectedFromWaitlists(schools, s.SelectedApplicants); //TODO This step is very inefficient. Optimize later.

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
            List<Applicant> selectedApplicants = new List<Applicant>();
            foreach(School s in schools)
            {
                selectedApplicants.AddRange(s.SelectedApplicants);
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
            foreach (Applicant a in applicants)
            {
                foreach (School s in schools)
                {
                    s.WaitlistedApplicants.Remove(a);
                }
            }
        }

        private List<School> GetSchoolsApplicantWasSelectedAt(Applicant applicant, IEnumerable<School> schools)
        {
            List<School> selectedSchools = new List<School>();
            
            foreach(School s in schools)
            {
                if(s.SelectedApplicants.Contains(applicant))
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
            int lowestIndex = 10000; //start very high
            
            List<School> shuffledSchools = new List<School>(selectedSchools);
            shuffledSchools.Shuffle(new Random());

            foreach(School s in shuffledSchools)
            {
                int index = s.SelectedApplicants.IndexOf(applicant);
                if(index < lowestIndex)
                {
                    lowestSchool = s;
                    lowestIndex = index;
                }
            }

            // Remove the student from the higher (number-wise) on the list schools and run lotteries for those schools
            bool effectsCurrentSchool = false;
            foreach(School s in selectedSchools)
            {
                if(s != lowestSchool)
                {
                    s.SelectedApplicants.Remove(applicant);
                    schoolLottery.Run(s, s.WaitlistedApplicants, false, false);
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
