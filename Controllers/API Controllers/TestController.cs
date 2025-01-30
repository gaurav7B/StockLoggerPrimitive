using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OtpNet;
using StockLogger.Data;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Dragonfly_Doji;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;

namespace StockLogger.Controllers.API_Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;

        public TestController(StockLoggerDbContext context)
        {
            _context = context;

            // Fetch stocks from StockList and transform them into the required tuple format
            _stocks = StockList2.GetStocks()
                .Select(stock => (stock.Ticker, stock.Exchange, stock.Name, stock.Id, stock.SymbolToken))
                .ToList();
        }

        [HttpGet]
        public IActionResult GetStocks()
        {
            var stocks = StockList2.GetStocks();
            return Ok(stocks);
        }

        public class TestRequestModel
        {
            public string Symboltoken { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        public class BulkTestRequestModel
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }


        public bool IsValidSoldierCandle(Candel candle, decimal bodyThreshold, decimal wickThreshold)
        {
            decimal range = candle.HighestPrice - candle.LowestPrice;
            decimal bodySize = candle.EndPrice - candle.StartPrice;
            decimal upperWick = candle.HighestPrice - candle.EndPrice;

            return bodySize >= bodyThreshold * range &&
                   upperWick <= wickThreshold * range &&
                   bodySize > 0;
        }

        private bool IsDowntrend(List<Candel> candles, int currentIndex, int lookback)
        {
            if (currentIndex < lookback) return false;

            decimal[] closes = new decimal[lookback];
            for (int i = 0; i < lookback; i++)
            {
                closes[i] = candles[currentIndex - i].EndPrice;
            }

            // Check descending closes with tolerance
            for (int i = 1; i < lookback; i++)
            {
                if (closes[i] >= closes[i - 1]) return false;
            }
            return true;
        }


        ///////////////////////////////////////

        public List<decimal> GetSupportResistanceLevels(List<Candel> candleData)
        {
            var levels = new List<decimal>();

            if (candleData == null || candleData.Count == 0)
                return levels;

            // Ensure candles are sorted by time
            var sortedCandles = candleData.OrderBy(c => c.OpenTime).ToList();

            // Calculate Fibonacci retracement levels
            decimal highestHigh = sortedCandles.Max(c => c.HighestPrice);
            decimal lowestLow = sortedCandles.Min(c => c.LowestPrice);
            decimal range = highestHigh - lowestLow;

            // Add Fibonacci levels
            levels.AddRange(new[]
            {
                highestHigh - 0.236m * range, // 23.6%
                highestHigh - 0.382m * range, // 38.2%
                highestHigh - 0.5m * range,   // 50%
                highestHigh - 0.618m * range, // 61.8%
                highestHigh - 0.786m * range  // 78.6%
            });

            // Calculate Moving Averages for common periods
            int[] periods = { 20, 50, 100, 200 };
            foreach (int period in periods)
            {
                int take = Math.Min(period, sortedCandles.Count);
                var recentCandles = sortedCandles.Skip(sortedCandles.Count - take).Take(take);
                decimal sum = recentCandles.Sum(c => c.EndPrice);
                decimal sma = take > 0 ? sum / take : 0;
                if (take > 0) levels.Add(sma);
            }

            // Remove duplicates and sort for clarity
            levels = levels.Distinct().OrderBy(l => l).ToList();

            return levels;
        }


        ////////////////////////////////////////

        public bool IsListInUptrendAdvanced(List<Candel> candles)
        {
            const int minCandles = 10; // Minimum required candles for analysis
            const int smaPeriod = 20;  // Moving average period
            const double bullishThreshold = 0.65; // 65% bullish candles
            const double volumeGrowthThreshold = 0.25; // 25% volume increase

            // Initial validation
            if (candles == null || candles.Count < minCandles)
                return false;

            // Ensure chronological order
            var sorted = candles.OrderBy(c => c.OpenTime).ToList();

            // 1. Price Structure Analysis
            var (higherHighs, higherLows) = AnalyzePriceStructure(sorted);
            if (higherHighs < 0.7 || higherLows < 0.7)
                return false;

            // 2. Moving Average Analysis
            var smaValues = CalculateSMA(sorted, smaPeriod);
            if (!IsPriceAboveSMA(sorted, smaValues, smaPeriod))
                return false;

            // 3. Momentum Analysis
            if (!HasStrongMomentum(sorted))
                return false;

            // 4. Volume Analysis
            if (!HasVolumeConfirmation(sorted, volumeGrowthThreshold))
                return false;

            // 5. Bullish Consistency
            if (CalculateBullishRatio(sorted) < bullishThreshold)
                return false;

            return true;
        }

        // Helper 1: Analyze higher highs and higher lows
        private (double higherHighs, double higherLows) AnalyzePriceStructure(List<Candel> candles)
        {
            int hhCount = 0, hlCount = 0;

            for (int i = 1; i < candles.Count; i++)
            {
                if (candles[i].HighestPrice > candles[i - 1].HighestPrice) hhCount++;
                if (candles[i].LowestPrice > candles[i - 1].LowestPrice) hlCount++;
            }

            return (
                (double)hhCount / (candles.Count - 1),
                (double)hlCount / (candles.Count - 1)
            );
        }

        // Helper 2: Calculate Simple Moving Average
        private List<decimal> CalculateSMA(List<Candel> candles, int period)
        {
            List<decimal> sma = new List<decimal>();

            for (int i = 0; i < candles.Count; i++)
            {
                if (i >= period - 1)
                {
                    decimal sum = candles.Skip(i - period + 1).Take(period).Sum(c => c.EndPrice);
                    sma.Add(sum / period);
                }
                else
                {
                    sma.Add(decimal.MinValue); // Invalid value
                }
            }
            return sma;
        }

        // Helper 3: Check price position relative to SMA
        private bool IsPriceAboveSMA(List<Candel> candles, List<decimal> sma, int validPeriod)
        {
            int validChecks = 0;
            int aboveCount = 0;

            for (int i = validPeriod - 1; i < candles.Count; i++)
            {
                validChecks++;
                if (candles[i].EndPrice > sma[i]) aboveCount++;
            }

            return validChecks > 0 && (double)aboveCount / validChecks >= 0.75;
        }

        // Helper 4: Check momentum using price changes
        private bool HasStrongMomentum(List<Candel> candles)
        {
            int positiveCloses = 0;
            decimal totalChange = 0;

            for (int i = 1; i < candles.Count; i++)
            {
                decimal change = candles[i].EndPrice - candles[i - 1].EndPrice;
                if (change > 0) positiveCloses++;
                totalChange += change;
            }

            double positiveRatio = (double)positiveCloses / (candles.Count - 1);
            decimal avgChange = totalChange / (candles.Count - 1);

            return positiveRatio >= 0.6 && avgChange > 0;
        }

        // Helper 5: Analyze volume growth
        private bool HasVolumeConfirmation(List<Candel> candles, double growthThreshold)
        {
            // Compare last third of data to first third
            int segment = candles.Count / 3;
            if (segment < 1) return true;

            decimal earlyVolume = candles.Take(segment).Average(c => c.Volume);
            decimal lateVolume = candles.TakeLast(segment).Average(c => c.Volume);

            if (earlyVolume == 0) return true; // Avoid division by zero

            double volumeGrowth = (double)((lateVolume - earlyVolume) / earlyVolume);
            return volumeGrowth >= growthThreshold;
        }

        // Helper 6: Calculate bullish candle ratio
        private double CalculateBullishRatio(List<Candel> candles)
        {
            int bullishCount = candles.Count(c => c.IsBullish == true);
            return (double)bullishCount / candles.Count;
        }





        // Helper method to check downtrend
        public bool CheckDowntrend(List<Candel> candles, int currentIndex, int lookback)
        {
            if (currentIndex < lookback) return false;

            for (int i = 1; i <= lookback; i++)
            {
                if (candles[currentIndex - i].EndPrice >= candles[currentIndex - i - 1].EndPrice)
                    return false;
            }
            return true;
        }



        /////////////////////////////////////////

        //TO Check wether the stock is volatile

        public bool IsStockVolatile(List<Candel> candelData, int shortPeriod = 5, int longPeriod = 20, decimal volatilityMultiplier = 2.0m)
        {
            if (candelData == null || candelData.Count < longPeriod)
                return false; // Not enough data to assess

            // Ensure data is sorted by time
            var orderedData = candelData.OrderBy(c => c.OpenTime).ToList();

            // Split into historical (long period) and recent (short period) data
            var historicalData = orderedData.Take(orderedData.Count - shortPeriod).ToList();
            var recentData = orderedData.Skip(orderedData.Count - shortPeriod).ToList();

            // Calculate historical averages
            decimal avgHistoricalRangePct = historicalData.Average(c =>
                (c.HighestPrice - c.LowestPrice) / c.StartPrice * 100);
            decimal avgHistoricalPriceChangePct = historicalData.Average(c =>
                Math.Abs(c.PriceChangePercentage));
            decimal avgHistoricalVolume = historicalData.Average(c => c.Volume);

            // Calculate recent averages
            decimal avgRecentRangePct = recentData.Average(c =>
                (c.HighestPrice - c.LowestPrice) / c.StartPrice * 100);
            decimal avgRecentPriceChangePct = recentData.Average(c =>
                Math.Abs(c.PriceChangePercentage));
            decimal avgRecentVolume = recentData.Average(c => c.Volume);

            // Check if recent metrics exceed historical averages by the multiplier
            bool isRangeVolatile = avgRecentRangePct > avgHistoricalRangePct * volatilityMultiplier;
            bool isPriceChangeVolatile = avgRecentPriceChangePct > avgHistoricalPriceChangePct * volatilityMultiplier;
            bool isVolumeSpiking = avgRecentVolume > avgHistoricalVolume * volatilityMultiplier;

            // Consider volatility if any two indicators spike
            return (isRangeVolatile && isPriceChangeVolatile) ||
                   (isRangeVolatile && isVolumeSpiking) ||
                   (isPriceChangeVolatile && isVolumeSpiking);
        }

        //TO check wether the list is in Uptrend
        public bool IsInUptrend(List<Candel> candelData, int lookbackPeriod = 3)
        {
            // Check for valid data and sufficient length
            if (candelData == null || candelData.Count < lookbackPeriod)
                return false;

            // Ensure the candles are ordered chronologically (oldest to newest)
            var orderedCandles = candelData.OrderBy(c => c.OpenTime).ToList();

            // Extract the most recent 'lookbackPeriod' candles
            var recentCandles = orderedCandles.Skip(orderedCandles.Count - lookbackPeriod)
                                              .Take(lookbackPeriod)
                                              .ToList();

            // Check each consecutive pair for higher highs and higher lows
            for (int i = 1; i < recentCandles.Count; i++)
            {
                var previous = recentCandles[i - 1];
                var current = recentCandles[i];

                // If current high is not higher than previous high, or low not higher, return false
                if (current.HighestPrice <= previous.HighestPrice || current.LowestPrice <= previous.LowestPrice)
                    return false;
            }

            // All checks passed; uptrend detected
            return true;
        }


        //POST https://localhost:44364/api/Test/BulkTestMasterAPI
        [HttpPost("BulkTestMasterAPI")]
        public async Task<IActionResult> BulkTestMasterAPI([FromBody] BulkTestRequestModel request)
        {
            //var Tokenresponse = await _context.Token.FirstOrDefaultAsync();

            //string authtoken = Tokenresponse.AuthToken;
            string authtoken = "";

            //string authtoken = await GetRefreshedAuthorizationTokenAsync();

            List<List<List<Candel>>> MainCorrectPredictionList = new List<List<List<Candel>>>();
            List<List<List<Candel>>> MainWrongPredictionList = new List<List<List<Candel>>>();

            List<List<Candel>> MainMasterList = new List<List<Candel>>();

            decimal ConstForProfit = 0;


            decimal MainProfit = 0;
            decimal MainLoss = 0;

            // Check if the StartDate is a Saturday or Sunday
            if (request.StartDate.DayOfWeek == DayOfWeek.Saturday || request.StartDate.DayOfWeek == DayOfWeek.Sunday)
            {
                return Ok(new
                {
                    Net = MainProfit - MainLoss,
                    MainProfit,
                    MainLoss,
                    MainCorrectPredictionList,
                    MainWrongPredictionList,
                    MainMasterList
                });
            }

            foreach (var stock in _stocks)
            {
                List<Candel> MasterList = new List<Candel>();

                List<Candel> DrafonFlyDojiCandels = new List<Candel>();

                List<List<Candel>> CorrectPredictionList = new List<List<Candel>>();
                List<List<Candel>> WrongPredictionList = new List<List<Candel>>();

                List<Candel> CandelData = new List<Candel>();

                // Variables for profit and loss tracking
                decimal TotalProfit = 0;
                decimal NetLoss = 0;

                var token = authtoken;

                var requestBody = new
                {
                    SymbolToken = stock.symboltoken,
                    AuthorizationToken = token,
                    StartDate = request.StartDate.ToString("o"), // ISO string format
                    EndDate = request.EndDate.ToString("o") // ISO string format
                };

                var client = new HttpClient();
                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");


                var adjustedStartDate = request.StartDate.AddDays(-1);
                if (adjustedStartDate.DayOfWeek == DayOfWeek.Saturday)
                    adjustedStartDate = adjustedStartDate.AddDays(-1); // Move to Friday
                else if (adjustedStartDate.DayOfWeek == DayOfWeek.Sunday)
                    adjustedStartDate = adjustedStartDate.AddDays(-2); // Move to Friday

                var adjustedEndDate = request.EndDate.AddDays(-1);
                if (adjustedEndDate.DayOfWeek == DayOfWeek.Saturday)
                    adjustedEndDate = adjustedEndDate.AddDays(-1); // Move to Friday
                else if (adjustedEndDate.DayOfWeek == DayOfWeek.Sunday)
                    adjustedEndDate = adjustedEndDate.AddDays(-2); // Move to Friday

                var requestBodyforPrevousDayData = new
                {
                    SymbolToken = stock.symboltoken,
                    AuthorizationToken = token,
                    StartDate = adjustedStartDate.ToString("o"), // Final adjusted date
                    EndDate = adjustedEndDate.ToString("o")     // Final adjusted date
                };

                var jsonRequestBodyForPreviousDaysData = JsonConvert.SerializeObject(requestBodyforPrevousDayData);
                var contentForPreviousDaysData = new StringContent(jsonRequestBodyForPreviousDaysData, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest", content);


                    if (response.IsSuccessStatusCode)
                    {

                        var responseData = await response.Content.ReadAsStringAsync();
                        CandelData = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                        Candel EndCandel = CandelData.FirstOrDefault(c => c.OpenTime.TimeOfDay == new TimeSpan(15, 15, 0));

                        List<Candel>? dragonFlyDojiCandles = new List<Candel>();

                        if (CandelData != null)
                        {
                            // Dont use INVERTED HAMMER logic without stoploss


                            //dragonFlyDojiCandles = IdentifyDragonflyDojiCandles(CandelData , CandelDataPreviousDay);

                            //dragonFlyDojiCandles = IdentifyBullishEngulfingCandles(CandelData, CandelDataPreviousDay);
                            //dragonFlyDojiCandles = IdentifyInvertedHammerCandles(CandelData, CandelDataPreviousDay);


                            //////////////////////////////////////////////////////////////////////////////

                            // IsListInUptrendAdvanced
                            foreach (Candel testCandel in CandelData)
                            {

                                List<Candel> CandelDataBeforeTestCandel = CandelData
                                              .Where(candel => candel.OpenTime < testCandel.OpenTime)
                                              .OrderBy(c => c.OpenTime)
                                              .ToList();

                                bool isStockVolatile = IsStockVolatile(CandelDataBeforeTestCandel);

                                bool isListinUptrend = IsListInUptrendAdvanced(CandelDataBeforeTestCandel);

                                bool exists = dragonFlyDojiCandles?.Any(c => c.Ticker == testCandel.Ticker) ?? false;

                                if (
                                   isListinUptrend
                                   && (exists == false)
                                   )
                                {
                                    dragonFlyDojiCandles.Add(testCandel);
                                }

                            }


                            //foreach (Candel testCandel in CandelData)
                            //{

                            //    List<Candel> CandelDataBeforeTestCandel = CandelData
                            //                  .Where(candel => candel.OpenTime < testCandel.OpenTime)
                            //                  .OrderBy(c => c.OpenTime)
                            //                  .ToList();

                            //    bool isStockVolatile = IsStockVolatile(CandelDataBeforeTestCandel);

                            //    bool isListinUptrend = IsListInUptrendAdvanced(CandelDataBeforeTestCandel);

                            //    bool exists = dragonFlyDojiCandles?.Any(c => c.Ticker == testCandel.Ticker) ?? false;

                            //    if (
                            //       isListinUptrend
                            //       )
                            //    {
                            //        dragonFlyDojiCandles.Add(testCandel);
                            //    }

                            //}

                            //foreach (Candel testCandel in CandelData)
                            //{

                            //    if (testCandel.OpenTime.TimeOfDay > new TimeSpan(15, 00, 0))
                            //    {
                            //        break;
                            //    }

                            //    //// COMPLEX HAMMER

                            //    // Configuration parameters (adjust based on backtesting)
                            //    const int trendPeriod = 3;          // Lookback period for trend analysis
                            //    const decimal minTrendStrength = 0.6m; // 60% of previous candles should follow trend
                            //    const decimal volatilityPeriod = 5; // Period for dynamic threshold calculations


                            //    // Initialize historical context
                            //    int currentIndex = CandelData.IndexOf(testCandel);
                            //    if (currentIndex < Math.Max(trendPeriod, volatilityPeriod)) continue;

                            //    //Checks if the candel is Bullish
                            //    if (testCandel.IsBullish != true) continue;

                            //    decimal bodySize = Math.Abs(testCandel.EndPrice - testCandel.StartPrice);
                            //    decimal totalRange = testCandel.HighestPrice - testCandel.LowestPrice;
                            //    if (totalRange <= 0) continue;

                            //    // 2. Trend analysis - verify preceding downtrend
                            //    var previousCandles = CandelData
                            //        .Skip(currentIndex - trendPeriod)
                            //        .Take(trendPeriod)
                            //        .ToList();

                            //    decimal trendStrength = previousCandles
                            //        .Count(c => c.IsBearish == true) / (decimal)trendPeriod;

                            //    bool isValidDowntrend = previousCandles
                            //        .Zip(previousCandles.Skip(1), (a, b) => a.EndPrice > b.EndPrice)
                            //        .All(x => x) && trendStrength >= minTrendStrength;

                            //    if (!isValidDowntrend) continue;

                            //    // 3. Volatility-adjusted calculations
                            //    var volatilityCandles = CandelData
                            //        .Skip(currentIndex - (int)volatilityPeriod)
                            //        .Take((int)volatilityPeriod)
                            //        .ToList();

                            //    decimal avgBodySize = volatilityCandles
                            //        .Average(c => Math.Abs(c.EndPrice - c.StartPrice));

                            //    decimal avgTotalRange = volatilityCandles
                            //        .Average(c => c.HighestPrice - c.LowestPrice);

                            //    // 4. Dynamic thresholds
                            //    decimal bodySizeThreshold = Math.Max(avgBodySize * 0.3m, totalRange * 0.15m);
                            //    decimal lowerShadowMultiplier = 2.5m - (avgTotalRange / totalRange);
                            //    decimal upperShadowThreshold = bodySize * 0.05m + avgBodySize * 0.02m;

                            //    // 5. Shadow calculations
                            //    decimal lowerShadow = testCandel.StartPrice - testCandel.LowestPrice;
                            //    decimal upperShadow = testCandel.HighestPrice - testCandel.EndPrice;

                            //    // 6. Price position validation
                            //    decimal bodyPosition = (testCandel.EndPrice - testCandel.LowestPrice) / totalRange;
                            //    bool isUpperPosition = bodyPosition >= 0.7m;

                            //    // 7. Volume confirmation
                            //    decimal avgVolume = volatilityCandles.Average(c => c.Volume);
                            //    bool volumeConfirmed = testCandel.Volume > avgVolume * 1.3m;

                            //    // 8. Final pattern validation
                            //    bool isHammer = bodySize <= bodySizeThreshold &&
                            //                    lowerShadow >= bodySize * lowerShadowMultiplier &&
                            //                    upperShadow <= upperShadowThreshold &&
                            //                    isUpperPosition &&
                            //                    volumeConfirmed;

                            //    //bool isHammer = bodySize <= bodySizeThreshold &&
                            //    //                 lowerShadow >= bodySize * lowerShadowMultiplier &&
                            //    //                 upperShadow <= upperShadowThreshold &&
                            //    //                 isUpperPosition;

                            //    Candel verificationCandel = CandelData
                            //                       .Where(c => c.OpenTime > testCandel.OpenTime)
                            //                       .OrderBy(c => c.OpenTime)
                            //                       .FirstOrDefault();

                            //    bool isVerificationCandelVerified = (verificationCandel != null)
                            //                                        && (verificationCandel.HighestPrice > testCandel.HighestPrice)
                            //                                        && (verificationCandel.EndPrice > verificationCandel.StartPrice);

                            //    //if (
                            //    //    isHammer
                            //    //    && isVerificationCandelVerified
                            //    //    )
                            //    //{
                            //    //    //dragonFlyDojiCandles.Add(testCandel);
                            //    //    dragonFlyDojiCandles.Add(verificationCandel);
                            //    //}

                            //    if (
                            //        isHammer
                            //        )
                            //    {
                            //        dragonFlyDojiCandles.Add(testCandel);
                            //    }
                            //}


                            //foreach (Candel testCandel in CandelData)
                            //{

                            //    // MARBOZU



                            //    if (testCandel.OpenTime.TimeOfDay < new TimeSpan(9, 30, 0))
                            //    {
                            //        continue; // Skip the rest of this iteration and proceed to the next object
                            //    }


                            //    //TREND DETECTION
                            //    List<Candel> TrendCandels = CandelData
                            //                  .Where(candel => candel.OpenTime < testCandel.OpenTime)
                            //                  .OrderBy(c => c.OpenTime)
                            //                  .Take(3)
                            //                  .ToList();



                            //    bool isUptrend = true;

                            //    // Ensure the candles are sorted by OpenTime in ascending order (oldest first)
                            //    List<Candel> sortedCandles = TrendCandels.OrderBy(c => c.OpenTime).ToList();

                            //    // Check each consecutive pair of candles
                            //    for (int i = 0; i < sortedCandles.Count - 1; i++)
                            //    {
                            //        // If the next candle's close is not higher than the current, it's not an uptrend
                            //        if (sortedCandles[i + 1].EndPrice <= sortedCandles[i].EndPrice)
                            //            isUptrend = false;
                            //    }


                            //    //bool isUptrend = IsListInUptrendAdvanced(TrendCandels);



                            //    List<Candel> CandelDataBeforeTestCandel = CandelData
                            //                  .Where(candel => candel.OpenTime < testCandel.OpenTime)
                            //                  .OrderBy(c => c.OpenTime)
                            //                  .ToList();

                            //    // Get support/resistance levels
                            //    var keyLevels = GetSupportResistanceLevels(CandelDataBeforeTestCandel);

                            //    if (!keyLevels.Any()) continue;

                            //    // IsNearKeyLevels

                            //    decimal candleClose = testCandel.EndPrice;
                            //    decimal proximityPercentage = 1.0m;
                            //    decimal threshold = candleClose * (proximityPercentage / 100);

                            //    bool IsNearKeyLevel = false;

                            //    var lowestLevel = keyLevels.Min();

                            //    if (Math.Abs(candleClose - lowestLevel) <= threshold)
                            //    {
                            //        IsNearKeyLevel = true;
                            //    }

                            //    // Define thresholds as percentages for adaptability
                            //    const decimal shadowThreshold = 0.01m; // 1% of total range allowed for shadows
                            //    const decimal bodyRangeThreshold = 0.98m; // Body must cover 98% of total range
                            //    const decimal minBodySizeRelative = 0.005m; // Body must be at least 0.5% of start price

                            //    // Calculate price components
                            //    decimal bodySize = testCandel.EndPrice - testCandel.StartPrice;
                            //    decimal totalRange = testCandel.HighestPrice - testCandel.LowestPrice;

                            //    // Validate basic candle structure
                            //    if (totalRange <= 0 || bodySize <= 0)
                            //        continue;

                            //    // Calculate shadows
                            //    decimal upperShadow = testCandel.HighestPrice - testCandel.EndPrice;
                            //    decimal lowerShadow = testCandel.StartPrice - testCandel.LowestPrice;

                            //    // Check shadow significance
                            //    bool acceptableShadows =
                            //        upperShadow <= totalRange * shadowThreshold &&
                            //        lowerShadow <= totalRange * shadowThreshold;

                            //    // Check body-range relationship
                            //    decimal bodyRatio = totalRange != 0 ? bodySize / totalRange : 0;
                            //    bool validBodyRange = bodyRatio >= bodyRangeThreshold;

                            //    // Check body significance
                            //    bool significantBody =
                            //        bodySize >= testCandel.StartPrice * minBodySizeRelative;


                            //    var averageVolume = CandelDataBeforeTestCandel.Average(c => c.Volume);

                            //    // Volume validation (adjust based on your market)
                            //    bool volumeValid = testCandel.Volume > averageVolume; // Basic check, consider comparing to average

                            //    // Final determination
                            //    if (acceptableShadows && validBodyRange && significantBody && volumeValid)
                            //    {
                            //        if (IsNearKeyLevel == true
                            //            && isUptrend == true
                            //            )
                            //        {
                            //            dragonFlyDojiCandles.Add(testCandel);
                            //        }
                            //    }
                            //}


                            //foreach (Candel candle in CandelData)
                            //{
                            //    const decimal bodyThreshold = 0.02m;  // Max 2% body relative to total range
                            //    const decimal lowerWickThreshold = 0.70m;  // Min 70% of total range
                            //    const decimal epsilon = 0.0001m;  // Precision tolerance
                            //    int trendLookback = 3; // CandelCount for Downtrned

                            //    // Calculate price components
                            //    decimal totalRange = candle.HighestPrice - candle.LowestPrice;
                            //    if (totalRange == 0) continue;  // Avoid division by zero

                            //    decimal bodySize = Math.Abs(candle.EndPrice - candle.StartPrice);
                            //    decimal upperWick = candle.HighestPrice - Math.Max(candle.StartPrice, candle.EndPrice);
                            //    decimal lowerWick = Math.Min(candle.StartPrice, candle.EndPrice) - candle.LowestPrice;

                            //    // Core Dragonfly Doji conditions
                            //    bool isDoji = bodySize <= totalRange * bodyThreshold;
                            //    bool longLowerWick = lowerWick >= totalRange * lowerWickThreshold;
                            //    bool minimalUpperWick = upperWick <= epsilon;

                            //    // Price position checks
                            //    bool openCloseNearHigh =
                            //        Math.Abs(candle.HighestPrice - Math.Max(candle.StartPrice, candle.EndPrice)) <= epsilon;

                            //    // Trend context check (optional but recommended)
                            //    bool inDowntrend = IsDowntrend(CandelData, CandelData.IndexOf(candle), trendLookback);


                            //    Candel verificationCandel = CandelData
                            //                       .Where(c => c.OpenTime > candle.OpenTime)
                            //                       .OrderBy(c => c.OpenTime)
                            //                       .FirstOrDefault();

                            //    bool isVerificationCandelVerified = (verificationCandel != null)
                            //                                        && (verificationCandel.HighestPrice > candle.HighestPrice)
                            //                                        && (verificationCandel.EndPrice > verificationCandel.StartPrice);


                            //    List<Candel> CandelDataAfterFirstCandel = CandelData
                            //                                             .Where(c => c.OpenTime < candle.OpenTime)
                            //                                             .OrderBy(c => c.OpenTime)
                            //                                             .ToList();

                            //    // Calculate the average volume
                            //    var averageVolume = CandelDataAfterFirstCandel.Average(c => c.Volume);

                            //    bool isVolumeGreater = candle.Volume > averageVolume;


                            //    if (
                            //        isDoji 
                            //        && longLowerWick 
                            //        && minimalUpperWick 
                            //        && openCloseNearHigh 
                            //        && inDowntrend 
                            //        && isVolumeGreater
                            //        //&& isVerificationCandelVerified
                            //        )
                            //    {
                            //        dragonFlyDojiCandles.Add(candle);
                            //        //dragonFlyDojiCandles.Add(verificationCandel);

                            //    }
                            //}


                            //foreach (Candel testCandel in CandelData)
                            //{
                            //    // THREE WHITE SOILDERS

                            //    if (testCandel.OpenTime.TimeOfDay < new TimeSpan(9, 30, 0))
                            //    {
                            //        continue; // Skip the rest of this iteration and proceed to the next object
                            //    }


                            //    //TREND DETECTION
                            //    List<Candel> TrendCandels = CandelData
                            //                  .Where(candel => candel.OpenTime < testCandel.OpenTime)
                            //                  .OrderBy(c => c.OpenTime)
                            //                  .Take(3)
                            //                  .ToList();



                            //    bool isUptrend = true;

                            //    // Ensure the candles are sorted by OpenTime in ascending order (oldest first)
                            //    List<Candel> sortedCandles = TrendCandels.OrderBy(c => c.OpenTime).ToList();

                            //    // Check each consecutive pair of candles
                            //    for (int i = 0; i < sortedCandles.Count - 1; i++)
                            //    {
                            //        // If the next candle's close is not higher than the current, it's not an uptrend
                            //        if (sortedCandles[i + 1].EndPrice <= sortedCandles[i].EndPrice)
                            //            isUptrend = false;
                            //    }


                            //    //bool isUptrend = IsListInUptrendAdvanced(TrendCandels);



                            //    List<Candel> CandelDataBeforeTestCandel = CandelData
                            //                  .Where(candel => candel.OpenTime < testCandel.OpenTime)
                            //                  .OrderBy(c => c.OpenTime)
                            //                  .ToList();

                            //    // Get support/resistance levels
                            //    var keyLevels = GetSupportResistanceLevels(CandelDataBeforeTestCandel);

                            //    if (!keyLevels.Any()) continue;

                            //    // IsNearKeyLevels

                            //    decimal candleClose = testCandel.EndPrice;
                            //    decimal proximityPercentage = 1.0m;
                            //    decimal threshold = candleClose * (proximityPercentage / 100);

                            //    bool IsNearKeyLevel = false;

                            //    var lowestLevel = keyLevels.Min();

                            //    if (Math.Abs(candleClose - lowestLevel) <= threshold)
                            //    {
                            //        IsNearKeyLevel = true;
                            //    }

                            //    const decimal bodyThreshold = 0.7m;  // Body must be at least 70% of total range
                            //    const decimal wickThreshold = 0.1m;  // Upper wick <= 10% of total range
                            //    const decimal minSizeRatio = 0.25m;  // Minimum body size relative to previous candle


                            //    Candel secondCandel = CandelData
                            //                       .Where(c => c.OpenTime > testCandel.OpenTime)
                            //                       .OrderBy(c => c.OpenTime)
                            //                       .FirstOrDefault();

                            //    Candel thirdCandel = new Candel();

                            //    if (secondCandel != null)
                            //    {
                            //        thirdCandel = CandelData
                            //                       .Where(c => c.OpenTime > secondCandel.OpenTime)
                            //                       .OrderBy(c => c.OpenTime)
                            //                       .FirstOrDefault();
                            //    }


                            //    Candel first = testCandel;
                            //    Candel second = secondCandel;
                            //    Candel third = thirdCandel;


                            //    if (testCandel != null && secondCandel != null && thirdCandel != null)
                            //    {
                            //        // Ensure all candles are bullish
                            //        if (!(first.IsBullish == true && second.IsBullish == true && third.IsBullish == true))
                            //            continue;

                            //        // Validate candle structure for each
                            //        if (!IsValidSoldierCandle(first, bodyThreshold, wickThreshold) ||
                            //            !IsValidSoldierCandle(second, bodyThreshold, wickThreshold) ||
                            //            !IsValidSoldierCandle(third, bodyThreshold, wickThreshold))
                            //            continue;

                            //        // Check progressive price advancement
                            //        if (!(second.EndPrice > first.EndPrice &&
                            //              third.EndPrice > second.EndPrice &&
                            //              second.StartPrice > first.StartPrice &&
                            //              third.StartPrice > second.StartPrice))
                            //            continue;

                            //        // Verify opening positions within previous bodies
                            //        if (!(second.StartPrice >= first.StartPrice &&
                            //              second.StartPrice <= first.EndPrice &&
                            //              third.StartPrice >= second.StartPrice &&
                            //              third.StartPrice <= second.EndPrice))
                            //            continue;

                            //        // Check for increasing body sizes (optional enhancement)
                            //        decimal firstBody = first.EndPrice - first.StartPrice;
                            //        decimal secondBody = second.EndPrice - second.StartPrice;
                            //        decimal thirdBody = third.EndPrice - third.StartPrice;

                            //        if (secondBody < firstBody * minSizeRatio ||
                            //            thirdBody < secondBody * minSizeRatio)
                            //            continue;

                            //        if(IsNearKeyLevel == true && isUptrend == true)
                            //        {
                            //            dragonFlyDojiCandles.Add(third);
                            //        }

                            //    }


                            //}


                            //foreach (Candel testCandel in CandelData)
                            //{

                            //    if (testCandel.OpenTime.TimeOfDay > new TimeSpan(11, 00, 0))
                            //    {
                            //        break; // Skip the rest of this iteration and proceed to the next object
                            //    }

                            //    try
                            //    {

                            //        decimal wickToBodyRatio = 2.0m;

                            //        decimal bodySize = Math.Abs(testCandel.EndPrice - testCandel.StartPrice);

                            //        decimal upperWickSize = testCandel.HighestPrice - Math.Max(testCandel.StartPrice, testCandel.EndPrice);

                            //        decimal lowerWickSize = Math.Min(testCandel.StartPrice, testCandel.EndPrice) - testCandel.LowestPrice;

                            //        bool longUpperWick = upperWickSize >= wickToBodyRatio * bodySize;

                            //        bool smallLowerWick = lowerWickSize <= 0.025m * bodySize;

                            //        bool smallBody = bodySize <= (testCandel.HighestPrice - testCandel.LowestPrice) * 0.2m;

                            //        Candel verificationCandel = CandelData
                            //                           .Where(c => c.OpenTime > testCandel.OpenTime)
                            //                           .OrderBy(c => c.OpenTime)
                            //                           .FirstOrDefault();

                            //        bool isVerificationCandelBullish = (verificationCandel != null) && (verificationCandel.IsBullish.HasValue) && (verificationCandel.IsBullish == true);

                            //        bool isVerificationCandelHighGreater = (verificationCandel != null) && (verificationCandel.EndPrice > testCandel.HighestPrice);

                            //        bool isverificationCandelVolumeGreater = (verificationCandel != null) && (verificationCandel.Volume > testCandel.Volume);

                            //        if (
                            //            longUpperWick
                            //            && smallLowerWick
                            //            && smallBody
                            //            && (testCandel.IsBullish.HasValue && testCandel.IsBullish == true)
                            //            && (testCandel.StartPrice == testCandel.LowestPrice)
                            //            //&& isVerificationCandelBullish
                            //            //&& isVerificationCandelHighGreater
                            //            //&& isverificationCandelVolumeGreater
                            //            )
                            //        {
                            //            //dragonFlyDojiCandles.Add(verificationCandel);
                            //            dragonFlyDojiCandles.Add(testCandel);
                            //        }


                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        Console.WriteLine($"An error occurred: {ex.Message}");
                            //    }

                            //}




                            //foreach (Candel testCandel in CandelData)
                            //{
                            //    decimal wickToBodyRatio = 2.0m;

                            //    //if (testCandel.OpenTime.TimeOfDay > new TimeSpan(11, 00, 0))
                            //    //{
                            //    //    break; // Skip the rest of this iteration and proceed to the next object
                            //    //}

                            //    // Calculate body size
                            //    decimal bodySize = Math.Abs(testCandel.EndPrice - testCandel.StartPrice);

                            //    // Calculate wick sizes
                            //    decimal upperWickSize = testCandel.HighestPrice - Math.Max(testCandel.StartPrice, testCandel.EndPrice);

                            //    decimal lowerWickSize = Math.Min(testCandel.StartPrice, testCandel.EndPrice) - testCandel.LowestPrice;

                            //    // Check if the lower wick is at least `wickToBodyRatio` times the body size
                            //    bool longLowerWick = lowerWickSize >= wickToBodyRatio * bodySize;

                            //    // Ensure the upper wick is very small
                            //    //bool smallUpperWick = upperWickSize <= 0.25m * bodySize;
                            //    bool smallUpperWick = upperWickSize <= 0.025m * bodySize;

                            //    // Ensure the real body is relatively small
                            //    bool smallBody = bodySize <= (testCandel.HighestPrice - testCandel.LowestPrice) * 0.2m;

                            //    Candel previousTenth = CandelData
                            //                          .Where(c => c.OpenTime < testCandel.OpenTime) // Select candles before the test candle
                            //                          .OrderByDescending(c => c.OpenTime)          // Sort in descending order of OpenTime
                            //                          .Skip(7)                                     // Skip the first 9 candles 
                            //                          .FirstOrDefault();                           // Take the 10th one (or default if none exist)

                            //    // Hammer must appear after a downtrend (additional logic to determine downtrend can be added)
                            //    bool isInDowntrend = false; // Example threshold for downtrend check

                            //    if ((previousTenth != null) && (testCandel.EndPrice < previousTenth.EndPrice))
                            //    {
                            //        isInDowntrend = true;
                            //    }


                            //    Candel verificationCandel = CandelData
                            //                       .Where(c => c.OpenTime > testCandel.OpenTime)
                            //                       .OrderBy(c => c.OpenTime)
                            //                       .FirstOrDefault();

                            //    bool isVerificationCandelBullish = (verificationCandel != null) && (verificationCandel.IsBullish.HasValue) && (verificationCandel.IsBullish == true);

                            //    bool isVerificationCandelHighGreater = (verificationCandel != null) && (verificationCandel.EndPrice > testCandel.HighestPrice);

                            //    bool isverificationCandelVolumeGreater = (verificationCandel != null) && (verificationCandel.Volume > testCandel.Volume);

                            //    if (
                            //        longLowerWick
                            //        && smallUpperWick
                            //        && smallBody
                            //        && isInDowntrend
                            //        && isVerificationCandelBullish
                            //        && isVerificationCandelHighGreater
                            //        && testCandel.IsBullish.HasValue
                            //        && testCandel.IsBullish == true
                            //        //&& isverificationCandelVolumeGreater
                            //        )
                            //    {
                            //        dragonFlyDojiCandles.Add(verificationCandel);
                            //        //dragonFlyDojiCandles.Add(testCandel);
                            //    }


                            //}


                            //foreach (Candel testCandel in CandelData)
                            //{
                            //    //if (testCandel.OpenTime.TimeOfDay > new TimeSpan(11, 00, 0))
                            //    //{
                            //    //    break; // Skip the rest of this iteration and proceed to the next object
                            //    //}

                            //    Candel firstCandel = testCandel;

                            //    Candel secondCandel = CandelData
                            //                       .Where(c => c.OpenTime > testCandel.OpenTime)
                            //                       .OrderBy(c => c.OpenTime)
                            //                       .FirstOrDefault();

                            //    Candel thirdCandel = null;

                            //    if (secondCandel != null)
                            //    {
                            //        thirdCandel = CandelData
                            //                          .Where(c => c.OpenTime > secondCandel.OpenTime)
                            //                          .OrderBy(c => c.OpenTime)
                            //                          .FirstOrDefault();
                            //    }


                            //    if(firstCandel !=  null && secondCandel != null && thirdCandel != null && firstCandel.IsBullish.HasValue && secondCandel.IsBullish.HasValue && thirdCandel.IsBullish.HasValue)
                            //    {
                            //        //bool allthreeCandelsAreBullish = firstCandel.IsBullish.HasValue && secondCandel.IsBullish.HasValue && thirdCandel.IsBullish.HasValue;

                            //        //bool threeConsicutiveClosesHigh = (thirdCandel.EndPrice > secondCandel.EndPrice) && (secondCandel.EndPrice > firstCandel.EndPrice);

                            //        //var size1 = firstCandel.EndPrice - firstCandel.StartPrice;
                            //        //var size2 = secondCandel.EndPrice - secondCandel.StartPrice;
                            //        //var size3 = thirdCandel.EndPrice - thirdCandel.StartPrice;

                            //        //bool isSizeIncreesing = (size3 > size2) && (size2 > size1);

                            //        //if (allthreeCandelsAreBullish && threeConsicutiveClosesHigh && isSizeIncreesing)
                            //        //{

                            //        // Check if all three candles are bullish
                            //        bool allThreeCandlesAreBullish = firstCandel.IsBullish == true &&
                            //                                         secondCandel.IsBullish == true &&
                            //                                         thirdCandel.IsBullish == true;

                            //        // Check for three consecutive higher closes
                            //        bool consecutiveClosesHigher = thirdCandel.EndPrice > secondCandel.EndPrice &&
                            //                                       secondCandel.EndPrice > firstCandel.EndPrice;

                            //        // Calculate the body size of the candles
                            //        decimal firstBodySize = firstCandel.EndPrice - firstCandel.StartPrice;
                            //        decimal secondBodySize = secondCandel.EndPrice - secondCandel.StartPrice;
                            //        decimal thirdBodySize = thirdCandel.EndPrice - thirdCandel.StartPrice;

                            //        // Check if the body sizes are increasing
                            //        bool bodySizesIncreasing = thirdBodySize > secondBodySize && secondBodySize > firstBodySize;

                            //        // Check if wicks are small (negligible upper and lower wicks)
                            //        bool smallWicks =
                            //            (firstCandel.HighestPrice - firstCandel.EndPrice < firstBodySize * 0.1m) &&
                            //            (firstCandel.StartPrice - firstCandel.LowestPrice < firstBodySize * 0.1m) &&
                            //            (secondCandel.HighestPrice - secondCandel.EndPrice < secondBodySize * 0.1m) &&
                            //            (secondCandel.StartPrice - secondCandel.LowestPrice < secondBodySize * 0.1m) &&
                            //            (thirdCandel.HighestPrice - thirdCandel.EndPrice < thirdBodySize * 0.1m) &&
                            //            (thirdCandel.StartPrice - thirdCandel.LowestPrice < thirdBodySize * 0.1m);

                            //        // Check if the candles overlap slightly or have proper gaps
                            //        bool properOverlapOrGap =
                            //            (secondCandel.StartPrice >= firstCandel.StartPrice && secondCandel.StartPrice <= firstCandel.EndPrice) &&
                            //            (thirdCandel.StartPrice >= secondCandel.StartPrice && thirdCandel.StartPrice <= secondCandel.EndPrice);

                            //        // Check if volume is increasing (optional)
                            //        bool volumeIncreasing = thirdCandel.Volume > secondCandel.Volume &&
                            //                                secondCandel.Volume > firstCandel.Volume;

                            //        // Final pattern detection condition
                            //        if (allThreeCandlesAreBullish &&
                            //            consecutiveClosesHigher &&
                            //            bodySizesIncreasing 
                            //            //&&
                            //            //smallWicks
                            //            //&&
                            //            //properOverlapOrGap
                            //            //&&
                            //            //volumeIncreasing
                            //            ) // Optionally include volume check
                            //        {
                            //            dragonFlyDojiCandles.Add(thirdCandel);
                            //        }

                            //    }





                            //}


                            //foreach (Candel testCandel in CandelData)
                            //{

                            //    //if (testCandel.OpenTime.TimeOfDay > new TimeSpan(11, 00, 0))
                            //    //{
                            //    //    break; // Skip the rest of this iteration and proceed to the next object
                            //    //}

                            //    /// INVERTED HAMMER

                            //    var diff = 5;

                            //    // Define thresholds as a percentage (adjust based on sensitivity)
                            //    const decimal bodyToWickRatio = 0.3m; // Body should be small relative to the wicks
                            //    const decimal upperWickLengthRatio = 5m; // Upper wick should be at least twice the body
                            //    const decimal lowerWickLimit = 0.2m; // Lower wick should be small/negligible
                            //    decimal bodySize = Math.Abs(testCandel.EndPrice - testCandel.StartPrice);
                            //    // Calculate body size, wick sizes, and ratios
                            //    decimal IHbodySize = Math.Abs(testCandel.EndPrice - testCandel.StartPrice);
                            //    decimal upperWickSize = testCandel.HighestPrice - Math.Max(testCandel.StartPrice, testCandel.EndPrice);
                            //    decimal lowerWickSize = Math.Min(testCandel.StartPrice, testCandel.EndPrice) - testCandel.LowestPrice;

                            //    decimal totalRange = testCandel.HighestPrice - testCandel.LowestPrice;

                            //    // Ensure totalRange is not zero to avoid divide by zero exception
                            //    decimal bodyRatio = totalRange != 0 ? bodySize / totalRange : 0;

                            //    // Validate conditions for Inverted Hammer
                            //    bool smallBody = bodyRatio <= bodyToWickRatio;
                            //    bool longUpperWick = upperWickSize >= bodySize * upperWickLengthRatio;
                            //    bool minimalLowerWick = lowerWickSize <= (totalRange * lowerWickLimit);



                            //    Candel verificationCandel = CandelData
                            //                       .Where(c => c.OpenTime > testCandel.OpenTime)
                            //                       .OrderBy(c => c.OpenTime)
                            //                       .FirstOrDefault();


                            //    // Check if it is a Doji with a small body
                            //    bool isDoji = false;

                            //    // Check for a long lower shadow (shadow size relative to the body)
                            //    bool longLowerShadow = false;

                            //    // The body of the candle should be small and at the top of the range
                            //    bool smallBodyAtTop = false;

                            //    Candel verificationCandel2 = null;

                            //    if(verificationCandel != null)
                            //    {
                            //        verificationCandel2 = CandelData
                            //                          .Where(c => c.OpenTime > verificationCandel.OpenTime)
                            //                          .OrderBy(c => c.OpenTime)
                            //                          .FirstOrDefault();
                            //    }

                            //    if (verificationCandel != null)
                            //    {

                            //        // Check if it is a Doji with a small body
                            //        isDoji = Math.Abs(verificationCandel.StartPrice - verificationCandel.EndPrice) < (verificationCandel.HighestPrice - verificationCandel.LowestPrice) * 0.1m;

                            //        // Check for a long lower shadow (shadow size relative to the body)
                            //        longLowerShadow = (verificationCandel.StartPrice - verificationCandel.LowestPrice) > 3 * (verificationCandel.EndPrice - verificationCandel.StartPrice);

                            //        // The body of the candle should be small and at the top of the range
                            //        smallBodyAtTop = Math.Abs(verificationCandel.StartPrice - verificationCandel.EndPrice) < (verificationCandel.HighestPrice - verificationCandel.LowestPrice) * 0.3m;
                            //    }


                            //    //bool isVerificationCandelHighestPriceGreater = (verificationCandel != null) && (verificationCandel.HighestPrice > testCandel.HighestPrice);

                            //    //bool isVerificationCandelBullish = (verificationCandel != null) && (verificationCandel.IsBullish.HasValue);

                            //    //bool isVerificationCandelHighestPriceGreater = (verificationCandel2 != null) && (verificationCandel2.HighestPrice > verificationCandel.HighestPrice);

                            //    //bool isVerificationCandelBullish = (verificationCandel2 != null) && (verificationCandel2.IsBullish.HasValue);

                            //    bool isVerificationCandelHighestPriceGreater = (verificationCandel != null) && (verificationCandel.HighestPrice > testCandel.HighestPrice);

                            //    bool isVerificationCandelBullish = (verificationCandel != null) && (verificationCandel.IsBullish.HasValue);



                            //    //DOWNTREND
                            //    List<Candel> previousCandels = CandelData
                            //                        .Where(c => c.OpenTime < testCandel.OpenTime) // Filter only candels before testCandel
                            //                        .OrderByDescending(c => c.OpenTime)  // Order them by OpenTime in descending order
                            //                        .Take(3) // Take the first 'numberOfCandels' from the sorted data
                            //                        .OrderBy(c => c.OpenTime)  // Reorder them back in chronological order
                            //                        .ToList();



                            //    bool isDowntrend = true;


                            //    // Sort the candles by OpenTime to ensure proper order
                            //    var orderedCandels = previousCandels.OrderBy(c => c.OpenTime).ToList();

                            //    // Check if each candle's closing price is lower than the previous one
                            //    for (int i = 1; i < orderedCandels.Count; i++)
                            //    {
                            //        if (orderedCandels[i].EndPrice >= orderedCandels[i - 1].EndPrice)
                            //        {
                            //            // If any candle breaks the downtrend, return false
                            //            isDowntrend = false;
                            //        }
                            //    }



                            //    // VOLUME
                            //    bool isVolumeHigh = false;

                            //    if (verificationCandel != null && testCandel != null)
                            //    {
                            //        isVolumeHigh = verificationCandel.Volume > testCandel.Volume;
                            //    }

                            //    //Candel previousCandel = CandelData
                            //    //            .Where(c => c.CloseTime < testCandel.CloseTime)
                            //    //            .OrderByDescending(c => c.CloseTime)
                            //    //            .FirstOrDefault();

                            //    //if (previousCandel != null && testCandel != null)
                            //    //{
                            //    //    isVolumeHigh = testCandel.Volume > previousCandel.Volume;
                            //    //}


                            //    //bool BFisFit = false;

                            //    //Candel BFPreviousDayLowestPriceCandel = null;

                            //    //if (testCandel != null)
                            //    //{
                            //    //    BFPreviousDayLowestPriceCandel = CandelDataPreviousDay
                            //    //        //.Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
                            //    //        .OrderByDescending(c => c.LowestPrice)                            // Order by HighestPrice descending
                            //    //        .FirstOrDefault();                                                 // Take the first (highest)
                            //    //}

                            //    //if (BFPreviousDayLowestPriceCandel != null && testCandel != null)
                            //    //{
                            //    //    //var percentageDifference = ((testCandel.EndPrice - BFPreviousDayLowestPriceCandel.LowestPrice) / testCandel.EndPrice) * 100;

                            //    //    //if (percentageDifference < diff)
                            //    //    //{
                            //    //    //    BFisFit = true; // IHisFit is true
                            //    //    //}

                            //    //    if (testCandel.EndPrice <= BFPreviousDayLowestPriceCandel.LowestPrice)
                            //    //    {
                            //    //        BFisFit = true;
                            //    //    }
                            //    //}


                            //    if (
                            //        smallBody
                            //        && longUpperWick
                            //        && minimalLowerWick
                            //        &&
                            //        isDoji
                            //        && longLowerShadow
                            //        && smallBodyAtTop
                            //        ////&& BFisFit
                            //        //&& isVerificationCandelHighestPriceGreater
                            //        //&& isVerificationCandelBullish
                            //        //&& isDowntrend
                            //        ////&& isVolumeHigh
                            //        )
                            //    {
                            //        if ((verificationCandel != null) && (verificationCandel.HighestPrice > testCandel.HighestPrice) && (verificationCandel.IsBullish.HasValue))
                            //        {
                            //            dragonFlyDojiCandles.Add(verificationCandel);
                            //        }

                            //        //if ((verificationCandel2 != null) && (verificationCandel2.HighestPrice > verificationCandel.HighestPrice) && (verificationCandel2.IsBullish.HasValue))
                            //        //{
                            //        //    dragonFlyDojiCandles.Add(verificationCandel2);
                            //        //}

                            //        //dragonFlyDojiCandles.Add(testCandel);

                            //        //dragonFlyDojiCandles.Add(verificationCandel);

                            //    }

                            //}

                            //foreach (Candel c in CandelData)
                            //{

                            //    // DRAGONFLY_DOJI

                            //    Candel recentCandel = c;

                            //    Candel verificationCandel = CandelData
                            //                       .Where(c => c.CloseTime > recentCandel.CloseTime)
                            //                       .OrderBy(c => c.CloseTime)
                            //                       .FirstOrDefault();

                            //    Candel previousCandel = CandelData
                            //                .Where(c => c.CloseTime < recentCandel.CloseTime)
                            //                .OrderByDescending(c => c.CloseTime)
                            //                .FirstOrDefault();


                            //    // Check if it is a Doji with a small body
                            //    bool isDoji = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;

                            //    // Check for a long lower shadow (shadow size relative to the body)
                            //    bool longLowerShadow = (recentCandel.StartPrice - recentCandel.LowestPrice) > 3 * (recentCandel.EndPrice - recentCandel.StartPrice);

                            //    // The body of the candle should be small and at the top of the range
                            //    bool smallBodyAtTop = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.3m;


                            //    ////The next candle should also be bullish for confirmation

                            //    bool nextCandleBullish = false;

                            //    if (verificationCandel != null)
                            //    {
                            //        if (verificationCandel.IsBullish == true)
                            //        {
                            //            nextCandleBullish = true;
                            //        }
                            //    }

                            //    bool isVerificationCandelHighestPriceGreater = (verificationCandel != null) && (verificationCandel.HighestPrice > recentCandel.HighestPrice);

                            //    bool previousCandelBearish = false;

                            //    if (previousCandel != null)
                            //    {
                            //        if (previousCandel.IsBearish == true)
                            //        {
                            //            previousCandelBearish = true;
                            //        }
                            //    }

                            //    // If all conditions match, then it's a Dragonfly Doji with high probability of upward movement
                            //    if (isDoji
                            //        && longLowerShadow
                            //        && smallBodyAtTop
                            //        && isVerificationCandelHighestPriceGreater
                            //        && nextCandleBullish
                            //        //&& previousCandelBearish
                            //        )
                            //    {
                            //        dragonFlyDojiCandles.Add(verificationCandel);
                            //    }

                            //}

                            //foreach (Candel testCandel in CandelData)
                            //{
                            //    //if (testCandel.OpenTime.TimeOfDay > new TimeSpan(11, 00, 0))
                            //    //{
                            //    //    break; // Skip the rest of this iteration and proceed to the next object
                            //    //}

                            //    ////if (testCandel.OpenTime.TimeOfDay < new TimeSpan(9, 45, 0))
                            //    ////{
                            //    ////    continue; // Skip the rest of this iteration and proceed to the next object
                            //    ////}


                            //    var diff = 10;

                            //    /// BULLISH ENGULFING
                            //    Candel BFfirst = testCandel;
                            //    Candel BFSecond = CandelData
                            //        .Where(c => c.OpenTime > BFfirst.OpenTime)
                            //        .OrderBy(c => c.OpenTime)
                            //        .FirstOrDefault();

                            //    Candel BFVerification = null;

                            //    if (BFSecond != null)
                            //    {
                            //        BFVerification = CandelData
                            //            .Where(c => c.OpenTime > BFSecond.OpenTime)
                            //            .OrderBy(c => c.OpenTime)
                            //            .FirstOrDefault();
                            //    }

                            //    if (BFfirst != null && BFSecond != null)
                            //    {
                            //        // Check if the previous candle is bearish
                            //        bool isPreviousBearish = BFfirst.EndPrice < BFfirst.StartPrice;

                            //        // Check if the current candle is bullish
                            //        bool isCurrentBullish = BFSecond.EndPrice > BFSecond.StartPrice;

                            //        // Check if the current candle's body engulfs the previous candle's body
                            //        bool isEngulfingBody =
                            //            BFSecond.StartPrice < BFfirst.EndPrice && // Current start below previous end
                            //            BFSecond.EndPrice > BFfirst.StartPrice;  // Current end above previous start

                            //        // Check if the current candle is larger (stronger) than the previous one
                            //        bool isCurrentCandleLarge =
                            //            (BFSecond.EndPrice - BFSecond.StartPrice) >= (2 * (BFfirst.EndPrice - BFfirst.StartPrice)); // Current body at least twice as large as the previous one

                            //        // Check if the current candle's volume is higher than the previous one
                            //        bool isVolumeHigh = BFSecond.Volume > BFfirst.Volume;

                            //        bool isVerificationCandelBullish = BFVerification != null && BFVerification.IsBullish.HasValue;
                            //        bool isVerificationCandelPriceGreater = BFVerification != null && BFVerification.EndPrice > BFSecond.EndPrice;


                            //        //bool BFisFit = false;

                            //        //Candel BFPreviousDayHighestPriceCandel = null;

                            //        //if (testCandel != null)
                            //        //{
                            //        //    BFPreviousDayHighestPriceCandel = CandelDataPreviousDay
                            //        //        .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
                            //        //        .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
                            //        //        .FirstOrDefault();                                                 // Take the first (highest)
                            //        //}

                            //        //if (BFPreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < BFPreviousDayHighestPriceCandel.HighestPrice)
                            //        //{
                            //        //    var percentageDifference = ((BFPreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

                            //        //    if (percentageDifference > diff)
                            //        //    {
                            //        //        BFisFit = true; // IHisFit is true
                            //        //    }
                            //        //}

                            //        if (
                            //         isPreviousBearish
                            //         && isCurrentBullish
                            //         && isEngulfingBody
                            //         && isCurrentCandleLarge
                            //         && isVolumeHigh
                            //         //&& isVerificationCandelBullish
                            //         //&& isVerificationCandelPriceGreater
                            //         //&& BFisFit
                            //         )
                            //        {
                            //            dragonFlyDojiCandles.Add(BFSecond);
                            //        }
                            //    }


                            //}


                        }

                        MasterList = CandelData;
                        MainMasterList.Add(MasterList);
                        DrafonFlyDojiCandels = dragonFlyDojiCandles;

                        if (dragonFlyDojiCandles.Count > 0)
                        {

                            foreach (Candel dojiCandle in dragonFlyDojiCandles)
                            {
                                Candel firstCandel = dojiCandle;

                                //var expectedPrice = firstCandel.EndPrice * 1.0008014m; 
                                //var expectedPrice = firstCandel.EndPrice * 1.001429m; // 1.429 R profit on 1000 R // 285 on 2 Lakh

                                decimal expectedPrice = 0; // 2.5 R profit on 1000 R //450 on 2Lakh

                                if (firstCandel != null)
                                {
                                    //expectedPrice = firstCandel.EndPrice * 1.000595m;
                                    //expectedPrice = firstCandel.EndPrice * 1.00061m;
                                    //expectedPrice = firstCandel.EndPrice * 1.00065m;
                                    //expectedPrice = firstCandel.EndPrice * 1.01m; //  10 R profit on 1000 R //1995 okkkn 2 lakh
                                    //expectedPrice = firstCandel.EndPrice * 1.005m; //  5 R profit on 1000 R //997 on 2lakh
                                    expectedPrice = firstCandel.EndPrice * 1.0025m; // 2.5 R profit on 1000 R //450 on 2Lakh

                                    //expectedPrice = firstCandel.EndPrice * 1.0004953m; // 4 LAKH

                                }

                                decimal profitMargin = 0;

                                if (expectedPrice == firstCandel.EndPrice * 1.00065m)
                                {
                                    profitMargin = 130 - 117;
                                }
                                else if (expectedPrice == firstCandel.EndPrice * 1.0008014m)
                                {
                                    profitMargin = 160 - 117;
                                }
                                else if (expectedPrice == firstCandel.EndPrice * 1.0025m)
                                {
                                    profitMargin = 450 - 117;
                                }
                                else if (expectedPrice == firstCandel.EndPrice * 1.01m)
                                {
                                    profitMargin = 1995 - 117;
                                }
                                else if (expectedPrice == firstCandel.EndPrice * 1.005m)
                                {
                                    profitMargin = 997 - 117;
                                }
                                else if (expectedPrice == firstCandel.EndPrice * 1.001429m)
                                {
                                    profitMargin = 285 - 117;
                                }

                                //profitMargin = 1;
                                //profitMargin = 5;
                                //profitMargin = 10;

                                ConstForProfit = profitMargin;

                                //var stopLoss = firstCandel.EndPrice - (firstCandel.EndPrice * 0.03m);// 5985 on 2 lakh
                                //var stopLoss = firstCandel.EndPrice - (firstCandel.EndPrice * 0.01m);// 2000 on 2 lakh
                                //var stopLoss = firstCandel.EndPrice - (firstCandel.EndPrice * 0.00065m);
                                //var stopLoss = firstCandel.EndPrice - (firstCandel.EndPrice * 0.005m);// 997.5 on 2 lakh
                                //var stopLoss = firstCandel.EndPrice - (firstCandel.EndPrice * 0.001429m); // 300 on 2 lakh
                                var stopLoss = firstCandel.EndPrice - (firstCandel.EndPrice * 0.0025m); // 300 on 2 lakh


                                decimal lossMargin = 0;
                                if (stopLoss == firstCandel.EndPrice - (firstCandel.EndPrice * 0.001429m))
                                {
                                    lossMargin = 300 + 117;
                                }
                                else if(stopLoss == firstCandel.EndPrice - (firstCandel.EndPrice * 0.0025m))
                                {
                                    lossMargin = 500 + 117;
                                }
                                else if (stopLoss == firstCandel.EndPrice - (firstCandel.EndPrice * 0.005m))
                                {
                                    lossMargin = 997 + 117;
                                }
                                else if (stopLoss == firstCandel.EndPrice - (firstCandel.EndPrice * 0.03m))
                                {
                                    lossMargin = 5985 + 117;
                                }
                                else if (stopLoss == firstCandel.EndPrice - (firstCandel.EndPrice * 0.01m))
                                {
                                    lossMargin = 2000 + 117;
                                }




                                List<Candel> CandelDataAfterFirstCandel = CandelData
                                         .Where(candel => candel.OpenTime > firstCandel.OpenTime)
                                         .OrderBy(c => c.OpenTime)
                                         .ToList();

                                Candel highestCandel = null;

                                foreach (Candel testCandel in CandelDataAfterFirstCandel)
                                {
                                    if (highestCandel == null || testCandel.HighestPrice > highestCandel.HighestPrice)
                                    {
                                        highestCandel = testCandel;
                                    }
                                }


                                //Higest StartPrice
                                Candel candleWithHighestStartPrice = CandelDataAfterFirstCandel
                                                                  .OrderByDescending(c => c.StartPrice)
                                                                  .FirstOrDefault();

                                //Highest HighestPrice
                                Candel candleWithHighestHighestPrice = CandelDataAfterFirstCandel
                                                                 .OrderByDescending(c => c.HighestPrice)
                                                                 .FirstOrDefault();

                                //Highest LowestPrice
                                Candel candleWithHighestLowestPrice = CandelDataAfterFirstCandel
                                                                 .OrderByDescending(c => c.LowestPrice)
                                                                 .FirstOrDefault();

                                //Highest EndPrice
                                Candel candleWithHighestEndPrice = CandelDataAfterFirstCandel
                                                                 .OrderByDescending(c => c.EndPrice)
                                                                 .FirstOrDefault();


                                Candel stoplossCandel = null;

                                stoplossCandel = CandelDataAfterFirstCandel
                                              .Where(c => c.LowestPrice <= stopLoss)
                                              .FirstOrDefault();


                                Candel profitCandel = null;

                                profitCandel = CandelDataAfterFirstCandel
                                              .Where(c =>c.HighestPrice >= expectedPrice)
                                              .FirstOrDefault();


                                Candel earliestCandle = new[]
                                {
                                    candleWithHighestStartPrice,
                                    candleWithHighestHighestPrice,
                                    candleWithHighestLowestPrice,
                                    candleWithHighestEndPrice,
                                    profitCandel
                                }
                                .Where(c => c != null) // Ensure c is not null
                                .OrderBy(c => c.OpenTime)
                                .FirstOrDefault();


                                bool suceessFound = false;

                                if (
                                       (candleWithHighestHighestPrice != null && candleWithHighestHighestPrice.HighestPrice >= expectedPrice)
                                    || (candleWithHighestStartPrice != null && candleWithHighestStartPrice.StartPrice >= expectedPrice)
                                    || (candleWithHighestEndPrice != null && candleWithHighestEndPrice.EndPrice >= expectedPrice)
                                    || (candleWithHighestLowestPrice != null && candleWithHighestLowestPrice.LowestPrice >= expectedPrice)
                                    || (profitCandel != null)
                                    )
                                {
                                    suceessFound = true;
                                }


                                /// RANGE LOGIC
                                Candel RANGE_HIGH = CandelDataAfterFirstCandel
                                                                  .OrderByDescending(c => c.HighestPrice)
                                                                  .FirstOrDefault();



                                if (
                                    (highestCandel != null)
                                    && (highestCandel.HighestPrice >= expectedPrice) || ((RANGE_HIGH != null && RANGE_HIGH.HighestPrice >= expectedPrice) || (suceessFound == true))
                                  )
                                {
                                    //if (stoplossCandel != null && earliestCandle.OpenTime > stoplossCandel.OpenTime)
                                    //{
                                    //    List<Candel> CandelPair = new List<Candel>();
                                    //    CandelPair.Add(dojiCandle);
                                    //    CandelPair.Add(stoplossCandel);
                                    //    CandelPair.Add(RANGE_HIGH);

                                    //    WrongPredictionList.Add(CandelPair);
                                    //    MainWrongPredictionList.Add(WrongPredictionList);
                                    //}
                                    //else
                                    //{
                                    //    List<Candel> CandelPair = new List<Candel>();

                                    //    CandelPair.Add(dojiCandle);
                                    //    CandelPair.Add(earliestCandle);
                                    //    CandelPair.Add(RANGE_HIGH);

                                    //    CorrectPredictionList.Add(CandelPair);
                                    //    MainCorrectPredictionList.Add(CorrectPredictionList);
                                    //}

                                    List<Candel> CandelPair = new List<Candel>();

                                    CandelPair.Add(dojiCandle);
                                    CandelPair.Add(earliestCandle);
                                    CandelPair.Add(RANGE_HIGH);

                                    CorrectPredictionList.Add(CandelPair);
                                    MainCorrectPredictionList.Add(CorrectPredictionList);
                                }
                                else
                                {
                                    //if (stoplossCandel != null)
                                    //{
                                    //    List<Candel> CandelPair = new List<Candel>();
                                    //    CandelPair.Add(dojiCandle);
                                    //    CandelPair.Add(stoplossCandel);
                                    //    CandelPair.Add(RANGE_HIGH);

                                    //    WrongPredictionList.Add(CandelPair);
                                    //    MainWrongPredictionList.Add(WrongPredictionList);
                                    //}
                                    //else
                                    //{
                                    //    List<Candel> CandelPair = new List<Candel>();
                                    //    CandelPair.Add(dojiCandle);
                                    //    CandelPair.Add(EndCandel);
                                    //    CandelPair.Add(RANGE_HIGH);

                                    //    WrongPredictionList.Add(CandelPair);
                                    //    MainWrongPredictionList.Add(WrongPredictionList);
                                    //}

                                    List<Candel> CandelPair = new List<Candel>();
                                    CandelPair.Add(dojiCandle);
                                    CandelPair.Add(EndCandel);
                                    CandelPair.Add(RANGE_HIGH);

                                    WrongPredictionList.Add(CandelPair);
                                    MainWrongPredictionList.Add(WrongPredictionList);

                                }

                            }

                        }

                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }





            }

            List<Candel> allCandelsOfCorrectPredictions = MainCorrectPredictionList
                                     .SelectMany(outerList => outerList.SelectMany(innerList => innerList))
                                     .ToList();


            // Assuming Candel is a predefined class
            List<List<Candel>> extractedListWrongPredictions = MainWrongPredictionList
                                              .SelectMany(innerList => innerList)
                                              .Distinct()
                                              .ToList();



            List<List<Candel>> extractedListCorrectPredictions = MainCorrectPredictionList
                                              .SelectMany(innerList => innerList)
                                              .Distinct()
                                              .ToList();






            //List<List<Candel>> TotalPredictions = new List<List<Candel>>();

            //TotalPredictions = MainWrongPredictionList
            //           .SelectMany(innerList => innerList)
            //           .Concat(MainCorrectPredictionList.SelectMany(innerList => innerList))
            //           .Distinct()
            //           .ToList();

            //List<Candel> CPred = new List<Candel>();
            //List<Candel> WPRed = new List<Candel>();

            //List<Candel> TotalList = new List<Candel>();


            //foreach (List<Candel> testList in TotalPredictions)
            //{
            //    TotalList.Add(testList[0]);
            //}

            //foreach (List<Candel> testList in extractedListCorrectPredictions)
            //{
            //    CPred.Add(testList[0]);
            //}

            //foreach (List<Candel> testList in extractedListWrongPredictions)
            //{
            //    WPRed.Add(testList[0]);
            //}



            //List<Candel> CorrectPred = new List<Candel>();
            //List<Candel> WrongPred = new List<Candel>();

            //Candel referenceCandel = new Candel();
            //List<Candel> referenceList = new List<Candel>();

            //List<Candel> MasterList2 = new List<Candel>();


            ////ORDER THE LIST
            //TotalList = TotalList.OrderBy(candel => candel.OpenTime).ToList();

            ////foreach (Candel testCandel in TotalList)
            //if (TotalList != null)
            //{
            //    Candel testCandel = TotalList[0];


            //    if (CPred.Any(c => c.Equals(testCandel)))
            //    {
            //        CorrectPred.Add(testCandel);
            //    }
            //    else if(WPRed.Any(c => c.Equals(testCandel)))
            //    {
            //        WrongPred.Add(testCandel);
            //    }


            //    referenceCandel = testCandel;

            //        referenceList = TotalList;

            //    referenceList.RemoveAll(c => c.OpenTime <= referenceCandel.CloseTime);

            //    Candel nextCandel = referenceList
            //                       .Where(c => c.OpenTime > testCandel.OpenTime)
            //                       .OrderBy(c => c.OpenTime)
            //                       .FirstOrDefault();


            //        MasterList2.Add(testCandel);

            //        referenceCandel = nextCandel;

            //        int maxIterations = 160;
            //        int iterationCount = 0;

            //        //while 
            //        //(nextCandel != TotalList.LastOrDefault()
            //        //&& iterationCount < maxIterations
            //        //)
            //        //{
            //        //    MasterList2.Add(nextCandel);
    
            //        //    referenceList.RemoveAll(c => c.OpenTime <= referenceCandel.CloseTime);

            //        //     Candel nextCandel2 = referenceList
            //        //        .Where(c => c.OpenTime > referenceCandel.OpenTime)
            //        //        .OrderBy(c => c.OpenTime)
            //        //        .FirstOrDefault();

            //        //    referenceCandel = nextCandel2;

            //        //    nextCandel = referenceCandel;

            //        //   if (CPred.Any(c => c.Equals(testCandel)))
            //        //   {
            //        //       CorrectPred.Add(testCandel);
            //        //   }
            //        //   else if (WPRed.Any(c => c.Equals(testCandel)))
            //        //   {
            //        //       WrongPred.Add(testCandel);
            //        //   }
            //        //   iterationCount++;
            //        //}

            //}



            //foreach (Candel testCandel in MasterList2)
            //{

            //    if (CPred.Any(c => c.Equals(testCandel)))
            //    {
            //        CorrectPred.Add(testCandel);
            //    }
            //    else if(WPRed.Any(c => c.Equals(testCandel)))
            //    {
            //        WrongPred.Add(testCandel);
            //    }
            //}






            foreach (List<Candel> mainList in extractedListWrongPredictions)
            {
                Candel firstCandel = mainList[0];
                Candel secondCandel = mainList[1];

                decimal NoofStocks = 200000 / firstCandel.EndPrice;
                //decimal NoofStocks = 400000 / firstCandel.EndPrice;
                //decimal NoofStocks = 50000 / firstCandel.EndPrice;
                //decimal NoofStocks = 3391 / firstCandel.EndPrice;

                MainLoss = MainLoss + (NoofStocks * (firstCandel.EndPrice - secondCandel.EndPrice)) + 117;

            }

            MainProfit = extractedListCorrectPredictions.Count * ConstForProfit;


            //MainProfit = CorrectPred.Count * ConstForProfit;


            //List<List<Candel>> WrongTotal = new List<List<Candel>>();

            //foreach (Candel testCandel in WrongPred)
            //{

            //    foreach (List<Candel> mainList in extractedListWrongPredictions)
            //    {
            //        if(testCandel == mainList[0])
            //        {
            //            WrongTotal.Add(mainList);
            //        }
            //    }

            //}


            //foreach (List<Candel> mainList in WrongTotal)
            //{
            //    Candel firstCandel = mainList[0];
            //    Candel secondCandel = mainList[1];

            //    decimal NoofStocks = 200000 / firstCandel.EndPrice;
            //    //decimal NoofStocks = 50000 / firstCandel.EndPrice;
            //    //decimal NoofStocks = 3391 / firstCandel.EndPrice;

            //    MainLoss = MainLoss + (NoofStocks * (firstCandel.EndPrice - secondCandel.EndPrice)) + 117;

            //}

            return Ok(new
            {
                Net = MainProfit - MainLoss,
                MainProfit,
                MainLoss,
                //CorrectPred,
                //WrongPred,
                extractedListCorrectPredictions,
                extractedListWrongPredictions,
                MainMasterList
            });

        }



        //POST https://localhost:44364/api/Test/InsertCandelsToDB
        [HttpPost("InsertCandelsToDB")]
        public async Task<IActionResult> InsertCandelsToDB([FromBody] BulkTestRequestModel request)
        {
            List<List<Candel>> MainCandelDataList = new List<List<Candel>>();

            var Tokenresponse = await _context.Token.FirstOrDefaultAsync();

            string authtoken = Tokenresponse.AuthToken;

            if (request.StartDate.DayOfWeek == DayOfWeek.Saturday || request.StartDate.DayOfWeek == DayOfWeek.Sunday)
            {
                return Ok(MainCandelDataList);
            }

            foreach (var stock in _stocks)
            {

                var token = authtoken;

                var requestBody = new
                {
                    SymbolToken = stock.symboltoken,
                    AuthorizationToken = token,
                    StartDate = request.StartDate.ToString("o"), // ISO string format
                    EndDate = request.EndDate.ToString("o") // ISO string format
                };

                var client = new HttpClient();
                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        List<Candel> CandelData = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                        MainCandelDataList.Add(CandelData);

                        foreach (var candel in CandelData)
                        {
                            _context.Candel.Add(candel);
                            await _context.SaveChangesAsync();
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

            }


            return Ok(MainCandelDataList);
        }






        private bool IsSmallWick(Candel candel)
        {
            decimal bodySize = candel.EndPrice - candel.StartPrice;
            decimal totalWickSize = candel.HighestPrice - candel.LowestPrice;

            // Consider the wick to be small if it is less than 20% of the body size (or whatever threshold you prefer)
            decimal wickRatio = totalWickSize / bodySize;

            return wickRatio < 0.2m;
        }


        // Helper method to fetch the JWT token
        private async Task<string> GetAuthorizationTokenAsync()
        {
            string authorizationToken = string.Empty;

            // Fetch public IP using ipify API
            string publicIp = await GetPublicIPAsync();

            // Setup login credentials and generate TOTP
            var loginData = new
            {
                clientcode = "AAAF282130",  // Your actual client code
                password = "6366",          // Your actual pin
                totp = GenerateTOTP("3IGPCM52A2WTQCH7FW2RYOCYIY") // Generate TOTP from secret key
            };

            var loginJsonData = JsonConvert.SerializeObject(loginData);
            var loginClient = new HttpClient();
            var loginRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://apiconnect.angelone.in/rest/auth/angelbroking/user/v1/loginByPassword")
            {
                Content = new StringContent(loginJsonData, Encoding.UTF8, "application/json")
            };

            // Set headers for login request
            loginRequestMessage.Headers.Add("Accept", "application/json");
            loginRequestMessage.Headers.Add("X-UserType", "USER");
            loginRequestMessage.Headers.Add("X-SourceID", "WEB");
            loginRequestMessage.Headers.Add("X-ClientLocalIP", "192.168.56.177");  // Your local IP from ipconfig
            loginRequestMessage.Headers.Add("X-ClientPublicIP", publicIp);
            loginRequestMessage.Headers.Add("X-MACAddress", "XX-XX-XX-XX-XX-XX"); // Replace with your MAC address
            loginRequestMessage.Headers.Add("X-PrivateKey", "DcsJlRJp");          // Your actual API Key

            try
            {
                // Send login request and fetch login token
                HttpResponseMessage loginResponse = await loginClient.SendAsync(loginRequestMessage);
                loginResponse.EnsureSuccessStatusCode();  // Throws an exception if not successful
                string loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
                dynamic loginResponseJson = JsonConvert.DeserializeObject(loginResponseContent);

                // Check login status
                if (loginResponseJson.status == true)
                {
                    authorizationToken = loginResponseJson.data.jwtToken;  // Assuming the token is present here
                }
                else
                {
                    throw new Exception("Failed to authenticate: " + loginResponseJson.message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                throw;
            }

            return authorizationToken;
        }

        // Fetches the public IP from ipify API
        private static async Task<string> GetPublicIPAsync()
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync("https://api.ipify.org?format=json");
                    response.EnsureSuccessStatusCode();
                    string content = await response.Content.ReadAsStringAsync();
                    dynamic ipData = JsonConvert.DeserializeObject(content);
                    return ipData.ip;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error fetching public IP: " + ex.Message);
                    return string.Empty;
                }
            }
        }

        // Generate TOTP based on the secret key
        private static string GenerateTOTP(string secretKey)
        {
            var otp = new Totp(Base32Encoding.ToBytes(secretKey));
            return otp.ComputeTotp(); // Generates the TOTP value
        }




    }


}
