using System.ComponentModel.DataAnnotations;

namespace StockLogger.Models.Candel
{
    public class StockTickerExchange
    {
        [Key]
        public int Id { get; set; }

        public string Ticker { get; set; }
        public string Exchange { get; set; }
    }
}
