using Haondt.Core.Extensions;
using Hestia.Domain.Attributes;
using Hestia.Persistence.Models;

namespace Hestia.Domain.Models
{
    public class FoodLogModel
    {
        [ValidDateString]
        public required string DateString { get; set; }
        public List<MealPlanSectionModel> Sections { get; set; } = [];
        public int? MealPlanId { get; set; }

        public static FoodLogModel FromDataModel(FoodLogDataModel model) => new()
        {
            DateString = model.DateString,
            Sections = model.Sections
                .OrderBy(s => s.Order)
                .Select(MealPlanSectionModel.FromDataModel)
                .ToList(),
            MealPlanId = model.MealPlanId
        };

        public FoodLogDataModel AsDataModel() => new()
        {
            DateString = DateString,
            MealPlanId = MealPlanId,
            Sections = Sections.Select((section, index) => section.AsFoodLogDataModel(index)).ToList()
        };

        public void ApplyUpdate(FoodLogDataModel model)
        {
            model.DateString = DateString;
            model.Sections = Sections.Select((section, index) => section.AsFoodLogDataModel(index)).ToList();
            model.MealPlanId = MealPlanId;
        }

        public FoodLogDiffModel CalculateDiff(MealPlanModel? mealPlan)
        {
            var diff = new FoodLogDiffModel
            {
                DateString = DateString,
                MealPlanId = MealPlanId,
                MealPlanName = mealPlan?.Name
            };

            if (mealPlan == null)
            {
                // No meal plan to compare against - everything is "only in log"
                diff.Sections = Sections.Select(section => new SectionDiffModel
                {
                    Name = section.Name,
                    Items = section.Items.Select(item => new ItemDiffModel
                    {
                        ItemName = item.ItemName,
                        RecipeOrIngredientId = item.RecipeOrIngredientId,
                        DiffType = ItemDiffType.OnlyInLog,
                        ActualQuantity = item.Quantity,
                        ActualUnit = item.Unit.Unwrap()
                    }).ToList()
                }).ToList();
                return diff;
            }

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
                var sectionDiff = new SectionDiffModel { Name = sectionName };
                
                var plannedSectionItems = plannedItems.GetValueOrDefault(sectionName, new());
                var loggedSectionItems = loggedItems.GetValueOrDefault(sectionName, new());
                
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
                    }
                    else if (plannedItem != null)
                    {
                        diffType = ItemDiffType.OnlyInPlan;
                    }
                    else
                    {
                        diffType = ItemDiffType.OnlyInLog;
                    }

                    sectionDiff.Items.Add(new ItemDiffModel
                    {
                        ItemName = (plannedItem ?? loggedItem)!.ItemName,
                        RecipeOrIngredientId = itemKey.id,
                        DiffType = diffType,
                        PlannedQuantity = plannedItem?.Quantity,
                        PlannedUnit = plannedItem?.Unit.Unwrap(),
                        ActualQuantity = loggedItem?.Quantity,
                        ActualUnit = loggedItem?.Unit.Unwrap()
                    });
                }

                diff.Sections.Add(sectionDiff);
            }

            return diff;
        }
    }

}