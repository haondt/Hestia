namespace Hestia.Domain.Services
{
    public interface IScanTextExtractor<T>
    {
        public Task<string> ExtractTextAsync(Stream imageData, Guid processingId);
    }
}
