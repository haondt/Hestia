using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hestia.Persistence.Models
{
    public record RecipeIngredientDataModel
    {
        public int Id { get; set; }

        public RecipeDataModel Recipe { get; set; } = default!;
        public int RecipeId { get; set; }

        public IngredientDataModel? Ingredient { get; set; }
        public int? IngredientId { get; set; }

        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }
        public required string Name { get; set; }  // in case the ingredient is deleted
    }

    public class RecipeIngredientDataModelConfiguration : IEntityTypeConfiguration<RecipeIngredientDataModel>
    {
        public void Configure(EntityTypeBuilder<RecipeIngredientDataModel> builder)
        {
            builder.HasOne(x => x.Recipe)
                .WithMany(r => r.Ingredients)
                .HasForeignKey(x => x.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Ingredient)
                .WithMany(r => r.Recipes)
                .HasForeignKey(x => x.IngredientId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
