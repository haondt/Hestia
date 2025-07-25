using Haondt.Core.Models;
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

        public async Task<List<IngredientModel>> GetIngredientsAsync()
        {
            var dataModels = await dbContext.Ingredients.ToListAsync();
            return dataModels.Select(IngredientModel.FromDataModel).ToList();
        }

        public async Task<Result<IngredientModel>> GetIngredientAsync(int id)
        {
            var dataModel = await dbContext.Ingredients.FirstOrDefaultAsync(i => i.Id == id);
            return dataModel is null ? new() : new(IngredientModel.FromDataModel(dataModel));
        }
    }
}
