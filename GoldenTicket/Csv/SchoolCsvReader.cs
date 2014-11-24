using CsvHelper;
using GoldenTicket.Models;
using GoldenTicket.Reader;
using System;
using System.Collections.Generic;
using System.IO;


namespace GoldenTicket.Csv
{
    public class SchoolCsvReader : SchoolReader
    {
        string csvFilePath;

        public SchoolCsvReader(string csvFilePath)
        {
            this.csvFilePath = csvFilePath;
        }

        public List<School> ReadSchools()
        {
            List<School> schools;
            using(StreamReader textReader = new StreamReader(csvFilePath))
            {
                CsvReader csvReader = new CsvReader(textReader);
                csvReader.Configuration.TrimFields = true;
                csvReader.Configuration.RegisterClassMap<SchoolCsvMapper>();

                var schoolRecords = csvReader.GetRecords<School>();
                schools = new List<School>(schoolRecords);
            }

            return schools;
        }
    }
}
