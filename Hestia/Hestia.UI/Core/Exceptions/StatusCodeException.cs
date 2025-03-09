namespace Hestia.UI.Core.Exceptions
{
    public class StatusCodeException(int statusCode, string? message, Exception? innerException) : Exception(message, innerException)
    {
        public int StatusCode { get; } = statusCode;
    }
}
