using Haondt.Core.Models;

namespace Hestia.Persistence.Models
{
    public record MealPlanDataModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required AbsoluteDateTime LastModified { get; set; }
        public ICollection<MealPlanSectionDataModel> Sections { get; set; } = [];
    }
}