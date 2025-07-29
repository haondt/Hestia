using Hestia.Domain.Models;
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

        public static Task DevSeedDbAsync(this IServiceProvider serviceProvider, Action<DevSeedOptions> configure)
        {
            using var scope = serviceProvider.CreateScope();

            var options = new DevSeedOptions();
            configure(options);

            var seeder = scope.ServiceProvider.GetRequiredService<IDevDbSeeder>();
            return seeder.SeedAsync(options);
        }
    }
}
