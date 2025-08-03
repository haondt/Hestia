using Haondt.Core.Models;

namespace Hestia.Domain.Models
{
    public record MacroEfficiencyInsightsModel(
        Optional<decimal> ProteinPerDollar = default,
        Optional<decimal> FatPerDollar = default,
        Optional<decimal> CarbsPerDollar = default,
        Optional<decimal> ProteinPerCalorie = default,
        Optional<decimal> FatPerCalorie = default,
        Optional<decimal> CarbsPerCalorie = default);
}
