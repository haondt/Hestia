using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hestia.UI.Recipes.Controllers
{
    [Route("recipes")]
    public class RecipesController(IComponentFactory componentFactory) : UIController
    {
        [HttpGet("view/{recipeId}")]
        public Task<IResult> GetRecipe(int recipeId)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public Task<IResult> GetRecipes()
        {
            return componentFactory.RenderComponentAsync<Recipes.Components.Recipes>();
        }

        [HttpGet("new")]
        public Task<IResult> GetCreateRecipe()
        {
            return componentFactory.RenderComponentAsync<Recipes.Components.EditRecipe>();
        }
    }
}
