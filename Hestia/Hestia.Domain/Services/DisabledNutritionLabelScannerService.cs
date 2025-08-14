namespace Hestia.Domain.Services
{
    public class DisabledNutritionLabelScannerService<T> : IScannerService<T>
    {
        public Guid StartBackgroundProcessing(Stream imageStream)
        {
            throw new NotSupportedException("Nutrition label scanning is disabled.");
        }
    }
}
