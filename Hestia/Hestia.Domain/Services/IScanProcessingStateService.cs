using Haondt.Core.Models;

namespace Hestia.Domain.Services
{
    public interface IScanProcessingStateService<T>
    {
        Guid StartProcessing();
        void SetProcessingResult(Guid id, T result);
        void SetProcessingFailure(Guid id, string reason);
        void UpdateProcessingStatus(Guid id, string status);
        Result<Union<string, DetailedResult<T, string>>> GetProcessingResult(Guid id);
    }
}