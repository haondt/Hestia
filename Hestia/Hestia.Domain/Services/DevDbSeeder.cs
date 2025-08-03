using Bogus;
using Haondt.Core.Extensions;
using Hestia.Domain.Models;

namespace Hestia.Domain.Services;

public class DevDbSeeder(IIngredientsService ingredientsService, IRecipesService recipesService) : IDevDbSeeder
{
    public async Task SeedAsync(DevSeedOptions options)
    {
        if (options.IngredientsCount.TryGetValue(out var ingredientsCount) && ingredientsCount > 0)
            await SeedIngredientsAsync(ingredientsCount);

        if (options.RecipesCount.TryGetValue(out var recipesCount) && recipesCount > 0)
            await SeedRecipesAsync(recipesCount);
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

    private async Task SeedRecipesAsync(int count)
    {
        // Get existing ingredients to use in recipes
        var ingredients = await ingredientsService.GetIngredientsAsync(0, 100);
        if (ingredients.Count == 0)
        {
            // Create some basic ingredients if none exist
            await SeedIngredientsAsync(20);
            ingredients = await ingredientsService.GetIngredientsAsync(0, 100);
        }

        var recipeTemplates = new[]
        {
            new {
                Title = "Grilled Chicken Breast",
                Description = "Juicy and flavorful grilled chicken breast seasoned with herbs and spices",
                Instructions = "1. Season chicken with salt, pepper, and herbs\n2. Preheat grill to medium-high heat\n3. Grill for 6-7 minutes per side\n4. Let rest for 5 minutes before serving",
                PrepTime = (15, "minutes"),
                CookTime = (20, "minutes"),
                Servings = 4,
                IngredientCount = (3, 5)
            },
            new {
                Title = "Vegetable Stir Fry",
                Description = "Quick and healthy vegetable stir fry with garlic and ginger",
                Instructions = "1. Heat oil in a wok over high heat\n2. Add garlic and ginger, stir for 30 seconds\n3. Add vegetables in order of cooking time\n4. Season with soy sauce and serve over rice",
                PrepTime = (10, "minutes"),
                CookTime = (8, "minutes"),
                Servings = 2,
                IngredientCount = (4, 7)
            },
            new {
                Title = "Chocolate Chip Cookies",
                Description = "Classic homemade chocolate chip cookies that are crispy on the outside and chewy inside",
                Instructions = "1. Cream butter and sugars together\n2. Beat in eggs and vanilla\n3. Mix in flour, baking soda, and salt\n4. Fold in chocolate chips\n5. Bake at 375°F for 9-11 minutes",
                PrepTime = (20, "minutes"),
                CookTime = (25, "minutes"),
                Servings = 24,
                IngredientCount = (6, 8)
            },
            new {
                Title = "Beef Tacos",
                Description = "Delicious ground beef tacos with fresh toppings",
                Instructions = "1. Brown ground beef with onions\n2. Season with taco seasoning\n3. Warm tortillas\n4. Fill with beef and desired toppings\n5. Serve with lime wedges",
                PrepTime = (15, "minutes"),
                CookTime = (15, "minutes"),
                Servings = 6,
                IngredientCount = (5, 8)
            },
            new {
                Title = "Caesar Salad",
                Description = "Classic Caesar salad with crisp romaine and homemade croutons",
                Instructions = "1. Wash and chop romaine lettuce\n2. Make croutons by toasting cubed bread with olive oil\n3. Toss lettuce with Caesar dressing\n4. Top with croutons and parmesan cheese",
                PrepTime = (15, "minutes"),
                CookTime = (5, "minutes"),
                Servings = 4,
                IngredientCount = (4, 6)
            },
            new {
                Title = "Banana Bread",
                Description = "Moist and delicious banana bread perfect for breakfast or snacking",
                Instructions = "1. Preheat oven to 350°F\n2. Mash ripe bananas\n3. Mix wet ingredients together\n4. Combine dry ingredients separately\n5. Fold wet into dry ingredients\n6. Bake for 60-65 minutes",
                PrepTime = (15, "minutes"),
                CookTime = (65, "minutes"),
                Servings = 12,
                IngredientCount = (6, 9)
            }
        };

        var faker = new Faker();
        var servingUnits = new[] { "cup", "tbsp", "tsp", "oz", "piece", "slice", "clove" };

        for (int i = 0; i < count; i++)
        {
            var template = faker.Random.CollectionItem(recipeTemplates);
            var ingredientCount = faker.Random.Int(template.IngredientCount.Item1, template.IngredientCount.Item2);
            var selectedIngredients = faker.Random.ListItems(ingredients, ingredientCount);

            var recipe = new RecipeModel
            {
                Title = faker.Random.Bool(0.7f) ? template.Title : faker.Commerce.ProductName(),
                Description = faker.Random.Bool(0.8f) ? template.Description.AsOptional() : new(),
                Instructions = faker.Random.Bool(0.9f) ? template.Instructions.AsOptional() : new(),

                PrepTimeQuantity = faker.Random.Bool(0.8f)
                    ? faker.Random.Int(template.PrepTime.Item1 - 5, template.PrepTime.Item1 + 10).AsOptional().Map(q => (double)q)
                    : new(),
                PrepTimeUnit = faker.Random.Bool(0.8f)
                    ? template.PrepTime.Item2.AsOptional()
                    : new(),

                CookTimeQuantity = faker.Random.Bool(0.8f)
                    ? faker.Random.Int(template.CookTime.Item1 - 5, template.CookTime.Item1 + 15).AsOptional().Map(q => (double)q)
                    : new(),
                CookTimeUnit = faker.Random.Bool(0.8f)
                    ? template.CookTime.Item2.AsOptional()
                    : new(),

                NumberOfServings = faker.Random.Bool(0.9f)
                    ? faker.Random.Int(Math.Max(1, template.Servings - 2), template.Servings + 4).AsOptional()
                    : new(),

                YieldQuantity = faker.Random.Bool(0.7f)
                    ? faker.Random.Decimal(1, 6).AsOptional()
                    : new(),
                YieldUnit = faker.Random.Bool(0.7f)
                    ? faker.Random.CollectionItem(new[] { "cups", "servings", "pieces", "loaves", "dozen", "portions" }).AsOptional()
                    : new(),

                Ingredients = selectedIngredients.Select(ingredient => new RecipeIngredientModel
                {
                    IngredientId = ingredient.Id.AsOptional(),
                    IngredientName = ingredient.Ingredient.Name,
                    Name = ingredient.Ingredient.Name,
                    Quantity = faker.Random.Bool(0.9f)
                        ? faker.Random.Decimal(0.25m, 3m).AsOptional()
                        : new(),
                    Unit = faker.Random.Bool(0.8f)
                        ? faker.Random.CollectionItem(servingUnits).AsOptional()
                        : new()
                }).ToList()
            };

            await recipesService.CreateRecipeAsync(recipe);
        }
    }
}