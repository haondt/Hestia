using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Hestia.Domain.Models;
using Hestia.Persistence;
using Hestia.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Hestia.Domain.Services
{
    public class FoodLogService(ApplicationDbContext context, IMealPlansService mealPlansService, IIngredientsService ingredientsService, IRecipesService recipesService) : IFoodLogService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IMealPlansService _mealPlansService = mealPlansService;
        private readonly IIngredientsService _ingredientsService = ingredientsService;
        private readonly IRecipesService _recipesService = recipesService;

        public async Task<FoodLogModel> GetOrCreateFoodLogAsync(string dateString)
        {
            var existingFoodLog = await GetFoodLogAsync(dateString);
            if (existingFoodLog.IsSuccessful)
                return FoodLogModel.FromDataModel(existingFoodLog.Value);

            var state = await HestiaStateDataModel.GetOrCreateAsync(_context);
            // TODO:
            var defaultSections = new List<string>(["Breakfast", "Lunch", "Dinner"]);
            return new FoodLogModel
            {
                DateString = dateString,
                Sections = defaultSections.Select(s => new MealPlanSectionModel { Name = s }).ToList()
            };
        }

        private async Task<Result<FoodLogDataModel>> GetFoodLogAsync(string dateString)
        {
            var foodLogData = await _context.FoodLogs
                .Include(fl => fl.Sections)
                    .ThenInclude(s => s.Items)
                    .ThenInclude(i => i.Recipe)
                .Include(fl => fl.Sections)
                    .ThenInclude(s => s.Items)
                    .ThenInclude(i => i.Ingredient)
                .Include(fl => fl.MealPlan)
                    .ThenInclude(mp => mp!.Sections)
                    .ThenInclude(s => s.Items)
                    .ThenInclude(s => s.Ingredient)
                .Include(fl => fl.MealPlan)
                    .ThenInclude(mp => mp!.Sections)
                    .ThenInclude(s => s.Items)
                    .ThenInclude(s => s.Recipe)
                .FirstOrDefaultAsync(fl => fl.DateString == dateString);

            if (foodLogData == null)
                return new();

            return foodLogData;
        }

        public async Task<FoodLogModel> UpdateFoodLogAsync(FoodLogModel foodLog)
        {
            var existingFoodLog = await GetFoodLogAsync(foodLog.DateString);

            if (existingFoodLog.IsSuccessful)
            {
                foodLog.ApplyUpdate(existingFoodLog.Value);
                _context.Update(existingFoodLog.Value);
            }
            else
            {
                _context.FoodLogs.Add(foodLog.AsDataModel());
            }

            await _context.SaveChangesAsync();
            return (await GetFoodLogAsync(foodLog.DateString))
                .AsOptional()
                .Map(FoodLogModel.FromDataModel)
                .Unwrap()
                ?? throw new InvalidOperationException("Created food log not found after saving to database.");
        }

        public async Task DeleteFoodLogAsync(string dateString)
        {
            var foodLogData = await _context.FoodLogs
                .FirstOrDefaultAsync(fl => fl.DateString == dateString);

            if (foodLogData != null)
            {
                _context.FoodLogs.Remove(foodLogData);
                await _context.SaveChangesAsync();
            }
        }
    }
}