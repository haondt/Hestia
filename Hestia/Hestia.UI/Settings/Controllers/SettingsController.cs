using Haondt.Web.Core.Services;
using Hestia.Domain.Models;
using Hestia.Domain.Services;
using Hestia.UI.Core.Controllers;
using Hestia.UI.Settings.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hestia.UI.Settings.Controllers
{
    [Route("settings")]
    public class SettingsController(IComponentFactory componentFactory, IUnitConversionsService unitConversionsService) : UIController(componentFactory)
    {
        private readonly IComponentFactory _componentFactory = componentFactory;

        [HttpGet]
        public async Task<IResult> Get()
        {
            var conversions = await unitConversionsService.GetAllAsync();
            return await _componentFactory.RenderComponentAsync(new Settings.Components.Settings
            {
                UnitConversionList = new UnitConversionList
                {
                    Conversions = conversions
                }
            });
        }

        [HttpPost("unit-conversions")]
        public async Task<IResult> AddUnitConversion([FromForm] UnitConversionModel model)
        {
            var result = await unitConversionsService.AddAsync(model);
            if (!result.IsSuccessful)
                return await ToastResponse(result.Reason, statusCode: 422);
            return await _componentFactory.RenderComponentAsync(new UnitConversionListItem { Conversion = result.Value });
        }

        [HttpDelete("unit-conversions")]
        public async Task<IResult> RemoveUnitConversion([FromQuery] UnitConversionModel model)
        {
            await unitConversionsService.RemoveAsync(model);
            return TypedResults.Ok();
        }
    }
}