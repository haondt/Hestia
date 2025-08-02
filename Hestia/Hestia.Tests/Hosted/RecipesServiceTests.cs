using Haondt.Core.Extensions;
using Hestia.Domain.Models;
using Hestia.Domain.Services;
using Hestia.Tests.Attributes;
using Hestia.Tests.Services;

namespace Hestia.Tests.Hosted
{
    public class RecipesServiceTests : IAsyncLifetime
    {
        private RecipesService _recipesService = default!;
        private IngredientsService _ingredientsService = default!;
        private ContextReference _context = default!;

        public async Task InitializeAsync()
        {
            _context = await ApplicationDbContextHelper.CreateContextAsync();
            _recipesService = new RecipesService(_context.Context);
            _ingredientsService = new IngredientsService(_context.Context);
        }

        public Task DisposeAsync()
        {
            _context.Dispose();
            return Task.CompletedTask;
        }

        [Theory, HestiaAutoData]
        public async Task CanCreateAndRetrieveRecipe(RecipeModel recipe, RecipeModel recipe2)
        {
            var (id, _) = await _recipesService.CreateRecipeAsync(recipe);
            var (id2, _) = await _recipesService.CreateRecipeAsync(recipe2);
            Assert.NotEqual(id, id2);

            var result = await _recipesService.GetRecipeAsync(id);
            var result2 = await _recipesService.GetRecipeAsync(id2);

            Assert.True(result.IsSuccessful);
            Assert.True(result2.IsSuccessful);
            Assert.Equal(recipe.Title, result.Value.Title);
            Assert.Equal(recipe2.Title, result2.Value.Title);
        }


        [Theory, HestiaAutoData]
        public async Task CreatingRecipeWithNewIngredientsCreatesIngredients(RecipeModel recipe, RecipeIngredientModel recipeIngredient, IngredientModel ingredient)
        {
            var (ingredientId, _) = await _ingredientsService.CreateIngredientAsync(ingredient);

            recipe = recipe with
            {
                Ingredients = new List<RecipeIngredientModel>
                {
                    recipeIngredient with
                    {
                        IngredientId = ingredientId
                    },
                    recipeIngredient with
                    {
                        Name = "New Ingredient"
                    }
                }
            };
            var (id, returned) = await _recipesService.CreateRecipeAsync(recipe);

            Assert.Equal(2, returned.Ingredients.Count);

            Assert.Equal(ingredientId, returned.Ingredients[0].IngredientId.Value);
            Assert.Equal(ingredient.Name, returned.Ingredients[0].IngredientName.Value);
            // original name is retained, UI layer can decide which to display
            Assert.Equal(recipeIngredient.Name, returned.Ingredients[0].Name);

            Assert.True(returned.Ingredients[1].IngredientId.HasValue);
            Assert.Equal("New Ingredient", returned.Ingredients[1].IngredientName.Value);
            Assert.Equal("New Ingredient", returned.Ingredients[1].Name);

            Assert.Equal(2, recipe.Ingredients.Count);

            var getRecipeResult = await _recipesService.GetRecipeAsync(id);
            Assert.True(getRecipeResult.TryGetValue(out var result));
            Assert.Equal(ingredientId, result.Ingredients[0].IngredientId.Value);
            Assert.Equal(ingredient.Name, result.Ingredients[0].IngredientName.Value);
            // original name is retained, UI layer can decide which to display
            Assert.Equal(recipeIngredient.Name, result.Ingredients[0].Name);

            Assert.True(result.Ingredients[1].IngredientId.HasValue);
            Assert.Equal("New Ingredient", result.Ingredients[1].IngredientName.Value);
            Assert.Equal("New Ingredient", result.Ingredients[1].Name);


            var createdIngredient = await _ingredientsService.GetIngredientAsync(recipe.Ingredients[1].IngredientId.Value);
            Assert.True(createdIngredient.IsSuccessful);
            Assert.Equal("New Ingredient", createdIngredient.Value.Name);
        }

