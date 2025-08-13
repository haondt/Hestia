using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Hestia.Domain.Attributes;
using Hestia.Domain.Models;
using Hestia.Domain.Services;
using Hestia.UI.Core.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hestia.UI.FoodLog.Controllers
{
    [Route("food-logs")]
    public class FoodLogController(IComponentFactory componentFactory, IFoodLogService foodLogService, IMealPlansService mealPlansService) : UIController(componentFactory)
    {
        private readonly IComponentFactory _componentFactory = componentFactory;

        [HttpGet]
        [HttpGet("food-log/{dateString?}")]
        public async Task<IResult> GetFoodLogAsync(string? dateString = null)
        {
            if (!ValidDateStringAttribute.Validate(dateString))
                dateString = null;

            if (dateString == null)
            {
                if (Request.AsRequestData().IsHxRequest())
                {
                    Response.AsResponseData()
                        .HxLocation($"/food-logs/food-log/{DateTime.Today:yyyy-MM-dd}", target: "#page-container");
                    return TypedResults.Ok();
                }
                return TypedResults.Redirect($"/food-logs/food-log/{DateTime.Today:yyyy-MM-dd}");
            }

            return await _componentFactory.RenderComponentAsync(new Components.FoodLog
            {
                DateString = dateString,
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
    }
}
