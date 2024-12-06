using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StockLogger.Helpers;
using StockLogger.Models.DTO;
using StockLogger.Models.Candel;
using System.Timers;

namespace StockLogger.BackgroundServices
{
    public class CandelAnalyzerService : BackgroundService
    {
        private readonly ILogger<CandelAnalyzerService> _logger;
        private readonly HttpClient _httpClient;

        public CandelAnalyzerService(ILogger<CandelAnalyzerService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var stocks = (await GetStockTickerExchanges())?.Select(x => new { x.Id, x.Ticker, x.Exchange }).ToArray();

                var stockTasks = stocks.Select(stock => FetchStockDataAsync((int)stock.Id, stock.Ticker, stock.Exchange)); // Create tasks dynamically for each stock
                await Task.WhenAll(stockTasks); // Process all tasks in parallel
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken); // Delay of 1 sec before the next iteration
            }
        }



        // Method to fetch stock TICKER and EXCHANGE data from the API
        private async Task<IEnumerable<StockTickerExchange>> GetStockTickerExchanges()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:44364/api/StockTickerExchange/GetStockTickerExchanges");

                if (response.IsSuccessStatusCode)
                {
                    var stockTickerExchanges = await response.Content.ReadAsAsync<IEnumerable<StockTickerExchange>>();
                    return stockTickerExchanges;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stock ticker exchanges.");
                return null;
            }
        }




        private async Task FetchStockDataAsync(int Id ,string ticker, string exchange) // Methode to save the StockPricePerSec Data in the Database
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://localhost:44364/Stock/GetStockPriceByTicker?ticker={ticker}"); // API to get the time and price on basis of TICKER

                if (response.IsSuccessStatusCode)
                {
                    var stockData = await response.Content.ReadAsAsync<StockDataDto>();

                    var CandelPayload = new Candel
                    {
                        StartPrice = 0,
                        HighestPrice = 0,
                        LowestPrice = 0,
                        EndPrice = 0,

                        OpenTime = DateTime.Now,
                        CloseTime = DateTime.Now,

                        Ticker = ticker,
                        TickerId = Id,
                        Exchange = exchange,
                    };
                    CandelPayload.SetBullBearStatus();

                    // Send the POST request to save the data in the database
                    var postResponse = await _httpClient.PostAsJsonAsync("https://localhost:44364/api/Candel", CandelPayload);

                    if (!postResponse.IsSuccessStatusCode)
                    {
                        var errorContent = await postResponse.Content.ReadAsStringAsync();
                        Console.WriteLine(errorContent); // Log or debug this to see the detailed error message
                    }

                    if (postResponse.IsSuccessStatusCode)
                    {
                        var stockTickerExchanges = await response.Content.ReadAsAsync<IEnumerable<StockTickerExchange>>();
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching stock price for {ticker}.");
            }
        }




    }
}
