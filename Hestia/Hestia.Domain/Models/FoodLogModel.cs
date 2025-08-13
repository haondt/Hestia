using Haondt.Core.Extensions;
using Hestia.Domain.Attributes;
using Hestia.Persistence.Models;

namespace Hestia.Domain.Models
{
    public class FoodLogModel
    {
        [ValidDateString]
        public required string DateString { get; set; }
        public List<MealPlanSectionModel> Sections { get; set; } = [];
        public MealPlanModel? MealPlan { get; set; }

        public static FoodLogModel FromDataModel(FoodLogDataModel model) => new()
        {
            DateString = model.DateString,
            Sections = model.Sections
                .OrderBy(s => s.Order)
                .Select(MealPlanSectionModel.FromDataModel)
                .ToList(),
            MealPlan = model.MealPlan.AsOptional().Map(MealPlanModel.FromDataModel).Unwrap()
        };

        public FoodLogDataModel AsDataModel() => new()
        {
            DateString = DateString,
            MealPlan = MealPlan.AsOptional().Map(q => q.AsDataModel()).Unwrap(),
            Sections = Sections.Select((section, index) => section.AsFoodLogDataModel(index)).ToList()
        };

        public void ApplyUpdate(FoodLogDataModel model)
        {
            model.DateString = DateString;
            model.Sections = Sections.Select((section, index) => section.AsFoodLogDataModel(index)).ToList();
            model.MealPlan = MealPlan.AsOptional().Map(q => q.AsDataModel()).Unwrap();
        }
    }

}