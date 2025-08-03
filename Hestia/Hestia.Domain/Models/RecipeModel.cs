using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Hestia.Core.Models;
using Hestia.Domain.Services;
using Hestia.Persistence.Models;
using System.ComponentModel.DataAnnotations;

namespace Hestia.Domain.Models
{
    public record RecipeModel
    {
        [Required]
        public required string Title { get; set; }

        public required Optional<string> Description { get; set; }

        public required Optional<decimal> YieldQuantity { get; set; }

        public required Optional<string> YieldUnit { get; set; }

        public required Optional<int> NumberOfServings { get; set; }

        public required Optional<double> PrepTimeQuantity { get; set; }

        public required Optional<string> PrepTimeUnit { get; set; }

        public required Optional<double> CookTimeQuantity { get; set; }

        public required Optional<string> CookTimeUnit { get; set; }

        public required Optional<string> Instructions { get; set; }

        public List<RecipeIngredientModel> Ingredients { get; set; } = [];

        public static RecipeModel FromDataModel(RecipeDataModel model) => new()
        {
            Title = model.Title,
            Description = model.Description.AsOptional(),
            YieldQuantity = model.YieldQuantity.AsOptional(),
            YieldUnit = model.YieldUnit.AsOptional(),
            NumberOfServings = model.NumberOfServings.AsOptional(),
            PrepTimeQuantity = model.PrepTimeQuantity.AsOptional(),
            PrepTimeUnit = model.PrepTimeUnit.AsOptional(),
            CookTimeQuantity = model.CookTimeQuantity.AsOptional(),
            CookTimeUnit = model.CookTimeUnit.AsOptional(),
            Instructions = model.Instructions.AsOptional(),
            Ingredients = model.Ingredients.Select(RecipeIngredientModel.FromDataModel).ToList()
        };

