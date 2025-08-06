using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Hestia.Core.Models;
using Hestia.Domain.Models;
using Hestia.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hestia.Domain.Services
{
    public class RecipesService(ApplicationDbContext dbContext) : IRecipesService
    {

        public async Task<Result<RecipeModel>> GetRecipeAsync(int id)
        {
            var dataModel = await dbContext.Recipes
                .Include(r => r.Ingredients)
                    .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id);
            return dataModel.AsOptional().Map(RecipeModel.FromDataModel).AsResult();
        }

        public async Task<List<(int Id, RecipeModel Recipe)>> GetRecipesAsync(int page = 0, int pageSize = 50)
        {
            var dataModels = await dbContext.Recipes
                .Include(r => r.Ingredients)
                    .ThenInclude(ri => ri.Ingredient)
                .OrderBy(r => r.Title)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return dataModels.Select(m => (m.Id, RecipeModel.FromDataModel(m))).ToList();
        }

        public async Task<(int Id, RecipeModel Recipe)> CreateRecipeAsync(RecipeModel recipe)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            // create nonexistent ingredients
            foreach (var ingredient in recipe.Ingredients)
            {
                if (!ingredient.IngredientId.HasValue)
                {
                    var newIngredient = dbContext.Ingredients.Add(ingredient.CreateIngredientDataModel());
                    await dbContext.SaveChangesAsync();
                    ingredient.IngredientName = newIngredient.Entity.Name;
                    ingredient.IngredientId = newIngredient.Entity.Id;
                }
            }

            var dataModel = dbContext.Recipes.Add(recipe.AsDataModel());
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // need to reload the recipe to get ingredient info
            var createdModel = await GetRecipeAsync(dataModel.Entity.Id);
            return (dataModel.Entity.Id, createdModel.Value
                ?? throw new InvalidOperationException("Created recipe not found after saving to database."));
        }

        public async Task<RecipeModel> UpdateRecipeAsync(int id, RecipeModel recipe)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            //var existing = await dbContext.Recipes.FindAsync(id)
            var existing = await dbContext.Recipes
                .Include(r => r.Ingredients)
                .FirstAsync(r => r.Id == id)
                ?? throw new InvalidOperationException($"Recipe with ID {id} does not exist.");

            // create nonexistent ingredients
            foreach (var ingredient in recipe.Ingredients)
            {
                if (!ingredient.IngredientId.HasValue)
                {
                    var newIngredient = dbContext.Ingredients.Add(ingredient.CreateIngredientDataModel());
                    await dbContext.SaveChangesAsync();
                    ingredient.IngredientName = newIngredient.Entity.Name;
                    ingredient.IngredientId = newIngredient.Entity.Id;
                }
            }

            recipe.ApplyUpdate(existing);
            var dataModel = dbContext.Recipes.Update(existing);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // need to reload the recipe to get ingredient info
            var updatedModel = await GetRecipeAsync(dataModel.Entity.Id);
            return updatedModel.Value
                ?? throw new InvalidOperationException("Created recipe not found after saving to database.");
        }

        public async Task<Result> DeleteRecipeAsync(int id)
        {
            var recipe = await dbContext.Recipes.FindAsync(id);
            if (recipe == null)
                return Result.Failure;

            dbContext.Recipes.Remove(recipe);
            await dbContext.SaveChangesAsync();
            return Result.Success;
        }

        public async Task<List<(int Id, RecipeModel Recipe)>> SearchRecipesAsync(NormalizedString query, int offset = 0, int limit = 50)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetRecipesAsync(offset, limit);

            var dataModels = await dbContext.Recipes
                .Include(r => r.Ingredients)
                    .ThenInclude(ri => ri.Ingredient)
                //.Where(r => r.NormalizedTitle.Contains(query) ||
                //           (r.Description != null && r.NormalizedDescription.Contains(query)))
                .Where(r => r.NormalizedTitle.Contains(query))
                .OrderByDescending(r => r.Id)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return dataModels.Select(m => (m.Id, RecipeModel.FromDataModel(m))).ToList();
        }

    }
}