using CsvHelper;
using GoldenTicket.Reader;
using System;
using System.Collections.Generic;
using System.IO;

namespace GoldenTicket.Csv
{
    public class IncomeCsvReader : IncomeReader
    {
        string csvFilePath;

        public IncomeCsvReader(string csvFilePath)
        {
            this.csvFilePath = csvFilePath;
        }

        public Dictionary<int,int> ReadIncome()
        {
            Dictionary<int,int> incomeLevels = new Dictionary<int,int>();
            using(StreamReader textReader = new StreamReader(csvFilePath))
            {
                CsvReader csvReader = new CsvReader(textReader);
                while(csvReader.Read())
                {
                    int numHouseholdMembers = csvReader.GetField<int>("NUMBER OF HOUSEHOLD MEMBERS");
                    int povertyLineAmount = csvReader.GetField<int>("POVERTY LINE AMOUNT");
                    incomeLevels.Add(numHouseholdMembers, povertyLineAmount);
                }
            }

            return incomeLevels;
        }
    }
}
