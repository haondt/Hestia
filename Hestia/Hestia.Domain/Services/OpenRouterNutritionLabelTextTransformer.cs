using Haondt.Core.Extensions;
using Hestia.Domain.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Hestia.Domain.Services
{
    public class OpenRouterNutritionLabelTextTransformer(
        HttpClient httpClient,
        IOptions<ScannerSettings> options,
        IScanProcessingStateService<ScannedNutritionLabel> stateService) : OpenRouterScanTextTransformer<ScannedNutritionLabel>(httpClient, options, stateService)
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        protected override string GetPrompt(string text) => $$"""
            Extract nutrition information from this nutrition label text and return ONLY a JSON object with these exact fields:
            {
              "servingSizeQuantity": <decimal or null>,
              "servingSizeUnit": "<string or null>",
              "alternateServingSizeQuantity": <decimal or null>,
              "alternateServingSizeUnit": "<string or null>",
              "calories": <decimal or null>,
              "carbohydrateGrams": <decimal or null>,
              "fatGrams": <decimal or null>,
              "proteinGrams": <decimal or null>,
              "fibreGrams": <decimal or null>,
              "sodiumGrams": <decimal or null>
            }

            Rules:
            - Convert fractions to decimals (e.g. "1/4" becomes 0.25, "1 1/2" becomes 1.5)
            - Use null for missing values
            - Extract numbers only, no units in numeric fields
            - Sodium should be in grams (convert from mg if needed: mg ÷ 1000)
            - Return ONLY the JSON, no explanation
            - If two serving sizes are given, the first one is the serving size and the second is the alternate serving size. (e.g. 1 cup (250 mL) = 1 cup serving size, 250 mL alternate serving size).
            - Normalize units to abbreviated, singular forms where appropriate. E.g. tablespoon -> tbsp, milliliters -> ml, lbs -> lb. If there is not a standard, well known conversion, leave it as-is (e.g. 1 egg -> 1 egg).

            Text from nutrition label:
            {{text}}
            """;

        protected override ScannedNutritionLabel MapToEntity(string responseText)
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
                var nutritionData = JsonSerializer.Deserialize<LLMNutritionData>(responseText, _jsonOptions)
                    ?? throw new JsonException();

                return new ScannedNutritionLabel
                {
                    ServingSizeQuantity = nutritionData.ServingSizeQuantity.AsOptional(),
                    ServingSizeUnit = nutritionData.ServingSizeUnit.AsOptional().Map(q => q.ToLower()),
                    AlternateServingSizeQuantity = nutritionData.AlternateServingSizeQuantity.AsOptional(),
                    AlternateServingSizeUnit = nutritionData.AlternateServingSizeUnit.AsOptional().Map(q => q.ToLower()),
                    Calories = nutritionData.Calories.AsOptional(),
                    CarbohydrateGrams = nutritionData.CarbohydrateGrams.AsOptional(),
                    FatGrams = nutritionData.FatGrams.AsOptional(),
                    ProteinGrams = nutritionData.ProteinGrams.AsOptional(),
                    FibreGrams = nutritionData.FibreGrams.AsOptional(),
                    SodiumGrams = nutritionData.SodiumGrams.AsOptional()
                };
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to parse nutrition data JSON: {ex.Message}. Raw response: {rawData}", ex);
            }
        }

    }
}
