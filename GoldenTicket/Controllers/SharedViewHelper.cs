using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using GoldenTicket.DAL;
using GoldenTicket.Models;
using GoldenTicket.Resources;

namespace GoldenTicket.Controllers
{
    /**
     * <summary>
     * Provides methods for working with and preparing views that are shared across controllers.
     * </summary>
     */
    public class SharedViewHelper
    {
        // Applicants need to be age 4 by September 1 of the current year
        private static readonly DateTime AGE_4_BY_DATE = new DateTime(DateTime.Today.Year, 9, 1);

        // Database connection to view stored model objects
        private readonly GoldenTicketDbContext database;

        /**
         * <summary>Create a new SharedViewHelper</summary>
         */
        public SharedViewHelper(GoldenTicketDbContext database)
        {
            this.database = database;
        }

        /**
         * <summary>
         * Prepares the student information form for an applicant.
         * </summary>
         * 
         * <param name="viewBag">Current response's ViewBag</param>
         * <param name="isCityEdit">Can the applicant edit the city (true if so)</param>
         */
        public void PrepareStudentInformationView(dynamic viewBag, bool isCityEditable = true)
        {
            viewBag.DistrictNames = GetDistrictNames();
            viewBag.IsCityEditable = isCityEditable;
        }

        /**
         * <summary>Get all district names in alphabetical order</summary>
         **/
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

        /**
         * <summary>Setup the guardian information form view</summary>
         * <param name="viewBag">The current response's ViewBag</param>
         */
        public void PrepareGuardianInformationView(dynamic viewBag)
        {
            var incomeRanges = GetIncomeRanges();
            viewBag.IncomeRanges = incomeRanges;
            viewBag.MaxIncome = incomeRanges.Last().Value;

            viewBag.GlobalConfig = database.GlobalConfigs.First();
        }

        /**
         * <summary>
         * Get all possible income ranges based on the poverty configuration. Formats
         * them into an enumerable that can easily be turned into a select input box
         * in a view.
         * </summary>
         */
        public IEnumerable<SelectListItem> GetIncomeRanges()
        {
            var incomeRanges = new List<SelectListItem>();

            // Get all the poverty configuration ranges
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

            // The last range number "and more"
            var maxIncome = previousIncomeLine + 1;
            var maxRange = new SelectListItem
            {
                Text = string.Format(GoldenTicketText.OrMore, maxIncome.ToString("")),
                Value = maxIncome.ToString()
            };
            incomeRanges.Add(maxRange);

            return incomeRanges;
        }

        /**
         * <summary>
         * Prepare variables that the school selection view possible.
         * </summary>
         * 
         * <param name="viewBag">The current response's ViewBag object</param>
         * <param name="applicant">The applicant with ID and city fields</param>
         */
        public void PrepareSchoolSelectionView(dynamic viewBag, Applicant applicant)
        {
            var eligiblePrograms = database.Schools.Where(p => p.City == applicant.StudentCity).OrderBy(p => p.Name).ToList();
            viewBag.Programs = eligiblePrograms;

            var applieds = database.Applieds.Where(a => a.ApplicantID == applicant.ID).ToList();
            viewBag.Applieds = applieds;
        }

        /**
         * <summary>
         * Validate that required fields are not missing from a student information form.
         * Fills in validity information to modelState arguement.
         * </summary>
         * 
         * <param name="modelState">The current request/response model state</param>
         * <param name="applicant">Applicant object with student information fields</param>
         */
        public void EmptyCheckStudentInformation(ModelStateDictionary modelState, Applicant applicant)
        {
            // First name
            if (string.IsNullOrEmpty(applicant.StudentFirstName))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentFirstName);
                modelState.AddModelError("StudentFirstName", message);
            }
            // Last name
            if (string.IsNullOrEmpty(applicant.StudentLastName))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentLastName);
                modelState.AddModelError("StudentLastName", message);
            }
            // Street 1
            if (string.IsNullOrEmpty(applicant.StudentStreetAddress1))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentStreetAddress1);
                modelState.AddModelError("StudentStreetAddress1", message);
            }
            // City
            if (string.IsNullOrEmpty(applicant.StudentCity))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentCity);
                modelState.AddModelError("StudentCity", message);
            }
            // Zip code
            if (string.IsNullOrEmpty(applicant.StudentZipCode))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentZipCode);
                modelState.AddModelError("StudentZipCode", message);
            }
            // Birthday
            if (applicant.StudentBirthday == null)
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentBirthday);
                modelState.AddModelError("StudentBirthday", message);
            }
            // Gender
            if (applicant.StudentGender == null)
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.StudentGender);
                modelState.AddModelError("StudentGender", message);
            }

            // Is the birthdate eligible?
            if (applicant.StudentBirthday != null && !IsAgeEligible(applicant.StudentBirthday.Value))
            {
                var message = string.Format(GoldenTicketText.IneligibleBirthday, DateTime.Today.Year.ToString());
                modelState.AddModelError("StudentBirthday", message);
            }

        }

        /**
         * <summary>
         * Verifies that a birthday is elibile for the lottery.
         * </summary>
         * 
         * <param name="birthday">The birthday to check for elibility</param>
         * <returns>True if the birthday is eligible</returns>
         */
        private static bool IsAgeEligible(DateTime birthday)
        {
            // Get the difference in years
            var ageByCutoff = AGE_4_BY_DATE.Year - birthday.Year;
            
            // Adjust the date in case the birthday is in a month after the cutoff date
            var adjustedDate = AGE_4_BY_DATE.AddYears(-ageByCutoff);
            if (birthday > adjustedDate)
            {
                ageByCutoff--;
            }

            return (ageByCutoff == 4);
        }

        /**
         * <summary>
         * Verify that all required guardian/contact person information fields have been entered. Updates
         * the model state with validity information.
         * </summary>
         * 
         * <param name="modelState">The model state of the current request/response</param>
         * <param name="applicant">Applicant object with guardian and contact person information entered</param>
         */
        public void EmptyCheckGuardianInformation(ModelStateDictionary modelState, Applicant applicant)
        {
            // Contact 1 first name
            if (string.IsNullOrEmpty(applicant.Contact1FirstName))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.Contact1FirstName);
                modelState.AddModelError("Contact1FirstName", message);
            }

            // Contact 1 last name
            if (string.IsNullOrEmpty(applicant.Contact1LastName))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.Contact1LastName);
                modelState.AddModelError("Contact1FirstName", message);
            }

            // Contact 1 phone number
            if (string.IsNullOrEmpty(applicant.Contact1Phone))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.Contact1Phone);
                modelState.AddModelError("Contact1Phone", message);
            }

            // Contact 1 email
            if (string.IsNullOrEmpty(applicant.Contact1Email))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.Contact1Email);
                modelState.AddModelError("Contact1Email", message);
            }

            // Contact 1 relationship
            if (string.IsNullOrEmpty(applicant.Contact1Relationship))
            {
                var message = string.Format(GoldenTicketText.PropertyMissing, GoldenTicketText.Contact1Relationship);
                modelState.AddModelError("Contact1Relationship", message);
            }

            // Household monthly income
            if (applicant.HouseholdMonthlyIncome == 0) // 1 is the bottom range, although to users 0 will appear as the minimum. This will help with validation checking, since an empty selection is assigned 0 by MVC framework. This does not impact income calculations for lottery selection.
            {
                modelState.AddModelError("HouseholdMonthlyIncome", GoldenTicketText.HouseholdIncomeMissing);
            }
        }

    }
}