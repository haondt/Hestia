using Haondt.Core.Models;
using Hestia.Persistence.Models;
using System.ComponentModel.DataAnnotations;

namespace Hestia.Domain.Models
{
    public record MealPlanModel
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        public required AbsoluteDateTime LastModified { get; set; }

        public List<MealPlanSectionModel> Sections { get; set; } = [];

        public static MealPlanModel FromDataModel(MealPlanDataModel model) => new()
        {
            Name = model.Name,
            LastModified = model.LastModified,
            Sections = model.Sections
                .OrderBy(s => s.Order)
                .Select(MealPlanSectionModel.FromDataModel)
                .ToList()
        };

        public MealPlanDataModel AsDataModel() => new()
        {
            Name = Name,
            LastModified = LastModified,
            Sections = Sections.Select((section, index) => section.AsDataModel(index)).ToList()
        };
        public void ApplyUpdate(MealPlanDataModel model)
        {
            model.Name = Name;
            model.LastModified = AbsoluteDateTime.Now;
            model.Sections = Sections.Select((section, index) => section.AsDataModel(index)).ToList();
        }
    }
}