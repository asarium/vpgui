using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace VPGUI.ValidationRules
{
    class FileNameValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string)
            {
                var str = value as string;

                char[] invalid = Path.GetInvalidFileNameChars();

                try
                {
                    var invalidChar = invalid.First(str.Contains);

                    return new ValidationResult(false, "The character '" + invalidChar + "' may not appear in a name.");
                }
                catch (InvalidOperationException)
                {
                    // string is valid
                }

                if (str.Length > MaxLength)
                {
                    return new ValidationResult(false, "The name is too long. Must be shorter than " + (MaxLength + 1) + "characters.");
                }

                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, "Not a string!");
            }
        }

        public int MaxLength
        { get; set; }
    }
}
