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

                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, "Not a string!");
            }
        }
    }
}
