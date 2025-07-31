using Haondt.Web.Components;
using Haondt.Web.Core.Services;
using Hestia.Domain.Services;
using Hestia.UI.Core.Components;
using Hestia.UI.Core.Extensions;
using Hestia.UI.Ingredients.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hestia.UI.Core.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("utils")]
    public class UtilsController(IComponentFactory componentFactory,
        IUnitConversionsService unitConversionsService) : UIController(componentFactory)
    {
        private readonly IComponentFactory _componentFactory = componentFactory;

        [HttpGet("check-unit-compatibility")]
        public async Task<IResult> CheckUnitCompatibility([FromQuery] string[] from, [FromQuery] string[] to, [FromQuery] string[] swap, [FromQuery] string message, [FromQuery] bool @default = false)
        {
            if (await unitConversionsService.CheckUnitCompatiblityAsync(from, to, @default))
                return await _componentFactory.RenderComponentAsync(new AppendComponentLayout
                {
                    Components = swap.Select(s => new FieldWarningTooltip
                    {
                        Id = s,
                        HxSwapOob = "true",
                        IsVisible = false,
                    }).Cast<IComponent>().ToList()
                });
            return await _componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = swap.Select(s => new FieldWarningTooltip
                {
                    Id = s,
                    HxSwapOob = "true",
                    Message = message,
                }).Cast<IComponent>().ToList()
            });

        }

    }
}