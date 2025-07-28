using Hestia.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Hestia.Persistence.Models
{
    [PrimaryKey(nameof(NormalizedFromUnit), nameof(NormalizedToUnit))]
    public record class UnitConversionDataModel
    {
        public required NormalizedString NormalizedFromUnit { get; set; }
        public required NormalizedString NormalizedToUnit { get; set; }
        public required string FromUnit { get; set; }
        public required string ToUnit { get; set; }
        public required decimal Multiplier { get; set; }
    }
}
