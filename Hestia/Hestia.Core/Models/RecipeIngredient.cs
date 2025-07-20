namespace Hestia.Core.Models
{
    public class RecipeIngredient
    {
        public int Id { get; set; }
        public Recipe Recipe { get; set; } = default!;
        public int RecipeId { get; set; }

        public Ingredient? Ingredient { get; set; }
        public int? IngredientId { get; set; }

        public string? IngredientName { get; set; }
    }
}
