using Haondt.Core.Models;
using Hestia.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Hestia.Domain.Services
{
    public class NutritionLabelProcessingStateService(IMemoryCache cache, IOptions<NutritionLabelScannerSettings> settings) : INutritionLabelProcessingStateService
    {
        private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(settings.Value.ProcessingCacheTimeoutMinutes);

        public Guid StartProcessing()
        {
            var id = Guid.NewGuid();
            UpdateProcessingStatus(id, "Processing...");
            return id;
        }

        public Result<Union<string, DetailedResult<ScannedNutritionLabel, string>>> GetProcessingResult(Guid id)
        {
            if (cache.TryGetValue<Union<string, DetailedResult<ScannedNutritionLabel, string>>>(id, out var value) && value != null)
                return new(value);
            return default;
        }

        public void SetProcessingResult(Guid id, ScannedNutritionLabel result)
        {
            cache.Set(id, new Union<string, DetailedResult<ScannedNutritionLabel, string>>(new DetailedResult<ScannedNutritionLabel, string>(result)), _cacheTimeout);
        }

        public void SetProcessingFailure(Guid id, string reason)
        {
            cache.Set(id, new Union<string, DetailedResult<ScannedNutritionLabel, string>>(new DetailedResult<ScannedNutritionLabel, string>(reason)), _cacheTimeout);
        }

        public void UpdateProcessingStatus(Guid id, string status)
        {
            cache.Set(id, new Union<string, DetailedResult<ScannedNutritionLabel, string>>(status), _cacheTimeout);
        }
    }
}