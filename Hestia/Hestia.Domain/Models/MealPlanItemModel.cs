using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Hestia.Persistence.Models;
using System.ComponentModel.DataAnnotations;

namespace Hestia.Domain.Models
{
    public record MealPlanItemModel
    {
        [Required]
        public required MealItemType ItemType { get; set; }

        [Required]
        public required int RecipeOrIngredientId { get; set; }

        [Required]
        public required decimal Quantity { get; set; }

        public Optional<string> Unit { get; set; }

        [Required]
        public required string ItemName { get; set; }

        public static MealPlanItemModel FromDataModel(MealPlanItemDataModel model) => new()
        {
            ItemType = model.ItemType,
            RecipeOrIngredientId = model.ItemType == MealItemType.Recipe ? model.RecipeId!.Value : model.IngredientId!.Value,
            Quantity = model.Quantity,
            Unit = model.Unit.AsOptional(),
            ItemName = model.ItemType == MealItemType.Recipe
                ? model.Recipe!.Title
                : model.Ingredient!.Name
        };

        public MealPlanItemDataModel AsDataModel(int order) => new()
        {
            ItemType = ItemType,
            RecipeId = ItemType == MealItemType.Recipe ? RecipeOrIngredientId : null,
            IngredientId = ItemType == MealItemType.Ingredient ? RecipeOrIngredientId : null,
            Quantity = Quantity,
            Unit = Unit.Unwrap(),
            MealSectionId = 0,
            Order = order,
        };
    }
}