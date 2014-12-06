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
        private double above10Multipler;

        private GoldenTicketDbContext db;

        private Dictionary<int, int> povertyLineByNumPeople = new Dictionary<int, int>();
            // key => num househould members, val => poverty line income amount

        public IncomeCalculator(GoldenTicketDbContext db)
        {
            this.db = db;

            for (var i = 2; i <= 10; i++)
            {
                povertyLineByNumPeople.Add(i, db.PovertyConfigs.Find(i).MinimumIncome);
            }

            GlobalConfig globalConfig = db.GlobalConfigs.First();
            above10Multipler = globalConfig.IncomeMultiplier;
        }

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

        public bool IsBelowPovertyLine(Applicant applicant)
        {
            return IsBelowPovertyLine(applicant.HouseholdMembers, applicant.HouseholdMonthlyIncome);
        }
    }
}