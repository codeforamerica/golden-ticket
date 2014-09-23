using GoldenTicket.Reader;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace GoldenTicket.Calc
{
    public class IncomeCalculator
    {
        private const int ABOVE_10_FACTOR_KEY = 0; // multiplier for number of people above 10 per household
        Dictionary<int, int> povertyLineByNumPeople; // key => num househould members, val => poverty line income amount


        public IncomeCalculator(IncomeReader incomeReader)
        {
            this.povertyLineByNumPeople = incomeReader.ReadIncome();
        }

        public bool IsBelowPovertyLine(int numHouseholdMembers, int incomeAmount)
        {
            if(numHouseholdMembers > 10)
            {
                int additionalPeopleOver10 = numHouseholdMembers - 10;
                int povertyLineAmount = povertyLineByNumPeople[10] + (povertyLineByNumPeople[ABOVE_10_FACTOR_KEY] * additionalPeopleOver10);

                return incomeAmount <= povertyLineAmount;
            }
            else
            {
                return incomeAmount <= povertyLineByNumPeople[numHouseholdMembers];
            }
        }

    }
}
