using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Hestia.UI.Core.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hestia.UI.Settings.Controllers
{
    [Route("settings")]
    public class SettingsController(IComponentFactory componentFactory) : UIController
    {
        [HttpGet]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<Hestia.UI.Settings.Components.Settings>();
        }
    }
}