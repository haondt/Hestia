using Haondt.Core.Extensions;
using Hestia.Domain.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Hestia.Domain.Services
{
    public class OpenRouterNutritionLabelTextTransformer : INutritionLabelTextTransformer
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly HttpClient _httpClient;
        private readonly OpenRouterSettings _settings;
        private readonly INutritionLabelProcessingStateService _stateService;

        public OpenRouterNutritionLabelTextTransformer(
            HttpClient httpClient,
            IOptions<NutritionLabelScannerSettings> options,
            INutritionLabelProcessingStateService stateService)
        {
            _httpClient = httpClient;
            _settings = options.Value.OpenRouter ?? throw new InvalidOperationException("OpenRouter settings not configured");
            _stateService = stateService;

            // Configure HttpClient for OpenRouter
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://hestia.haondt.dev");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "Hestia Nutrition Scanner");
        }

        public async Task<ScannedNutritionLabel> TransformText(string text, Guid processingId)
        {
            _stateService.UpdateProcessingStatus(processingId, "Analyzing nutrition data with LLM...");

            var prompt = $$"""
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

            var requestBody = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = _settings.Temperature,
                max_tokens = _settings.MaxTokens
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenRouter API error: {response.StatusCode} - {error}");
            }

            var responseText = await response.Content.ReadAsStringAsync();
            var openRouterResponse = JsonSerializer.Deserialize<OpenRouterResponse>(responseText, JsonOptions);

            var extractedJson = openRouterResponse?.Choices?[0]?.Message?.Content?.Trim();
            if (string.IsNullOrEmpty(extractedJson))
                throw new Exception("No content returned from OpenRouter");

            // Clean up the JSON (remove markdown code blocks if present)
            if (extractedJson.StartsWith("```"))
            {
                var lines = extractedJson.Split('\n');
                extractedJson = string.Join('\n', lines.Skip(1).SkipLast(1));
            }

            try
            {
                var nutritionData = JsonSerializer.Deserialize<LLMNutritionData>(extractedJson, JsonOptions);
                return MapToScannedNutritionLabel(nutritionData!);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse nutrition data JSON: {ex.Message}. Raw response: {extractedJson}");
            }
        }

        private static ScannedNutritionLabel MapToScannedNutritionLabel(LLMNutritionData data)
        {
            return new ScannedNutritionLabel
            {
                ServingSizeQuantity = data.ServingSizeQuantity.AsOptional(),
                ServingSizeUnit = data.ServingSizeUnit.AsOptional().Map(q => q.ToLower()),
                AlternateServingSizeQuantity = data.AlternateServingSizeQuantity.AsOptional(),
                AlternateServingSizeUnit = data.AlternateServingSizeUnit.AsOptional().Map(q => q.ToLower()),
                Calories = data.Calories.AsOptional(),
                CarbohydrateGrams = data.CarbohydrateGrams.AsOptional(),
                FatGrams = data.FatGrams.AsOptional(),
                ProteinGrams = data.ProteinGrams.AsOptional(),
                FibreGrams = data.FibreGrams.AsOptional(),
                SodiumGrams = data.SodiumGrams.AsOptional()
            };
        }
    }
}