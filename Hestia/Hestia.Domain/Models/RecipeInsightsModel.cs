using Haondt.Core.Models;

namespace Hestia.Domain.Models
{
    public record RecipeInsightsModel
    {
        public Optional<MacroNutrientBreakdownInsightsModel> MacroNutrientBreakdown { get; set; }
        public MacroEfficiencyInsightsModel MacroEfficiency { get; set; } = new();
        public RecipeCostAnalysisInsightsModel CostAnalysis { get; set; } = new();
        public Optional<decimal> ProteinScore { get; set; }

        public Optional<RecipeServingInsightsModel> ServingInsights { get; set; }
        public Optional<RecipeTotalInsightsModel> TotalInsights { get; set; }
    }

    public record RecipeCostAnalysisInsightsModel(
        decimal CostPerServing = default,
        Optional<decimal> CaloriesPerDollar = default);

    public record RecipeServingInsightsModel(
        double CaloriesPerServing,
        double ProteinGramsPerServing,
        double FatGramsPerServing,
        double CarbGramsPerServing);
    public record RecipeTotalInsightsModel(
        double Calories,
        double ProteinGrams,
        double FatGrams,
        double CarbGrams,
        decimal Cost);

}