namespace Hestia.Domain.Models
{
    public class NutritionLabelScannerSettings
    {
        public bool Enabled { get; set; } = false;
        public DocumentAISettings? DocumentAI { get; set; } = null;
        public int ProcessingCacheTimeoutMinutes { get; set; } = 5;
    }

    public class DocumentAISettings
    {
        public required string ProjectId { get; set; } = "gabbro-417317";
        public required string ProcessorLocationId { get; set; } = "us";
        public required string ProcessorId { get; set; } = "44aecde7a9f75dc9";
    }
}
