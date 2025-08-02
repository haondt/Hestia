using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Hestia.Core.Models;
using Hestia.Domain.Models;
using Hestia.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hestia.Domain.Services
{
    public class IngredientsService(ApplicationDbContext dbContext) : IIngredientsService
    {
        public async Task<(int Id, IngredientModel Ingredient)> CreateIngredientAsync(IngredientModel ingredient)
        {
            var dataModel = dbContext.Ingredients.Add(ingredient.AsDataModel());

            await dbContext.SaveChangesAsync();
            return (dataModel.Entity.Id, IngredientModel.FromDataModel(dataModel.Entity));
        }

        public async Task<IngredientModel> UpdateIngredientAsync(int id, IngredientModel ingredient)
        {
            var dataModel = dbContext.Ingredients.Update(ingredient.AsDataModel() with
            {
                Id = id
            });
            await dbContext.SaveChangesAsync();
            return IngredientModel.FromDataModel(dataModel.Entity);
        }

        public async Task<List<(int Id, IngredientModel Ingredient)>> GetIngredientsAsync(int page = 0, int pageSize = 20)
        {
            var dataModels = await dbContext.Ingredients
                .OrderBy(i => i.Id)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return dataModels.Select(m => (m.Id, IngredientModel.FromDataModel(m))).ToList();
        }

        public async Task<Result> DeleteIngredientAsync(int id)
        {
            var ingredient = await dbContext.Ingredients.FindAsync(id);
            if (ingredient == null)
                return Result.Failure;

            dbContext.Ingredients.Remove(ingredient);
            await dbContext.SaveChangesAsync();
            return Result.Success;
        }

        public async Task<Result<IngredientModel>> GetIngredientAsync(int id)
        {
            var dataModel = await dbContext.Ingredients.FirstOrDefaultAsync(i => i.Id == id);
            return dataModel.AsOptional().Map(IngredientModel.FromDataModel).AsResult();
        }

        public async Task<List<(int Id, IngredientModel Ingredient)>> SearchIngredientsAsync(NormalizedString searchTerm, int page = 0, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetIngredientsAsync(page, pageSize);

            var dataModels = await dbContext.Ingredients
                .Where(i => i.NormalizedName.Contains(searchTerm))
                .OrderBy(i => i.Id)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return dataModels.Select(m => (m.Id, IngredientModel.FromDataModel(m))).ToList();
        }
    }
}
