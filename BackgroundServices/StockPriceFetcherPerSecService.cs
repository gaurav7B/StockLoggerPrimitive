using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using StockLogger.Helpers;
using StockLogger.Models.DTO;
using System.Timers;
using StockLogger.Models.Candel;

namespace StockLogger.BackgroundServices
{
    public class StockPriceFetcherPerSecService : BackgroundService
    {
        private readonly ILogger<StockPriceFetcherPerSecService> _logger;
        private readonly HttpClient _httpClient;

        public StockPriceFetcherPerSecService(ILogger<StockPriceFetcherPerSecService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }

        //// Mandatory override for the BackgroundService
        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        var stocks = (await GetStockTickerExchanges()).Select(x => new { Ticker = (string)x.Ticker, Exchange = (string)x.Exchange }).ToArray(); //got the TICKERS here using the API

        //        var stockTasks = stocks.Select(stock => FetchStockDataAsync(stock.Ticker, stock.Exchange)); // Create tasks dynamically for each stock
        //        await Task.WhenAll(stockTasks); // Process all tasks in parallel
        //        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);  // Delay of 1 sec before the next iteration
        //    }

        //}


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new System.Timers.Timer(1000); // Trigger every 1 second
            timer.Elapsed += async (sender, e) =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    timer.Stop();
                    timer.Dispose();
                    return;
                }

                await FetchStockPricesForAllTickers();
            };

            timer.Start();

            await Task.Delay(Timeout.Infinite, stoppingToken); // Keeps the service alive
        }

        private async Task FetchStockPricesForAllTickers()
        {
            var stocks = (await GetStockTickerExchanges()).Select(x => new { Ticker = (string)x.Ticker, Exchange = (string)x.Exchange }).ToArray();
            //            var stocks = (await GetStockTickerExchanges() ?? Enumerable.Empty<dynamic>())
            //.Where(x => x.Ticker != null && x.Exchange != null)
            //.Select(x => new { Ticker = (string)x.Ticker, Exchange = (string)x.Exchange })
            //.ToArray();
            var stockTasks = stocks.Select(stock => FetchStockDataAsync(stock.Ticker, stock.Exchange));
            await Task.WhenAll(stockTasks);
        }


        // Method to fetch stock TICKER and EXCHANGE data from the API
        private async Task<IEnumerable<dynamic>> GetStockTickerExchanges()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:44364/api/StockTickerExchange/GetStockTickerExchangesWithoutID"); // API to get the TICKER and EXCHANGE

                if (response.IsSuccessStatusCode)
                {
                    var stockTickerExchanges = await response.Content.ReadAsAsync<IEnumerable<StockTickerExchangeDto>>();
                    return stockTickerExchanges.Select(x => new { Ticker = x.Ticker, Exchange = x.Exchange });
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;

            }
        }

        private async Task FetchStockDataAsync(string ticker, string exchange) // Methode to save the StockPricePerSec Data in the Database
        {
            try
            {
                var TickerArray = new StockTickerExchange[] { };

                var response = await _httpClient.GetAsync($"https://localhost:44364/Stock/GetStockPriceByTicker?ticker={ticker}&exchange={exchange}"); // API to get the TICKER time and price

                var TickerData = await _httpClient.GetAsync("https://localhost:44364/api/StockTickerExchange/GetStockTickerExchanges"); // API to get the list of Tickers available with there IDs

                if (TickerData.IsSuccessStatusCode)
                {
                    var stockTickerExchanges = await TickerData.Content.ReadAsAsync<IEnumerable<StockTickerExchange>>();
                    TickerArray = stockTickerExchanges.ToArray();
                }

                if (response.IsSuccessStatusCode)
                {
                    var stockData = await response.Content.ReadAsAsync<StockDataDto>();

                    // Find the TickerId where the ticker matches
                    var tickerMatch = TickerArray.FirstOrDefault(t => t.Ticker == ticker);

                    var stockPricePayload = new StockPricePerSec
                    {
                        Ticker = ticker,
                        TickerId = tickerMatch != null ? tickerMatch.Id : 0, // Assign TickerId if match found, else 0
                        StockDateTime = stockData.time,
                        StockPrice = stockData.price,
                    };

                    // Send the POST request to save the data in the database
                    var postResponse = await _httpClient.PostAsJsonAsync("https://localhost:44364/api/StockPricePerSec/PostStockPricePerSec", stockPricePayload);

                }
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"An error occurred while fetching stock price for {ticker}.");
            }
        }
    }
}
