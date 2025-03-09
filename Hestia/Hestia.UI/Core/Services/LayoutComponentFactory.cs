using Haondt.Web.Services;
using Hestia.UI.Core.Components;
using Microsoft.AspNetCore.Components;

namespace Hestia.UI.Core.Services
{
    public class LayoutComponentFactory : ILayoutComponentFactory
    {
        public Task<IComponent> GetLayoutAsync(IComponent content)
        {
            return Task.FromResult<IComponent>(new Layout
            {
                Content = content
            });
        }
    }
}
