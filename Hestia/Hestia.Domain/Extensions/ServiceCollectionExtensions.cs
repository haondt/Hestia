using Haondt.Core.Extensions;
using Hestia.Domain.Models;
using Hestia.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hestia.Domain.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHestiaDomainServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IIngredientsService, IngredientsService>();
            services.AddScoped<IUnitConversionsService, UnitConversionsService>();
            services.AddScoped<IRecipesService, RecipesService>();
            services.AddScoped<IMealPlansService, MealPlansService>();
            services.AddScoped<IDbSeeder, DbSeeder>();
            services.AddScoped<IDevDbSeeder, DevDbSeeder>();

            var scannerSettings = configuration.GetRequiredSection<NutritionLabelScannerSettings>();
            if (scannerSettings.Enabled)
            {
                services.Configure<NutritionLabelScannerSettings>(configuration.GetSection(nameof(NutritionLabelScannerSettings)));
                if (scannerSettings.DocumentAI != null)
                {

                    services.AddScoped<INutritionLabelScannerService, NutritionLabelScannerService>();
                    services.AddDocumentProcessorServiceClient(b =>
                    {
                        b.Endpoint = $"{scannerSettings.DocumentAI.ProcessorLocationId}-documentai.googleapis.com";
                    });
                }
            }
            else
            {
                services.AddScoped<INutritionLabelScannerService, DisabledNutritionLabelScannerService>();
            }
            services.AddSingleton<INutritionLabelProcessingStateService, NutritionLabelProcessingStateService>();

            return services;
        }
    }
}
