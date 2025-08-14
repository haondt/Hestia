namespace Hestia.Domain.Models
{
    public class ScannerSettings
    {
        public bool Enabled { get; set; } = false;
        public bool SaveTrainingData { get; set; } = false;
        public int ProcessingCacheTimeoutMinutes { get; set; } = 5;

        public required OcrProvider OcrProvider { get; set; }
        public required LlmProvider LlmProvider { get; set; }

        public DocumentAISettings? DocumentAI { get; set; }
        public OpenRouterSettings? OpenRouter { get; set; }
    }

    public enum OcrProvider
    {
        DocumentAI,
        CloudVision
    }
    public enum LlmProvider
    {
        OpenRouter
    }
    public class DocumentAISettings
    {
        public required string ProjectId { get; set; }
        public required string ProcessorLocationId { get; set; }
        public required string ProcessorId { get; set; }
    }

    public class OpenRouterSettings
    {
        public required string ApiKey { get; set; }
        public required string Model { get; set; }
        public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";
        public double Temperature { get; set; } = 0.0;
        public int MaxTokens { get; set; } = 1000;
    }

}
