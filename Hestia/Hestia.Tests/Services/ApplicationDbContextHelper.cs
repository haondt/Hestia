using Hestia.Domain.Services;
using Hestia.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Hestia.Tests.Services
{
    public class ApplicationDbContextHelper
    {
        public static async Task<ContextReference> CreateContextAsync()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            var contextOptions = new DbContextOptionsBuilder<SqliteApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new SqliteApplicationDbContext(contextOptions);
            context.Database.Migrate();

            var unitConverter = new UnitConversionsService(context);
            var seeder = new DbSeeder(context, unitConverter);
            await seeder.SeedAsync();

            return new ContextReference { Context = new SqliteApplicationDbContext(contextOptions) };
        }
    }

    public record ContextReference : IDisposable
    {
        public required ApplicationDbContext Context { get; init; }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}
