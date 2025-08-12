namespace Hestia.Persistence.Models
{
    public record FoodLogItemDataModel
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
        public required int FoodLogSectionId { get; set; }
        public FoodLogSectionDataModel FoodLogSection { get; set; } = default!;
    }
}