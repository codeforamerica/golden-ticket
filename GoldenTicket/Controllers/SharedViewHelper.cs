using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GoldenTicket.DAL;
using GoldenTicket.Models;
using GoldenTicket.Resources;

namespace GoldenTicket.Controllers
{
    public class SharedViewHelper
    {
        private static readonly DateTime AGE_4_BY_DATE = new DateTime(DateTime.Today.Year, 9, 1);

        private readonly GoldenTicketDbContext database;

        public SharedViewHelper(GoldenTicketDbContext database)
        {
            this.database = database;
        }

        public void PrepareStudentInformationView(dynamic viewBag)
        {
            PrepareStudentInformationView(viewBag, true);
        }

        public void PrepareStudentInformationView(dynamic viewBag, bool isCityEditable)
        {
            viewBag.DistrictNames = GetDistrictNames();
            viewBag.IsCityEditable = isCityEditable;
        }

        public string[] GetDistrictNames()
        {
            ISet<string> districtNames = new HashSet<string>();
            districtNames.Add("");

            foreach (var school in database.Schools)
            {
                districtNames.Add(school.City);
            }

            return districtNames.OrderBy(s => s).ToArray();
        }

        public void PrepareGuardianInformationView(dynamic viewBag)
        {
            var incomeRanges = GetIncomeRanges();
            viewBag.IncomeRanges = incomeRanges;
            viewBag.MaxIncome = incomeRanges.Last().Value;

            viewBag.GlobalConfig = database.GlobalConfigs.First();
        }

        public IEnumerable<SelectListItem> GetIncomeRanges()
        {
            var incomeRanges = new List<SelectListItem>();

            var previousIncomeLine = 1; // 1 is the bottom range, although to users 0 will appear as the minimum. This will help with validation checking.
            foreach (var householdMembers in Enumerable.Range(2, 9))
            {
                var povertyConfig = database.PovertyConfigs.First(p => p.HouseholdMembers == householdMembers);
                var item = new SelectListItem
                {
                    Text = previousIncomeLine.ToString("") + " - " + povertyConfig.MinimumIncome.ToString(""),
                    Value = povertyConfig.MinimumIncome.ToString()
                };

                previousIncomeLine = povertyConfig.MinimumIncome;
                incomeRanges.Add(item);
            }

            var maxIncome = previousIncomeLine + 1;
            var maxRange = new SelectListItem
            {
                Text = string.Format(GoldenTicketText.OrMore, maxIncome.ToString("")),
                Value = maxIncome.ToString()
            };
            incomeRanges.Add(maxRange);

            return incomeRanges;
        }

        public void PrepareSchoolSelectionView(dynamic viewBag, Applicant applicant)
        {
            var eligiblePrograms = database.Schools.Where(p => p.City == applicant.StudentCity).OrderBy(p => p.Name).ToList();
            viewBag.Programs = eligiblePrograms;

            var applieds = database.Applieds.Where(a => a.ApplicantID == applicant.ID).ToList();
            viewBag.Applieds = applieds;
        }

        public void EmptyCheckStudentInformation(ModelStateDictionary modelState, Applicant applicant)
        {
            if (string.IsNullOrEmpty(applicant.StudentFirstName))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentFirstName);
                modelState.AddModelError("StudentFirstName", message);
            }
            if (string.IsNullOrEmpty(applicant.StudentLastName))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentLastName);
                modelState.AddModelError("StudentLastName", message);
            }
            if (string.IsNullOrEmpty(applicant.StudentStreetAddress1))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentStreetAddress1);
                modelState.AddModelError("StudentStreetAddress1", message);
            }
            if (string.IsNullOrEmpty(applicant.StudentCity))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentCity);
                modelState.AddModelError("StudentCity", message);
            }
            if (string.IsNullOrEmpty(applicant.StudentZipCode))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentZipCode);
                modelState.AddModelError("StudentZipCode", message);
            }
            if (applicant.StudentBirthday == null)
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentBirthday);
                modelState.AddModelError("StudentBirthday", message);
            }
            if (applicant.StudentGender == null)
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentGender);
                modelState.AddModelError("StudentGender", message);
            }

            if (applicant.StudentBirthday != null && !IsAgeEligible(applicant.StudentBirthday.Value))
            {
                var message = string.Format(GoldenTicketText.IneligibleBirthday, DateTime.Today.Year.ToString());
                modelState.AddModelError("StudentBirthday", message);
            }

        }

        private static bool IsAgeEligible(DateTime birthday)
        {
            var ageByCutoff = AGE_4_BY_DATE.Year - birthday.Year;
            var adjustedDate = AGE_4_BY_DATE.AddYears(-ageByCutoff);
            if (birthday > adjustedDate)
            {
                ageByCutoff--;
            }

            return (ageByCutoff == 4);
        }

        public void EmptyCheckGuardianInformation(ModelStateDictionary modelState, Applicant applicant)
        {
            if (string.IsNullOrEmpty(applicant.Contact1FirstName))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.Contact1FirstName);
                modelState.AddModelError("Contact1FirstName", message);
            }
            if (string.IsNullOrEmpty(applicant.Contact1LastName))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.Contact1LastName);
                modelState.AddModelError("Contact1FirstName", message);
            }
            if (string.IsNullOrEmpty(applicant.Contact1Phone))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.Contact1Phone);
                modelState.AddModelError("Contact1Phone", message);
            }
            if (string.IsNullOrEmpty(applicant.Contact1Email))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.Contact1Email);
                modelState.AddModelError("Contact1Email", message);
            }
            if (string.IsNullOrEmpty(applicant.Contact1Relationship))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.Contact1Relationship);
                modelState.AddModelError("Contact1Relationship", message);
            }
            if (applicant.HouseholdMonthlyIncome == null || applicant.HouseholdMonthlyIncome == 0) // 1 is the bottom range, although to users 0 will appear as the minimum. This will help with validation checking, since an empty selection is assigned 0 by MVC framework. This does not impact income calculations for lottery selection.
            {
                modelState.AddModelError("HouseholdMonthlyIncome", GoldenTicketText.HouseholdIncomeMissing);
            }
        }

    }
}