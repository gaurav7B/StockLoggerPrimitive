using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StockLogger.Helpers;  // Import the new helper class
using StockLogger.Models.DTO;

namespace StockLogger.BackgroundServices
{
    public class StockPriceFetcherService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StockPriceFetcherService> _logger;
        private readonly StockDataLogger _stockDataLogger;

        public StockPriceFetcherService(HttpClient httpClient, ILogger<StockPriceFetcherService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _stockDataLogger = new StockDataLogger(@"C:\Users\Admin\Desktop\Project");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var infyTask = FetchAndLogStockPriceAsync("INFY", "NSE");
                var relianceTask = FetchAndLogStockPriceAsync("RELIANCE", "NSE");

                await Task.WhenAll(infyTask, relianceTask);

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task FetchAndLogStockPriceAsync(string ticker, string exchange)
        {
            var requestDto = new GetStockPriceRequestDto
            {
                Ticker = ticker,
                Exchange = exchange
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

                        _stockDataLogger.LogStockData(ticker, stockData);
                        _stockDataLogger.LogStockDataInJSON(ticker, stockData);
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
                _logger.LogError(ex, $"An error occurred while fetching stock price for {ticker}.");
            }
        }
    }
}

