namespace Hestia.Domain.Models
{
    public record MealPlanInsightsModel
    {
        public required double TotalCalories { get; set; }
        public required double TotalProtein { get; set; }
        public required double TotalFat { get; set; }
        public required double TotalCarbs { get; set; }
        public required decimal TotalCost { get; set; }
    }
}