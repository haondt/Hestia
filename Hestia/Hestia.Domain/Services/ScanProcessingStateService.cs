using Haondt.Core.Models;
using Hestia.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Hestia.Domain.Services
{
    public class ScanProcessingStateService<T>(IMemoryCache cache, IOptions<ScannerSettings> settings) : IScanProcessingStateService<T>
    {
        private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(settings.Value.ProcessingCacheTimeoutMinutes);

        public Guid StartProcessing()
        {
            var id = Guid.NewGuid();
            UpdateProcessingStatus(id, "Processing...");
            return id;
        }

        public Result<Union<string, DetailedResult<T, string>>> GetProcessingResult(Guid id)
        {
            if (cache.TryGetValue<Union<string, DetailedResult<T, string>>>(id, out var value) && value != null)
                return new(value);
            return default;
        }

        public void SetProcessingResult(Guid id, T result)
        {
            cache.Set(id, new Union<string, DetailedResult<T, string>>(new DetailedResult<T, string>(result)), _cacheTimeout);
        }

        public void SetProcessingFailure(Guid id, string reason)
        {
            cache.Set(id, new Union<string, DetailedResult<T, string>>(new DetailedResult<T, string>(reason)), _cacheTimeout);
        }

        public void UpdateProcessingStatus(Guid id, string status)
        {
            cache.Set(id, new Union<string, DetailedResult<T, string>>(status), _cacheTimeout);
        }
    }
}