using Hestia.Domain.Models;

namespace Hestia.Domain.Services;

public interface IDevDbSeeder
{
    Task SeedAsync(DevSeedOptions options);
}
