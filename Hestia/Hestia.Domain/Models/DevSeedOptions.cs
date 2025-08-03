using Haondt.Core.Models;

namespace Hestia.Domain.Models;

public class DevSeedOptions
{
    public Optional<int> IngredientsCount { get; private set; } = 0;
    public Optional<int> RecipesCount { get; private set; } = 0;

    public DevSeedOptions AddIngredients(int count)
    {
        IngredientsCount = count;
        return this;
    }

    public DevSeedOptions AddRecipes(int count)
    {
        RecipesCount = count;
        return this;
    }
}