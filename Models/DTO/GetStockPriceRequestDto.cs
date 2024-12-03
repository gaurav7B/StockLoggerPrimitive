namespace StockLogger.Models.DTO
{
  public class GetStockPriceRequestDto
  {
    public string Ticker { get; set; } = "INFY";
    public string Exchange { get; set; } = "NSE";
  }
}