        public RecipeDataModel AsDataModel() => new()
        {
            Title = Title,
            NormalizedTitle = NormalizedString.Create(Title),
            Description = Description.Unwrap(),
            NormalizedDescription = Description.Map(NormalizedString.Create).Unwrap(),
            YieldQuantity = YieldQuantity.Unwrap(),
            YieldUnit = YieldUnit.Unwrap(),
            NumberOfServings = NumberOfServings.Unwrap(),
            PrepTimeQuantity = PrepTimeQuantity.Unwrap(),
            PrepTimeUnit = PrepTimeUnit.Unwrap(),
            CookTimeQuantity = CookTimeQuantity.Unwrap(),
            CookTimeUnit = CookTimeUnit.Unwrap(),
            Instructions = Instructions.Unwrap(),
            Ingredients = Ingredients.Select(i => i.AsDataModel()).ToList()
        };
        public void ApplyUpdate(RecipeDataModel model)
        {
            model.Title = Title;
            model.NormalizedTitle = NormalizedString.Create(Title);
            model.Description = Description.Unwrap();
            model.NormalizedDescription = Description.Map(NormalizedString.Create).Unwrap();
            model.YieldQuantity = YieldQuantity.Unwrap();
            model.YieldUnit = YieldUnit.Unwrap();
            model.NumberOfServings = NumberOfServings.Unwrap();
            model.PrepTimeQuantity = PrepTimeQuantity.Unwrap();
            model.PrepTimeUnit = PrepTimeUnit.Unwrap();
            model.CookTimeQuantity = CookTimeQuantity.Unwrap();
            model.CookTimeUnit = CookTimeUnit.Unwrap();
            model.Instructions = Instructions.Unwrap();
            model.Ingredients = Ingredients.Select(i => i.AsDataModel()).ToList();
        }
        public async Task<(RecipeInsightsModel insights, List<(string Title, List<string> Warnings)> Warnings)> GetInsightsAsync(IUnitConversionsService unitConversion, IIngredientsService ingredientsService)
        {
            List<(string Title, List<string> Warnings)> warnings = Ingredients.Select(i => (i.IngredientName.Or(i.Name), new List<string>())).ToList();
            var insights = new RecipeInsightsModel();
            var totalProteinGrams = 0m;
            var totalFatGrams = 0m;
            var totalCarbGrams = 0m;
            var totalCost = 0m;
            var totalCalories = 0m;

            for (int i = 0; i < Ingredients.Count; i++)
            {
                var ingredient = Ingredients[i];
                if (!ingredient.IngredientId.TryGetValue(out var ingredientId))
                {
                    warnings[i].Warnings.Add($"Ingredient is orphaned and was be excluded from calculations.");
                    continue;
                }

                var ingredientModelResult = await ingredientsService.GetIngredientAsync(ingredientId);
                if (!ingredientModelResult.TryGetValue(out var ingredientModel))
                {
                    warnings[i].Warnings.Add($"Ingredient ID {ingredientId} not found and was excluded from calculations.");
                    continue;
                }

                if (!ingredient.Unit.TryGetValue(out var recipeIngredientUnit))
                {
                    warnings[i].Warnings.Add("Recipe does not have units for this ingredient.");
                    continue;
                }
                if (!ingredient.Quantity.TryGetValue(out var recipeIngredientQuantity))
                {
                    warnings[i].Warnings.Add("Recipe does not have a quantity for this ingredient.");
                    continue;
                }

                // this number  times the ingredient values will give how much of those are in the recipe (the whole recipe, not per recipe serving)
                var numberOfIngredientServingsInRecipe = 0m;
                bool couldConvert = false;
                var availableUnits = new List<string>();
                if (ingredientModel.ServingSizeUnit.TryGetValue(out var ingredientServingSizeUnit)
                    && ingredientModel.ServingSizeQuantity.TryGetValue(out var ingredientServingSizeQuantity))
                {
                    availableUnits.Add(ingredientServingSizeUnit);
                    var recipeIngredientQuantityInIngredientServingSizeUnitsResult = await unitConversion.ConvertAsync(recipeIngredientUnit, ingredientServingSizeUnit, recipeIngredientQuantity);
                    if (recipeIngredientQuantityInIngredientServingSizeUnitsResult.TryGetValue(out var recipeIngredientQuantityInIngredientServingSizeUnits))
                    {
                        couldConvert = true;
                        numberOfIngredientServingsInRecipe = recipeIngredientQuantityInIngredientServingSizeUnits / ingredientServingSizeQuantity;
                    }
                }
                if (!couldConvert
                    && ingredientModel.AlternateServingSizeUnit.TryGetValue(out var ingredientAlternateServingSizeUnit)
                    && ingredientModel.AlternateServingSizeQuantity.TryGetValue(out var ingredientAlternateServingSizeQuantity))
                {
                    availableUnits.Add(ingredientAlternateServingSizeUnit);
                    var recipeIngredientQuantityInIngredientAlternateServingSizeUnitsResult = await unitConversion.ConvertAsync(recipeIngredientUnit, ingredientAlternateServingSizeUnit, recipeIngredientQuantity);
                    if (recipeIngredientQuantityInIngredientAlternateServingSizeUnitsResult.TryGetValue(out var recipeIngredientQuantityInIngredientAlternateServingSizeUnits))
                    {
                        couldConvert = true;
                        numberOfIngredientServingsInRecipe = recipeIngredientQuantityInIngredientAlternateServingSizeUnits / ingredientAlternateServingSizeQuantity;
                    }
                }
                if (!couldConvert)
                {
                    var availableUnitsString = availableUnits.Count > 0 ? string.Join(", ", availableUnits) : "none";
                    warnings[i].Warnings.Add($"Could not convert recipe ingredient unit '{recipeIngredientUnit}' to ingredient serving size units ({availableUnitsString}).");
                    continue;
                }

                if (ingredientModel.ProteinGrams.TryGetValue(out var ingredientProteinGrams))
                {
                    totalProteinGrams += ingredientProteinGrams * numberOfIngredientServingsInRecipe;
                }
                else
                {
                    warnings[i].Warnings.Add("Ingredient does not have protein grams defined, value of 0 was used.");
                }

                if (ingredientModel.FatGrams.TryGetValue(out var ingredientFatGrams))
                {
                    totalFatGrams += ingredientFatGrams * numberOfIngredientServingsInRecipe;
                }
                else
                {
                    warnings[i].Warnings.Add("Ingredient does not have fat grams defined, value of 0 was used.");
                }

                if (ingredientModel.CarbGrams.TryGetValue(out var ingredientCarbGrams))
                {
                    totalCarbGrams += ingredientCarbGrams * numberOfIngredientServingsInRecipe;
                }
                else
                {
                    warnings[i].Warnings.Add("Ingredient does not have carb grams defined, value of 0 was used.");
                }


                var ingredientInsights = await ingredientModel.GetInsightsAsync(unitConversion);
                if (ingredientInsights.CostAnalysis.CostPerServing.TryGetValue(out var ingredientCostPerserving))
                    totalCost += ingredientCostPerserving * numberOfIngredientServingsInRecipe;
                else
                    warnings[i].Warnings.Add("Not enough information to determine ingredient cost, value of 0 was used.");

                if (ingredientModel.Calories.TryGetValue(out var ingredientCalories))
                    totalCalories += ingredientCalories * numberOfIngredientServingsInRecipe;
                else
                    warnings[i].Warnings.Add("Ingredient does not have calories defined, value of 0 was used.");

            }


            var totalMacronutrientGrams = totalProteinGrams + totalFatGrams + totalCarbGrams;
            if (totalMacronutrientGrams > 0)
            {
                insights.MacroNutrientBreakdown = new(new(
                    ProportionProtein: (double)(totalProteinGrams / totalMacronutrientGrams),
                    ProportionFat: (double)(totalFatGrams / totalMacronutrientGrams),
                    ProportionCarbs: (double)(totalCarbGrams / totalMacronutrientGrams)));
            }

            if (totalCost > 0)
            {
                insights.MacroEfficiency = new(
                    ProteinPerDollar: totalProteinGrams / totalCost,
                    FatPerDollar: totalFatGrams / totalCost,
                    CarbsPerDollar: totalCarbGrams / totalCost);

                insights.CostAnalysis = insights.CostAnalysis with
                {
                    CaloriesPerDollar = totalCalories / totalCost,
                };
            }

            if (totalCalories > 0)
            {
                insights.MacroEfficiency = insights.MacroEfficiency with
                {
                    ProteinPerCalorie = totalProteinGrams / totalCalories,
                    FatPerCalorie = totalFatGrams / totalCalories,
                    CarbsPerCalorie = totalCarbGrams / totalCalories
                };
            }

            if (totalCost > 0 && totalCalories > 0)
                insights.ProteinScore = (totalProteinGrams / totalCost) * (totalProteinGrams / totalCalories);

            var numberOfServings = NumberOfServings.Or(1);
            if (numberOfServings <= 0)
                numberOfServings = 1;
            insights.CostAnalysis = insights.CostAnalysis with
            {
                CostPerServing = totalCost / numberOfServings,
            };
            insights.ServingInsights = new(new(
                CaloriesPerServing: (double)totalCalories / numberOfServings,
                ProteinGramsPerServing: (double)totalProteinGrams / numberOfServings,
                FatGramsPerServing: (double)totalFatGrams / numberOfServings,
                CarbGramsPerServing: (double)totalCarbGrams / numberOfServings));

            warnings = warnings
                .Where(w => w.Warnings.Count > 0)
                .Select((w, i) => (i, w.Title, w.Warnings))
                .GroupBy(t =>
                {
                    var ingredient = Ingredients[t.i];
                    if (ingredient.IngredientId.TryGetValue(out var id))
                        return (id, "");
                    else
                        return (-1, ingredient.Name);
                })
                .Select(grp =>
                {
                    if (grp.Key.Item1 == -1)
                        return (grp.Key.Item2, grp.SelectMany(t => t.Warnings).Distinct().ToList());
                    return (grp.First().Title, grp.SelectMany(t => t.Warnings).Distinct().ToList());
                }).ToList();

            return (insights, warnings);
        }
    }
}