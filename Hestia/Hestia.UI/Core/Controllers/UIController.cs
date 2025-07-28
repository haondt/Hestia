using Haondt.Web.BulmaCSS.Services;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Hestia.UI.Core.Middlewares;
using Hestia.UI.Library.Components.Element;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hestia.UI.Core.Controllers
{

    [Produces("text/html")]
    [ServiceFilter(typeof(ModelStateValidationFilter))]
    public class UIController(IComponentFactory componentFactory) : Controller
    {
        public Task<IResult> ToastResponse(
            string message,
            ToastSeverity severity = ToastSeverity.Error,
            int statusCode = 500)
        {
            var toast = new Toast
            {
                Message = message,
                Severity = severity
            };

            Response.AsResponseData()
                .Status(statusCode)
                .HxReswap("none");
            return componentFactory.RenderComponentAsync(toast);
        }
    }
}
