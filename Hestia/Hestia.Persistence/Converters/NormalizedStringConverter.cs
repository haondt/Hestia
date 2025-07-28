using Hestia.Core.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hestia.Persistence.Converters
{
    internal class NormalizedStringConverter : ValueConverter<NormalizedString, string>
    {
        public NormalizedStringConverter() : base(
            normalizedString => normalizedString.Value,
            value => NormalizedString.Create(value))
        {
        }
    }
}
