using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using OtpNet;
using Skender.Stock.Indicators;
using StockLogger.BackgroundServices.BackgroundStratergyServices;
using StockLogger.BackgroundServices.Helper_methods;
using StockLogger.Data;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Dragonfly_Doji;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using Talib.Indicators;

namespace StockLogger.Controllers.API_Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestShortSellController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;
        private readonly CandleAnalysisMethods _candleAnalysisMethods;

        public TestShortSellController(StockLoggerDbContext context , CandleAnalysisMethods candleAnalysisMethods)
        {
            _context = context;
            _candleAnalysisMethods = candleAnalysisMethods;
            _stocks = StockList2.GetStocks()
                .Select(stock => (stock.Ticker, stock.Exchange, stock.Name, stock.Id, stock.SymbolToken))
                .ToList();
        }

        public class BulkTestRequestModel
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        public static decimal ForecastMaxHigh(List<Candel> candels)
        {
            if (candels == null || candels.Count == 0)
                throw new ArgumentException("Candel list cannot be empty.");

            // 1) Find the actual max high from historical data
            decimal historicalMaxHigh = candels.Max(c => c.HighestPrice);

            // 2) Get the latest candel
            var lastCandel = candels.OrderByDescending(c => c.OpenTime).First();

            // 3) Calculate average volatility (High - Low)
            decimal avgVolatility = candels.Average(c => c.HighestPrice - c.LowestPrice);

            // 4) Calculate recent bullish momentum (last 5 candels)
            var recentCandels = candels.OrderByDescending(c => c.OpenTime).Take(5).ToList();
            decimal avgRecentChange = recentCandels.Average(c => c.PriceChangePercentage);

            // 5) Forecast logic
            decimal forecastedHigh = lastCandel.EndPrice + avgVolatility;

            if (avgRecentChange > 0) // bullish trend
            {
                forecastedHigh += (lastCandel.EndPrice * (avgRecentChange / 100));
            }

            // Ensure forecast is at least as high as historical max
            return Math.Max(historicalMaxHigh, forecastedHigh);
        }

        public static decimal? GetFuturePriceForRSI100(List<Candel> candels, int period)
        {
            if (candels == null || candels.Count < period)
                return null; // Not enough data

            // Calculate gains and losses for each candle
            List<decimal> gains = new List<decimal>();
            List<decimal> losses = new List<decimal>();

            for (int i = candels.Count - period; i < candels.Count; i++)
            {
                decimal change = candels[i].EndPrice - candels[i].StartPrice;
                gains.Add(Math.Max(0, change));
                losses.Add(Math.Max(0, -change));
            }

            decimal averageGain = gains.Sum() / period;
            decimal averageLoss = losses.Sum() / period;

            // If averageLoss is already 0, RSI is already 100
            if (averageLoss == 0)
            {
                return candels.Last().EndPrice;
            }

            // To get RSI = 100, we need averageLoss = 0
            // Assume next candle closes at future price X
            decimal lastClose = candels.Last().EndPrice;

            // Minimum gain needed to eliminate loss
            // Let's approximate by adding next candle gain equal to last loss
            decimal requiredGain = averageLoss * period;

            decimal futurePrice = lastClose + requiredGain;

            return futurePrice;
        }

        public static decimal? GetFuturePriceForRSI70(List<Candel> candels, int period)
        {
            if (candels == null || candels.Count < period)
                return null; // Not enough data

            List<decimal> gains = new List<decimal>();
            List<decimal> losses = new List<decimal>();

            for (int i = candels.Count - period; i < candels.Count; i++)
            {
                decimal change = candels[i].EndPrice - candels[i].StartPrice;
                gains.Add(Math.Max(0, change));
                losses.Add(Math.Max(0, -change));
            }

            decimal averageGain = gains.Sum() / period;
            decimal averageLoss = losses.Sum() / period;

            // If averageLoss is 0, RSI will stay at 100
            if (averageLoss == 0)
                return candels.Last().EndPrice;

            // Target RS for RSI = 70
            decimal targetRS = 7m / 3m;

            // Equation: (averageGain + requiredGain/period) / averageLoss = targetRS
            // Solve for requiredGain
            decimal requiredGain = (targetRS * averageLoss - averageGain) * period;

            if (requiredGain < 0)
                return candels.Last().EndPrice; // Already above RSI 70

            decimal lastClose = candels.Last().EndPrice;
            decimal futurePrice = lastClose + requiredGain;

            return futurePrice;
        }


        // POST https://localhost:44364/api/TestShortSell/RSIBulkTester
        [HttpPost("RSIBulkTester")]
        public async Task<IActionResult> RSIBulkTester([FromBody] BulkTestRequestModel request)
        {
            try
            {
               List<List<Candel>> CorrectStock = new List<List<Candel>>();
               List<List<Candel>> WrongStock = new List<List<Candel>>();
               List<List<Candel>> TotalList = new List<List<Candel>>();

                List<CandelPairs> CorrectStockData = new List<CandelPairs>();
                List<CandelPairs> WrongStockData = new List<CandelPairs>();

                DateTime PreviousDay = request.StartDate.AddDays(-20);
                DateTime TestDay = request.StartDate;

                // If Saturday or Sunday, move back to Friday
                if (PreviousDay.DayOfWeek == DayOfWeek.Saturday)
                {
                    PreviousDay = PreviousDay.AddDays(-1); // Friday
                }
                else if (PreviousDay.DayOfWeek == DayOfWeek.Sunday)
                {
                    PreviousDay = PreviousDay.AddDays(-2); // Friday
                }

                DateTime Previous14DayDate = request.StartDate.AddDays(-20);

                // If Saturday or Sunday, move back to Friday
                if (Previous14DayDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    Previous14DayDate = Previous14DayDate.AddDays(-1); // Friday
                }
                else if (Previous14DayDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    Previous14DayDate = Previous14DayDate.AddDays(-2); // Friday
                }



                // If Saturday or Sunday, move Forward to Monday
                if (TestDay.DayOfWeek == DayOfWeek.Saturday)
                {
                    TestDay = TestDay.AddDays(+2); // Monday
                }
                else if (TestDay.DayOfWeek == DayOfWeek.Sunday)
                {
                    TestDay = TestDay.AddDays(+1); // Monday
                }


                using (var httpClient = new HttpClient())
                {
                    foreach (var stock in _stocks)
                    {
                        // Create request object (map from your BulkTestRequestModel if needed)
                        var stockRequestForPreviousDay = new StockRequestModified
                        {
                            SymbolToken = stock.symboltoken,  // coming from BulkTestRequestModel
                            AuthorizationToken = "",            // set if required
                            StartDate = PreviousDay,
                            EndDate = TestDay.AddDays(-1),
                            Duration = "1d"
                        };

                        // Convert request object to JSON
                        var jsonPreviousDay = JsonConvert.SerializeObject(stockRequestForPreviousDay);
                        var contentPreviousDay = new StringContent(jsonPreviousDay, Encoding.UTF8, "application/json");

                        // Call internal API
                        var responsePreviousDay = await httpClient.PostAsync(
                            "https://localhost:44364/api/AngelCandel/getCandleDataForTest5PaisaSeprated",
                            contentPreviousDay);

                        var stockRequestForTestDay = new StockRequestModified
                        {
                            SymbolToken = stock.symboltoken,  // coming from BulkTestRequestModel
                            AuthorizationToken = "",            // set if required
                            StartDate = TestDay,
                            EndDate = TestDay,
                            Duration = "1m"
                        };

                        // Convert request object to JSON
                        var jsonTestDay = JsonConvert.SerializeObject(stockRequestForTestDay);
                        var contentTestDay = new StringContent(jsonTestDay, Encoding.UTF8, "application/json");

                        // Call internal API
                        var responseTestDay = await httpClient.PostAsync(
                            "https://localhost:44364/api/AngelCandel/getCandleDataForTest5PaisaSeprated",
                            contentTestDay);

                        //// Read response content
                        //var responseData = await response.Content.ReadAsStringAsync();


                        if (responsePreviousDay.IsSuccessStatusCode && responseTestDay.IsSuccessStatusCode)
                        {

                            var responsePreviousDayData = await responsePreviousDay.Content.ReadAsStringAsync();
                            List<Candel> PreviousDayList = JsonConvert.DeserializeObject<List<Candel>>(responsePreviousDayData);

                            var responseTestDayData = await responseTestDay.Content.ReadAsStringAsync();
                            List<Candel> TestDayCandelList = JsonConvert.DeserializeObject<List<Candel>>(responseTestDayData);

                            // **<-- ADD THIS NULL/EMPTY CHECK TO PREVENT THE CRASH -->**
                            if (PreviousDayList == null || !PreviousDayList.Any()
                                                        ||
                              TestDayCandelList == null || !TestDayCandelList.Any())
                            {
                                continue; // Skip the rest of the loop for this stock
                            }

                            //var PredictedRSIPrice = ForecastMaxHigh(PreviousDayList); // make function and assign the predicted value
                            //var PredictedRSIPrice = GetFuturePriceForRSI100(PreviousDayList, 14); // make function and assign the predicted value

                            //var forecast = AnalyzePriceForecast(PreviousDayList, PreviousDayList.Count);
                            //var PredictedRSIPrice = forecast.ForecastedMaxHigh;
                            var PredictedRSIPrice = GetFuturePriceForRSI70(PreviousDayList, 14);

                            decimal profitPercent = 0.0025m; // 0.25% less than predicted price
                            //decimal profitPercent = 0.0014m; // 0.25% less than predicted price 35 Rupees profit on 50000 at 5x
                            //decimal profitPercent = 0.01m;
                            decimal stoplossPercent = 0.01m;

                            //var expectedPrice = PredictedRSIPrice * (1 - 0.0025m); // 0.25% less than predicted price
                            var expectedPrice = PredictedRSIPrice * (1 - profitPercent); // 1% less than predicted price
                            var stopLossPrice = PredictedRSIPrice * (1 + stoplossPercent); // 1% greater than predicted price

                            //1) Get the Candel where Predicted RSI was achived
                            //Get the candel where HighestPrice >= PredictedRSI price
                            Candel? targetCandle = null;

                            foreach (Candel candel in TestDayCandelList)
                            {
                                //decimal startPrice = candel.StartPrice;
                                //decimal HighestPrice = candel.HighestPrice;

                                // targetCandle = TestDayCandelList
                                //.OrderBy(c => c.OpenTime) // ensure earliest first
                                //.FirstOrDefault(c =>
                                //    (c.StartPrice < PredictedRSIPrice && c.HighestPrice >= PredictedRSIPrice) ||
                                //    (c.StartPrice > PredictedRSIPrice && c.LowestPrice <= PredictedRSIPrice)
                                //);

                                decimal RangeHigh = candel.HighestPrice;
                                decimal RangeLow = candel.LowestPrice;

                                if (PredictedRSIPrice >= RangeLow && PredictedRSIPrice <= RangeHigh)
                                {
                                    targetCandle = candel;
                                    break;
                                }

                            }

                            // Assuming targetCandel.OpenTime is of type DateTime
                            if (targetCandle == null || targetCandle.OpenTime.TimeOfDay > new TimeSpan(13, 0, 0))
                            {
                                // It's after 12 PM, so skip/continue
                                continue;
                            }

                            if (targetCandle != null)
                            {

                                //2) Get the List of candels further of the candel where Predicted RSI was achived
                                List<Candel> ListFurtherOfTargetCandel = _candleAnalysisMethods.GetCandlesAfterTarget(TestDayCandelList, targetCandle);


                                //3) In THis Further List check wether the expected Price was achived
                                Candel? profitCandle = null;


                                foreach (Candel candel in ListFurtherOfTargetCandel)
                                {
                                    //decimal startPrice = candel.StartPrice;
                                    //decimal HighestPrice = candel.HighestPrice;
                                    //decimal lowestPrice = candel.LowestPrice;

                                    // profitCandle = ListFurtherOfTargetCandel
                                    //.OrderBy(c => c.OpenTime) // ensure earliest first
                                    //.FirstOrDefault(c =>
                                    //    (c.StartPrice < expectedPrice && c.HighestPrice >= expectedPrice) ||
                                    //    (c.StartPrice > expectedPrice && c.LowestPrice <= expectedPrice)
                                    //);

                                    Candel? lowestCandle = ListFurtherOfTargetCandel
                                                      .MinBy(c => c.LowestPrice);

                                    if (lowestCandle?.LowestPrice <= expectedPrice)
                                    {
                                        profitCandle = candel;
                                        break;
                                    }

                                }

                                Candel? stopLossCandle = null;

                                if (targetCandle != null && profitCandle != null)
                                {
                                    stopLossCandle = ListFurtherOfTargetCandel
                                                     .Where(c => c.HighestPrice >= stopLossPrice
                                                              && c.OpenTime > targetCandle.OpenTime
                                                              && c.OpenTime < profitCandle.OpenTime)
                                                     .OrderBy(c => c.OpenTime)
                                                     .FirstOrDefault();
                                }

                                if (profitCandle != null)
                                {
                                    CorrectStock.Add(TestDayCandelList);
                                    //TotalList.Add(TestDayCandelList);

                                    CorrectStockData.Add(new CandelPairs
                                    {
                                        ShortSoldCandel = targetCandle,
                                        ShortSoldCandelTime = targetCandle?.OpenTime,
                                        ProfitCandel = profitCandle,
                                        ProfitCandelTime = profitCandle?.OpenTime,
                                        StopLossCandel = stopLossCandle,
                                        StopLossCandelTime = stopLossCandle?.OpenTime,
                                        ProfitPercentage = profitPercent,
                                        StopLossPercentage = stoplossPercent,
                                        StopLossHit = false,
                                    });
                                }
                                else if(targetCandle != null && profitCandle == null)
                                {
                                    WrongStock.Add(TestDayCandelList);
                                    //TotalList.Add(TestDayCandelList);

                                    WrongStockData.Add(new CandelPairs
                                    {
                                        ShortSoldCandel = targetCandle,
                                        ShortSoldCandelTime = targetCandle?.OpenTime,
                                        ProfitCandel = profitCandle,
                                        ProfitCandelTime = profitCandle?.OpenTime,
                                        StopLossCandel = stopLossCandle,
                                        StopLossCandelTime = stopLossCandle?.OpenTime,
                                        ProfitPercentage = profitPercent,
                                        StopLossPercentage = stoplossPercent,
                                        StopLossHit = true,
                                    });
                                }

                            }
                        }
                        else
                        {
                            return StatusCode(500, $"Didn't get data From Third Party API");
                        }

                    }

                    // Return the API response
                    return Ok(new
                    {
                        CorrectStockCount = CorrectStock.Count,
                        WrongStockCount = WrongStock.Count,
                        TotalStockCount = TotalList.Count,
                        CorrectStockData = CorrectStockData,
                        WrongStockData = WrongStockData,
                        //CorrectStock,
                        //WrongStock,
                        //TotalList
                    });
                }
           
            
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class CandelPairs
        {
            public Candel? ShortSoldCandel { get; set; }
            public DateTime? ShortSoldCandelTime { get; set; }
            public Candel? ProfitCandel { get; set; }
            public DateTime? ProfitCandelTime { get; set; }
            public Candel? StopLossCandel { get; set; }
            public DateTime? StopLossCandelTime { get; set; }
            public decimal StopLossPercentage { get; set; }
            public decimal ProfitPercentage { get; set; }
            public bool? StopLossHit { get; set; }
        }


        public class PriceForecast
        {
            public decimal ForecastedMaxHigh { get; set; }
            public decimal ConfidenceLevel { get; set; } // 0-100%
            public string AnalysisSummary { get; set; }
            public List<decimal> ResistanceLevels { get; set; } = new List<decimal>();
            public TrendDirection Trend { get; set; }
        }

        public enum TrendDirection
        {
            Bullish,
            Bearish,
            Sideways,
            Uncertain
        }

        public PriceForecast AnalyzePriceForecast(List<Candel> candles, int period = 20, decimal volatilityMultiplier = 2.0m)
        {
            if (candles == null || candles.Count == 0)
                throw new ArgumentException("Candles list cannot be empty");

            if (candles.Count < period)
                period = candles.Count; // Use available data if less than requested period

            var forecast = new PriceForecast();
            var recentCandles = candles.TakeLast(period).ToList();
            var latestCandle = recentCandles.Last();

            // 1. Calculate basic statistics
            var avgHigh = recentCandles.Average(c => c.HighestPrice);
            var maxHigh = recentCandles.Max(c => c.HighestPrice);
            var minHigh = recentCandles.Min(c => c.HighestPrice);
            var currentPrice = latestCandle.EndPrice;

            // 2. Calculate volatility (Standard Deviation of highs)
            var variance = recentCandles.Average(c => (c.HighestPrice - avgHigh) * (c.HighestPrice - avgHigh));
            var volatility = (decimal)Math.Sqrt((double)variance);

            // 3. Identify trend direction
            forecast.Trend = AnalyzeTrend(recentCandles);

            // 4. Calculate momentum
            var momentum = CalculateMomentum(recentCandles);

            // 5. Identify resistance levels
            forecast.ResistanceLevels = IdentifyResistanceLevels(candles);

            // 6. Calculate forecast based on different methods
            var method1 = CalculateVolatilityBasedForecast(currentPrice, volatility, volatilityMultiplier, forecast.Trend);
            var method2 = CalculateMomentumBasedForecast(currentPrice, momentum, forecast.Trend);
            var method3 = CalculateResistanceBasedForecast(currentPrice, forecast.ResistanceLevels);

            // 7. Weighted average of methods
            forecast.ForecastedMaxHigh = CalculateWeightedForecast(
                new[] { method1, method2, method3 },
                new[] { 0.4m, 0.35m, 0.25m } // Weights based on reliability
            );

            // 8. Calculate confidence level
            forecast.ConfidenceLevel = CalculateConfidenceLevel(recentCandles, volatility, forecast.Trend);

            // 9. Generate analysis summary
            forecast.AnalysisSummary = GenerateAnalysisSummary(forecast, recentCandles, currentPrice);

            return forecast;
        }

        private TrendDirection AnalyzeTrend(List<Candel> candles)
        {
            if (candles.Count < 3) return TrendDirection.Uncertain;

            var firstThird = candles.Take(candles.Count / 3).Average(c => c.EndPrice);
            var lastThird = candles.Skip(2 * candles.Count / 3).Average(c => c.EndPrice);

            var priceChangePercentage = (lastThird - firstThird) / firstThird * 100;

            if (priceChangePercentage > 2) return TrendDirection.Bullish;
            if (priceChangePercentage < -2) return TrendDirection.Bearish;
            return Math.Abs(priceChangePercentage) < 1 ? TrendDirection.Sideways : TrendDirection.Uncertain;
        }

        private decimal CalculateMomentum(List<Candel> candles)
        {
            if (candles.Count < 5) return 0;

            var recentAvg = candles.TakeLast(5).Average(c => c.EndPrice);
            var previousAvg = candles.Skip(candles.Count - 10).Take(5).Average(c => c.EndPrice);

            return (recentAvg - previousAvg) / previousAvg * 100;
        }

        private List<decimal> IdentifyResistanceLevels(List<Candel> candles)
        {
            var resistanceLevels = new List<decimal>();
            var tolerance = candles.Average(c => c.HighestPrice) * 0.005m; // 0.5% tolerance

            // Group similar high prices
            var significantHighs = candles
                .OrderByDescending(c => c.HighestPrice)
                .Take(candles.Count / 4) // Top 25% highs
                .Select(c => c.HighestPrice)
                .OrderBy(p => p)
                .ToList();

            for (int i = 0; i < significantHighs.Count; i++)
            {
                var currentLevel = significantHighs[i];
                bool isNewLevel = true;

                foreach (var existingLevel in resistanceLevels)
                {
                    if (Math.Abs(currentLevel - existingLevel) <= tolerance)
                    {
                        isNewLevel = false;
                        break;
                    }
                }

                if (isNewLevel)
                {
                    resistanceLevels.Add(currentLevel);
                    if (resistanceLevels.Count >= 5) break; // Limit to top 5 resistance levels
                }
            }

            return resistanceLevels.OrderByDescending(p => p).ToList();
        }

        private decimal CalculateVolatilityBasedForecast(decimal currentPrice, decimal volatility, decimal multiplier, TrendDirection trend)
        {
            var baseForecast = currentPrice + (volatility * multiplier);

            // Adjust based on trend
            return trend switch
            {
                TrendDirection.Bullish => baseForecast * 1.1m,
                TrendDirection.Bearish => baseForecast * 0.9m,
                _ => baseForecast
            };
        }

        private decimal CalculateMomentumBasedForecast(decimal currentPrice, decimal momentum, TrendDirection trend)
        {
            var momentumFactor = 1 + (momentum / 100);
            var forecast = currentPrice * momentumFactor;

            // Cap extreme momentum values
            if (momentum > 20) forecast = currentPrice * 1.2m;
            if (momentum < -20) forecast = currentPrice * 0.8m;

            return trend == TrendDirection.Bullish ? forecast * 1.05m : forecast;
        }

        private decimal CalculateResistanceBasedForecast(decimal currentPrice, List<decimal> resistanceLevels)
        {
            if (!resistanceLevels.Any()) return currentPrice * 1.1m;

            var nextResistance = resistanceLevels.FirstOrDefault(r => r > currentPrice);
            if (nextResistance == 0) // Current price above all resistance levels
                return resistanceLevels.Max() * 1.05m; // 5% above highest resistance

            return nextResistance;
        }

        private decimal CalculateWeightedForecast(decimal[] forecasts, decimal[] weights)
        {
            if (forecasts.Length != weights.Length)
                throw new ArgumentException("Forecasts and weights must have same length");

            decimal weightedSum = 0;
            for (int i = 0; i < forecasts.Length; i++)
            {
                weightedSum += forecasts[i] * weights[i];
            }

            return weightedSum;
        }

        private decimal CalculateConfidenceLevel(List<Candel> candles, decimal volatility, TrendDirection trend)
        {
            decimal confidence = 60m; // Base confidence

            // Adjust based on volatility (lower volatility = higher confidence)
            var avgPrice = candles.Average(c => c.EndPrice);
            var volatilityPercentage = (volatility / avgPrice) * 100;

            if (volatilityPercentage < 2) confidence += 15;
            else if (volatilityPercentage > 8) confidence -= 20;

            // Adjust based on trend consistency
            var bullishCount = candles.Count(c => c.IsBullish == true);
            var bearishCount = candles.Count(c => c.IsBearish == true);
            var consistency = (decimal)Math.Abs(bullishCount - bearishCount) / candles.Count * 100;

            confidence += consistency * 0.2m;

            return Math.Max(0, Math.Min(100, confidence)); // Clamp between 0-100
        }

        private string GenerateAnalysisSummary(PriceForecast forecast, List<Candel> recentCandles, decimal currentPrice)
        {
            var summary = new StringBuilder();
            var priceDifference = forecast.ForecastedMaxHigh - currentPrice;
            var percentageIncrease = (priceDifference / currentPrice) * 100;

            summary.AppendLine($"Current Price: {currentPrice:F4}");
            summary.AppendLine($"Forecasted Max High: {forecast.ForecastedMaxHigh:F4}");
            summary.AppendLine($"Potential Increase: {percentageIncrease:+#.##;-#.##}%");
            summary.AppendLine($"Trend: {forecast.Trend}");
            summary.AppendLine($"Confidence: {forecast.ConfidenceLevel:F1}%");

            if (forecast.ResistanceLevels.Any())
            {
                summary.AppendLine("Key Resistance Levels:");
                foreach (var level in forecast.ResistanceLevels.Take(3))
                {
                    var distance = (level - currentPrice) / currentPrice * 100;
                    summary.AppendLine($"- {level:F4} ({distance:+#.##;-#.##}%)");
                }
            }

            summary.AppendLine(forecast.Trend switch
            {
                TrendDirection.Bullish => "Market shows bullish tendencies suggesting upward momentum.",
                TrendDirection.Bearish => "Caution: Bearish trend detected. Consider resistance levels carefully.",
                TrendDirection.Sideways => "Market is consolidating. Breakout needed for significant movement.",
                _ => "Insufficient data for clear trend analysis."
            });

            return summary.ToString();
        }

        // Usage example:
        // var forecast = AnalyzePriceForecast(PreviousDayList, period: 30, volatilityMultiplier: 2.5m);
        // Console.WriteLine(forecast.AnalysisSummary);

    }
}
