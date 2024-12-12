using Polly;
using StockLogger.Models.Candel;
using StockLogger.Models.DTO;
using System.Diagnostics;
using System.Text.Json;

namespace StockLogger.BackgroundServices.BackGroundServiceForEach
{
    public class INFY_Service : BackgroundService
    {
        private readonly ILogger<INFY_Service> _logger;
        private readonly HttpClient _httpClient;
        private bool _isRunning = true;

        public INFY_Service(ILogger<INFY_Service> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const int tickerId = 1;
            const string ticker = "INFY";
            const string exchange = "NSE";

            while (!stoppingToken.IsCancellationRequested)
            {
                await CandelMaker(tickerId, ticker, exchange);
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Delay for 30 seconds
            }
        }

        public async Task<List<List<StockPricePerSec>>> GroupStockPricesByMinute(StockPricePerSec[] stockPrices)
        {
            var groupedStockPrices = stockPrices
                .GroupBy(sp => new DateTime(sp.StockDateTime.Year, sp.StockDateTime.Month, sp.StockDateTime.Day, sp.StockDateTime.Hour, sp.StockDateTime.Minute, 0))
                .OrderBy(g => g.Key)
                .ToList();

            return groupedStockPrices.Select(group => group.ToList()).ToList();
        }

        public static StockPricePerSec GetHighestStockPrice(List<StockPricePerSec> stockList) => stockList.OrderByDescending(stock => stock.StockPrice).FirstOrDefault();
        public static StockPricePerSec GetLowestStockPrice(List<StockPricePerSec> stockList) => stockList.OrderBy(stock => stock.StockPrice).FirstOrDefault();
        public static StockPricePerSec GetFirstStockPrice(List<StockPricePerSec> stockList) => stockList.FirstOrDefault();
        public static StockPricePerSec GetLastStockPrice(List<StockPricePerSec> stockList) => stockList.LastOrDefault();

        public async void AddCandelToDatabase(List<List<StockPricePerSec>> sortedArray)
        {
            foreach (List<StockPricePerSec> stockList in sortedArray)
            {
                var lastStock = stockList.Last();
                if (lastStock.StockDateTime.Second < 59)
                {
                    return;
                }

                var firstStockPrice = GetFirstStockPrice(stockList);
                var highestStockPrice = GetHighestStockPrice(stockList);
                var lowestStockPrice = GetLowestStockPrice(stockList);
                var lastStockPrice = GetLastStockPrice(stockList);

                var candelPayload = new Candel
                {
                    StartPrice = firstStockPrice.StockPrice,
                    HighestPrice = highestStockPrice.StockPrice,
                    LowestPrice = lowestStockPrice.StockPrice,
                    EndPrice = lastStockPrice.StockPrice,

                    OpenTime = firstStockPrice.StockDateTime,
                    CloseTime = lastStockPrice.StockDateTime,

                    Ticker = firstStockPrice.Ticker,
                    TickerId = firstStockPrice.TickerId,
                    Exchange = "NSE",
                };

                candelPayload.SetBullBearStatus();
                candelPayload.SetPriceChange();

                var retryPolicy = Policy.Handle<TaskCanceledException>()
                    .Or<TimeoutException>()
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

                var response =  await _httpClient.PostAsJsonAsync("https://localhost:44364/api/Candel", candelPayload);
            }
        }

        private async Task CandelMaker(int id, string ticker, string exchange)
        {
            try
            {
                var stockPricePerSecList = new List<StockPricePerSec>();
                var stopwatch = new Stopwatch();

                while (_isRunning)
                {
                    stopwatch.Restart();

                    try
                    {
                        var response = await _httpClient.GetAsync($"https://localhost:44364/Stock/GetStockPriceByTicker?ticker={ticker}&exchange={exchange}");

                        if (response.IsSuccessStatusCode)
                        {
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            var stockData = JsonSerializer.Deserialize<StockDataDto>(jsonResponse);

                            if (stockData != null)
                            {
                                var stockPerSecPayload = new StockPricePerSec
                                {
                                    Ticker = ticker,
                                    TickerId = id,
                                    StockDateTime = stockData.time,
                                    StockPrice = stockData.price,
                                };

                                stockPricePerSecList.Add(stockPerSecPayload);
                                var sortedArray = await GroupStockPricesByMinute(stockPricePerSecList.ToArray());
                                AddCandelToDatabase(sortedArray);
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
