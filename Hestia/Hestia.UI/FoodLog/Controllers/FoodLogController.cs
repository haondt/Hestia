using Haondt.Core.Extensions;
using Haondt.Web.BulmaCSS.Services;
using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Hestia.Domain.Attributes;
using Hestia.Domain.Models;
using Hestia.Domain.Services;
using Hestia.UI.Core.Controllers;
using Hestia.UI.Core.Extensions;
using Hestia.UI.FoodLog.Components;
using Hestia.UI.Library.Components.Element;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hestia.UI.FoodLog.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("food-logs")]
    public class FoodLogController(IComponentFactory componentFactory, IFoodLogService foodLogService, IMealPlansService mealPlansService) : UIController(componentFactory)
    {
        private readonly IComponentFactory _componentFactory = componentFactory;

        [HttpGet]
        [HttpGet("food-log/{dateString?}")]
        public async Task<IResult> GetFoodLogAsync(string? dateString = null, string? clientSideDateString = null)
        {
            if (!ValidDateStringAttribute.Validate(dateString))
                dateString = null;
            if (!ValidDateStringAttribute.Validate(clientSideDateString))
                clientSideDateString = null;

            var finalDateString = clientSideDateString ?? dateString ?? DateTime.Today.ToString("yyyy-MM-dd");

            if (!Request.AsRequestData().IsHxRequest() && string.IsNullOrEmpty(dateString))
                return TypedResults.Redirect($"/food-logs/food-log/{finalDateString}");
            Response.AsResponseData()
                .HxPushUrl($"/food-logs/food-log/{finalDateString}");
            return await _componentFactory.RenderComponentAsync(new Components.FoodLog
            {
                DateString = finalDateString,
            });
        }

        [HttpPut("food-log/{dateString?}")]
        public async Task<IResult> UpdateFoodLogAsync([ValidDateString] string dateString, [FromForm] FoodLogModel foodLog)
        {
            if (!ModelState.IsValid)
                return TypedResults.NotFound();

            foodLog.DateString = dateString;
            await foodLogService.UpdateFoodLogAsync(foodLog);
            return TypedResults.Ok();
        }

        [HttpGet("search-meal-plans")]
        public async Task<IResult> SearchItems([FromQuery] string search = "")
        {
            var mealPlans = await mealPlansService.SearchMealPlansAsync(search, 0, 5);

            return await _componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = mealPlans.Select(r => new MealPlanPickerSearchResult
                {
                    MealPlan = r.MealPlan,
                    Id = r.Id,
                })
                .Cast<IComponent>()
                .ToList()
            });
        }
        [HttpGet("fragments/meal-plan-picker")]
        public Task<IResult> GetMealPlanPickerFragment()
        {
            return _componentFactory.RenderComponentAsync<MealPlanPicker>();
        }
        [HttpGet("fragments/embedded-meal-plan")]
        public async Task<IResult> GetEmbeddedMealPlanFragment(int? id)
        {
            if (!id.HasValue)
                return await _componentFactory.RenderComponentAsync<EmbeddedMealPlan>();
            var mealPlan = await mealPlansService.GetMealPlanAsync(id.Value);
            if (!mealPlan.IsSuccessful)
                return await _componentFactory.RenderComponentAsync(new Toast
                {
                    Message = $"Meal plan with id {id.Value} not found.",
                    Severity = ToastSeverity.Error
                });
            Response.AsResponseData()
                .HxTriggerAfterSwap("closeModal");
            return await _componentFactory.RenderComponentAsync(new EmbeddedMealPlan
            {
                MealPlanId = id.AsOptional()
            });
        }

        [HttpGet("diff/{dateString}")]
        public async Task<IResult> GetFoodLogDiffAsync([ValidDateString] string dateString)
        {
            if (!ModelState.IsValid)
                return TypedResults.NotFound();

            var foodLogResult = await foodLogService.GetOrCreateFoodLogAsync(dateString);

            if (foodLogResult.MealPlanId.HasValue)
            {
                var mealPlan = await mealPlansService.GetMealPlanAsync(foodLogResult.MealPlanId.Value);
                if (mealPlan.IsSuccessful)
                {
                    var diff = foodLogResult.CalculateDiff(foodLogResult.MealPlanId.Value, mealPlan.Value);
                    return await _componentFactory.RenderComponentAsync(new Components.FoodLogDiff
                    {
                        Diff = diff
                    });
                }
            }


            return await _componentFactory.RenderComponentAsync(new Components.FoodLogDiff
            {
            });
        }

        [HttpGet("insights/{dateString}")]
        public async Task<IResult> GetFoodLogInsightsAsync([ValidDateString] string dateString)
        {
            var foodLog = await foodLogService.GetOrCreateFoodLogAsync(dateString);
            return await _componentFactory.RenderComponentAsync(new FoodLogInsights
            {
                FoodLog = foodLog
            });
        }
    }
}
