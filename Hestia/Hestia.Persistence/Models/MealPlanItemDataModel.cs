namespace Hestia.Persistence.Models
{
    public enum MealItemType
    {
        Recipe,
        Ingredient
    }

    public record MealPlanItemDataModel
    {
        public int Id { get; set; }
        public required int Order { get; set; }
        public required MealItemType ItemType { get; set; }
        public int? RecipeId { get; set; }
        public RecipeDataModel? Recipe { get; set; }
        public int? IngredientId { get; set; }
        public IngredientDataModel? Ingredient { get; set; }
        public required decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public required int MealSectionId { get; set; }
        public MealPlanSectionDataModel MealSection { get; set; } = default!;
    }
}