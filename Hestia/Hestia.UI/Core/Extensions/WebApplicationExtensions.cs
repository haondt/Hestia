using Microsoft.AspNetCore.Builder;

namespace Hestia.UI.Core.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseHestiaUI(this WebApplication app)
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = "/static"
            });
            return app;
        }
    }
}
