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
      return View();
    }
  }
}
