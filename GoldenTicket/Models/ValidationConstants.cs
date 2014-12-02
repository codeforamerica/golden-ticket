using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoldenTicket.Models
{
    public class ValidationConstants
    {
        public const string LETTERS_SPACES_DASHES_APOSTROPHE_REGEX = @"^(\p{L}|\s|-|'|\(|\)|\.|\d)+$"; //TODO Rename, including error message text (since the name is no longer accurate)
        public const string STREET_ADDRESS_REGEX = @"^[ \p{L}0-9.#;-]+$"; //TODO Update the error message in the translation file
        public const string ZIP_CODE_REGEX = @"^\d{5}(-\d{4})?$";
        public const string PHONE_REGEX = @"^\d{3}-?\d{3}-?\d{4}$";
    }
}