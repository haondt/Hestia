using Bogus;
using Haondt.Core.Extensions;
using Hestia.Domain.Models;

namespace Hestia.Domain.Services;

public class DevDbSeeder(IIngredientsService ingredientsService) : IDevDbSeeder
{
    public async Task SeedAsync(DevSeedOptions options)
    {
        if (options.IngredientsCount.TryGetValue(out var ingredientsCount) && ingredientsCount > 0)
            await SeedIngredientsAsync(ingredientsCount);
    }

    private async Task SeedIngredientsAsync(int count)
    {
        // Food categories with appropriate nutritional profiles
        var foodCategories = new Dictionary<string, (string[] Names, (int, int) Protein, (int, int) Fat, (int, int) Carb, (int, int) Calories)>
        {
            ["proteins"] = (
                new[] { "Chicken Breast", "Ground Beef", "Salmon Fillet", "Eggs", "Greek Yogurt", "Cottage Cheese", "Tofu", "Turkey Breast", "Tuna", "Black Beans" },
                (15, 35), (2, 20), (0, 8), (80, 250)
            ),
            ["grains"] = (
                new[] { "Brown Rice", "Quinoa", "Whole Wheat Pasta", "Oats", "Barley", "Bulgur", "Wild Rice", "Farro", "Buckwheat" },
                (3, 8), (1, 4), (20, 45), (100, 200)
            ),
            ["vegetables"] = (
                new[] { "Broccoli", "Spinach", "Bell Peppers", "Carrots", "Tomatoes", "Onions", "Mushrooms", "Kale", "Zucchini", "Sweet Potatoes" },
                (1, 4), (0, 1), (3, 12), (15, 80)
            ),
            ["fruits"] = (
                new[] { "Apples", "Bananas", "Strawberries", "Blueberries", "Oranges", "Grapes", "Avocados", "Mangoes", "Pineapple" },
                (0, 2), (0, 15), (8, 25), (30, 160)
            ),
            ["dairy"] = (
                new[] { "Milk 2%", "Cheddar Cheese", "Mozzarella", "Plain Yogurt", "Butter", "Cream Cheese", "Parmesan" },
                (2, 25), (3, 35), (2, 8), (60, 350)
            ),
            ["pantry"] = (
                new[] { "Olive Oil", "Honey", "All-Purpose Flour", "Brown Sugar", "Sea Salt", "Black Pepper", "Garlic Powder", "Vanilla Extract" },
                (0, 3), (0, 100), (0, 95), (5, 900)
            )
        };

        var servingUnits = new[] { "cup", "tbsp", "tsp", "oz", "fl oz", "lb", "g", "kg", "ml", "l", "piece", "slice", "clove" };
        var packageUnits = new[] { "box", "bag", "bottle", "can", "jar", "container", "pack", "carton", "tube", "pouch" };
        var brands = new[] { "Organic Valley", "Whole Foods", "Trader Joe's", "Great Value", "Kirkland", "Simply Organic", "365 Everyday" };

        var faker = new Faker();

        for (int i = 0; i < count; i++)
        {
            // Choose a random food category
            var categoryKey = faker.Random.CollectionItem(foodCategories.Keys.ToArray());
            var category = foodCategories[categoryKey];

            // Generate ingredient based on category
            var ingredient = new IngredientModel
            {
                Name = faker.Random.Bool(0.7f)
                    ? faker.Random.CollectionItem(category.Names)
                    : faker.Commerce.ProductName(),

                Brand = faker.Random.Bool(0.6f)
                    ? faker.Random.CollectionItem(brands).AsOptional()
                    : new(),

                Vendor = faker.Random.Bool(0.4f)
                    ? faker.Company.CompanyName().AsOptional()
                    : new(),

                ServingSizeQuantity = faker.Random.Bool(0.8f)
                    ? faker.Random.Decimal(0.25m, 5m).AsOptional()
                    : new(),

                ServingSizeUnit = faker.Random.Bool(0.8f)
                    ? faker.Random.CollectionItem(servingUnits).AsOptional()
                    : new(),

                AlternateServingSizeQuantity = new(),
                AlternateServingSizeUnit = new(),

                Calories = faker.Random.Bool(0.9f)
                    ? faker.Random.Decimal(category.Calories.Item1, category.Calories.Item2).AsOptional()
                    : new(),

                ProteinGrams = faker.Random.Bool(0.8f)
                    ? faker.Random.Decimal(category.Protein.Item1, category.Protein.Item2).AsOptional()
                    : new(),

                FatGrams = faker.Random.Bool(0.8f)
                    ? faker.Random.Decimal(category.Fat.Item1, category.Fat.Item2).AsOptional()
                    : new(),

                CarbGrams = faker.Random.Bool(0.8f)
                    ? faker.Random.Decimal(category.Carb.Item1, category.Carb.Item2).AsOptional()
                    : new(),

                PackageSizeQuantity = faker.Random.Bool(0.5f)
                    ? faker.Random.Decimal(0.5m, 20m).AsOptional()
                    : new(),

                PackageSizeUnit = faker.Random.Bool(0.5f)
                    ? faker.Random.CollectionItem(packageUnits).AsOptional()
                    : new(),

                PackageCostDollars = faker.Random.Bool(0.5f)
                    ? faker.Random.Decimal(0.99m, 25.99m).AsOptional()
                    : new(),

                Notes = faker.Random.Bool(0.2f) ? faker.Lorem.Sentence() : ""
            };

            var result = await ingredientsService.CreateIngredientAsync(ingredient);
        }
    }
}