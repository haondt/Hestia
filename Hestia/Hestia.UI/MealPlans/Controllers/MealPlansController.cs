using Haondt.Core.Extensions;
using Haondt.Web.BulmaCSS.Services;
using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Hestia.Domain.Models;
using Hestia.Domain.Services;
using Hestia.Persistence.Models;
using Hestia.UI.Core.Components;
using Hestia.UI.Core.Controllers;
using Hestia.UI.Core.Extensions;
using Hestia.UI.Library.Components.Element;
using Hestia.UI.Library.Components.Htmx;
using Hestia.UI.MealPlans.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Hestia.UI.MealPlans.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("meal-plans")]
    public class MealPlansController(IComponentFactory componentFactory, IMealPlansService mealPlansService, IRecipesService recipesService, IIngredientsService ingredientsService) : UIController(componentFactory)
    {
        private readonly IComponentFactory _componentFactory = componentFactory;
        private readonly IRecipesService _recipesService = recipesService;
        private readonly IIngredientsService _ingredientsService = ingredientsService;

        [HttpGet]
        public Task<IResult> GetMealPlans()
        {
            return _componentFactory.RenderComponentAsync<MealPlans.Components.MealPlans>();
        }

        [HttpGet("meal-plan/{id}")]
        public async Task<IResult> GetMealPlan(int id)
        {
            var mealPlan = await mealPlansService.GetMealPlanAsync(id);
            if (!mealPlan.IsSuccessful)
                return await _componentFactory.RenderComponentAsync(new Error
                {
                    StatusCode = StatusCodes.Status404NotFound,
                });
            return await _componentFactory.RenderComponentAsync(new MealPlan
            {
                MealPlanId = id,
                MealPlanModel = mealPlan.Value
            });
        }

        [HttpGet("insights/{id}")]
        public async Task<IResult> GetMealPlanInsights(int id)
        {
            var mealPlan = await mealPlansService.GetMealPlanAsync(id);
            if (!mealPlan.IsSuccessful)
                return await _componentFactory.RenderComponentAsync(new Error
                {
                    StatusCode = StatusCodes.Status404NotFound,
                });
            return await _componentFactory.RenderComponentAsync(new MealPlanInsights
            {
                MealPlanId = id,
                MealPlan = mealPlan.Value
            });
        }

        [HttpPost("create")]
        public async Task<IResult> CreateMealPlan()
        {
            var mealPlan = await mealPlansService.CreateDefaultMealPlanAsync();
            Response.AsResponseData()
                .HxPushUrl($"/meal-plans/meal-plan/{mealPlan.Id}");
            return await _componentFactory.RenderComponentAsync(new MealPlan
            {
                MealPlanId = mealPlan.Id,
                MealPlanModel = mealPlan.MealPlan
            });
        }

        [HttpPut("meal-plan/{mealPlanId}")]
        public async Task<IResult> UpdateMealPlan(int mealPlanId, [FromForm] MealPlanModel mealPlan)
        {
            await mealPlansService.UpdateMealPlanAsync(mealPlanId, mealPlan);
            return TypedResults.Ok();
        }

        [HttpGet("search-items")]
        public async Task<IResult> SearchItems([FromQuery] string? search)
        {
            var recipes = new List<(int Id, Domain.Models.RecipeModel Recipe)>();
            var ingredients = new List<(int Id, Domain.Models.IngredientModel Ingredient)>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var recipesTask = _recipesService.SearchRecipesAsync(search, 0, 5);
                var ingredientsTask = _ingredientsService.SearchIngredientsAsync(search, 0, 5);

                await Task.WhenAll(recipesTask, ingredientsTask);

                recipes = await recipesTask;
                ingredients = await ingredientsTask;
            }

            return await _componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = recipes.Select(r => new MealPlanItemSearchResult
                {
                    Entity = r.Recipe,
                    Id = r.Id,
                })
                .Cast<IComponent>()
                .Concat(ingredients.Select(i => new MealPlanItemSearchResult
                {
                    Entity = i.Ingredient,
                    Id = i.Id
                }))
                .ToList()
            });
        }

        [HttpDelete("meal-plan/{mealPlanId}")]
        public async Task<IResult> DeleteMealPlan(int mealPlanId)
        {
            var result = await mealPlansService.DeleteMealPlanAsync(mealPlanId);
            if (!result.IsSuccessful)
            {
                Response.AsResponseData()
                    .Status(404);
                return await _componentFactory.RenderComponentAsync(new Toast
                {
                    Message = $"Meal plan with id {mealPlanId} not found",
                    Severity = ToastSeverity.Error
                });
            }

            Response.AsResponseData()
                .HxLocation("/meal-plans", target: "#page-container");
            return TypedResults.Ok();
        }

        [HttpGet("fragments/section-container")]
        public Task<IResult> GetSectionContainerFragment()
        {
            return _componentFactory.RenderComponentAsync<MealPlanSectionContainer>();
        }
        [HttpGet("fragments/item-picker")]
        public Task<IResult> GetItemPickerFragment(
            [FromQuery, Required] string targetSection)
        {
            return _componentFactory.RenderComponentAsync(new MealPlanItemPicker()
            {
                TargetSection = targetSection
            });
        }
        [HttpGet("fragments/item-configurator")]
        public Task<IResult> GetItemConfiguratorFragment(
            [FromQuery] int itemId,
            [FromQuery] string itemName,
            [FromQuery] MealItemType itemType,
            [FromQuery] string? targetSection,
            [FromQuery] string? targetItem,
            [FromQuery] decimal? itemQuantity,
            [FromQuery] string? itemUnit,
            [FromQuery] bool modal = true,
            [FromQuery] bool autoSelectUnit = true
            )

        {
            return _componentFactory.RenderComponentAsync(new MealPlanItemConfigurator()
            {
                Modal = modal,
                ItemId = itemId,
                ItemName = itemName,
                ItemType = itemType,
                ItemUnit = itemUnit.AsOptional(),
                AutoSelectUnit = autoSelectUnit,
                ItemQuantity = itemQuantity.AsOptional(),
                TargetSection = targetSection.AsOptional(),
                TargetItem = targetItem.AsOptional()
            });
        }

        [HttpGet("fragments/add-item")]
        public Task<IResult> GetAddItem(
            [FromQuery] string? targetSection,
            [FromQuery] string? targetItem,
            [FromQuery] MealPlanItemModel mealPlanItem)
        {
            string target;
            string strategy;
            if (!string.IsNullOrEmpty(targetSection))
            {
                target = $"[data-meal-plan-section-id='{targetSection}'] .meal-plan-item-container";
                strategy = "beforeend";
            }
            else if (!string.IsNullOrEmpty(targetItem))
            {
                target = $"[data-meal-plan-item-id='{targetItem}']";
                strategy = "outerHTML";
            }
            else
            {
                throw new InvalidOperationException("Neither target section or target item were provided");
            }

            Response.AsResponseData()
                .HxTriggerAfterSwap("closeModal");

            return _componentFactory.RenderComponentAsync(new HxSwapOob
            {
                Content = new MealPlanItem
                {
                    Entity = mealPlanItem,
                    Id = mealPlanItem.RecipeOrIngredientId,
                    EmitDirtyOnLoad = true
                },
                Target = target,
                Strategy = strategy
            });
        }

    }
}