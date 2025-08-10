using Haondt.Core.Models;
using Hestia.Domain.Models;

namespace Hestia.Domain.Services
{
    public interface INutritionLabelProcessingStateService
    {
        Guid StartProcessing();
        void SetProcessingResult(Guid id, ScannedNutritionLabel result);
        void SetProcessingFailure(Guid id, string reason);
        void UpdateProcessingStatus(Guid id, string status);
        Result<Union<string, DetailedResult<ScannedNutritionLabel, string>>> GetProcessingResult(Guid id);
    }
}