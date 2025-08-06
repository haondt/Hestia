using Haondt.Core.Models;
using Hestia.Domain.Models;

namespace Hestia.Domain.Services
{
    public interface IMealPlansService
    {
        Task<Result<MealPlanModel>> GetMealPlanAsync(int id);
        Task<List<(int Id, MealPlanModel MealPlan)>> GetMealPlansAsync(int page = 0, int pageSize = 50);
        Task<(int Id, MealPlanModel MealPlan)> CreateMealPlanAsync(MealPlanModel mealPlan);
        Task<MealPlanModel> UpdateMealPlanAsync(int id, MealPlanModel mealPlan);
        Task<Result> DeleteMealPlanAsync(int id);
        Task<int> GetNextMealPlanNumberAsync();
        Task<List<string>> GetDefaultSectionsAsync();
    }
}