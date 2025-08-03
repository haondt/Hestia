using Haondt.Core.Models;

namespace Hestia.Domain.Models
{
    public class IngredientInsightsModel
    {
        public Optional<MacroNutrientBreakdownInsightsModel> MacroNutrientBreakdown { get; set; }
        public MacroEfficiencyInsightsModel MacroEfficiency { get; set; } = new();
        public CostAnalysisInsightsModel CostAnalysis { get; set; } = new();
        public Optional<decimal> ProteinScore { get; set; }
    }

    public record CostAnalysisInsightsModel(
        Optional<decimal> ServingsPerPackage = default,
        Optional<decimal> CostPerServing = default,
        Optional<decimal> CaloriesPerDollar = default);
}
