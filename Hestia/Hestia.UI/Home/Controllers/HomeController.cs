using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Hestia.UI.Core.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hestia.UI.Home.Controllers
{
    [Route("")]
    public class HomeController(IComponentFactory componentFactory) : UIController(componentFactory)
    {
        private readonly IComponentFactory _componentFactory = componentFactory;

        [HttpGet]
        public Task<IResult> Get()
        {
            return _componentFactory.RenderComponentAsync<Home.Components.Home>();
        }
    }
}