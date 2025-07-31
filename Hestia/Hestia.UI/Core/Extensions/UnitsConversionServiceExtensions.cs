using Hestia.Domain.Models;
using Hestia.Domain.Services;

namespace Hestia.UI.Core.Extensions
{
    public static class UnitsConversionServiceExtensions
    {
        public static async Task<bool> CheckIngredientUnitCompatibilityAsync(this IUnitConversionsService unitConversionsService, IngredientModel ingredient, bool defaultResult = true)
        {

            if (!ingredient.PackageSizeUnit.TryGetValue(out var packageSizeUnit))
                return defaultResult;

            var leftUnits = new List<string> { packageSizeUnit };
            var rightUnits = new List<string>();
            if (ingredient.ServingSizeUnit.TryGetValue(out var servingSizeUnit))
                rightUnits.Add(servingSizeUnit);
            if (ingredient.AlternateServingSizeUnit.TryGetValue(out var alternateServingSizeUnit))
                rightUnits.Add(alternateServingSizeUnit);

            if (rightUnits.Count == 0)
                return defaultResult;

            return await unitConversionsService.CheckUnitCompatiblityAsync(leftUnits, rightUnits, defaultResult);
        }

        public static async Task<bool> CheckUnitCompatiblityAsync(this IUnitConversionsService unitConversionsService, IEnumerable<string> leftUnits, IEnumerable<string> rightUnits, bool defaultResult = true)
        {
            var leftUnitsList = leftUnits.Where(s => !string.IsNullOrEmpty(s)).ToList();
            var rightUnitsList = rightUnits.Where(s => !string.IsNullOrEmpty(s)).ToList();

            if (leftUnitsList.Count == 0 || rightUnitsList.Count == 0)
                return defaultResult;

            foreach (var leftUnit in leftUnitsList)
                foreach (var rightUnit in rightUnitsList)
                    if (await unitConversionsService.CheckUnitCompatibilityAsync(leftUnit, rightUnit))
                        return true;
            return false;
        }
    }
}
