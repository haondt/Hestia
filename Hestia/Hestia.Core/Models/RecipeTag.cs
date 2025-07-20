namespace Hestia.Core.Models
{
    public class RecipeTag
    {
        public int Id { get; set; }

        public Recipe Recipe { get; set; } = default!;
        public int RecipeId { get; set; }
    }
}
