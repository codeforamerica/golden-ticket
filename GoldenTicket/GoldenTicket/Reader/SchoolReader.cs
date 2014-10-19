using GoldenTicket.Domain;
using System;
using System.Collections.Generic;

namespace GoldenTicket.Reader
{
    public interface SchoolReader
    {
        List<School> ReadSchools();
    }
}
