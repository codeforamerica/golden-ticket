using System.Linq;
using GoldenTicket.DAL;
using GoldenTicket.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace GoldenTicket.Calc
{
    public class IncomeCalculator
    {
        private static IncomeCalculator incomeCalc = new IncomeCalculator();

        private GoldenTicketDbContext db = new GoldenTicketDbContext();
        private Dictionary<int, int> povertyLineByNumPeople = new Dictionary<int, int>(); // key => num househould members, val => poverty line income amount
        private int above10Multipler = 0;

        private IncomeCalculator()
        {
            for (int i = 2; i <= 10; i++)
            {
                povertyLineByNumPeople.Add(i,db.PovertyConfigs.Find(i).MinimumIncome);
            }

            above10Multipler = db.GlobalConfigs.First().MinimumIncomeMultiplier();
        }

        public static bool IsBelowPovertyLine(int numHouseholdMembers, int incomeAmount)
        {
            if(numHouseholdMembers > 10)
            {
                var additionalPeopleOver10 = numHouseholdMembers - 10;
                var povertyLineAmount = incomeCalc.povertyLineByNumPeople[10] + (incomeCalc.above10Multipler * additionalPeopleOver10);

                return incomeAmount <= povertyLineAmount;
            }
            else
            {
                return incomeAmount <= incomeCalc.povertyLineByNumPeople[numHouseholdMembers];
            }
        }

        public static bool IsBelowPovertyLine(Applicant applicant)
        {
            return IsBelowPovertyLine(applicant.HouseholdMembers, applicant.HouseholdMonthlyIncome);
        }

    }
}
