using Haondt.Core.Extensions;
using Haondt.Web.BulmaCSS.Services;
using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Hestia.Domain.Models;
using Hestia.Domain.Services;
using Hestia.UI.Core.Components;
using Hestia.UI.Core.Controllers;
using Hestia.UI.Library.Components.Element;
using Hestia.UI.Library.Components.Htmx;
using Hestia.UI.Recipes.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hestia.UI.Recipes.Controllers
{
    [Route("recipes")]
    public class RecipesController(IComponentFactory componentFactory, IIngredientsService ingredientsService, IRecipesService recipesService) : UIController(componentFactory)
    {
        private readonly IComponentFactory _componentFactory = componentFactory;
        private readonly IRecipesService _recipesService = recipesService;

        [HttpGet("view/{recipeId}")]
        public async Task<IResult> GetRecipe(int recipeId)
        {
            var result = await _recipesService.GetRecipeAsync(recipeId);
            if (!result.TryGetValue(out var recipe))
                return await _componentFactory.RenderComponentAsync(new Error
                {
                    StatusCode = StatusCodes.Status404NotFound,
                });

            return await _componentFactory.RenderComponentAsync(new ViewRecipe
            {
                Recipe = recipe,
                RecipeId = recipeId
            });
        }

        [HttpGet]
        public Task<IResult> GetRecipes()
        {
            return _componentFactory.RenderComponentAsync<Recipes.Components.Recipes>();
        }

        [HttpGet("edit/{recipeId}")]
        public async Task<IResult> EditRecipe(int recipeId)
        {
            var result = await _recipesService.GetRecipeAsync(recipeId);
            if (!result.TryGetValue(out var recipe))
                return await _componentFactory.RenderComponentAsync(new Error
                {
                    StatusCode = StatusCodes.Status404NotFound,
                });

            return await _componentFactory.RenderComponentAsync(new EditRecipe
            {
                Recipe = recipe.AsOptional(),
                RecipeId = recipeId.AsOptional()
            });
        }

        [HttpGet("new")]
        public Task<IResult> GetCreateRecipe()
        {
            return _componentFactory.RenderComponentAsync<Recipes.Components.EditRecipe>();
        }

        [HttpPost("new")]
        public async Task<IResult> CreateRecipe([FromForm] RecipeModel recipe)
        {
            var (newRecipeId, createdRecipe) = await _recipesService.CreateRecipeAsync(recipe);

            Response.AsResponseData().HxPushUrl($"/recipes/view/{newRecipeId}");
            return await _componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = new()
                {
                    new HxSwapOob
                    {
                        Content = new ViewRecipe
                        {
                            Recipe = createdRecipe,
                            RecipeId = newRecipeId
                        },
                        Target = "#page-container",
                        ScrollToTop = true
                    },
                    new Toast
                    {
                        Message = $"Created recipe \"{createdRecipe.Title}\"",
                        Severity = ToastSeverity.Success
                    }
                }
            });
        }

        [HttpPut("edit/{recipeId}")]
        public async Task<IResult> UpdateRecipe(int recipeId, [FromForm] RecipeModel recipe)
        {
            var updatedRecipe = await _recipesService.UpdateRecipeAsync(recipeId, recipe);

            Response.AsResponseData().HxPushUrl($"/recipes/view/{recipeId}");
            return await _componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = new()
                {
                    new HxSwapOob
                    {
                        Content = new ViewRecipe
                        {
                            Recipe = updatedRecipe,
                            RecipeId = recipeId
                        },
                        Target = "#page-container",
                        ScrollToTop = true
                    },
                    new Toast
                    {
                        Message = $"Updated recipe \"{updatedRecipe.Title}\"",
                        Severity = ToastSeverity.Success
                    }
                }
            });
        }

        [HttpGet("search-ingredients")]
        public async Task<IResult> SearchIngredients([FromQuery] string search = "")
        {
            var ingredients = await ingredientsService.SearchIngredientsAsync(search, 0, 5); // todo: appsettings

            return await _componentFactory.RenderComponentAsync(new RecipeIngredientAutocompleteSuggestions
            {
                Suggestions = ingredients.Select(i => (i.Id, i.Ingredient.Name)).ToList()
            });
        }
        [HttpGet("add-ingredient")]
        public async Task<IResult> AddIngredient([FromQuery] RecipeIngredientModel ingredient, [FromQuery] int groupId)
        {
            return await _componentFactory.RenderComponentAsync(new RecipeIngredientsListItem
            {
                Ingredient = ingredient,
                GroupId = groupId
            });
        }
    }
}
