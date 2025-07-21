namespace Hestia.Core.Models
{
    public class Ingredient
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public decimal? CaloriesPerUnit { get; set; }
        public decimal? ProteinPerUnit { get; set; }
        public decimal? CarbsPerUnit { get; set; }
        public decimal? FatPerUnit { get; set; }
        public string? Unit { get; set; }
        public decimal? CostPerUnitInDollars { get; set; }

        public decimal? UnitsPerServing { get; set; }
        public decimal? UnitsPerPurchase { get; set; }
    }
}
