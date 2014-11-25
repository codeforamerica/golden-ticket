using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoldenTicket.Models
{
    public class ValidationConstants
    {
        public const string LETTERS_SPACES_DASHES_APOSTROPHE_REGEX = @"^(\p{L}|\s|-|'|\(|\)|\.)+$"; //TODO Rename, including error message text
        public const string STREET_ADDRESS_REGEX = @"^[ \p{L}0-9.#,;-]+$"; //TODO Update the error message in the translation
        public const string ZIP_CODE_REGEX = @"^\d{5}(-\d{4})?$";
        public const string PHONE_REGEX = @"^\d{3}-?\d{3}-?\d{4}$";
    }
}