using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Hestia.UI.Core.Controllers;
using Hestia.UI.Library.Components.Element;
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

        [HttpPost("new")]
        public async Task<IResult> CreateIngredient()
        {

            var model = new Toast { Message = $"FooException: something really really really really really really really really really bad happened", Severity = Haondt.Web.BulmaCSS.Services.ToastSeverity.Error };
            var errorComponent = await componentFactory.RenderComponentAsync(model);
            HttpContext.Response.AsResponseData()
                .Status(500)
                .HxReswap("none");
            return errorComponent;
        }
    }
}
