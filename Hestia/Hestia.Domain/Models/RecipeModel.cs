using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Hestia.Persistence.Models;
using System.ComponentModel.DataAnnotations;

namespace Hestia.Domain.Models
{
    public class RecipeModel
    {
        [Required]
        public required string Title { get; set; }

        public required Optional<string> Description { get; set; }

        public required Optional<decimal> YieldQuantity { get; set; }

        public required Optional<string> YieldUnit { get; set; }

        public required Optional<int> NumberOfServings { get; set; }

        public required Optional<int> PrepTimeMinutes { get; set; }

        public required Optional<int> CookTimeMinutes { get; set; }

        public List<object> Ingredients { get; set; } = [];// todo

        public static RecipeModel FromDataModel(RecipeDataModel model) => new()
        {
            Title = model.Title,
            Description = model.Description.AsOptional(),
            YieldQuantity = model.YieldQuantity.AsOptional(),
            YieldUnit = model.YieldUnit.AsOptional(),
            NumberOfServings = model.NumberOfServings.AsOptional(),
            PrepTimeMinutes = model.PrepTimeMinutes.AsOptional(),
            CookTimeMinutes = model.CookTimeMinutes.AsOptional()
        };

        public RecipeDataModel AsDataModel() => new()
        {
            Title = Title,
            NormalizedTitle = Title,
            Description = Description.Unwrap(),
            YieldQuantity = YieldQuantity.Unwrap(),
            YieldUnit = YieldUnit.Unwrap(),
            NumberOfServings = NumberOfServings.Unwrap(),
            PrepTimeMinutes = PrepTimeMinutes.Unwrap(),
            CookTimeMinutes = CookTimeMinutes.Unwrap()
        };
    }
}