using Hestia.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Hestia.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<IngredientDataModel> Ingredients { get; set; } = default!;

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

    }
}
