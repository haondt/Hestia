using Hestia.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hestia.Domain.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static Task SeedDbAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var seeder = scope.ServiceProvider.GetRequiredService<IDbSeeder>();
            return seeder.SeedAsync();
        }
    }
}
