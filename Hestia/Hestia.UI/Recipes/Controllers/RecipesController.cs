using Haondt.Core.Extensions;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Hestia.Domain.Services;
using Hestia.UI.Core.Controllers;
using Hestia.UI.Recipes.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Hestia.UI.Recipes.Controllers
{
    [Route("recipes")]
    public class RecipesController(IComponentFactory componentFactory, IIngredientsService ingredientsService) : UIController(componentFactory)
    {
        private readonly IComponentFactory _componentFactory = componentFactory;

        [HttpGet("view/{recipeId}")]
        public Task<IResult> GetRecipe(int recipeId)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public Task<IResult> GetRecipes()
        {
            return _componentFactory.RenderComponentAsync<Recipes.Components.Recipes>();
        }

        [HttpGet("new")]
        public Task<IResult> GetCreateRecipe()
        {
            return _componentFactory.RenderComponentAsync<Recipes.Components.EditRecipe>();
        }

        [HttpGet("search-ingredients")]
        public async Task<IResult> SearchIngredients([FromQuery] string search = "")
        {
            var ingredients = await ingredientsService.SearchIngredientsAsync(search, 0, 5); // todo: appsettings

            return await _componentFactory.RenderComponentAsync(new RecipeIngredientAutocompleteSuggestions
            {
                Suggestions = ingredients.Select(i => (i.Id, i.Ingredient.Name)).ToList()
            });
        }
        [HttpGet("add-ingredient")]
        public async Task<IResult> AddIngredient(
            [FromQuery] int? id,
            [FromQuery, Required] string name,
            [FromQuery, Required] decimal quantity,
            [FromQuery, Required] string unit)
        {
            return await _componentFactory.RenderComponentAsync(new RecipeIngredientsListItem
            {
                Id = id.AsOptional(),
                Name = new(name),
                Quantity = new(quantity),
                Unit = new(unit),
                GroupId = 4 // random number, chosen by fair dice roll
            });
        }
    }
}
