namespace Hestia.Persistence.Models
{
    public record RecipeDataModel
    {
        public int Id { get; set; }

        public required string Title { get; set; }
        public required string NormalizedTitle { get; set; }
        public string? Description { get; set; }
        public string? NormalizedDescription { get; set; }
        public decimal? YieldQuantity { get; set; }
        public string? YieldUnit { get; set; }
        public int? NumberOfServings { get; set; }
        public double? PrepTimeQuantity { get; set; }
        public string? PrepTimeUnit { get; set; }
        public double? CookTimeQuantity { get; set; }
        public string? CookTimeUnit { get; set; }
        public string? Instructions { get; set; }
        public ICollection<RecipeIngredientDataModel> Ingredients { get; set; } = [];
    }

}