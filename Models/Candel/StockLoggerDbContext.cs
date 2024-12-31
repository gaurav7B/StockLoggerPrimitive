using Microsoft.EntityFrameworkCore;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models;
using StockLogger.Models.Stratergic_Models.Breakaway__Bullish_;
using StockLogger.Models.Stratergic_Models.Bullish_Engulfing;
using StockLogger.Models.Stratergic_Models.Bullish_Harami;
using StockLogger.Models.Stratergic_Models.Dragonfly_Doji;
using StockLogger.Models.Stratergic_Models.Hammer;
using StockLogger.Models.Stratergic_Models.Inverted_Hammer;
using StockLogger.Models.Stratergic_Models.Morning_Star;
using StockLogger.Models.Stratergic_Models.Piercing_Line;
using StockLogger.Models.Stratergic_Models.Rising_Three_Methods;
using StockLogger.Models.Stratergic_Models.Tweezer_Bottom;

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

        //CANDELS
        public DbSet<Candel> Candel { get; set; } //For 1 minute candel
        public DbSet<Candel5min> Candel5min { get; set; } // For 5 minute candel
        public DbSet<Candel10min> Candel10min { get; set; } // For 10 minute candel
        public DbSet<Candel15min> Candel15min { get; set; } // For 15 minute candel

        //3WS-TRADING STRATERGY
        public DbSet<ThreeWhiteSoilderDb> ThreeWhiteSoilderDbs { get; set; }
        public DbSet<ThreeWhiteSoilderCandels> ThreeWhiteSoilderCandelss { get; set; }

        //INVERTED_HAMMER TRADING STRATERGY
        public DbSet<InvertedHammerDb> InvertedHammerDb { get; set; }
        public DbSet<InvertedHammerCandels> InvertedHammerCandels { get; set; }

        //MORNING_STAR TRADING STARTERGY
        public DbSet<MorningStarDb> MorningStarDb { get; set; }
        public DbSet<MorningStarCandels> MorningStarCandels { get; set; }

        //BULLISH_ENGULFING TRADING STARTERGY
        public DbSet<BullishEngulfingDb> BullishEngulfingDb { get; set; }
        public DbSet<BullishEngulfingCandels> BullishEngulfingCandels { get; set; }

        //BULLISH_HARAMI TRADING STARTERGY
        public DbSet<BullishHaramiDb> BullishHaramiDb { get; set; }
        public DbSet<BullishHaramiCandels> BullishHaramiCandels { get; set; }

        //DRAGONFLY_DOJI TRADING STARTERGY
        public DbSet<DragonflyDojiDb> DragonflyDojiDb { get; set; }
        public DbSet<DragonflyDojiCandels> DragonflyDojiCandels { get; set; }

        //HAMMER TRADING STRATERGY
        public DbSet<HammerDb> HammerDb { get; set; }
        public DbSet<HammerCandels> HammerCandels { get; set; }

        //PIERCING LINE TRADING STRATERGY
        public DbSet<PiercingLineDb> PiercingLineDb { get; set; }
        public DbSet<PiercingLineCandels> PiercingLineCandels { get; set; }

        //RISING_THREE TRADING STRATERGY
        public DbSet<RisingThreeMethodsDb> RisingThreeMethodsDb { get; set; }
        public DbSet<RisingThreeMethodsCandels> RisingThreeMethodsCandels { get; set; }

        //TWEEZER_BOTTOM TRADING STRTERGY
        public DbSet<TweezerBottomDb> TweezerBottomDb { get; set; }
        public DbSet<TweezerBottomCandels> TweezerBottomCandels { get; set; }

        //BREAKAWAY_BULLISH TRADING STARTERGY
        public DbSet<BreakawayDb> BreakawayDb { get; set; }
        public DbSet<BreakawayCandels> BreakawayCandels { get; set; }

    }
}
