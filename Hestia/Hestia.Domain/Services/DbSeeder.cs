using Hestia.Domain.Models;
using Hestia.Persistence;
using Hestia.Persistence.Models;

namespace Hestia.Domain.Services;

public class DbSeeder(ApplicationDbContext dbContext, IUnitConversionsService unitConversionsService) : IDbSeeder
{
    public async Task SeedAsync()
    {
        var state = await HestiaStateDataModel.GetOrCreateAsync(dbContext);
        if (state.HasSeededUnitConversions)
            return;

        var conversions = new List<UnitConversionModel>
        {
            // Mass units
            new() { FromUnit = "g", ToUnit = "kg", Multiplier = 0.001m },
            new() { FromUnit = "kg", ToUnit = "lbs", Multiplier = 2.20462m },
            new() { FromUnit = "lbs", ToUnit = "oz", Multiplier = 16m },

            // Metric units
            new() { FromUnit = "ml", ToUnit = "l", Multiplier = 0.001m },
            new() { FromUnit = "tbsp", ToUnit = "tsp", Multiplier = 3m },
            new() { FromUnit = "tsp", ToUnit = "ml", Multiplier = 5m },

            // Canadian units
            new() { FromUnit = "cup", ToUnit = "ml", Multiplier = 250m },

            // U.S. Customary units
            new() { FromUnit = "cup", ToUnit = "fl oz", Multiplier = 8 },
            new() { FromUnit = "pint", ToUnit = "fl oz", Multiplier = 16 },
            new() { FromUnit = "quart", ToUnit = "fl oz", Multiplier = 32 },
            new() { FromUnit = "gallon", ToUnit = "fl oz", Multiplier = 128 },
        };

        var result = await unitConversionsService.AddAsync(conversions);
        if (!result.IsSuccessful)
            throw new InvalidOperationException($"Failed to seed unit conversions: {result.Reason}");

        state = await HestiaStateDataModel.GetOrCreateAsync(dbContext);
        state.HasSeededUnitConversions = true;
        await dbContext.SaveChangesAsync();
    }
}
