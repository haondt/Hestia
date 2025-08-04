using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Web.BulmaCSS.Services;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Hestia.Core.Constants;
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

        [HttpDelete("view/{recipeId}")]
        public async Task<IResult> DeleteRecipe(int recipeId)
        {
            var result = await _recipesService.DeleteRecipeAsync(recipeId);
            if (!result.IsSuccessful)
            {
                Response.AsResponseData()
                    .Status(404);
                return await componentFactory.RenderComponentAsync(new Toast
                {
                    Message = $"Recipe with id {recipeId} not found",
                    Severity = ToastSeverity.Error
                });
            }

            Response.AsResponseData()
                .HxLocation("/recipes", target: "#page-container");
            return TypedResults.Ok();
        }

        [HttpGet]
        public Task<IResult> GetRecipes()
        {
            return _componentFactory.RenderComponentAsync<Recipes.Components.Recipes>();
        }

        [HttpGet("search")]
        public async Task<IResult> Search([FromQuery] string? search, [FromQuery] int page = 0, [FromQuery] bool isScroll = false)
        {
            var recipes = string.IsNullOrWhiteSpace(search)
                ? await _recipesService.GetRecipesAsync(page, HestiaConstants.PageSize)
                : await _recipesService.SearchRecipesAsync(search, page, HestiaConstants.PageSize);

            return await _componentFactory.RenderComponentAsync(new RecipesGrid
            {
                IsScroll = isScroll,
                Recipes = recipes,
                CurrentSearch = search.AsOptional(),
                NextPage = recipes.Count == HestiaConstants.PageSize
                    ? page + 1
                    : new Optional<int>()
            });
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
        public async Task<IResult> CreateRecipe([FromForm] RecipeModel recipe, [FromForm] bool createAnother)
        {
            var (newRecipeId, createdRecipe) = await _recipesService.CreateRecipeAsync(recipe);

            if (createAnother)
                return await _componentFactory.RenderComponentAsync(new HxSwapOob
                {
                    Content = new EditRecipe(),
                    Target = "#page-container",
                    ScrollToTop = true
                });

            Response.AsResponseData().HxPushUrl($"/recipes/view/{newRecipeId}");
            return await _componentFactory.RenderComponentAsync(new HxSwapOob
            {
                Content = new ViewRecipe
                {
                    Recipe = createdRecipe,
                    RecipeId = newRecipeId
                },
                Target = "#page-container",
                ScrollToTop = true
            });
        }

        [HttpPut("edit/{recipeId}")]
        public async Task<IResult> UpdateRecipe(int recipeId, [FromForm] RecipeModel recipe)
        {
            var updatedRecipe = await _recipesService.UpdateRecipeAsync(recipeId, recipe);

            Response.AsResponseData().HxPushUrl($"/recipes/view/{recipeId}");
            return await _componentFactory.RenderComponentAsync(new HxSwapOob
            {
                Content = new ViewRecipe
                {
                    Recipe = updatedRecipe,
                    RecipeId = recipeId
                },
                Target = "#page-container",
                ScrollToTop = true
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
