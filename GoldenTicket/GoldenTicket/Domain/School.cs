using System;
using System.Collections.Generic;

namespace GoldenTicket.Domain
{
    public class School
    {
        string name;
        string district;
        double percentBelowPovertyLine;
        int numClassrooms;

        List<Applicant> applicants = new List<Applicant>(); // raw list of applicants
        List<Applicant> selectedApplicants = new List<Applicant>(); // list of applicants selected
        List<Applicant> waitlistedApplicants = new List<Applicant>(); // list of applicants waitlist
        List<Applicant> shuffledApplicants = new List<Applicant>(); // same as raw list of applicants, but randomly shuffled -- preserved for auditing
        List<Applicant> filteredApplicants = new List<Applicant>(); // same as shuffled applicants, but filtering performed (age, duplicates, not in district) -- preserved for auditing

        public School()
        {
        }

        public School(string name, string district)
        {
            this.name = name;
            this.district = district;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string District
        {
            get { return district; }
            set { district = value; }
        }

        public double PercentBelowPovertyLine
        {
            get { return percentBelowPovertyLine; }
            set { percentBelowPovertyLine = value; }
        }

        public int NumClassrooms
        {
            get { return numClassrooms; }
            set { numClassrooms = value; }
        }

        public List<Applicant> Applicants
        {
            get { return applicants; }
        }

        public List<Applicant> SelectedApplicants
        {
            get { return selectedApplicants; }
        }

        public List<Applicant> WaitlistedApplicants
        {
            get { return waitlistedApplicants; }
        }

        public List<Applicant> ShuffledApplicants
        {
            get { return shuffledApplicants; }
        }

        public List<Applicant> FilteredApplicants
        {
            get { return filteredApplicants; }
            set { filteredApplicants = value; }
        }

        public void ClearApplicants()
        {
            applicants.Clear();
            selectedApplicants.Clear();
            waitlistedApplicants.Clear();
            shuffledApplicants.Clear();
            filteredApplicants.Clear();
        }
    }
}
