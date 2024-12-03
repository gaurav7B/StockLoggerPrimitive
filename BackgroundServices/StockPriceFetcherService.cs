using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StockLogger.Helpers;
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
            try
            {
                var response = await _httpClient.GetAsync($"https://localhost:44364/Stock/GetStockPriceByTicker?ticker={ticker}");

                if (response.IsSuccessStatusCode)
                {
                    var stockData = await response.Content.ReadAsAsync<StockDataDto>();

                    _stockDataLogger.LogStockDataTXT(ticker, stockData);
                    _stockDataLogger.LogStockDataInJSON(ticker, stockData);
                }
                else
                {
                    _logger.LogWarning($"Failed to fetch stock price for {ticker}. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching stock price for {ticker}.");
            }
        }
    }
}

