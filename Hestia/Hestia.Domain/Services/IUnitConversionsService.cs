using Haondt.Core.Models;
using Hestia.Core.Models;
using Hestia.Domain.Models;
using Hestia.Persistence.Models;

namespace Hestia.Domain.Services
{
    public interface IUnitConversionsService
    {
        Task<List<UnitConversionModel>> GetAllAsync();
        Task<List<UnitConversionDataModel>> GetAllDataModelsAsync();
        Task<DetailedResult<UnitConversionModel, string>> AddAsync(UnitConversionModel conversion);
        Task RemoveAsync(UnitConversionModel conversion);
        Task<DetailedResult<decimal, string>> ConvertAsync(NormalizedString fromUnit, NormalizedString toUnit, decimal amount);
        Task<DetailedResult<List<UnitConversionModel>, string>> AddAsync(ICollection<UnitConversionModel> conversions);
        Task<bool> CheckUnitCompatibilityAsync(NormalizedString unit1, NormalizedString unit2);
    }
}
