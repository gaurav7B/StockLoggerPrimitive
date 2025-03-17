using System.ComponentModel.DataAnnotations;

namespace StockLogger.Models.Candel
{
    public class LTP
    {
        [Key]
        public long Id { get; set; }
        public string SymbolToken { get; set; }
        public string Ticker { get; set; }
        public string Exchange { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateTime Time { get; set; }
    }
}
