using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Hestia.Persistence.Models;

namespace Hestia.Domain.Models
{
    public class FoodLogModel
    {
        public required string DateString { get; set; }
        public List<MealPlanSectionModel> Sections { get; set; } = [];
        public Optional<MealPlanModel> MealPlan { get; set; }

        public static FoodLogModel FromDataModel(FoodLogDataModel model) => new()
        {
            DateString = model.DateString,
            Sections = model.Sections
                .OrderBy(s => s.Order)
                .Select(MealPlanSectionModel.FromDataModel)
                .ToList(),
            MealPlan = model.MealPlan.AsOptional().Map(MealPlanModel.FromDataModel)
        };

        public FoodLogDataModel AsDataModel() => new()
        {
            DateString = DateString,
            MealPlan = MealPlan.Map(q => q.AsDataModel()).Unwrap(),
            Sections = Sections.Select((section, index) => section.AsFoodLogDataModel(index)).ToList()
        };

        public void ApplyUpdate(FoodLogDataModel model)
        {
            model.DateString = DateString;
            model.Sections = Sections.Select((section, index) => section.AsFoodLogDataModel(index)).ToList();
            model.MealPlan = MealPlan.Map(q => q.AsDataModel()).Unwrap();
        }
    }

}