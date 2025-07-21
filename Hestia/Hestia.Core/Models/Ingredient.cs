namespace Hestia.Core.Models
{
    public class Ingredient
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        public decimal CaloriesPerUnit { get; set; } = 0;
        public decimal ProteinPerUnit { get; set; } = 0;
        public decimal CarbsPerUnit { get; set; } = 0;
        public decimal FatPerUnit { get; set; } = 0;
        public decimal UnitsPerServing { get; set; } = 0;
        public string NutritionUnit { get; set; } = string.Empty;

        public decimal CostPerUnitInDollars { get; set; } = 0;
        public decimal UnitsPerPurchase { get; set; } = 0;
        public string PurchaseUnit { get; set; } = string.Empty;
    }
}
