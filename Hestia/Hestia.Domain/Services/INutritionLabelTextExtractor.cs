namespace Hestia.Domain.Services
{
    public interface INutritionLabelTextExtractor
    {
        public Task<string> ExtractTextAsync(Stream imageData, Guid processingId);
    }
}
