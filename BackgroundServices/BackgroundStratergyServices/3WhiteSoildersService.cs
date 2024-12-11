﻿using StockLogger.BackgroundServices.BackgroundStratergyServices.HelperMethods;
using StockLogger.Helpers;
using StockLogger.Models.Candel;
using StockLogger.Models.DTO;
using System.Diagnostics;
using System.Text.Json;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices
{
    public class _3WhiteSoildersService : BackgroundService
    {
        private readonly ILogger<_3WhiteSoildersService> _logger;
        private readonly HttpClient _httpClient;
        private readonly GetTickerExchangeHelperMethode _GetTickerExchangeHelperMethode;
        private bool _isRunning = true;

        public _3WhiteSoildersService(ILogger<_3WhiteSoildersService> logger)
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

                var stockTasks = stocks.Select(stock => GetRecentThreeCandels((int)stock.Id, stock.Ticker, stock.Exchange)); // Create tasks dynamically for each stock
                await Task.WhenAll(stockTasks); // Process all tasks in parallel
            }
        }


        //public async void ThreeWhiteSoilderAnalyzer(List<Candel> candelList)
        //{
        //    // Ensure there are at least 3 candles in the list
        //    if (candelList.Count < 3)
        //    {
        //        Console.WriteLine("The list must contain at least 3 candles.");
        //        return;
        //    }

        //    // Get the last 3 candles from the list (the most recent 3)
        //    List<Candel> recentThreeCandles = candelList.OrderByDescending(c => c.CloseTime).Take(3).ToList();

        //    // Check if all three candles are bullish
        //    bool allThreeBullish = recentThreeCandles.All(c => c.IsBullish == true);

        //    // Check if the three candles close higher than the previous one
        //    bool progressiveCloses = recentThreeCandles[0].EndPrice > recentThreeCandles[1].EndPrice &&
        //                              recentThreeCandles[1].EndPrice > recentThreeCandles[2].EndPrice;

        //    // Check if the bodies of the candles are progressively larger
        //    bool increasingBodySize = (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice) >
        //                              (recentThreeCandles[1].EndPrice - recentThreeCandles[1].StartPrice) &&
        //                              (recentThreeCandles[1].EndPrice - recentThreeCandles[1].StartPrice) >
        //                              (recentThreeCandles[2].EndPrice - recentThreeCandles[2].StartPrice);

        //    if (allThreeBullish && progressiveCloses && increasingBodySize)
        //    {
        //        Console.WriteLine("Three White Soldiers Pattern Detected");
        //    }
        //    else
        //    {
        //        Console.WriteLine("Three White Soldiers Pattern Not Detected");
        //    }
        //}

        public async void ThreeWhiteSoilderAnalyzer(List<Candel> candelList)
        {
            // Ensure there are at least 4 candles in the list
            if (candelList.Count < 4)
            {
                Console.WriteLine("The list must contain at least 4 candles.");
                return;
            }

            // Get the last 3 candles from the list (the most recent 3)
            List<Candel> recentThreeCandles = candelList.OrderByDescending(c => c.CloseTime).Take(3).ToList();

            // Check if all three candles are bullish
            bool allThreeBullish = recentThreeCandles.All(c => c.IsBullish == true);

            // Check if the three candles close higher than the previous one
            bool progressiveCloses = recentThreeCandles[0].EndPrice > recentThreeCandles[1].EndPrice &&
                                      recentThreeCandles[1].EndPrice > recentThreeCandles[2].EndPrice;

            // Check if the bodies of the candles are progressively larger
            bool increasingBodySize = (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice) > (recentThreeCandles[1].EndPrice - recentThreeCandles[1].StartPrice) 
                                                                                                          &&
                                      (recentThreeCandles[1].EndPrice - recentThreeCandles[1].StartPrice) > (recentThreeCandles[2].EndPrice - recentThreeCandles[2].StartPrice);

            // Check for small upper and lower shadows
            bool smallUpperShadow = (recentThreeCandles[0].HighestPrice - recentThreeCandles[0].EndPrice) < (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice);

            bool smallLowerShadow = (recentThreeCandles[0].StartPrice - recentThreeCandles[0].LowestPrice) < (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice);

            // Check if the body is at least 60% of the total range (strong body)
            decimal range = recentThreeCandles[0].HighestPrice - recentThreeCandles[0].LowestPrice;
            bool strongBodyRatio = range != 0 && (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice) / range > 0.6m;

            // Check the 4th previous candle for a potential downtrend or neutral pattern
            bool priorConsolidationOrBearish = candelList.Count > 3 &&
                                               (candelList[3].IsBearish == true || candelList[3].IsBullish == false);

            // Combine all conditions to detect the Three White Soldiers pattern
            if (allThreeBullish && progressiveCloses && increasingBodySize && smallUpperShadow && smallLowerShadow &&
                strongBodyRatio && priorConsolidationOrBearish)
            {
                Console.WriteLine("Three White Soldiers Pattern Detected");
            }
            else
            {
                Console.WriteLine("Three White Soldiers Pattern Not Detected");
            }
        }





        private async Task GetRecentThreeCandels(int Id, string ticker, string exchange)
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
                        var response = await _httpClient.GetAsync($"https://localhost:44364/api/candel/recent?ticker={ticker}&exchange={exchange}");

                        if (response.IsSuccessStatusCode)
                        {
                            // Deserialize the response body into a list of Candel objects
                            List<Candel> candelList = await response.Content.ReadAsAsync<List<Candel>>();

                            // Analyzes the recent four candels for 3 WHITE SOILDER PATTERN and adds it to the Database
                            ThreeWhiteSoilderAnalyzer(candelList);

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
