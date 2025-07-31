using Hestia.Core.Models;

namespace Hestia.Persistence.Models
{
    public class RecipeDataModel
    {
        public int Id { get; set; }

        public required string Title { get; set; }
        public required NormalizedString NormalizedTitle { get; set; }
        public string? Description { get; set; }
        public decimal? YieldQuantity { get; set; }
        public string? YieldUnit { get; set; }
        public int? NumberOfServings { get; set; }
    }
}