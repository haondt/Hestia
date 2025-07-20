using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;

namespace Hestia.UI.Core.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseHestiaUI(this WebApplication app)
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new EmbeddedFileProvider(
                    assembly: typeof(WebApplicationExtensions).Assembly,
                    baseNamespace: typeof(WebApplicationExtensions).Assembly.GetName().Name),
                RequestPath = "/static"
            });

            return app;
        }
    }
}
