using Haondt.Core.Models;
using Hestia.Core.Models;
using Hestia.Persistence.Converters;
using Hestia.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Hestia.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<IngredientDataModel> Ingredients { get; set; } = default!;
        public DbSet<RecipeDataModel> Recipes { get; set; } = default!;
        public DbSet<RecipeIngredientDataModel> RecipeIngredients { get; set; } = default!;
        public DbSet<UnitConversionDataModel> UnitConversions { get; set; } = default!;
        public DbSet<HestiaStateDataModel> HestiaStates { get; set; } = default!;
        public DbSet<MealPlanDataModel> MealPlans { get; set; } = default!;
        public DbSet<FoodLogDataModel> FoodLogs { get; set; } = default!;

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<NormalizedString>()
                .HaveConversion<NormalizedStringConverter>();
            configurationBuilder
                .Properties<AbsoluteDateTime>()
                .HaveConversion<AbsoluteDateTimeConverter>();
        }
    }
}
