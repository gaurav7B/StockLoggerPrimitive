using System.ComponentModel.DataAnnotations;

namespace StockLogger.Models.Candel
{
    public class Token
    {
        [Key]
        public long Id { get; set; }
        public string AuthToken { get; set; }
        public string RefreshToken { get; set; }
        public string FeedToken { get; set; }
        public DateTime AuthTokenCreationTime { get; set; }
    }
}
