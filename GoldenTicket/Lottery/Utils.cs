using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GoldenTicket.Models;

namespace GoldenTicket.Lottery
{
    public class Utils
    {
        public static List<Applicant> GetApplicants(List<Selected> selecteds)
        {
            var applicants = new List<Applicant>();
            foreach (var s in selecteds)
            {
                applicants.Add(s.Applicant);
            }

            return applicants;
        }

        public static List<Applicant> GetApplicants(List<Shuffled> shuffleds)
        {
            var applicants = new List<Applicant>();
            foreach (var s in shuffleds)
            {
                applicants.Add(s.Applicant);
            }

            return applicants;
        }

        public static List<Applicant> GetApplicants(List<Waitlisted> waitlisteds)
        {
            var applicants = new List<Applicant>();
            foreach (var w in waitlisteds)
            {
                applicants.Add(w.Applicant);
            }

            return applicants;
        }

        public static List<Applicant> GetApplicants(List<Applied> applieds)
        {
            var applicants = new List<Applicant>();
            foreach (var a in applieds)
            {
                applicants.Add(a.Applicant);
            }

            return applicants;
        }

        public static List<School> GetSchools(List<Applied> applieds)
        {
            var schools = new List<School>();
            foreach(var a in applieds)
            {
                schools.Add(a.School);
            }

            return schools;
        }

        public static List<School> GetSchools(List<Waitlisted> waitlisteds)
        {
            var schools = new List<School>();
            foreach (var a in waitlisteds)
            {
                schools.Add(a.School);
            }

            return schools;
        }

    }
}