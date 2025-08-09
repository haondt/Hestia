using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Hestia.Domain.Models;
using Hestia.Persistence;
using Hestia.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Hestia.Domain.Services
{
    public class MealPlansService(ApplicationDbContext context) : IMealPlansService
    {
        public async Task<Result<MealPlanModel>> GetMealPlanAsync(int id)
        {
            var mealPlan = await context.MealPlans
                .Include(mp => mp.Sections)
                .ThenInclude(s => s.Items)
                .ThenInclude(i => i.Recipe)
                .Include(mp => mp.Sections)
                .ThenInclude(s => s.Items)
                .ThenInclude(i => i.Ingredient)
                .FirstOrDefaultAsync(mp => mp.Id == id);

            return mealPlan.AsOptional().Map(MealPlanModel.FromDataModel).AsResult();
        }

        public async Task<List<(int Id, MealPlanModel MealPlan)>> GetMealPlansAsync(int page = 0, int pageSize = 50)
        {
            var mealPlans = await context.MealPlans
                .Include(mp => mp.Sections)
                .ThenInclude(s => s.Items)
                .ThenInclude(i => i.Recipe)
                .Include(mp => mp.Sections)
                .ThenInclude(s => s.Items)
                .ThenInclude(i => i.Ingredient)
                .OrderByDescending(mp => mp.LastModified)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return mealPlans.Select(mp => (mp.Id, MealPlanModel.FromDataModel(mp))).ToList();
        }

        public async Task<(int Id, MealPlanModel MealPlan)> CreateMealPlanAsync(MealPlanModel mealPlan)
        {
            var dataModel = mealPlan.AsDataModel();

            context.MealPlans.Add(dataModel);

            var state = await HestiaStateDataModel.GetOrCreateAsync(context);
            state.NextMealPlanNumber += 1;
            context.Update(state);

            await context.SaveChangesAsync();


            // Set the MealPlanId for sections and MealSectionId for items
            //foreach (var section in dataModel.Sections)
            //{
            //    section.MealPlanId = dataModel.Id;
            //    foreach (var item in section.Items)
            //    {
            //        item.MealSectionId = section.Id;
            //    }
            //}

            //await context.SaveChangesAsync();

            var createdMealPlan = await GetMealPlanAsync(dataModel.Id);
            return (dataModel.Id, createdMealPlan.Value
                ?? throw new InvalidOperationException("Created meal plan not found after saving to database."));
        }
        public async Task<(int Id, MealPlanModel MealPlan)> CreateDefaultMealPlanAsync()
        {
            var state = await HestiaStateDataModel.GetOrCreateAsync(context);
            var mealPlan = new MealPlanModel
            {
                Name = $"Meal Plan #{state.NextMealPlanNumber}",
                LastModified = AbsoluteDateTime.Now,
                Sections = (await GetDefaultSectionsAsync())
                    .Select(s => new MealPlanSectionModel
                    {
                        Name = s
                    }).ToList()
            };

            var dataModel = mealPlan.AsDataModel();
            context.MealPlans.Add(dataModel);

            state.NextMealPlanNumber += 1;
            context.Update(state);

            await context.SaveChangesAsync();


            // Set the MealPlanId for sections and MealSectionId for items
            //foreach (var section in dataModel.Sections)
            //{
            //    section.MealPlanId = dataModel.Id;
            //    foreach (var item in section.Items)
            //    {
            //        item.MealSectionId = section.Id;
            //    }
            //}

            //await context.SaveChangesAsync();

            var createdMealPlan = await GetMealPlanAsync(dataModel.Id);
            return (dataModel.Id, createdMealPlan.Value
                ?? throw new InvalidOperationException("Created meal plan not found after saving to database."));
        }

        public async Task<MealPlanModel> UpdateMealPlanAsync(int id, MealPlanModel mealPlan)
        {
            var existingMealPlan = await context.MealPlans
                .Include(mp => mp.Sections)
                .ThenInclude(s => s.Items)
                .FirstAsync(mp => mp.Id == id);

            // Remove existing sections and items
            //context.MealItems.RemoveRange(existingMealPlan.Sections.SelectMany(s => s.Items));
            //context.MealSections.RemoveRange(existingMealPlan.Sections);

            mealPlan.ApplyUpdate(existingMealPlan);

            // Update meal plan properties
            //existingMealPlan.Date = mealPlan.Date;
            //existingMealPlan.Name = mealPlan.Name.Unwrap();
            //existingMealPlan.LastModified = DateTime.UtcNow;

            // Add new sections and items
            //var newDataModel = mealPlan.AsDataModel();
            //existingMealPlan.Sections = newDataModel.Sections.Select(s => new MealPlanSectionDataModel
            //{
            //    Name = s.Name,
            //    Order = s.Order,
            //    MealPlanId = id,
            //    Items = s.Items.Select(i => new MealPlanItemDataModel
            //    {
            //        ItemType = i.ItemType,
            //        RecipeId = i.RecipeId,
            //        IngredientId = i.IngredientId,
            //        Quantity = i.Quantity,
            //        Unit = i.Unit,
            //        MealSectionId = 0 // Will be set after saving
            //    }).ToList()
            //}).ToList();

            await context.SaveChangesAsync();

            // Update MealSectionId for items
            //foreach (var section in existingMealPlan.Sections)
            //{
            //    foreach (var item in section.Items)
            //    {
            //        item.MealSectionId = section.Id;
            //    }
            //}

            //await context.SaveChangesAsync();

            var updatedMealPlan = await GetMealPlanAsync(id);
            return updatedMealPlan.Value
                ?? throw new InvalidOperationException("Updated meal plan not found after saving to database.");
        }

        public async Task<Result> DeleteMealPlanAsync(int id)
        {
            var mealPlan = await context.MealPlans
                .Include(mp => mp.Sections)
                .ThenInclude(s => s.Items)
                .FirstOrDefaultAsync(mp => mp.Id == id);

            if (mealPlan == null)
                return Result.Failure;

            context.MealPlans.Remove(mealPlan);
            await context.SaveChangesAsync();

            return Result.Success;
        }

        public async Task<int> GetNextMealPlanNumberAsync()
        {
            var state = await HestiaStateDataModel.GetOrCreateAsync(context);
            return state.NextMealPlanNumber;
        }

        public async Task<List<string>> GetDefaultSectionsAsync()
        {
            return ["Breakfast", "Lunch", "Dinner"];
        }
    }
}