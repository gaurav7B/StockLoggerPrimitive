using StockLogger.Models.Candel;
using StockLogger.Models.DTO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

namespace StockLogger.BackgroundServices
{
    public class CandelMakerWithPriceCallEvery30sec : BackgroundService
    {
        private readonly ILogger<CandelMakerWithPriceCallEvery30sec> _logger;
        private readonly HttpClient _httpClient;
        private bool _isRunning = true;

        public CandelMakerWithPriceCallEvery30sec(ILogger<CandelMakerWithPriceCallEvery30sec> logger)
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

        public async Task<List<List<StockPricePerSec>>> GroupStockPricesByMinute(StockPricePerSec[] stockPrices)
        {

            // Group by minute (rounded down to the minute)
            var groupedStockPrices = stockPrices
                .GroupBy(sp => new DateTime(sp.StockDateTime.Year, sp.StockDateTime.Month, sp.StockDateTime.Day, sp.StockDateTime.Hour, sp.StockDateTime.Minute, 0))  // Round down to the minute
                .OrderBy(g => g.Key)  // Sort by datetime
                .ToList();

            int groupedStockPricesCount = groupedStockPrices.Count;

            // Create a list of lists (child arrays) for each group
            List<List<StockPricePerSec>> result = new List<List<StockPricePerSec>>();

            foreach (var group in groupedStockPrices)
            {
                result.Add(group.ToList());  // Add each group as a child array
            }

            return result;
        }


        public static StockPricePerSec GetHighestStockPrice(List<StockPricePerSec> stockList)
        {
            // Get the object with the highest StockPrice
            var highestStockPrice = stockList.OrderByDescending(stock => stock.StockPrice).FirstOrDefault();

            return highestStockPrice;
        }

        public static StockPricePerSec GetLowestStockPrice(List<StockPricePerSec> stockList)
        {
            // Get the object with the lowest StockPrice
            var lowestStockPrice = stockList.OrderBy(stock => stock.StockPrice).FirstOrDefault();

            return lowestStockPrice;
        }


        public static StockPricePerSec GetFirstStockPrice(List<StockPricePerSec> stockList)
        {
            // Get the first object in the list
            var firstStockPrice = stockList.FirstOrDefault();

            return firstStockPrice;
        }

        public static StockPricePerSec GetLastStockPrice(List<StockPricePerSec> stockList)
        {
            // Get the last object in the list
            var lastStockPrice = stockList.LastOrDefault();

            return lastStockPrice;
        }



        public async void AddCandelToDatabase(List<List<StockPricePerSec>> soretedArray)
        {
            StockPricePerSec firstStockPrice = null;
            StockPricePerSec highestStockPrice = null;
            StockPricePerSec lowestStockPrice = null;
            StockPricePerSec lastStockPrice = null;
            string ticker = null;
            long Id = 0;
            string exchange = null;

            foreach (List<StockPricePerSec> stockList in soretedArray)
            {
                if(stockList.Count > 58)
                {
                    List<Candel> CandelList = new List<Candel>();

                    firstStockPrice = GetFirstStockPrice(stockList);
                    highestStockPrice = GetHighestStockPrice(stockList);
                    lowestStockPrice = GetLowestStockPrice(stockList);
                    lastStockPrice = GetLastStockPrice(stockList);

                    var CandelPayLoad = new Candel
                    {
                        StartPrice = firstStockPrice.StockPrice,  // Use the first price from the list
                        HighestPrice = highestStockPrice.StockPrice, // Get the highest price from the list
                        LowestPrice = lowestStockPrice.StockPrice,  // Get the lowest price from the list
                        EndPrice = lastStockPrice.StockPrice,     // Use the last price from the list

                        OpenTime = firstStockPrice.StockDateTime,
                        CloseTime = lastStockPrice.StockDateTime,

                        Ticker = firstStockPrice.Ticker,
                        TickerId = firstStockPrice.TickerId,
                        Exchange = "NSE",
                    };

                    // Set the BullBear status based on your logic
                    CandelPayLoad.SetBullBearStatus();
                    CandelPayLoad.SetPriceChange();

                    CandelList.Add(CandelPayLoad);

                    foreach (Candel CurrentCandel in CandelList)
                    {
                        await _httpClient.PostAsJsonAsync("https://localhost:44364/api/Candel", CurrentCandel);
                    }

                    firstStockPrice = null;
                    highestStockPrice = null;
                    lowestStockPrice = null;
                    lastStockPrice = null;
                    ticker = null;
                    Id = 0;
                    exchange = null;
                }


            }
        }


        private async Task CandelMaker(int Id, string ticker, string exchange)
        {
            try
            {

                List<List<StockPricePerSec>> soretedArray = new List<List<StockPricePerSec>>();

                // List to hold all the generated Candels
                List<Candel> candlesList = new List<Candel>();
                List<StockPricePerSec> StockPricePerSecList = new List<StockPricePerSec>();


                var stopwatch = new Stopwatch();

                while (_isRunning)
                {
                    stopwatch.Restart();

                    try
                    {
                        // Make API request
                        var response = await _httpClient.GetAsync($"https://localhost:44364/Stock/GetStockPriceByTicker?ticker={ticker}&exchange={exchange}");

                        if (response.IsSuccessStatusCode)
                        {
                            // Parse JSON response
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            var stockData = JsonSerializer.Deserialize<StockDataDto>(jsonResponse);

                            if (stockData != null)
                            {
                                var StockPerSecPayload = new StockPricePerSec
                                {
                                    Ticker = ticker,
                                    TickerId = Id,
                                    StockDateTime = stockData.time,
                                    StockPrice = stockData.price,
                                };

                                StockPricePerSecList.Add(StockPerSecPayload);

                                soretedArray = await GroupStockPricesByMinute(StockPricePerSecList.ToArray());  // Use await here to get the actual result

                                AddCandelToDatabase(soretedArray);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Failed to fetch data. Status code: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }

                    // Adjust delay to maintain 1-second intervals
                    stopwatch.Stop();
                    var delay = Math.Max(0, 1000 - (int)stopwatch.ElapsedMilliseconds);
                    await Task.Delay(delay);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching stock price for {ticker}.");
            }
        }


    }
}
