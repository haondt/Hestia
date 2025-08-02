using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Hestia.Core.Models;
using Hestia.Persistence.Models;
using System.ComponentModel.DataAnnotations;

namespace Hestia.Domain.Models
{
    public record RecipeIngredientModel
    {
        public Optional<int> IngredientId { get; set; }
        public Optional<string> IngredientName { get; set; }

        public Optional<decimal> Quantity { get; set; }
        public Optional<string> Unit { get; set; }

        [Required]
        public required string Name { get; set; }  // in case the ingredient is deleted

        internal static RecipeIngredientModel FromDataModel(RecipeIngredientDataModel model)
        {
            return new()
            {
                IngredientId = model.IngredientId.AsOptional(),
                Quantity = model.Quantity.AsOptional(),
                Unit = model.Unit.AsOptional(),
                Name = model.Name,
                IngredientName = model.Ingredient.AsOptional().Map(x => x.Name)
            };
        }

        internal RecipeIngredientDataModel AsDataModel()
        {
            return new()
            {
                Name = Name,
                IngredientId = IngredientId.Unwrap(),
                Quantity = Quantity.Unwrap(),
                Unit = Unit.Unwrap()
            };
        }

        internal IngredientDataModel CreateIngredientDataModel()
        {
            return new()
            {
                Name = IngredientName.Or(Name),
                NormalizedName = NormalizedString.Create(IngredientName.Or(Name)),
                Brand = null,
                Vendor = null,
                ServingSizeQuantity = null,
                ServingSizeUnit = null,
                AlternateServingSizeQuantity = null,
                AlternateServingSizeUnit = null,
                Calories = null,
                FatGrams = null,
                CarbGrams = null,
                ProteinGrams = null,
                PackageSizeQuantity = null,
                PackageSizeUnit = null,
                PackageCostDollars = null,
                Notes = ""
            };
        }
    }
}
