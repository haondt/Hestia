using Hestia.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Hestia.Persistence.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static void PerformDatabaseMigrations(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var persistenceSettings = scope.ServiceProvider.GetRequiredService<IOptions<PersistenceSettings>>();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (persistenceSettings.Value.DropDatabaseOnStartup)
                db.Database.EnsureDeleted();

            switch (persistenceSettings.Value.Driver)
            {
                case StorageDrivers.Sqlite:
                    db.Database.Migrate();
                    break;
                default:
                    db.Database.EnsureCreated();
                    break;
            }
        }
    }
}
