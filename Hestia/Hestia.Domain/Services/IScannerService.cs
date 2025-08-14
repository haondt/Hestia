namespace Hestia.Domain.Services
{
    public interface IScannerService<T>
    {
        Guid StartBackgroundProcessing(Stream imageStream);
    }
}
