using Haondt.Web.BulmaCSS.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Services;
using Hestia.UI.Core.Middlewares;
using Hestia.UI.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ComponentFactory = Hestia.UI.Core.Services.ComponentFactory;

namespace Hestia.UI.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHestiaUI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ILayoutComponentFactory, LayoutComponentFactory>();
            services.AddBulmaCSSAssetSources();
            services.AddHestiaHeadEntries();
            services.AddSingleton<IComponentFactory, ComponentFactory>();

            services.AddSingleton<IExceptionActionResultFactory, ToastExceptionActionResultFactory>();
            services.AddScoped<ModelStateValidationFilter>();
            services.AddSingleton<ILucideIconService, LucideIconService>();

            return services;
        }

        public static IServiceCollection AddHestiaHeadEntries(this IServiceCollection services)
        {
            services.AddScoped<IHeadEntryDescriptor>(_ => new StyleSheetDescriptor
            {
                Uri = "/static/shared/vendored/bulma/css/bulma.min.css"
            });

            var assembly = typeof(ServiceCollectionExtensions).Assembly;
            var assemblyPrefix = assembly.GetName().Name;

            services.AddScoped<IHeadEntryDescriptor>(sp => new LinkDescriptor
            {
                Uri = "/static/shared/logo.svg",
                Relationship = "icon",
                Type = "image/svg+xml"
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new StyleSheetDescriptor
            {
                Uri = "/static/shared/style.css",
            });
            services.AddScoped<IHeadEntryDescriptor>(sp => new ScriptDescriptor
            {
                Uri = "/static/shared/vendored/minimasonry/build/minimasonry.min.js",
            });
            services.AddScoped<IHeadEntryDescriptor>(sp => new ScriptDescriptor
            {
                Uri = "/static/shared/vendored/quill/dist/quill.js"
            });
            services.AddScoped<IHeadEntryDescriptor>(sp => new StyleSheetDescriptor
            {
                Uri = "/static/shared/vendored/quill/dist/quill.snow.css"
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new StyleSheetDescriptor
            {
                Uri = "/static/shared/quill.css",
            });
            services.AddScoped<IHeadEntryDescriptor>(sp => new ScriptDescriptor
            {
                Uri = "/static/shared/vendored/@floating-ui/core/dist/floating-ui.core.umd.js"
            });
            services.AddScoped<IHeadEntryDescriptor>(sp => new ScriptDescriptor
            {
                Uri = "/static/shared/vendored/@floating-ui/dom/dist/floating-ui.dom.umd.js"
            });
            services.AddScoped<IHeadEntryDescriptor>(sp => new ScriptDescriptor
            {
                Uri = "/static/shared/js/tooltip.js"
            });
            services.AddScoped<IHeadEntryDescriptor>(sp => new ScriptDescriptor
            {
                Uri = "/static/shared/js/hx-rename.js"
            });
            services.AddScoped<IHeadEntryDescriptor>(sp => new ScriptDescriptor
            {
                Uri = "/static/shared/vendored/sortablejs/Sortable.min.js"
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new TitleDescriptor
            {
                Title = "Hestia",
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new MetaDescriptor
            {
                Name = "htmx-config",
                Content = @"{
                    ""responseHandling"": [
                        { ""code"": ""204"", ""swap"": false },
                        { ""code"": "".*"", ""swap"": true }
                    ],
                    ""scrollIntoViewOnBoost"": false
                }",
            });
            return services;
        }
    }
}
