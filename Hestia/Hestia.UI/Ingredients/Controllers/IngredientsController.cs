using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Web.BulmaCSS.Services;
using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Hestia.Core.Constants;
using Hestia.Domain.Models;
using Hestia.Domain.Services;
using Hestia.UI.Core.Components;
using Hestia.UI.Core.Controllers;
using Hestia.UI.Ingredients.Components;
using Hestia.UI.Library.Components.Element;
using Hestia.UI.Library.Components.Htmx;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hestia.UI.Ingredients.Controllers
{
    [Route("ingredients")]
    public class IngredientsController(IComponentFactory componentFactory,
        IIngredientsService ingredientsService,
        IUnitConversionsService unitConversionsService) : UIController(componentFactory)
    {
        private readonly IComponentFactory _componentFactory = componentFactory;

        [HttpGet]
        public Task<IResult> Get()
        {
            return _componentFactory.RenderComponentAsync<Ingredients.Components.Ingredients>();
        }

        [HttpGet("search")]
        public async Task<IResult> Search([FromQuery] string? search, [FromQuery] int page = 0)
        {
            var ingredients = string.IsNullOrWhiteSpace(search)
                ? await ingredientsService.GetIngredientsAsync(page, HestiaConstants.PageSize)
                : await ingredientsService.SearchIngredientsAsync(search, page, HestiaConstants.PageSize);

            return await _componentFactory.RenderComponentAsync(new IngredientsGrid
            {
                Ingredients = ingredients,
                CurrentSearch = search.AsOptional(),
                NextPage = ingredients.Count == HestiaConstants.PageSize
                    ? page + 1
                    : new Optional<int>()
            });
        }

        [HttpGet("view/{id}")]
        public async Task<IResult> ViewIngredient(int id)
        {
            var result = await ingredientsService.GetIngredientAsync(id);
            if (!result.TryGetValue(out var model))
                return await _componentFactory.RenderComponentAsync(new Error
                {
                    StatusCode = StatusCodes.Status404NotFound,
                });

            return await _componentFactory.RenderComponentAsync(new ViewIngredient
            {
                Ingredient = model,
                IngredientId = id
            });
        }

        [HttpGet("edit/{id}")]
        public async Task<IResult> EditIngredient(int id)
        {
            var result = await ingredientsService.GetIngredientAsync(id);
            if (!result.TryGetValue(out var model))
                return await _componentFactory.RenderComponentAsync(new Error
                {
                    StatusCode = StatusCodes.Status404NotFound,
                });

            return await _componentFactory.RenderComponentAsync(new EditIngredient
            {
                Ingredient = model,
                IngredientId = id
            });
        }

        [HttpPut("edit/{id}")]
        public async Task<IResult> UpdateIngredient(int id, [FromForm] IngredientModel ingredient)
        {
            var model = await ingredientsService.UpdateIngredientAsync(id, ingredient);

            Response.AsResponseData().HxPushUrl($"/ingredients/view/{id}");
            return await _componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = new()
                {
                    new HxSwapOob
                    {
                        Content =  new ViewIngredient
                        {
                            Ingredient = model,
                            IngredientId = id
                        },
                        Target = "#page-container",
                        ScrollToTop = true
                    },
                    new Toast
                    {
                        Message = $"Updated ingredient \"{model.Name}\"",
                        Severity = ToastSeverity.Success
                    }
                }
            });
        }

        [HttpGet("new")]
        public Task<IResult> GetCreateIngredient()
        {
            return _componentFactory.RenderComponentAsync<Ingredients.Components.EditIngredient>();
        }

        [HttpPost("new")]
        public async Task<IResult> CreateIngredient([FromForm] IngredientModel ingredient, [FromForm] bool createAnother)
        {
            var (id, model) = await ingredientsService.CreateIngredientAsync(ingredient);

            if (createAnother)
                return await _componentFactory.RenderComponentAsync(new AppendComponentLayout
                {
                    Components = new()
                    {
                        new HxSwapOob
                        {
                            Content =  new EditIngredient(),
                            Target = "#page-container",
                            ScrollToTop = true
                        },
                        new Toast
                        {
                            Message = $"Created ingredient \"{model.Name}\"",
                            Severity = ToastSeverity.Success
                        }
                    }
                });

            Response.AsResponseData().HxPushUrl($"/ingredients/view/{id}");
            return await _componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = new()
                {
                    new HxSwapOob
                    {
                        Content =  new ViewIngredient
                        {
                            Ingredient = model,
                            IngredientId = id
                        },
                        Target = "#page-container",
                        ScrollToTop = true
                    },
                    new Toast
                    {
                        Message = $"Created ingredient \"{model.Name}\"",
                        Severity = ToastSeverity.Success
                    }
                }
            });
        }

        [HttpGet("unit-compatibility")]
        public async Task<IResult> CheckUnitCompatibility([FromQuery] string? unit1, [FromQuery] string? unit2, [FromQuery] string? servingUnit, [FromQuery] string? altServingUnit)
        {
            var compatibilities = new
            {
                ServingPackageCompatible = await GetCompatibility(servingUnit, unit1),
                AltServingPackageCompatible = await GetCompatibility(altServingUnit, unit1),
                ServingAltServingCompatible = await GetCompatibility(servingUnit, altServingUnit)
            };

            return Results.Json(compatibilities);
        }

        private async Task<bool> GetCompatibility(string? unitA, string? unitB)
        {
            if (string.IsNullOrWhiteSpace(unitA) || string.IsNullOrWhiteSpace(unitB))
                return true;

            return await unitConversionsService.CheckUnitCompatibilityAsync(unitA, unitB);
        }
    }
}
