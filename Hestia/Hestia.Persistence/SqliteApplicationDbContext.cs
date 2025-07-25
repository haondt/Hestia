using Microsoft.EntityFrameworkCore;

namespace Hestia.Persistence
{
    public class SqliteApplicationDbContext(DbContextOptions<SqliteApplicationDbContext> options) : ApplicationDbContext(options)
    {
    }
}
