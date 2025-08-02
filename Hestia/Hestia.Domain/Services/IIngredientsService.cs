using Haondt.Core.Models;
using Hestia.Core.Models;
using Hestia.Domain.Models;

namespace Hestia.Domain.Services
{
    public interface IIngredientsService
    {
        Task<(int Id, IngredientModel Ingredient)> CreateIngredientAsync(IngredientModel ingredient);
        Task<Result> DeleteIngredientAsync(int id);
        Task<Result<IngredientModel>> GetIngredientAsync(int id);
        Task<List<(int Id, IngredientModel Ingredient)>> GetIngredientsAsync(int page = 0, int pageSize = 20);
        Task<List<(int Id, IngredientModel Ingredient)>> SearchIngredientsAsync(NormalizedString searchTerm, int page = 0, int pageSize = 20);
        Task<IngredientModel> UpdateIngredientAsync(int id, IngredientModel ingredient);
    }
}
