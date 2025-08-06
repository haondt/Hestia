namespace Hestia.Persistence.Models
{
    public record MealPlanSectionDataModel
    {
        public int Id { get; set; }

        public required int MealPlanId { get; set; }
        public MealPlanDataModel MealPlan { get; set; } = default!;

        public required string Name { get; set; }
        public required int Order { get; set; }
        public ICollection<MealPlanItemDataModel> Items { get; set; } = [];
    }
}