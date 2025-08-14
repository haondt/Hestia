using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Hestia.Domain.Attributes;
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
    }

}