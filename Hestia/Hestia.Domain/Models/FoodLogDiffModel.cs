using System.ComponentModel.DataAnnotations;

namespace Hestia.Domain.Models
{
    public record FoodLogDiffModel
    {
        public required string DateString { get; set; }
        public int? MealPlanId { get; set; }
        public string? MealPlanName { get; set; }
        public List<SectionDiffModel> Sections { get; set; } = [];
    }

    public record SectionDiffModel
    {
        public required string Name { get; set; }
        public List<ItemDiffModel> Items { get; set; } = [];
    }

    public record ItemDiffModel
    {
        public required string ItemName { get; set; }
        public required int RecipeOrIngredientId { get; set; }
        public ItemDiffType DiffType { get; set; }
        
        // Planned values (from meal plan)
        public decimal? PlannedQuantity { get; set; }
        public string? PlannedUnit { get; set; }
        
        // Actual values (from food log)
        public decimal? ActualQuantity { get; set; }
        public string? ActualUnit { get; set; }
        
        // Calculated difference
        public decimal? QuantityDifference => ActualQuantity - PlannedQuantity;
        public bool HasQuantityDifference => PlannedQuantity.HasValue && ActualQuantity.HasValue && PlannedQuantity != ActualQuantity;
        public bool HasUnitDifference => PlannedUnit != ActualUnit;
    }

    public enum ItemDiffType
    {
        Matched,        // Item exists in both plan and log
        OnlyInPlan,     // Item was planned but not logged
        OnlyInLog,      // Item was logged but not planned
        Modified        // Item exists in both but with different quantities/units
    }
}