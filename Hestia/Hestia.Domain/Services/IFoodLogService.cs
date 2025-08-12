using Hestia.Domain.Models;

namespace Hestia.Domain.Services
{
    public interface IFoodLogService
    {
        Task<FoodLogModel> GetOrCreateFoodLogAsync(string dateString);
        Task<FoodLogModel> UpdateFoodLogAsync(FoodLogModel foodLog);
        Task DeleteFoodLogAsync(string dateString);
    }
}