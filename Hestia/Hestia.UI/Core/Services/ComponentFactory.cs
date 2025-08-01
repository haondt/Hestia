using Haondt.Web.Core.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Http;
using Haondt.Web.Core.Services;
using Haondt.Web.Services;
using Hestia.UI.Core.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Hestia.UI.Core.Services
{
    public class ComponentFactory(
        IHttpContextAccessor httpContext,
        ILayoutComponentFactory layoutFactory) : IComponentFactory
    {
        public Task<IResult> RenderComponentAsync<T>(T component, IRequestData? requestData = null, IResponseData? responseData = null) where T : IComponent
        {
            return RenderComponentAsync(component, typeof(T), requestData, responseData);
        }

        public Task<IResult> RenderComponentAsync(IComponent component, IRequestData? requestData = null, IResponseData? responseData = null)
        {
            return RenderComponentAsync(component, component.GetType(), requestData, responseData);
        }

        public async Task<IResult> RenderComponentAsync(IComponent component, Type componentType, IRequestData? requestData = null, IResponseData? responseData = null)
        {
            var request = requestData ?? httpContext.HttpContext?.Request.AsRequestData() ?? throw new ArgumentNullException(nameof(requestData));
            var response = responseData ??= httpContext.HttpContext?.Response.AsResponseData() ?? throw new ArgumentNullException(nameof(responseData));


            if (!request.IsHxRequest() && componentType.GetCustomAttributes(typeof(RenderPageAttribute), false).Length != 0)
            {
                component = await layoutFactory.GetLayoutAsync(component);
                component = new Core.Components.Page { Content = component };
                componentType = typeof(Core.Components.Page);
            }

            var rootComponent = new RootComponent
            {
                Component = component,
                Request = request,
                Response = response,
                Type = componentType,
            };

            return new RazorComponentResult<RootComponent>(rootComponent.ToDictionary());
        }
    }
}