        [Theory, HestiaAutoData]
        public async Task DeletingIngredientDoesNotDeleteRecipe(RecipeModel recipe, RecipeIngredientModel recipeIngredient, IngredientModel ingredient)
        {
            var (ingredientId, _) = await _ingredientsService.CreateIngredientAsync(ingredient);

            recipe = recipe with
            {
                Ingredients = new List<RecipeIngredientModel>
                {
                    recipeIngredient with
                    {
                        IngredientId = ingredientId
                    }
                }
            };
            var (id, recipeModel) = await _recipesService.CreateRecipeAsync(recipe);
            Assert.Equal(ingredientId, recipeModel.Ingredients[0].IngredientId.Value);
            Assert.Equal(ingredient.Name, recipeModel.Ingredients[0].IngredientName.Value);
            Assert.Equal(recipeIngredient.Name, recipeModel.Ingredients[0].Name);

            Assert.True((await _ingredientsService.DeleteIngredientAsync(ingredientId)).IsSuccessful);
            var deletedIngredient = await _ingredientsService.GetIngredientAsync(ingredientId);
            Assert.False(deletedIngredient.IsSuccessful);

            var getRecipeResult = await _recipesService.GetRecipeAsync(id);
            Assert.True(getRecipeResult.TryGetValue(out recipeModel));
            Assert.False(recipeModel.Ingredients[0].IngredientId.HasValue);
            Assert.False(recipeModel.Ingredients[0].IngredientName.HasValue);
            Assert.Equal(recipeIngredient.Name, recipeModel.Ingredients[0].Name);
        }

        [Theory, HestiaAutoData]
        public async Task DeletingRecipeDoesNotDeleteIngredient(IngredientModel ingredient, RecipeIngredientModel recipeIngredient, RecipeModel recipe)
        {
            var (ingredientId, _) = await _ingredientsService.CreateIngredientAsync(ingredient);

            recipe = recipe with
            {
                Ingredients = new List<RecipeIngredientModel>
                {
                    recipeIngredient with
                    {
                        IngredientId = ingredientId,
                    }
                }
            };

            var (recipeId, _) = await _recipesService.CreateRecipeAsync(recipe);

            var deleteResult = await _recipesService.DeleteRecipeAsync(recipeId);
            Assert.True(deleteResult.IsSuccessful);

            var ingredientResult = await _ingredientsService.GetIngredientAsync(ingredientId);
            Assert.True(ingredientResult.IsSuccessful);
            Assert.Equal(ingredient.Name, ingredientResult.Value.Name);
        }

        [Theory, HestiaAutoData]
        public async Task UpdatingRecipeByDeletingIngredients(RecipeModel recipe, IngredientModel ingredient1, IngredientModel ingredient2, RecipeIngredientModel recipeIngredient1, RecipeIngredientModel recipeIngredient2, RecipeIngredientModel recipeIngredient3, RecipeIngredientModel recipeIngredient4)
        {
            // Create 2 existing ingredients
            var (existingId1, _) = await _ingredientsService.CreateIngredientAsync(ingredient1);
            var (existingId2, _) = await _ingredientsService.CreateIngredientAsync(ingredient2);

            // Create recipe with 2 existing and 2 new ingredients
            recipe = recipe with
            {
                Ingredients = new List<RecipeIngredientModel>
                {
                    recipeIngredient1 with { IngredientId = existingId1 },
                    recipeIngredient2 with { IngredientId = existingId2 },
                    recipeIngredient3 with { Name = "New Ingredient 1" },
                    recipeIngredient4 with { Name = "New Ingredient 2" }
                }
            };

            var (recipeId, createdRecipe) = await _recipesService.CreateRecipeAsync(recipe);
            Assert.Equal(4, createdRecipe.Ingredients.Count);

            // Update recipe by removing one existing and one new ingredient
            var updatedRecipe = createdRecipe with
            {
                Ingredients = new List<RecipeIngredientModel>
                {
                    createdRecipe.Ingredients[0], // Keep first existing
                    createdRecipe.Ingredients[3]  // Keep second new
                }
            };

            var result = await _recipesService.UpdateRecipeAsync(recipeId, updatedRecipe);
            Assert.Equal(2, result.Ingredients.Count);
            Assert.Equal(existingId1, result.Ingredients[0].IngredientId.Value);
            Assert.Equal("New Ingredient 2", result.Ingredients[1].Name);
        }

