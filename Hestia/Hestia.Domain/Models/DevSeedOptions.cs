using Haondt.Core.Models;

namespace Hestia.Domain.Models;

public class DevSeedOptions
{
    public Optional<int> IngredientsCount { get; private set; } = 0;

    public DevSeedOptions AddIngredients(int count)
    {
        IngredientsCount = count;
        return this;
    }
}