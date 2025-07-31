using Haondt.Core.Models;
using Hestia.Core.Models;
using Hestia.Domain.Models;
using Hestia.Persistence;
using Hestia.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Hestia.Domain.Services;

public class UnitConversionsService(ApplicationDbContext dbContext) : IUnitConversionsService
{
    private static readonly MutexResource<UnitConversionGraph> _graphCache = new();

    public Task<List<UnitConversionDataModel>> GetAllDataModelsAsync() =>
        dbContext.UnitConversions.ToListAsync();

    private ValueTask<UnitConversionGraph> GetCachedGraphAsync() =>
        _graphCache.GetOrCreateAsync(async () =>
        {
            var dataModels = await GetAllDataModelsAsync();
            var (graph, _) = UnitConversionGraph.FromConversions(dataModels);
            return graph;
        });

    public async Task<List<UnitConversionModel>> GetAllAsync()
    {
        var dataModels = await GetAllDataModelsAsync();
        return dataModels.Select(UnitConversionModel.FromDataModel).ToList();
    }

    public async Task<DetailedResult<decimal, string>> ConvertAsync(NormalizedString fromUnit, NormalizedString toUnit, decimal amount)
    {
        var graph = await GetCachedGraphAsync();
        return graph.Convert(fromUnit, toUnit, amount);
    }

    public async Task<DetailedResult<List<UnitConversionModel>, string>> AddAsync(ICollection<UnitConversionModel> conversions)
    {
        var conversionList = conversions;
        if (conversionList.Count == 0)
            return new([]);

        var newDataModels = conversionList.Select(c => c.AsDataModel()).ToList();
        var existingConversions = await GetAllDataModelsAsync();
        var combinedConversions = existingConversions.Concat(newDataModels);
        var (testGraph, warnings) = UnitConversionGraph.FromConversions(combinedConversions);

        if (testGraph.HasCycle())
            return new($"Adding these conversions would create a cycle in the conversion graph");

        if (warnings.Count != 0)
            return new(string.Join("\n", warnings));

        await dbContext.UnitConversions.AddRangeAsync(newDataModels);
        await dbContext.SaveChangesAsync();
        _graphCache.Invalidate();

        return new(newDataModels.Select(UnitConversionModel.FromDataModel).ToList());
    }

    public async Task<DetailedResult<UnitConversionModel, string>> AddAsync(UnitConversionModel conversion)
    {
        var model = conversion.AsDataModel();
        if (model.NormalizedFromUnit == model.NormalizedToUnit)
            return new($"Cannot create conversion from unit '{conversion.FromUnit}' to itself");

        if (conversion.Multiplier == 0)
            return new("Cannot create conversion with a multiplier of zero");

        var existingConversions = await GetAllDataModelsAsync();
        var testConversions = existingConversions.Concat([model]);
        var (testGraph, warnings) = UnitConversionGraph.FromConversions(testConversions);
        if (warnings.Count != 0)
            return new(string.Join("\n", warnings));

        if (testGraph.HasCycle())
            return new($"Adding conversion from '{conversion.FromUnit}' to '{conversion.ToUnit}' would create a cycle in the conversion graph");

        dbContext.UnitConversions.Add(model);
        await dbContext.SaveChangesAsync();
        _graphCache.Invalidate();
        return new(UnitConversionModel.FromDataModel(model));
    }

    public async Task RemoveAsync(UnitConversionModel conversion)
    {
        var model = conversion.AsDataModel();
        if (await dbContext.UnitConversions.AnyAsync(c =>
            c.NormalizedFromUnit == model.NormalizedFromUnit && c.NormalizedToUnit == model.NormalizedToUnit))
        {
            dbContext.UnitConversions.Remove(model);
            await dbContext.SaveChangesAsync();
            _graphCache.Invalidate();
        }
    }

    public async Task<bool> CheckUnitCompatibilityAsync(NormalizedString unit1, NormalizedString unit2)
    {
        var graph = await GetCachedGraphAsync();
        return graph.Convert(unit1, unit2, 1m).IsSuccessful;
    }
}