        [Theory, HestiaAutoData]
        public async Task UpdatingRecipeByAddingIngredients(RecipeModel recipe, IngredientModel ingredient1, IngredientModel ingredient2, IngredientModel ingredient3, RecipeIngredientModel recipeIngredient1, RecipeIngredientModel recipeIngredient2, RecipeIngredientModel recipeIngredient3, RecipeIngredientModel recipeIngredient4, RecipeIngredientModel recipeIngredient5, RecipeIngredientModel recipeIngredient6)
        {
            // Create 2 existing ingredients
            var (existingId1, _) = await _ingredientsService.CreateIngredientAsync(ingredient1);
            var (existingId2, _) = await _ingredientsService.CreateIngredientAsync(ingredient2);
            var (existingId3, _) = await _ingredientsService.CreateIngredientAsync(ingredient3);

            // Create recipe with 2 existing and 2 new ingredients
            recipe = recipe with
            {
                Ingredients = new List<RecipeIngredientModel>
                {
                    recipeIngredient1 with { IngredientId = existingId1 },
                    recipeIngredient2 with { IngredientId = existingId2 },
                    recipeIngredient3 with { Name = "New Ingredient 1" },
                    recipeIngredient4 with { Name = "New Ingredient 2" }
                }
            };

            var (recipeId, createdRecipe) = await _recipesService.CreateRecipeAsync(recipe);
            Assert.Equal(4, createdRecipe.Ingredients.Count);

            // Update recipe by adding one more existing and one more new ingredient
            var updatedRecipe = createdRecipe with
            {
                Ingredients = new List<RecipeIngredientModel>
                {
                    createdRecipe.Ingredients[0], // Keep existing ingredients
                    createdRecipe.Ingredients[1],
                    createdRecipe.Ingredients[2],
                    createdRecipe.Ingredients[3],
                    recipeIngredient5 with { IngredientId = existingId3 }, // Add existing
                    recipeIngredient6 with { Name = "New Ingredient 3" }   // Add new
                }
            };

            var result = await _recipesService.UpdateRecipeAsync(recipeId, updatedRecipe);
            Assert.Equal(6, result.Ingredients.Count);
            Assert.Equal(existingId3, result.Ingredients[4].IngredientId.Value);
            Assert.Equal("New Ingredient 3", result.Ingredients[5].Name);
        }

        [Theory, HestiaAutoData]
        public async Task UpdatingRecipeDescriptionOnly(RecipeModel recipe, IngredientModel ingredient1, IngredientModel ingredient2, RecipeIngredientModel recipeIngredient1, RecipeIngredientModel recipeIngredient2, RecipeIngredientModel recipeIngredient3, RecipeIngredientModel recipeIngredient4)
        {
            // Create 2 existing ingredients
            var (existingId1, _) = await _ingredientsService.CreateIngredientAsync(ingredient1);
            var (existingId2, _) = await _ingredientsService.CreateIngredientAsync(ingredient2);

            // Create recipe with 2 existing and 2 new ingredients
            recipe = recipe with
            {
                Ingredients = new List<RecipeIngredientModel>
                {
                    recipeIngredient1 with { IngredientId = existingId1 },
                    recipeIngredient2 with { IngredientId = existingId2 },
                    recipeIngredient3 with { Name = "New Ingredient 1" },
                    recipeIngredient4 with { Name = "New Ingredient 2" }
                }
            };

            var (recipeId, createdRecipe) = await _recipesService.CreateRecipeAsync(recipe);
            Assert.Equal(4, createdRecipe.Ingredients.Count);

            // Update only the description, keep ingredients the same
            var updatedRecipe = createdRecipe with
            {
                Description = "Updated description".AsOptional()
            };

            var result = await _recipesService.UpdateRecipeAsync(recipeId, updatedRecipe);
            Assert.Equal("Updated description", result.Description.Value);
            Assert.Equal(4, result.Ingredients.Count);
            Assert.Equal(existingId1, result.Ingredients[0].IngredientId.Value);
            Assert.Equal(existingId2, result.Ingredients[1].IngredientId.Value);
            Assert.Equal("New Ingredient 1", result.Ingredients[2].Name);
            Assert.Equal("New Ingredient 2", result.Ingredients[3].Name);
        }

    }
}
