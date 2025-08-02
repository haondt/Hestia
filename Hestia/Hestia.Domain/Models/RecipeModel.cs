using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Hestia.Core.Models;
using Hestia.Persistence.Models;
using System.ComponentModel.DataAnnotations;

namespace Hestia.Domain.Models
{
    public record RecipeModel
    {
        [Required]
        public required string Title { get; set; }

        public required Optional<string> Description { get; set; }

        public required Optional<decimal> YieldQuantity { get; set; }

        public required Optional<string> YieldUnit { get; set; }

        public required Optional<int> NumberOfServings { get; set; }

        public required Optional<double> PrepTimeQuantity { get; set; }

        public required Optional<string> PrepTimeUnit { get; set; }

        public required Optional<double> CookTimeQuantity { get; set; }

        public required Optional<string> CookTimeUnit { get; set; }

        public required Optional<string> Instructions { get; set; }

        public List<RecipeIngredientModel> Ingredients { get; set; } = [];

        public static RecipeModel FromDataModel(RecipeDataModel model) => new()
        {
            Title = model.Title,
            Description = model.Description.AsOptional(),
            YieldQuantity = model.YieldQuantity.AsOptional(),
            YieldUnit = model.YieldUnit.AsOptional(),
            NumberOfServings = model.NumberOfServings.AsOptional(),
            PrepTimeQuantity = model.PrepTimeQuantity.AsOptional(),
            PrepTimeUnit = model.PrepTimeUnit.AsOptional(),
            CookTimeQuantity = model.CookTimeQuantity.AsOptional(),
            CookTimeUnit = model.CookTimeUnit.AsOptional(),
            Instructions = model.Instructions.AsOptional(),
            Ingredients = model.Ingredients.Select(RecipeIngredientModel.FromDataModel).ToList()
        };

        public RecipeDataModel AsDataModel() => new()
        {
            Title = Title,
            NormalizedTitle = NormalizedString.Create(Title),
            Description = Description.Unwrap(),
            NormalizedDescription = Description.Map(NormalizedString.Create).Unwrap(),
            YieldQuantity = YieldQuantity.Unwrap(),
            YieldUnit = YieldUnit.Unwrap(),
            NumberOfServings = NumberOfServings.Unwrap(),
            PrepTimeQuantity = PrepTimeQuantity.Unwrap(),
            PrepTimeUnit = PrepTimeUnit.Unwrap(),
            CookTimeQuantity = CookTimeQuantity.Unwrap(),
            CookTimeUnit = CookTimeUnit.Unwrap(),
            Instructions = Instructions.Unwrap(),
            Ingredients = Ingredients.Select(i => i.AsDataModel()).ToList()
        };
        public void ApplyUpdate(RecipeDataModel model)
        {
            model.Title = Title;
            model.NormalizedTitle = NormalizedString.Create(Title);
            model.Description = Description.Unwrap();
            model.NormalizedDescription = Description.Map(NormalizedString.Create).Unwrap();
            model.YieldQuantity = YieldQuantity.Unwrap();
            model.YieldUnit = YieldUnit.Unwrap();
            model.NumberOfServings = NumberOfServings.Unwrap();
            model.PrepTimeQuantity = PrepTimeQuantity.Unwrap();
            model.PrepTimeUnit = PrepTimeUnit.Unwrap();
            model.CookTimeQuantity = CookTimeQuantity.Unwrap();
            model.CookTimeUnit = CookTimeUnit.Unwrap();
            model.Instructions = Instructions.Unwrap();
            model.Ingredients = Ingredients.Select(i => i.AsDataModel()).ToList();
        }
    }
}