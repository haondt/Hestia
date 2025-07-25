using Haondt.Core.Models;

namespace Hestia.Core.Extensions
{
    public static class OptionalExtensions
    {
        public static Optional<T2> Bind<T1, T2>(this Optional<T1> optional, Func<T1, Optional<T2>> projection) where T1 : notnull where T2 : notnull =>
            optional.TryGetValue(out var value) ? projection(value) : new();
    }
}
