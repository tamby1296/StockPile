using Microsoft.EntityFrameworkCore;

namespace StockPile.Infrastructure.Persistence;

public sealed class StockPileDbContext(DbContextOptions<StockPileDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockPileDbContext).Assembly);
    }
}
