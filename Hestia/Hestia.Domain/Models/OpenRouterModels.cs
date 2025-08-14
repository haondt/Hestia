namespace Hestia.Domain.Models
{
    // Response models for OpenRouter API
    public class OpenRouterResponse
    {
        public OpenRouterChoice[]? Choices { get; set; }
    }

    public class OpenRouterChoice
    {
        public OpenRouterMessage? Message { get; set; }
    }

    public class OpenRouterMessage
    {
        public string? Content { get; set; }
    }

    // LLM response data models
    public class LLMNutritionData
    {
        public decimal? ServingSizeQuantity { get; set; }
        public string? ServingSizeUnit { get; set; }
        public decimal? AlternateServingSizeQuantity { get; set; }
        public string? AlternateServingSizeUnit { get; set; }
        public decimal? Calories { get; set; }
        public decimal? CarbohydrateGrams { get; set; }
        public decimal? FatGrams { get; set; }
        public decimal? ProteinGrams { get; set; }
        public decimal? FibreGrams { get; set; }
        public decimal? SodiumGrams { get; set; }
    }

    public class LLMPackagingData
    {
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public decimal? PackageSize { get; set; }
        public string? PackageSizeUnit { get; set; }
    }
}