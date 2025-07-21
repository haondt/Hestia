﻿using Haondt.Web.BulmaCSS.Extensions;
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
            services.AddBulmaCSSHeadEntries();
            services.AddBulmaCSSAssetSources();
            services.AddHestiaHeadEntries();
            services.AddSingleton<IComponentFactory, ComponentFactory>();

            services.AddSingleton<IExceptionActionResultFactory, ToastExceptionActionResultFactory>();
            services.AddScoped<ModelStateValidationFilter>();

            return services;
        }

        public static IServiceCollection AddHestiaHeadEntries(this IServiceCollection services)
        {

            var assembly = typeof(ServiceCollectionExtensions).Assembly;
            var assemblyPrefix = assembly.GetName().Name;

            services.AddScoped<IHeadEntryDescriptor>(sp => new LinkDescriptor
            {
                Uri = "/static/wwwroot.logo.svg",
                Relationship = "icon",
                Type = "image/svg+xml"
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new StyleSheetDescriptor
            {
                Uri = "/static/wwwroot.style.css",
            });
            services.AddScoped<IHeadEntryDescriptor>(sp => new ScriptDescriptor
            {
                Uri = "https://cdn.jsdelivr.net/npm/minimasonry@1.3.2/build/minimasonry.min.js",
            });
            services.AddScoped<IHeadEntryDescriptor>(sp => new ScriptDescriptor
            {
                Uri = "https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.js",
            });
            services.AddScoped<IHeadEntryDescriptor>(sp => new StyleSheetDescriptor
            {
                Uri = "https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.snow.css",
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new StyleSheetDescriptor
            {
                Uri = "/static/wwwroot.quill.css",
            });
            services.AddScoped<IHeadEntryDescriptor>(sp => new ScriptDescriptor
            {
                Uri = "https://kit.fontawesome.com/afd44816da.js",
                CrossOrigin = "anonymous"
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
