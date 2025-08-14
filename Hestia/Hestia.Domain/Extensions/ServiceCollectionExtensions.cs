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
            services.AddScoped<IFoodLogService, FoodLogService>();
            services.AddScoped<IDbSeeder, DbSeeder>();
            services.AddScoped<IDevDbSeeder, DevDbSeeder>();

            var scannerSettings = configuration.GetRequiredSection<ScannerSettings>();
            if (scannerSettings.Enabled)
            {
                services.Configure<ScannerSettings>(configuration.GetSection(nameof(ScannerSettings)));
                services.AddScoped(typeof(IScannerService<>), typeof(ScannerService<>));

                if (scannerSettings.OcrProvider == OcrProvider.DocumentAI)
                {
                    if (scannerSettings.DocumentAI == null)
                        throw new InvalidOperationException("DocumentAI settings are required when OcrProvider is DocumentAI");

                    services.AddScoped(typeof(IScanTextExtractor<>), typeof(DocumentAINutritionLabelTextExtractor<>));

                    services.AddDocumentProcessorServiceClient(b =>
                    {
                        b.Endpoint = $"{scannerSettings.DocumentAI.ProcessorLocationId}-documentai.googleapis.com";
                    });
                }
                else if (scannerSettings.OcrProvider == OcrProvider.CloudVision)
                {
                    services.AddScoped(typeof(IScanTextExtractor<>), typeof(CloudVisionScanTextExtractor<>));

                    services.AddImageAnnotatorClient(b =>
                    {

                    });
                }

                if (scannerSettings.LlmProvider == LlmProvider.OpenRouter)
                {
                    if (scannerSettings.OpenRouter == null)
                        throw new InvalidOperationException("OpenRouter settings are required when LlmProvider is OpenRouter");

                    services.AddHttpClient<IScanTextTransformer<ScannedNutritionLabel>, OpenRouterNutritionLabelTextTransformer>();
                    services.AddHttpClient<IScanTextTransformer<ScannedPackaging>, OpenRouterPackagingTextTransformer>();
                }
            }
            else
            {
                services.AddScoped(typeof(IScannerService<>), typeof(DisabledNutritionLabelScannerService<>));
            }
            services.AddSingleton(typeof(IScanProcessingStateService<>), typeof(ScanProcessingStateService<>));

            return services;
        }
    }
}
