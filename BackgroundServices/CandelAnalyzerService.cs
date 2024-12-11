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

                var stockTasks = stocks.Select(stock => CandelMaker((int)stock.Id, stock.Ticker, stock.Exchange)); // Create tasks dynamically for each stock
                await Task.WhenAll(stockTasks); // Process all tasks in parallel
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Delay of 1 Min before the next iteration
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

        private async Task CandelMaker(int Id, string ticker, string exchange)
        {
            try
            {
                //// Create a list to store the stock price responses
                //var stockPrices = new List<StockDataDto>();

                //// Register the start time of the function
                //var startTime = DateTime.Now;

                //// Calculate the end time as one minute after the start time
                //var endTime = startTime.AddMinutes(1);

                //// Continue until the end time is reached
                //while (DateTime.Now < endTime)
                //{
                //    // Check if the current time is at the start of a new minute (00 milliseconds)
                //    if (DateTime.Now.Second == 0)
                //    {
                //        var response = await _httpClient.GetAsync($"https://localhost:44364/Stock/GetStockPriceByTicker?ticker={ticker}&exchange={exchange}");

                //        if (response.IsSuccessStatusCode)
                //        {
                //            var stockData = await response.Content.ReadAsAsync<StockDataDto>();
                //            stockPrices.Add(stockData); // Store the response in the list
                //        }
                //        else
                //        {
                //            _logger.LogError($"Failed to fetch stock price for {ticker}. Response: {response.StatusCode}");
                //        }
                //    }

                //    // Wait for a short period (e.g., 100 milliseconds) to avoid checking multiple times within the same second
                //    await Task.Delay(100);
                //}

                var stockPrices = new List<StockDataDto>();

                // Register the start time of the function
                var startTime = DateTime.Now;

                // Calculate the end time as one minute after the start time
                var endTime = startTime.AddMinutes(1);

                // Continue until the end time is reached
                while (DateTime.Now < endTime)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync($"https://localhost:44364/Stock/GetStockPriceByTicker?ticker={ticker}&exchange={exchange}");

                        if (response.IsSuccessStatusCode)
                        {
                            var stockData = await response.Content.ReadAsAsync<StockDataDto>();
                            stockPrices.Add(stockData); // Store the response in the list
                        }
                        else
                        {
                            _logger.LogError($"Failed to fetch stock price for {ticker}. Response: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while fetching stock price.");
                    }

                    // Wait for 1 second before the next API call
                    await Task.Delay(1000);
                }

                // At this point, `stockPrices` contains multiple objects collected every second for 1 minute


                // After collecting the data, create the Candel payload
                var CandelPayload = new Candel
                {
                    StartPrice = stockPrices.FirstOrDefault()?.price ?? 0,  // Use the first price from the list or 0 if empty
                    HighestPrice = stockPrices.Max(sp => sp.price),         // Get the highest price from the list
                    LowestPrice = stockPrices.Min(sp => sp.price),          // Get the lowest price from the list
                    EndPrice = stockPrices.LastOrDefault()?.price ?? 0,     // Use the last price from the list or 0 if empty

                    OpenTime = startTime,
                    CloseTime = DateTime.Now,

                    Ticker = ticker,
                    TickerId = Id,
                    Exchange = exchange,
                };

                // Set the BullBear status based on your logic
                CandelPayload.SetBullBearStatus();
                CandelPayload.SetPriceChange();

                // Send the POST request to save the data in the database
                //var postResponse = await _httpClient.PostAsJsonAsync("https://localhost:44364/api/Candel", CandelPayload);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching stock price for {ticker}.");
            }
        }


    }
}
