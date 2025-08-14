using Haondt.Core.Extensions;
using Hestia.Domain.Models;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

namespace Hestia.Domain.Services
{
    public class OpenRouterPackagingTextTransformer(
        HttpClient httpClient,
        IOptions<NutritionLabelScannerSettings> options,
        IScanProcessingStateService<ScannedPackaging> stateService) : OpenRouterScanTextTransformer<ScannedPackaging>(httpClient, options, stateService)
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        protected override string GetPrompt(string text) => $$"""
            Extract product packaging information from this text and return ONLY a JSON object with these exact fields:
            {
              "name": "<string or null>",
              "brand": "<string or null>",
              "packageSize": <decimal or null>,
              "packageSizeUnit": "<string or null>"
            }

            Rules:
            - Extract the product name (not the brand name)
            - Extract the brand name separately from the product name
            - Extract package size as a number (no units in this field)
            - Extract package size unit separately (e.g., "oz", "lb", "ml", "g", etc.)
            - Use null for missing values
            - Normalize units to abbreviated, singular forms where appropriate (e.g., ounces -> oz, pounds -> lb, milliliters -> ml)
            - Return ONLY the JSON, no explanation
            - If multiple sizes are mentioned, use the primary/main package size

            Example:
            Text: "Coca-Cola Classic 12 fl oz Can"
            Response: {"name": "Classic", "brand": "Coca-Cola", "packageSize": 12, "packageSizeUnit": "fl oz"}

            Text from packaging:
            {{text}}
            """;

        protected override ScannedPackaging MapToEntity(string responseText)
        {
            var rawData = responseText;
            // Clean up the JSON (remove markdown code blocks if present)
            if (responseText.StartsWith("```"))
            {
                var lines = responseText.Split('\n');
                responseText = string.Join('\n', lines.Skip(1).SkipLast(1));
            }

            try
            {
                var packagingData = JsonSerializer.Deserialize<LLMPackagingData>(responseText, _jsonOptions)
                    ?? throw new JsonException();

                return new ScannedPackaging
                {
                    Name = packagingData.Name.AsOptional().Map(q => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(q.ToLower())),
                    Brand = packagingData.Brand.AsOptional().Map(q => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(q.ToLower())),
                    PackageSize = packagingData.PackageSize.AsOptional(),
                    PackageSizeUnit = packagingData.PackageSizeUnit.AsOptional().Map(q => q.ToLower())
                };
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to parse packaging data JSON: {ex.Message}. Raw response: {rawData}", ex);
            }
        }
    }
}
