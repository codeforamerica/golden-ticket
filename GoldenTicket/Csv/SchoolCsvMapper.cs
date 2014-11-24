using CsvHelper.Configuration;
using GoldenTicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldenTicket.Csv
{
    public sealed class SchoolCsvMapper : CsvClassMap<School>
    {
        public SchoolCsvMapper()
        {
            Map(s => s.Name).Name("SCHOOL NAME");
            Map(s => s.City).Name("DISTRICT");
            Map(s => s.NumClassrooms).Name("NUMBER OF CLASSROOMS");
            Map(s => s.PercentBelowPovertyLine).Name("PERCENT OF STUDENTS BELOW POVERTY LINE");
        }
    }
}
