using Haondt.Web.BulmaCSS.Services;
using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
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
        IIngredientsService ingredientsService) : UIController
    {
        [HttpGet]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<Ingredients.Components.Ingredients>();
        }

        [HttpGet("view/{id}")]
        public async Task<IResult> ViewIngredient(int id)
        {
            var result = await ingredientsService.GetIngredientAsync(id);
            if (!result.TryGetValue(out var model))
                return await componentFactory.RenderComponentAsync(new Error
                {
                    StatusCode = StatusCodes.Status404NotFound,
                });

            return await componentFactory.RenderComponentAsync(new ViewIngredient
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
                return await componentFactory.RenderComponentAsync(new Error
                {
                    StatusCode = StatusCodes.Status404NotFound,
                });

            return await componentFactory.RenderComponentAsync(new EditIngredient
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
            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
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
                        Target = "#page-container"
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
            return componentFactory.RenderComponentAsync<Ingredients.Components.EditIngredient>();
        }

        [HttpPost("new")]
        public async Task<IResult> CreateIngredient([FromForm] IngredientModel ingredient, [FromForm] bool createAnother)
        {
            var (id, model) = await ingredientsService.CreateIngredientAsync(ingredient);

            if (createAnother)
                return await componentFactory.RenderComponentAsync(new AppendComponentLayout
                {
                    Components = new()
                    {
                        new HxSwapOob
                        {
                            Content =  new EditIngredient(),
                            Target = "#page-container"
                        },
                        new Toast
                        {
                            Message = $"Created ingredient \"{model.Name}\"",
                            Severity = ToastSeverity.Success
                        }
                    }
                });

            Response.AsResponseData().HxPushUrl($"/ingredients/view/{id}");
            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
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
                        Target = "#page-container"
                    },
                    new Toast
                    {
                        Message = $"Created ingredient \"{model.Name}\"",
                        Severity = ToastSeverity.Success
                    }
                }
            });
        }
    }
}
