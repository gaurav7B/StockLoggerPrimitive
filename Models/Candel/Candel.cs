namespace StockLogger.Models.Candel
{
    public class Candel
    {
        // Core properties
        public decimal StartPrice { get; set; }
        public decimal HighestPrice { get; set; }
        public decimal LowestPrice { get; set; }
        public decimal EndPrice { get; set; }

        // Time information
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }

        // Volume data
        public decimal Volume { get; set; }
        public decimal Turnover { get; set; }  // Optional, for total trade value

        // Meta-information
        public string Ticker { get; set; }
        public string Exchange { get; set; }

        // Calculated properties
        public bool IsBullish => EndPrice > StartPrice;
        public bool IsBearish => EndPrice < StartPrice;
        public decimal PriceChange => EndPrice - StartPrice;
        public decimal PriceChangePercentage => StartPrice != 0 ? (PriceChange / StartPrice) * 100 : 0;

        // Indicators (optional, for advanced analytics)
        public decimal? MovingAverage { get; set; }
        public decimal? BollingerUpper { get; set; }
        public decimal? BollingerLower { get; set; }
    }
}
