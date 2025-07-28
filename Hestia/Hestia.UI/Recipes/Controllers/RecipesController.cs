using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Hestia.UI.Core.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hestia.UI.Recipes.Controllers
{
    [Route("recipes")]
    public class RecipesController(IComponentFactory componentFactory) : UIController(componentFactory)
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
    }
}
