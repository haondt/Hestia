using Haondt.Core.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hestia.Persistence.Converters
{
    internal class AbsoluteDateTimeConverter() : ValueConverter<AbsoluteDateTime, long>(
        v => v.UnixTimeSeconds,
        v => AbsoluteDateTime.Create(v))
    {
    }
}
