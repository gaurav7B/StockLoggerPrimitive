namespace StockLogger.Models.Candel
{
    public class RawCandel
    {
        public DateTime OpenTime { get; set; }
        public decimal StartPrice { get; set; }
        public decimal HighestPrice { get; set; }
        public decimal LowestPrice { get; set; }
        public decimal EndPrice { get; set; }
        public decimal Volume { get; set; }
    }
}
