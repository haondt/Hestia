using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Hestia.Domain.Attributes
{
    public class ValidDateStringAttribute : ValidationAttribute
    {
        public static bool Validate(object? value)
        {
            if (value is not string dateString)
                return false;

            return DateTime.TryParseExact(
                dateString,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);
        }
        public override bool IsValid(object? value)
        {
            return Validate(value);
        }
    }
}
