using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Hestia.Core.Models;
using Hestia.Domain.Services;
using Hestia.Persistence.Models;
using System.ComponentModel.DataAnnotations;

namespace Hestia.Domain.Models
{
    public record MealPlanModel
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        public required AbsoluteDateTime LastModified { get; set; }

        public List<MealPlanSectionModel> Sections { get; set; } = [];

        public static MealPlanModel FromDataModel(MealPlanDataModel model) => new()
        {
            Name = model.Name,
            LastModified = model.LastModified,
            Sections = model.Sections
                .OrderBy(s => s.Order)
                .Select(MealPlanSectionModel.FromDataModel)
                .ToList(),
        };

        public MealPlanDataModel AsDataModel() => new()
        {
            Name = Name,
            NormalizedName = NormalizedString.Create(Name),
            LastModified = LastModified,
            Sections = Sections.Select((section, index) => section.AsDataModel(index)).ToList()
        };
        public void ApplyUpdate(MealPlanDataModel model)
        {
            model.Name = Name;
            model.NormalizedName = NormalizedString.Create(Name);
            model.LastModified = AbsoluteDateTime.Now;
            model.Sections = Sections.Select((section, index) => section.AsDataModel(index)).ToList();
        }

        public async Task<(MealPlanInsightsModel Insights, List<(string Title, List<string> Warnings)> Warnings)> GetInsightsAsync(
            IUnitConversionsService unitConversionsService,
            IIngredientsService ingredientsService,
            IRecipesService recipesService)
        {
            double totalCalories = 0;
            double totalProtein = 0;
            double totalFat = 0;
            double totalCarbs = 0;
            decimal totalCost = 0;
            var warnings = new List<(string Title, List<string> Warnings)>();

            foreach (var section in Sections)
            {
                foreach (var item in section.Items)
                {
                    if (item.ItemType == Persistence.Models.MealItemType.Recipe)
                    {
                        var recipeResult = await recipesService.GetRecipeAsync(item.RecipeOrIngredientId);
                        if (recipeResult.TryGetValue(out var recipe))
                        {
                            var (recipeInsights, recipeWarnings) = await recipe.GetInsightsAsync(unitConversionsService, ingredientsService);

                            if (!item.Unit.TryGetValue(out var unit) || unit == "")
                            {
                                // assume unit to be # of servings
                                if (recipeInsights.ServingInsights.TryGetValue(out var servingInsights))
                                {
                                    totalCalories += servingInsights.CaloriesPerServing * (double)item.Quantity;
                                    totalProtein += servingInsights.ProteinGramsPerServing * (double)item.Quantity;
                                    totalFat += servingInsights.FatGramsPerServing * (double)item.Quantity;
                                    totalCarbs += servingInsights.CarbGramsPerServing * (double)item.Quantity;
                                    totalCost += recipeInsights.CostAnalysis.CostPerServing * item.Quantity;
                                    warnings.Add((item.ItemName, recipeWarnings.SelectMany(w => w.Warnings.Select(w2 => $"{w.Title} - {w2}")).ToList()));
                                }
                                // assume to be just 1x recipe
                                else if (recipeInsights.TotalInsights.TryGetValue(out var totalInsights))
                                {
                                    totalCalories += totalInsights.Calories * (double)item.Quantity;
                                    totalProtein += totalInsights.ProteinGrams * (double)item.Quantity;
                                    totalFat += totalInsights.FatGrams * (double)item.Quantity;
                                    totalCarbs += totalInsights.CarbGrams * (double)item.Quantity;
                                    totalCost += totalInsights.Cost;
                                    warnings.Add((item.ItemName, recipeWarnings.SelectMany(w => w.Warnings.Select(w2 => $"{w.Title} - {w2}")).ToList()));
                                }
                                else
                                {
                                    warnings.Add((item.ItemName, ["Unable to retrieve recipe insights."]));
                                }
                            }
                            else
                            {
                                if (recipe.YieldUnit.TryGetValue(out var yieldUnit) && recipe.YieldQuantity.TryGetValue(out var yieldQuantity))
                                {
                                    var multiplier = (await unitConversionsService.ConvertAsync(yieldUnit, unit, 1))
                                        .AsOptional()
                                        .Map(m => item.Quantity / (m * yieldQuantity));

                                    if (multiplier.TryGetValue(out var multiplierValue))
                                    {

                                        if (recipeInsights.TotalInsights.TryGetValue(out var totalInsights))
                                        {
                                            totalCalories += totalInsights.Calories * (double)multiplierValue;
                                            totalProtein += totalInsights.ProteinGrams * (double)multiplierValue;
                                            totalFat += totalInsights.FatGrams * (double)multiplierValue;
                                            totalCarbs += totalInsights.CarbGrams * (double)multiplierValue;
                                            totalCost += totalInsights.Cost * multiplierValue;
                                            warnings.Add((item.ItemName, recipeWarnings.SelectMany(w => w.Warnings.Select(w2 => $"{w.Title} - {w2}")).ToList()));
                                        }
                                        else
                                        {
                                            warnings.Add((item.ItemName, ["Unable to retrieve recipe insights."]));
                                        }
                                    }
                                    else
                                    {
                                        warnings.Add((item.ItemName, [$"Could not convert from recipe yield unit ({yieldUnit}) to meal plan unit ({unit})."]));
                                    }
                                }
                                else
                                {
                                    warnings.Add((item.ItemName, ["Recipe does not specify a yield unit and/or quantity"]));
                                }
                            }

                        }
                        else
                        {
                            warnings.Add((item.ItemName, [$"Recipe with id {item.RecipeOrIngredientId} not found."]));
                        }
                    }
                    else if (item.ItemType == Persistence.Models.MealItemType.Ingredient)
                    {
                        var ingredientResult = await ingredientsService.GetIngredientAsync(item.RecipeOrIngredientId);
                        if (ingredientResult.TryGetValue(out var ingredient))
                        {
                            var itemWarnings = new List<string>();

                            if (!item.Unit.TryGetValue(out var unit) || unit == "")
                            {
                                // assume unit to be # of servings
                                if (ingredient.ServingSizeQuantity.TryGetValue(out var servingSizeQuantity) && servingSizeQuantity > 0)
                                {
                                    var servingMultiplier = (double)item.Quantity;

                                    if (ingredient.Calories.TryGetValue(out var calories))
                                        totalCalories += (double)calories * servingMultiplier;
                                    if (ingredient.ProteinGrams.TryGetValue(out var protein))
                                        totalProtein += (double)protein * servingMultiplier;
                                    if (ingredient.FatGrams.TryGetValue(out var fat))
                                        totalFat += (double)fat * servingMultiplier;
                                    if (ingredient.CarbGrams.TryGetValue(out var carbs))
                                        totalCarbs += (double)carbs * servingMultiplier;

                                    var ingredientInsights = await ingredient.GetInsightsAsync(unitConversionsService);
                                    if (ingredientInsights.CostAnalysis.CostPerServing.TryGetValue(out var costPerServing))
                                        totalCost += costPerServing * item.Quantity;
                                }
                                else
                                {
                                    itemWarnings.Add("Ingredient does not specify a serving size quantity");
                                }
                            }
                            else
                            {
                                // Convert from meal plan unit to ingredient's serving unit
                                if (ingredient.ServingSizeQuantity.TryGetValue(out var servingSizeQuantity) && servingSizeQuantity > 0
                                    && ingredient.ServingSizeUnit.TryGetValue(out var servingSizeUnit))
                                {
                                    var conversionResult = await unitConversionsService.ConvertAsync(unit, servingSizeUnit, item.Quantity);
                                    if (conversionResult.TryGetValue(out var convertedQuantity))
                                    {
                                        var servingMultiplier = (double)(convertedQuantity / servingSizeQuantity);

                                        if (ingredient.Calories.TryGetValue(out var calories))
                                            totalCalories += (double)calories * servingMultiplier;
                                        if (ingredient.ProteinGrams.TryGetValue(out var protein))
                                            totalProtein += (double)protein * servingMultiplier;
                                        if (ingredient.FatGrams.TryGetValue(out var fat))
                                            totalFat += (double)fat * servingMultiplier;
                                        if (ingredient.CarbGrams.TryGetValue(out var carbs))
                                            totalCarbs += (double)carbs * servingMultiplier;

                                        var ingredientInsights = await ingredient.GetInsightsAsync(unitConversionsService);
                                        if (ingredientInsights.CostAnalysis.CostPerServing.TryGetValue(out var costPerServing))
                                            totalCost += costPerServing * (decimal)servingMultiplier;
                                    }
                                    else if (ingredient.AlternateServingSizeQuantity.TryGetValue(out var altServingSizeQuantity) && altServingSizeQuantity > 0
                                        && ingredient.AlternateServingSizeUnit.TryGetValue(out var altServingSizeUnit))
                                    {
                                        // Try alternate serving size
                                        var altConversionResult = await unitConversionsService.ConvertAsync(unit, altServingSizeUnit, item.Quantity);
                                        if (altConversionResult.TryGetValue(out var altConvertedQuantity))
                                        {
                                            var servingMultiplier = (double)(altConvertedQuantity / altServingSizeQuantity);

                                            if (ingredient.Calories.TryGetValue(out var calories))
                                                totalCalories += (double)calories * servingMultiplier;
                                            if (ingredient.ProteinGrams.TryGetValue(out var protein))
                                                totalProtein += (double)protein * servingMultiplier;
                                            if (ingredient.FatGrams.TryGetValue(out var fat))
                                                totalFat += (double)fat * servingMultiplier;
                                            if (ingredient.CarbGrams.TryGetValue(out var carbs))
                                                totalCarbs += (double)carbs * servingMultiplier;

                                            var ingredientInsights = await ingredient.GetInsightsAsync(unitConversionsService);
                                            if (ingredientInsights.CostAnalysis.CostPerServing.TryGetValue(out var costPerServing))
                                                totalCost += costPerServing * (decimal)servingMultiplier;
                                        }
                                        else
                                        {
                                            itemWarnings.Add($"Could not convert from meal plan unit ({unit}) to ingredient serving unit ({servingSizeUnit}) or alternate serving unit ({altServingSizeUnit})");
                                        }
                                    }
                                    else
                                    {
                                        itemWarnings.Add($"Could not convert from meal plan unit ({unit}) to ingredient serving unit ({servingSizeUnit})");
                                    }
                                }
                                else
                                {
                                    itemWarnings.Add("Ingredient does not specify a serving size unit and/or quantity");
                                }
                            }

                            if (itemWarnings.Any())
                                warnings.Add((item.ItemName, itemWarnings));
                        }
                        else
                        {
                            warnings.Add((item.ItemName, [$"Ingredient with id {item.RecipeOrIngredientId} not found."]));
                        }
                    }
                }
            }

            var insights = new MealPlanInsightsModel
            {
                TotalCalories = totalCalories,
                TotalProtein = totalProtein,
                TotalFat = totalFat,
                TotalCarbs = totalCarbs,
                TotalCost = totalCost
            };

            return (insights, warnings);
        }
    }
}