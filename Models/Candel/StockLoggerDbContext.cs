using Microsoft.EntityFrameworkCore;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models;

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
        public DbSet<Candel> Candel { get; set; } //For 1 minute candel
        public DbSet<Candel5min> Candel5min { get; set; } // For 5 minute candel
        public DbSet<Candel10min> Candel10min { get; set; } // For 10 minute candel
        public DbSet<Candel15min> Candel15min { get; set; } // For 15 minute candel
        public DbSet<ThreeWhiteSoilderDb> ThreeWhiteSoilderDbs { get; set; }
        public DbSet<ThreeWhiteSoilderCandels> ThreeWhiteSoilderCandelss { get; set; }
    }
}
