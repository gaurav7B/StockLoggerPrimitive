using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly string _baseDirectory = @"C:\Users\Admin\Desktop\Project";

        public StockPriceFetcherService(HttpClient httpClient, ILogger<StockPriceFetcherService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
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

                        LogStockData(ticker, stockData);
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

        private void LogStockData(string ticker, StockDataDto stockData)
        {
            var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            var directoryPath = _baseDirectory;

            var dataDirectoryPath = Path.Combine(directoryPath, "Data");
            if (!Directory.Exists(dataDirectoryPath))
            {
                Directory.CreateDirectory(dataDirectoryPath);
            }

            var filePath = Path.Combine(dataDirectoryPath, $"{ticker}_{currentDate}.txt");

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
