using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Hestia.UI.Core.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hestia.UI.Ingredients.Controllers
{
    [Route("ingredients")]
    public class IngredientsController(IComponentFactory componentFactory) : UIController
    {
        [HttpGet]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<Ingredients.Components.Ingredients>();

        }

        [HttpGet("new")]
        public Task<IResult> GetCreateIngredient()
        {
            return componentFactory.RenderComponentAsync<Ingredients.Components.EditIngredient>();
        }
    }
}
