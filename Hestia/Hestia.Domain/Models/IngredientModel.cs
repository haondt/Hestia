using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Hestia.Domain.Services;
using Hestia.Persistence.Models;
using System.ComponentModel.DataAnnotations;

namespace Hestia.Domain.Models
{
    public class IngredientModel
    {
        [Required]
        public required string Name { get; set; }
        public required Optional<string> Brand { get; set; }
        public required Optional<string> Vendor { get; set; }

        public required Optional<decimal> ServingSizeQuantity { get; set; }
        public required Optional<string> ServingSizeUnit { get; set; }
        public required Optional<decimal> AlternateServingSizeQuantity { get; set; }
        public required Optional<string> AlternateServingSizeUnit { get; set; }
        public required Optional<decimal> Calories { get; set; }
        public required Optional<decimal> FatGrams { get; set; }
        public required Optional<decimal> CarbGrams { get; set; }
        public required Optional<decimal> ProteinGrams { get; set; }

        public required Optional<decimal> PackageSizeQuantity { get; set; }
        public required Optional<string> PackageSizeUnit { get; set; }
        public required Optional<decimal> PackageCostDollars { get; set; }

        private string _notes = "";
        public string Notes { get => _notes; set => _notes = value ?? ""; }

        public static IngredientModel FromDataModel(IngredientDataModel model) => new()
        {
            Name = model.Name,
            Brand = model.Brand.AsOptional(),
            Vendor = model.Vendor.AsOptional(),
            ServingSizeQuantity = model.ServingSizeQuantity.AsOptional(),
            ServingSizeUnit = model.ServingSizeUnit.AsOptional(),
            AlternateServingSizeQuantity = model.AlternateServingSizeQuantity.AsOptional(),
            AlternateServingSizeUnit = model.AlternateServingSizeUnit.AsOptional(),
            Calories = model.Calories.AsOptional(),
            FatGrams = model.FatGrams.AsOptional(),
            CarbGrams = model.CarbGrams.AsOptional(),
            ProteinGrams = model.ProteinGrams.AsOptional(),
            PackageSizeQuantity = model.PackageSizeQuantity.AsOptional(),
            PackageSizeUnit = model.PackageSizeUnit.AsOptional(),
            PackageCostDollars = model.PackageCostDollars.AsOptional(),
            Notes = model.Notes
        };

        public IngredientDataModel AsDataModel() => new()
        {
            Name = Name,
            Brand = Brand.Unwrap(),
            Vendor = Vendor.Unwrap(),
            ServingSizeQuantity = ServingSizeQuantity.Unwrap(),
            ServingSizeUnit = ServingSizeUnit.Unwrap(),
            AlternateServingSizeQuantity = AlternateServingSizeQuantity.Unwrap(),
            AlternateServingSizeUnit = AlternateServingSizeUnit.Unwrap(),
            Calories = Calories.Unwrap(),
            FatGrams = FatGrams.Unwrap(),
            CarbGrams = CarbGrams.Unwrap(),
            ProteinGrams = ProteinGrams.Unwrap(),
            PackageSizeQuantity = PackageSizeQuantity.Unwrap(),
            PackageSizeUnit = PackageSizeUnit.Unwrap(),
            PackageCostDollars = PackageCostDollars.Unwrap(),
            Notes = Notes
        };

        private async Task<(Result<decimal> ServingsPerPackage, Result<decimal> CostPerServing)> ComputeCostPerServing(IUnitConversionsService unitConversionsService)
        {
            if (!(PackageSizeQuantity.TryGetValue(out var packageSize)
                && PackageSizeUnit.TryGetValue(out var packageSizeUnit))
                || packageSize <= 0)
                return new();

            Result<decimal> servingsPerPackage;
            if (ServingSizeQuantity.TryGetValue(out var servingSizeQuantity)
                && servingSizeQuantity > 0
                && ServingSizeUnit.TryGetValue(out var servingSizeUnit)
                && (await unitConversionsService.ConvertAsync(servingSizeUnit, packageSizeUnit, servingSizeQuantity))
                    .AsOptional()
                    .TryGetValue(out var converted))
                servingsPerPackage = packageSize / converted;
            else if (AlternateServingSizeQuantity.TryGetValue(out var alternateServingSizeQuantity)
                && alternateServingSizeQuantity > 0
                && AlternateServingSizeUnit.TryGetValue(out var alternateServingSizeUnit)
                && (await unitConversionsService.ConvertAsync(alternateServingSizeUnit, packageSizeUnit, alternateServingSizeQuantity))
                    .AsOptional()
                    .TryGetValue(out var alternateConverted))
                servingsPerPackage = packageSize / alternateConverted;
            else
                return new();

            if (!PackageCostDollars.TryGetValue(out var packageCost)
                || servingsPerPackage.Value <= 0)
                return (servingsPerPackage, new());

            return new(servingsPerPackage, packageCost / servingsPerPackage.Value);
        }

