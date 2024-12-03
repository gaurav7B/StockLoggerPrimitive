using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using StockLogger.Models.DTO;

namespace StockLogger.Controllers
{

  public class StockController : Controller
  {
    private readonly HttpClient _httpClient;

    public StockController(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetLatestStockPrice()
    {
      var requestDto = new GetStockPriceRequestDto
      {
        Ticker = "INFY",
        Exchange = "NSE"
      };

      var url = $"https://www.google.com/finance/quote/{requestDto.Ticker}:{requestDto.Exchange}";

      var response = await _httpClient.GetStringAsync(url);

      var htmlDoc = new HtmlDocument();
      htmlDoc.LoadHtml(response);

      var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'YMlKec fxKbKc')]");
      if (priceNode == null)
      {
        return NotFound("Price not found.");
      }

      var priceText = priceNode.InnerText.Trim();
      if (!decimal.TryParse(priceText.Substring(1).Replace(",", ""), out var price))
      {
        return BadRequest("Failed to parse price.");
      }

      var stockData = new StockDataDto
      {
        time = DateTime.Now.ToString("HH:mm:ss"),
        price = price
      };

      return View(stockData);
    }
  }
}
