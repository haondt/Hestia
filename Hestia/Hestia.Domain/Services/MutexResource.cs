using Haondt.Core.Models;

namespace Hestia.Domain.Services;

public class MutexResource<T> where T : notnull
{
    private Optional<T> _value = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async ValueTask<T> GetOrCreateAsync(Func<Task<T>> valueFactory)
    {
        if (_value.TryGetValue(out var cached))
            return cached;

        await _semaphore.WaitAsync();
        try
        {
            if (_value.TryGetValue(out cached))
                return cached;

            var newValue = await valueFactory();
            _value = new(newValue);
            return newValue;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Invalidate()
    {
        _value = new();
    }
}
