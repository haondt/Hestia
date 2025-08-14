using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hestia.Persistence.Models
{
    public record FoodLogDataModel
    {
        public int Id { get; set; }
        public required string DateString { get; set; }
        public ICollection<FoodLogSectionDataModel> Sections { get; set; } = [];

        public int? MealPlanId { get; set; }
        public MealPlanDataModel? MealPlan { get; set; }

    }

    public class FoodLogStateEntityTypeConfiguration : IEntityTypeConfiguration<FoodLogDataModel>
    {
        public void Configure(EntityTypeBuilder<FoodLogDataModel> builder)
        {
            builder
                .HasIndex(x => x.DateString)
                .IsUnique();

            builder
                .HasOne(fl => fl.MealPlan)
                .WithMany(mp => mp.FoodLogs)
                .HasForeignKey(fl => fl.MealPlanId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}