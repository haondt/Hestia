namespace Hestia.Domain.Services
{
    public class DisabledNutritionLabelScannerService : INutritionLabelScannerService
    {
        public Guid StartBackgroundProcessing(Stream imageStream)
        {
            throw new NotSupportedException("Nutrition label scanning is disabled.");
        }
    }
}
