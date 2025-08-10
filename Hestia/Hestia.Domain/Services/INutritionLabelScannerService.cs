namespace Hestia.Domain.Services
{
    public interface INutritionLabelScannerService
    {
        Guid StartBackgroundProcessing(Stream imageStream);
    }
}
