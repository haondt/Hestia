namespace Hestia.Domain.Services
{
    public interface IScanTextTransformer<T>
    {
        public Task<T> TransformText(string text, Guid processingId);
    }
}
