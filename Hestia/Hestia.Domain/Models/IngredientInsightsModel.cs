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

    public record MacroNutrientBreakdownInsightsModel(
        double ProportionProtein,
        double ProportionFat,
        double ProportionCarbs);

    public record MacroEfficiencyInsightsModel(
        Optional<decimal> ProteinPerDollar = default,
        Optional<decimal> FatPerDollar = default,
        Optional<decimal> CarbsPerDollar = default,
        Optional<decimal> ProteinPerCalorie = default,
        Optional<decimal> FatPerCalorie = default,
        Optional<decimal> CarbsPerCalorie = default);

    public record CostAnalysisInsightsModel(
        Optional<decimal> ServingsPerPackage = default,
        Optional<decimal> CostPerServing = default,
        Optional<decimal> CaloriesPerDollar = default);
}
