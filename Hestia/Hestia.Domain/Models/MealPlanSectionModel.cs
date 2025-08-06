using Hestia.Persistence.Models;
using System.ComponentModel.DataAnnotations;

namespace Hestia.Domain.Models
{
    public record MealPlanSectionModel
    {
        [Required]
        public required string Name { get; set; }

        public List<MealPlanItemModel> Items { get; set; } = [];

        public static MealPlanSectionModel FromDataModel(MealPlanSectionDataModel model) => new()
        {
            Name = model.Name,
            Items = model.Items
                .OrderBy(s => s.Order)
                .Select(MealPlanItemModel.FromDataModel).ToList()
        };

        public MealPlanSectionDataModel AsDataModel(int order) => new()
        {
            Name = Name,
            Order = order,
            MealPlanId = 0,
            Items = Items.Select((item, i) => item.AsDataModel(i)).ToList()
        };
    }
}