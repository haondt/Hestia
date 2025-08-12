
using Hestia.Core.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hestia.Persistence.Models
{
    public class HestiaStateDataModel
    {
        public int Id { get; set; }
        public bool HasSeededUnitConversions { get; set; } = false;
        public int NextMealPlanNumber { get; set; } = 1;

        public static async Task<HestiaStateDataModel> GetOrCreateAsync(ApplicationDbContext dbContext)
        {
            var state = await dbContext.HestiaStates.FirstOrDefaultAsync(q => q.Id == HestiaConstants.HestiaStateId);
            if (state != null)
                return state;
            state = new()
            {
                Id = HestiaConstants.HestiaStateId
            };
            dbContext.Add(state);
            await dbContext.SaveChangesAsync();
            return state;
        }

    }

    public class HestiaStateEntityTypeConfiguration : IEntityTypeConfiguration<HestiaStateDataModel>
    {
        public void Configure(EntityTypeBuilder<HestiaStateDataModel> builder)
        {
            builder.Property(e => e.Id)
                .ValueGeneratedNever();
        }
    }

}
