namespace Hestia.Core.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<RecipeTag> Tags { get; set; } = [];
        public ICollection<RecipeIngredient> Ingredients { get; set; } = [];
    }
}
