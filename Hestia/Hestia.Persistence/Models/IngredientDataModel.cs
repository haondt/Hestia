using Hestia.Core.Models;

namespace Hestia.Persistence.Models
{
    public record class IngredientDataModel
    {
        public int Id { get; set; }

        public required string Name { get; set; }
        public required string NormalizedName { get; set; }
        public required string? Brand { get; set; }
        public required string? Vendor { get; set; }

        public required decimal? ServingSizeQuantity { get; set; }
        public required string? ServingSizeUnit { get; set; }
        public required decimal? AlternateServingSizeQuantity { get; set; }
        public required string? AlternateServingSizeUnit { get; set; }
        public required decimal? Calories { get; set; }
        public required decimal? FatGrams { get; set; }
        public required decimal? CarbGrams { get; set; }
        public required decimal? ProteinGrams { get; set; }

        public required decimal? PackageSizeQuantity { get; set; }
        public required string? PackageSizeUnit { get; set; }
        public required decimal? PackageCostDollars { get; set; }

        public required string Notes { get; set; }
    }
}
