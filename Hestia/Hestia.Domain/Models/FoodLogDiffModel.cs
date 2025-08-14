using Haondt.Core.Models;

namespace Hestia.Domain.Models
{
    public record FoodLogDiffModel
    {
        public required string DateString { get; set; }
        public required string MealPlanName { get; set; }
        public required int MealPlanId { get; set; }
        public List<SectionDiffModel> Sections { get; set; } = [];
    }

    public record SectionDiffModel
    {
        public required ItemDiffType DiffType { get; set; }
        public required string Name { get; set; }
        public List<ItemDiffModel> Items { get; set; } = [];
    }

    public record ItemDiffModel
    {
        public required string ItemName { get; set; }
        public required int RecipeOrIngredientId { get; set; }
        public required ItemDiffType DiffType { get; set; }

        // Planned values (from meal plan)
        public required decimal PlannedQuantity { get; set; }
        public Optional<string> PlannedUnit { get; set; }

        // Actual values (from food log)
        public required decimal ActualQuantity { get; set; }
        public Optional<string> ActualUnit { get; set; }

        // Calculated difference
        public decimal QuantityDifference => ActualQuantity - PlannedQuantity;
        public bool HasQuantityDifference => QuantityDifference != 0;
        public bool HasUnitDifference => !PlannedUnit.Equals(ActualUnit);
    }

    public enum ItemDiffType
    {
        Matched,        // Item exists in both plan and log
        OnlyInPlan,     // Item was planned but not logged
        OnlyInLog,      // Item was logged but not planned
        Modified        // Item exists in both but with different quantities/units
    }
}