using Haondt.Web.BulmaCSS.Services;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Hestia.UI.Core.Exceptions;
using Hestia.UI.Library.Components.Element;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Hestia.UI.Core.Middlewares
{
    public class ToastExceptionActionResultFactory(IComponentFactory componentFactory, ILogger<ToastExceptionActionResultFactory> logger) : IExceptionActionResultFactory
    {
        public bool CanHandle(Exception exception)
        {
            return true;
        }

        public async Task<IResult> CreateAsync(Exception exception, HttpContext context)
        {
            logger.LogError(exception, "Toasting exception {Exception}", exception.Message);
            var severity = ToastSeverity.Error;
            var statusCode = 500;
            var model = new Toast { Message = $"{exception.GetType().Name}: {exception.Message}", Severity = severity };
            if (exception is StatusCodeException statusCodeException)
            {
                model.Message = exception.Message;
                model.Severity = statusCodeException.StatusCode >= 500 ? ToastSeverity.Error : ToastSeverity.Warning;
                statusCode = statusCodeException.StatusCode;
            }

            var errorComponent = await componentFactory.RenderComponentAsync(model);
            context.Response.AsResponseData()
                .Status(statusCode)
                .HxReswap("none");
            return errorComponent;
        }
    }
}