        public async Task<IngredientInsightsModel> GetInsightsAsync(IUnitConversionsService unitConversionsService)
        {
            var model = new IngredientInsightsModel();
            if (ProteinGrams.TryGetValue(out var proteinGrams)
                && FatGrams.TryGetValue(out var fatGrams)
                && CarbGrams.TryGetValue(out var carbGrams))
            {
                var totalMacronutrientGrams = proteinGrams + fatGrams + carbGrams;
                if (totalMacronutrientGrams > 0)
                    model.MacroNutrientBreakdown = new MacroNutrientBreakdownInsightsModel
                    (
                        ProportionProtein: ((double)proteinGrams / (double)totalMacronutrientGrams),
                        ProportionFat: ((double)fatGrams / (double)totalMacronutrientGrams),
                        ProportionCarbs: ((double)carbGrams / (double)totalMacronutrientGrams)
                    );
            }

            // Macro efficiency calculations - populate as many values as possible
            var macroEfficiencyBuilder = new MacroEfficiencyInsightsModel();
            var costAnalysisBuilder = new CostAnalysisInsightsModel();

            // Calculate cost per serving if possible
            var (servingsPerPackage, costPerServing) = await ComputeCostPerServing(unitConversionsService);
            if (servingsPerPackage.IsSuccessful)
                costAnalysisBuilder = costAnalysisBuilder with
                {
                    ServingsPerPackage = servingsPerPackage.Value,
                };
            if (costPerServing.IsSuccessful)
                costAnalysisBuilder = costAnalysisBuilder with
                {
                    CostPerServing = costPerServing.Value
                };
            // Calculate calories per dollar
            if (Calories.TryGetValue(out var calories) && costPerServing.TryGetValue(out var cost) && calories > 0 && cost > 0)
            {
                costAnalysisBuilder = costAnalysisBuilder with { CaloriesPerDollar = calories / cost };
            }

            // Calculate macro per dollar values
            if (costPerServing.TryGetValue(out var costPerServingValue) && costPerServingValue > 0)
            {
                if (ProteinGrams.TryGetValue(out var protein))
                    macroEfficiencyBuilder = macroEfficiencyBuilder with { ProteinPerDollar = protein / costPerServingValue };

                if (FatGrams.TryGetValue(out var fat))
                    macroEfficiencyBuilder = macroEfficiencyBuilder with { FatPerDollar = fat / costPerServingValue };

                if (CarbGrams.TryGetValue(out var carbs))
                    macroEfficiencyBuilder = macroEfficiencyBuilder with { CarbsPerDollar = carbs / costPerServingValue };
            }

            // Calculate macro per calorie values
            if (Calories.TryGetValue(out var caloriesForMacro) && caloriesForMacro > 0)
            {
                if (ProteinGrams.TryGetValue(out var protein))
                    macroEfficiencyBuilder = macroEfficiencyBuilder with { ProteinPerCalorie = protein / caloriesForMacro };

                if (FatGrams.TryGetValue(out var fat))
                    macroEfficiencyBuilder = macroEfficiencyBuilder with { FatPerCalorie = fat / caloriesForMacro };

                if (CarbGrams.TryGetValue(out var carbs))
                    macroEfficiencyBuilder = macroEfficiencyBuilder with { CarbsPerCalorie = carbs / caloriesForMacro };
            }

            // Calculate protein score if we have both protein per dollar and protein per calorie
            if (macroEfficiencyBuilder.ProteinPerDollar.TryGetValue(out var proteinPerDollar)
                && macroEfficiencyBuilder.ProteinPerCalorie.TryGetValue(out var proteinPerCalorie))
            {
                model.ProteinScore = proteinPerDollar * proteinPerCalorie;
            }

            model.MacroEfficiency = macroEfficiencyBuilder;
            model.CostAnalysis = costAnalysisBuilder;

            return model;
        }
    }

}
