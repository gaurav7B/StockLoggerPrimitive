using StockLogger.BackgroundServices.BackgroundStratergyServices.HelperMethods;
using StockLogger.Helpers;
using StockLogger.Models.Candel;
using StockLogger.Models.DTO;
using System.Diagnostics;
using System.Text.Json;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices
{
    public class CupAndHandelService : BackgroundService
    {
        private readonly ILogger<CupAndHandelService> _logger;
        private readonly HttpClient _httpClient;
        private readonly GetTickerExchangeHelperMethode _GetTickerExchangeHelperMethode;
        private bool _isRunning = true;

        public CupAndHandelService(ILogger<CupAndHandelService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            _GetTickerExchangeHelperMethode = new GetTickerExchangeHelperMethode();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var stocks = (await _GetTickerExchangeHelperMethode.GetStockTickerExchanges())?.Select(x => new { x.Id, x.Ticker, x.Exchange }).ToArray();

                var stockTasks = stocks.Select(stock => GetRecentTenCandels((int)stock.Id, stock.Ticker, stock.Exchange)); // Create tasks dynamically for each stock
                await Task.WhenAll(stockTasks); // Process all tasks in parallel
            }
        }

        public async void CupAndHandleAnalyzer(List<Candel> candelList)
        {
            // Ensure there are enough candles for analysis (min. 10 for a cup and handle)
            if (candelList.Count < 10)
            {
                Console.WriteLine("The list must contain at least 10 candles.");
                return;
            }

            // Get the last 10 candles for detecting the pattern (you can adjust this as needed)
            List<Candel> recentCandles = candelList.OrderByDescending(c => c.CloseTime).Take(10).ToList();

            // 1. Check for Cup Formation (Rounded bottom)
            bool cupFormed = IsCupFormed(recentCandles);

            // 2. Check for Handle Formation (Consolidation after Cup)
            bool handleFormed = IsHandleFormed(recentCandles);

            // 3. Check if the breakout occurs (price rises above the handle)
            bool breakoutConfirmed = recentCandles[0].EndPrice > recentCandles[1].EndPrice; // Ensure the most recent candle closes higher than the previous

            // Combine all conditions to detect the Cup and Handle pattern
            if (cupFormed && handleFormed && breakoutConfirmed)
            {
                Console.WriteLine("Cup and Handle Pattern Detected");
            }
            else
            {
                Console.WriteLine("Cup and Handle Pattern Not Detected");
            }
        }

        private bool IsCupFormed(List<Candel> candles)
        {
            // A cup is formed when the price drops and then gradually rises again
            // Check for progressively lower lows followed by progressively higher highs
            bool lowerLows = candles[0].LowestPrice < candles[1].LowestPrice && candles[1].LowestPrice < candles[2].LowestPrice;
            bool higherHighs = candles[3].HighestPrice > candles[4].HighestPrice && candles[4].HighestPrice > candles[5].HighestPrice;

            return lowerLows && higherHighs;
        }

        private bool IsHandleFormed(List<Candel> candles)
        {
            // The handle forms after the cup when the price consolidates or pulls back but doesn't fall below the lowest point of the cup
            bool priceConsolidating = candles[6].EndPrice < candles[7].EndPrice && candles[7].EndPrice < candles[8].EndPrice;

            // Ensure that the handle does not fall below the lowest point of the cup
            bool noDeepPullback = candles[8].LowestPrice > candles[2].LowestPrice;

            return priceConsolidating && noDeepPullback;
        }



        private async Task GetRecentTenCandels(int Id, string ticker, string exchange)
        {
            try
            {
                var stopwatch = new Stopwatch();
                while (_isRunning)
                {
                    stopwatch.Restart();

                    try
                    {
                        // Send GET request to the API endpoint
                        var response = await _httpClient.GetAsync($"https://localhost:44364/api/candel/recentTen?ticker={ticker}&exchange={exchange}");

                        if (response.IsSuccessStatusCode)
                        {
                            // Deserialize the response body into a list of Candel objects
                            List<Candel> candelList = await response.Content.ReadAsAsync<List<Candel>>();

                            // Analyzes the recent four candels for 3 WHITE SOILDER PATTERN and adds it to the Database
                            CupAndHandleAnalyzer(candelList);

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


