using Haondt.Core.Extensions;
using Hestia.Persistence.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Hestia.Persistence.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHestiaPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PersistenceSettings>(configuration.GetSection(nameof(PersistenceSettings)));
            var persistenceSettings = configuration.GetSection<PersistenceSettings>();

            switch (persistenceSettings.Driver)
            {
                case StorageDrivers.Memory:
                    services.AddDbContext<ApplicationDbContext>(o =>
                        o.UseInMemoryDatabase("Hestia"));
                    break;

                case StorageDrivers.Sqlite:
                    var sqliteConnection = new SqliteConnectionStringBuilder
                    {
                        DataSource = persistenceSettings.Sqlite!.FilePath
                    }.ToString();

                    services.AddDbContext<SqliteApplicationDbContext>(o =>
                        o.UseSqlite(sqliteConnection));
                    services.AddScoped<ApplicationDbContext>(sp =>
                        sp.GetRequiredService<SqliteApplicationDbContext>());
                    break;
            }

            return services;
        }
    }
}
