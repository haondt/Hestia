using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Hestia.Domain.Attributes;
using Hestia.Domain.Services;
using Hestia.Persistence.Models;

namespace Hestia.Domain.Models
{

    public class FoodLogModel
    {
        [ValidDateString]
        public required string DateString { get; set; }
        public List<MealPlanSectionModel> Sections { get; set; } = [];
        public Optional<int> MealPlanId { get; set; }

        public static FoodLogModel FromDataModel(FoodLogDataModel model) => new()
        {
            DateString = model.DateString,
            Sections = model.Sections
                .OrderBy(s => s.Order)
                .Select(MealPlanSectionModel.FromDataModel)
                .ToList(),
            MealPlanId = model.MealPlanId.AsOptional()
        };

        public FoodLogDataModel AsDataModel() => new()
        {
            DateString = DateString,
            MealPlanId = MealPlanId.Unwrap(),
            Sections = Sections.Select((section, index) => section.AsFoodLogDataModel(index)).ToList()
        };

        public void ApplyUpdate(FoodLogDataModel model)
        {
            model.DateString = DateString;
            model.Sections = Sections.Select((section, index) => section.AsFoodLogDataModel(index)).ToList();
            model.MealPlanId = MealPlanId.Unwrap();
        }

        public FoodLogDiffModel CalculateDiff(int mealPlanId, MealPlanModel mealPlan)
        {
            var diff = new FoodLogDiffModel
            {
                DateString = DateString,
                MealPlanId = mealPlanId,
                MealPlanName = mealPlan.Name
            };

            // Create a lookup for meal plan items by section and item identity
            var plannedItems = new Dictionary<string, Dictionary<(int id, string name), MealPlanItemModel>>();
            foreach (var section in mealPlan.Sections)
            {
                plannedItems[section.Name] = section.Items.ToDictionary(
                    item => (item.RecipeOrIngredientId, item.ItemName),
                    item => item);
            }

            // Create a lookup for actual logged items
            var loggedItems = new Dictionary<string, Dictionary<(int id, string name), MealPlanItemModel>>();
            foreach (var section in Sections)
            {
                loggedItems[section.Name] = section.Items.ToDictionary(
                    item => (item.RecipeOrIngredientId, item.ItemName),
                    item => item);
            }

            // Get all section names from both plans
            var allSectionNames = plannedItems.Keys.Union(loggedItems.Keys).ToList();

            foreach (var sectionName in allSectionNames)
            {
                var sectionDiff = new SectionDiffModel { Name = sectionName, DiffType = ItemDiffType.Matched };

                var plannedSectionItems = plannedItems.GetValueOrDefault(sectionName, new());
                var loggedSectionItems = loggedItems.GetValueOrDefault(sectionName, new());

                if (plannedItems.ContainsKey(sectionName) && loggedItems.ContainsKey(sectionName))
                    sectionDiff.DiffType = ItemDiffType.Matched;
                else if (plannedItems.ContainsKey(sectionName))
                    sectionDiff.DiffType = ItemDiffType.OnlyInPlan;
                else
                    sectionDiff.DiffType = ItemDiffType.OnlyInLog;

                // Get all item keys from both sections
                var allItemKeys = plannedSectionItems.Keys.Union(loggedSectionItems.Keys).ToList();

                foreach (var itemKey in allItemKeys)
                {
                    var plannedItem = plannedSectionItems.GetValueOrDefault(itemKey);
                    var loggedItem = loggedSectionItems.GetValueOrDefault(itemKey);

                    ItemDiffType diffType;
                    if (plannedItem != null && loggedItem != null)
                    {
                        // Item exists in both - check if modified
                        var hasQuantityDiff = plannedItem.Quantity != loggedItem.Quantity;
                        var hasUnitDiff = plannedItem.Unit.Unwrap() != loggedItem.Unit.Unwrap();
                        diffType = hasQuantityDiff || hasUnitDiff ? ItemDiffType.Modified : ItemDiffType.Matched;
                        if (diffType == ItemDiffType.Modified)
                            sectionDiff.DiffType = ItemDiffType.Modified;
                    }
                    else if (plannedItem != null)
                    {
                        diffType = ItemDiffType.OnlyInPlan;
                        sectionDiff.DiffType = ItemDiffType.Modified;
                    }
                    else
                    {
                        diffType = ItemDiffType.OnlyInLog;
                        sectionDiff.DiffType = ItemDiffType.Modified;
                    }

                    sectionDiff.Items.Add(new ItemDiffModel
                    {
                        ItemName = (plannedItem ?? loggedItem)!.ItemName,
                        RecipeOrIngredientId = itemKey.id,
                        DiffType = diffType,
                        PlannedQuantity = plannedItem?.Quantity ?? 0,
                        PlannedUnit = plannedItem.AsOptional().Bind(q => q.Unit),
                        ActualQuantity = loggedItem?.Quantity ?? 0,
                        ActualUnit = loggedItem.AsOptional().Bind(q => q.Unit)
                    });
                }

                diff.Sections.Add(sectionDiff);
            }

            return diff;
        }

        public async Task<(FoodLogInsightsModel Insights, List<(string Title, List<string> Warnings)> Warnings)> GetInsightsAsync(
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
                                        warnings.Add((item.ItemName, [$"Could not convert from recipe yield unit ({yieldUnit}) to food log unit ({unit})."]));
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
                                // Convert from food log unit to ingredient's serving unit
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
                                            itemWarnings.Add($"Could not convert from food log unit ({unit}) to ingredient serving unit ({servingSizeUnit}) or alternate serving unit ({altServingSizeUnit})");
                                        }
                                    }
                                    else
                                    {
                                        itemWarnings.Add($"Could not convert from food log unit ({unit}) to ingredient serving unit ({servingSizeUnit})");
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

            var insights = new FoodLogInsightsModel
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