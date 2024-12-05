using Microsoft.EntityFrameworkCore;
using StockLogger.Models.Candel;

namespace StockLogger.Data
{
    public class StockLoggerDbContext : DbContext
    {
        public StockLoggerDbContext(DbContextOptions<StockLoggerDbContext> options)
            : base(options)
        {
        }

        public DbSet<StockTickerExchange> StockTickerExchanges { get; set; }
        public DbSet<StockPricePerSec> StockPricePerSec { get; set; }
        public DbSet<Candel> Candel { get; set; }
    }
}
