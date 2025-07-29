using Haondt.Core.Models;
using Hestia.Core.Models;
using Hestia.Domain.Models;

namespace Hestia.Domain.Services
{
    public interface IIngredientsService
    {
        Task<(int Id, IngredientModel Ingredient)> CreateIngredientAsync(IngredientModel ingredient);
        Task<Result<IngredientModel>> GetIngredientAsync(int id);
        Task<List<(int Id, IngredientModel Ingredient)>> GetIngredientsAsync();
        Task<List<(int Id, IngredientModel Ingredient)>> SearchIngredientsAsync(NormalizedString searchTerm);
        Task<IngredientModel> UpdateIngredientAsync(int id, IngredientModel ingredient);
    }
}
