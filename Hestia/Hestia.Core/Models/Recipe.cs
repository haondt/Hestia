namespace Hestia.Core.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? YieldQuantity { get; set; }
        public string? YieldUnit { get; set; }
        public int? NumberOfServings { get; set; }
        public ICollection<RecipeTag> Tags { get; set; } = [];
        public ICollection<RecipeIngredient> Ingredients { get; set; } = [];
    }
}
