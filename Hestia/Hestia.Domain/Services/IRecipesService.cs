using Haondt.Core.Models;
using Hestia.Core.Models;
using Hestia.Domain.Models;

namespace Hestia.Domain.Services
{
    public interface IRecipesService
    {
        Task<Result<RecipeModel>> GetRecipeAsync(int id);
        Task<List<(int Id, RecipeModel Recipe)>> GetRecipesAsync(int offset = 0, int limit = 50);
        Task<(int Id, RecipeModel Recipe)> CreateRecipeAsync(RecipeModel recipe);
        Task<RecipeModel> UpdateRecipeAsync(int id, RecipeModel recipe);
        Task<Result> DeleteRecipeAsync(int id);
        Task<List<(int Id, RecipeModel Recipe)>> SearchRecipesAsync(NormalizedString query, int offset = 0, int limit = 50);
    }
}