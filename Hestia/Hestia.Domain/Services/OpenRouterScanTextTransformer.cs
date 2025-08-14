using Hestia.Domain.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Hestia.Domain.Services
{
    public abstract class OpenRouterScanTextTransformer<T> : IScanTextTransformer<T>
    {

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        private readonly HttpClient _httpClient;
        private readonly OpenRouterSettings _settings;
        private readonly IScanProcessingStateService<T> _stateService;

        public OpenRouterScanTextTransformer(
            HttpClient httpClient,
            IOptions<ScannerSettings> options,
            IScanProcessingStateService<T> stateService)
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
        protected abstract string GetPrompt(string text);
        protected abstract T MapToEntity(string responseText);

        public async Task<T> TransformText(string text, Guid processingId)
        {
            _stateService.UpdateProcessingStatus(processingId, "Analyzing data with LLM...");


            var requestBody = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new { role = "user", content = GetPrompt(text) }
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
            var openRouterResponse = JsonSerializer.Deserialize<OpenRouterResponse>(responseText, _jsonOptions);

            responseText = openRouterResponse?.Choices?[0]?.Message?.Content?.Trim();
            if (string.IsNullOrEmpty(responseText))
                throw new Exception("No content returned from OpenRouter");

            return MapToEntity(responseText);
        }

    }
}