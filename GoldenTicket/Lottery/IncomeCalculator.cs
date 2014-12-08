using System.Collections.Generic;
using System.Linq;
using GoldenTicket.DAL;
using GoldenTicket.Models;

namespace GoldenTicket.Lottery
{
    /**
     * <summary>
     *  Contains static methods for calculating whether and applicant is below the poverty line, based on an applicant's average monthly income.
     *  
     *  <para>
     *      The class depends on a database connection to GoldenTicketDbContext. It uses the configuration
     *      in the PovertyConfigs table and the GlobalConfigs.IncomeMultiplier to perform the calculation.
     *  </para>
     *  
     * </summary>
     * 
     **/
    public class IncomeCalculator
    {
        // Multiplier to be applied for every household member above 10 people
        private double above10Multipler;

        // Database connection
        private GoldenTicketDbContext db;

        // Poverty line information
        // key => num househould members, val => poverty line income amount
        private Dictionary<int, int> povertyLineByNumPeople = new Dictionary<int, int>();
            
        /**
         * <summary>
         * Creates a new income calculator based on the poverty line
         * configuration in the database
         * </summary>
         * 
         * <param name="db">Database connection</param>
         */
        public IncomeCalculator(GoldenTicketDbContext db)
        {
            this.db = db;

            for (var i = 2; i <= 10; i++)
            {
                povertyLineByNumPeople.Add(i, db.PovertyConfigs.Find(i).MinimumIncome);
            }

            var globalConfig = db.GlobalConfigs.First();
            above10Multipler = globalConfig.IncomeMultiplier;
        }

        /**
         * <param name="numHouseholdMembers">Number of people in the household</param>
         * <param name="incomeAmount">Combined average income of all household members per month</param>
         * <returns>True if income amount and number of household members equals being below the poverty line</returns>
         */
        public bool IsBelowPovertyLine(int numHouseholdMembers, int incomeAmount)
        {
            if (numHouseholdMembers > 10)
            {
                int additionalPeopleOver10 = numHouseholdMembers - 10;
                double povertyLineAmount = povertyLineByNumPeople[10] +
                                           (above10Multipler*additionalPeopleOver10);

                return incomeAmount <= povertyLineAmount;
            }
            return incomeAmount <= povertyLineByNumPeople[numHouseholdMembers];
        }

        /**
         * <param name="applicant">Applicant with household fields populated</param>
         * <returns>True if income amount and number of household members equals being below the poverty line</returns>
         */
        public bool IsBelowPovertyLine(Applicant applicant)
        {
            return IsBelowPovertyLine(applicant.HouseholdMembers, applicant.HouseholdMonthlyIncome);
        }
    }
}