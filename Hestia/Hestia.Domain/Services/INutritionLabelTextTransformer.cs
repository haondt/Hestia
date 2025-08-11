using Hestia.Domain.Models;

namespace Hestia.Domain.Services
{
    public interface INutritionLabelTextTransformer
    {
        public Task<ScannedNutritionLabel> TransformText(string text, Guid processingId);
    }
}
