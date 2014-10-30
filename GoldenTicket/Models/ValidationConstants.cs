using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoldenTicket.Models
{
    public class ValidationConstants
    {
        public const string LETTERS_SPACES_DASHES_REGEX = @"^(\p{L}|\s|-)+$";
        public const string STREET_ADDRESS_REGEX = @"^[ \p{L}0-9.#-]+$"; //      (\s|\p{L}|\d|\.|-)+
        public const string ZIP_CODE_REGEX = @"^\d{5}(-\d{4})?$";
    }
}