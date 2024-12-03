using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StockLogger.Models.DTO;


namespace StockLogger.BackgroundServices
{
  public class StockPriceFetcherService : BackgroundService
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<StockPriceFetcherService> _logger;
    private readonly string _baseDirectory = @"C:\Users\Admin\Desktop\Project\Data";

    public StockPriceFetcherService(HttpClient httpClient, ILogger<StockPriceFetcherService> logger)
    {
      _httpClient = httpClient;
      _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        await FetchAndLogStockPriceAsync();
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
      }
    }

    private async Task FetchAndLogStockPriceAsync()
    {
      var requestDto = new GetStockPriceRequestDto
      {
        Ticker = "INFY",
        Exchange = "NSE"
      };

      var url = $"https://www.google.com/finance/quote/{requestDto.Ticker}:{requestDto.Exchange}";

      try
      {
        var response = await _httpClient.GetStringAsync(url);
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(response);

        var priceNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'YMlKec fxKbKc')]");
        if (priceNode != null)
        {
          var priceText = priceNode.InnerText.Trim();
          if (decimal.TryParse(priceText.Substring(1).Replace(",", ""), out var price))
          {
            var stockData = new StockDataDto
            {
              time = DateTime.Now.ToString("hh:mm:ss tt"),
              price = price
            };

            LogStockData(stockData);
          }
          else
          {
            _logger.LogWarning("Failed to parse price.");
          }
        }
        else
        {
          _logger.LogWarning("Price not found.");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An error occurred while fetching stock price.");
      }
    }

    private void LogStockData(StockDataDto stockData)
    {
      var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
      var filePath = Path.Combine(_baseDirectory, $"{currentDate}.txt");

      var stockDataList = new List<StockDataDto>();

      if (File.Exists(filePath))
      {
        var existingData = File.ReadAllText(filePath);
        stockDataList = JsonSerializer.Deserialize<List<StockDataDto>>(existingData) ?? new List<StockDataDto>();
      }

      stockDataList.Add(stockData);

      var jsonOptions = new JsonSerializerOptions
      {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };

      var json = JsonSerializer.Serialize(stockDataList, jsonOptions);
      File.WriteAllText(filePath, json);
    }
  }
}
