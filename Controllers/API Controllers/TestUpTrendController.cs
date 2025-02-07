using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OtpNet;
using StockLogger.BackgroundServices.BackgroundStratergyServices;
using StockLogger.Data;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Dragonfly_Doji;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;

namespace StockLogger.Controllers.API_Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestUpTrendController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;

        public TestUpTrendController(StockLoggerDbContext context)
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


        //        ///////////////////////////////////////

        //        public List<decimal> GetSupportResistanceLevels(List<Candel> candleData)
        //        {
        //            var levels = new List<decimal>();

        //            if (candleData == null || candleData.Count == 0)
        //                return levels;

        //            // Ensure candles are sorted by time
        //            var sortedCandles = candleData.OrderBy(c => c.OpenTime).ToList();

        //            // Calculate Fibonacci retracement levels
        //            decimal highestHigh = sortedCandles.Max(c => c.HighestPrice);
        //            decimal lowestLow = sortedCandles.Min(c => c.LowestPrice);
        //            decimal range = highestHigh - lowestLow;

        //            // Add Fibonacci levels
        //            levels.AddRange(new[]
        //            {
        //                highestHigh - 0.236m * range, // 23.6%
        //                highestHigh - 0.382m * range, // 38.2%
        //                highestHigh - 0.5m * range,   // 50%
        //                highestHigh - 0.618m * range, // 61.8%
        //                highestHigh - 0.786m * range  // 78.6%
        //            });

        //            // Calculate Moving Averages for common periods
        //            int[] periods = { 20, 50, 100, 200 };
        //            foreach (int period in periods)
        //            {
        //                int take = Math.Min(period, sortedCandles.Count);
        //                var recentCandles = sortedCandles.Skip(sortedCandles.Count - take).Take(take);
        //                decimal sum = recentCandles.Sum(c => c.EndPrice);
        //                decimal sma = take > 0 ? sum / take : 0;
        //                if (take > 0) levels.Add(sma);
        //            }

        //            // Remove duplicates and sort for clarity
        //            levels = levels.Distinct().OrderBy(l => l).ToList();

        //            return levels;
        //        }


        /////////////////////////////////////////////////////

        // TO_CHECK_UPTREND

        public bool IsListInUptrendAdvanced(List<Candel> candles)
        {
            const int minCandles = 10; // Minimum required candles for analysis
            const int smaPeriod = 20;  // Moving average period
            //const int smaPeriod = 10;  // Moving average period
            const double bullishThreshold = 0.65; // 65% bullish candles
            const double volumeGrowthThreshold = 0.90; // 25% volume increase

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



        //        /////////////////////////////////////////

        //        //TO Check wether the stock is volatile

        //        public bool IsStockVolatile(List<Candel> candelData, int shortPeriod = 5, int longPeriod = 20, decimal volatilityMultiplier = 2.0m)
        //        {
        //            if (candelData == null || candelData.Count < longPeriod)
        //                return false; // Not enough data to assess

        //            // Ensure data is sorted by time
        //            var orderedData = candelData.OrderBy(c => c.OpenTime).ToList();

        //            // Split into historical (long period) and recent (short period) data
        //            var historicalData = orderedData.Take(orderedData.Count - shortPeriod).ToList();
        //            var recentData = orderedData.Skip(orderedData.Count - shortPeriod).ToList();

        //            // Calculate historical averages
        //            decimal avgHistoricalRangePct = historicalData.Average(c =>
        //                (c.HighestPrice - c.LowestPrice) / c.StartPrice * 100);
        //            decimal avgHistoricalPriceChangePct = historicalData.Average(c =>
        //                Math.Abs(c.PriceChangePercentage));
        //            decimal avgHistoricalVolume = historicalData.Average(c => c.Volume);

        //            // Calculate recent averages
        //            decimal avgRecentRangePct = recentData.Average(c =>
        //                (c.HighestPrice - c.LowestPrice) / c.StartPrice * 100);
        //            decimal avgRecentPriceChangePct = recentData.Average(c =>
        //                Math.Abs(c.PriceChangePercentage));
        //            decimal avgRecentVolume = recentData.Average(c => c.Volume);

        //            // Check if recent metrics exceed historical averages by the multiplier
        //            bool isRangeVolatile = avgRecentRangePct > avgHistoricalRangePct * volatilityMultiplier;
        //            bool isPriceChangeVolatile = avgRecentPriceChangePct > avgHistoricalPriceChangePct * volatilityMultiplier;
        //            bool isVolumeSpiking = avgRecentVolume > avgHistoricalVolume * volatilityMultiplier;

        //            // Consider volatility if any two indicators spike
        //            return (isRangeVolatile && isPriceChangeVolatile) ||
        //                   (isRangeVolatile && isVolumeSpiking) ||
        //                   (isPriceChangeVolatile && isVolumeSpiking);
        //        }

        //        //TO check wether the list is in Uptrend
        //        public bool IsInUptrend(List<Candel> candelData, int lookbackPeriod = 3)
        //        {
        //            // Check for valid data and sufficient length
        //            if (candelData == null || candelData.Count < lookbackPeriod)
        //                return false;

        //            // Ensure the candles are ordered chronologically (oldest to newest)
        //            var orderedCandles = candelData.OrderBy(c => c.OpenTime).ToList();

        //            // Extract the most recent 'lookbackPeriod' candles
        //            var recentCandles = orderedCandles.Skip(orderedCandles.Count - lookbackPeriod)
        //                                              .Take(lookbackPeriod)
        //                                              .ToList();

        //            // Check each consecutive pair for higher highs and higher lows
        //            for (int i = 1; i < recentCandles.Count; i++)
        //            {
        //                var previous = recentCandles[i - 1];
        //                var current = recentCandles[i];

        //                // If current high is not higher than previous high, or low not higher, return false
        //                if (current.HighestPrice <= previous.HighestPrice || current.LowestPrice <= previous.LowestPrice)
        //                    return false;
        //            }

        //            // All checks passed; uptrend detected
        //            return true;
        //        }


        //// TO CHECK WETHER 10 DEGREE ANGLE IS MADE WITH THE CANDEL

        //        public bool CheckCandleConditions(List<Candel> candles)
        //        {
        //            if (candles.Count != 5) return false;

        //            // Check if all candles are bearish
        //            foreach (var candle in candles)
        //            {
        //                //if (candle.IsBearish != true) return false;
        //                if (candle.IsBullish.HasValue != true) return false;
        //                if (candle.IsBullish != true) return false;
        //            }

        //            // Check if end prices are in ascending order
        //            for (int i = 0; i < 4; i++)
        //            {
        //                if (candles[i].EndPrice >= candles[i + 1].EndPrice) return false;
        //            }

        //            // Check if the fifth candle's end price is greater than the first candle's start price
        //            if (candles[4].EndPrice <= candles[0].StartPrice) return false;

        //            // Calculate deltaY and check the angle condition
        //            decimal deltaY = candles[4].EndPrice - candles[0].StartPrice;
        //            decimal requiredMinDeltaY = (decimal)(4 / Math.Tan(10 * Math.PI / 180));

        //            return deltaY >= requiredMinDeltaY;
        //        }

        //        //  20 EMA STRATERGY
        //        // Function to calculate 20 EMA
        //        public static List<decimal> CalculateEMA(List<Candel> candles, int period = 20)
        //        {
        //            List<decimal> emaValues = new List<decimal>();
        //            if (candles.Count < period) return emaValues;

        //            decimal multiplier = 2m / (period + 1);
        //            decimal ema = candles.Take(period).Average(c => c.EndPrice);
        //            emaValues.Add(ema);

        //            for (int i = period; i < candles.Count; i++)
        //            {
        //                ema = ((candles[i].EndPrice - ema) * multiplier) + ema;
        //                emaValues.Add(ema);
        //            }

        //            return emaValues;
        //        }

        //        // Detect pullback and volume increase
        //        public static bool DetectStrongBuySignal(List<Candel> candles)
        //        {
        //            if (candles.Count < 21) return false;

        //            List<decimal> ema20 = CalculateEMA(candles);
        //            if (ema20.Count < 2) return false;

        //            int lastIndex = ema20.Count - 1;
        //            Candel lastCandle = candles[candles.Count - 1];
        //            Candel prevCandle = candles[candles.Count - 2];

        //            decimal lastEMA = ema20[lastIndex];
        //            decimal prevEMA = ema20[lastIndex - 1];

        //            // Condition 1: Pullback to 20 EMA
        //            bool priceTouchesEMA = lastCandle.LowestPrice <= lastEMA && lastCandle.EndPrice >= lastEMA;

        //            // Condition 2: Volume increase
        //            bool volumeIncrease = lastCandle.Volume > prevCandle.Volume;

        //            return priceTouchesEMA && volumeIncrease;
        //        }



        //        ///// TRIANGLE

        //        public static bool DetectBullishTriangle(List<Candel> candles, Candel testCandel)
        //        {
        //            if (candles.Count < 5) return false; // Need enough data

        //            // Identify Peaks (Highs) and Troughs (Lows)
        //            var highs = candles.Select(c => c.HighestPrice).ToList();
        //            var lows = candles.Select(c => c.LowestPrice).ToList();

        //            // Check for Lower Highs
        //            bool lowerHighs = highs.Zip(highs.Skip(1), (prev, curr) => curr < prev).All(x => x);

        //            // Check for Higher Lows
        //            bool higherLows = lows.Zip(lows.Skip(1), (prev, curr) => curr > prev).All(x => x);

        //            // Validate trendlines forming a triangle
        //            if (!lowerHighs || !higherLows) return false;

        //            // Calculate Upper & Lower Trendline Slopes
        //            decimal upperSlope = (highs.Last() - highs.First()) / candles.Count;
        //            decimal lowerSlope = (lows.Last() - lows.First()) / candles.Count;

        //            if (upperSlope >= 0 || lowerSlope <= 0) return false; // Ensure convergence

        //            // Detect Bullish Breakout (Price Breaking Above Upper Trendline)
        //            decimal expectedUpper = highs.Last() + upperSlope;
        //            bool breakout = testCandel.EndPrice > expectedUpper;

        //            // Confirm with Volume Surge
        //            bool volumeIncrease = testCandel.Volume > candles.Last().Volume;

        //            return breakout && volumeIncrease;
        //        }


























        // Perform Linear Regression to find trendline equation (y = mx + b)
        private static (decimal slope, decimal intercept) CalculateTrendline(List<decimal> prices)
        {
            int n = prices.Count;
            decimal sumX = Enumerable.Range(0, n).Sum();
            decimal sumY = prices.Sum();
            decimal sumXY = prices.Select((p, i) => p * i).Sum();
            decimal sumX2 = Enumerable.Range(0, n).Select(i => i * i).Sum();

            decimal denominator = (n * sumX2 - sumX * sumX);
            if (denominator == 0) return (0, prices[0]); // Avoid division by zero

            decimal slope = (n * sumXY - sumX * sumY) / denominator;
            decimal intercept = (sumY - slope * sumX) / n;
            return (slope, intercept);
        }

        // Calculate RSI (Relative Strength Index) for bullish confirmation
        private static decimal CalculateRSI(List<Candel> candles, int period = 14)
        {
            var gains = new List<decimal>();
            var losses = new List<decimal>();

            for (int i = 1; i < candles.Count; i++)
            {
                decimal change = candles[i].EndPrice - candles[i - 1].EndPrice;
                if (change > 0) gains.Add(change);
                else losses.Add(Math.Abs(change));
            }

            decimal avgGain = gains.DefaultIfEmpty(0).Average();
            decimal avgLoss = losses.DefaultIfEmpty(0).Average();

            if (avgLoss == 0) return 100; // If no losses, RSI is 100 (strong bullish)
            decimal rs = avgGain / avgLoss;
            return 100 - (100 / (1 + rs));
        }

        // Standard deviation to check price compression before breakout
        private static decimal CalculateStandardDeviation(List<decimal> prices)
        {
            decimal mean = prices.Average();
            decimal variance = prices.Select(p => (p - mean) * (p - mean)).Sum() / prices.Count;
            return (decimal)Math.Sqrt((double)variance);
        }

        public static bool DetectBullishTriangle(List<Candel> candles, Candel testCandel)
        {
            if (candles.Count < 10) return false; // Need enough historical data

            // Identify highs and lows
            var highs = candles.Select(c => c.HighestPrice).ToList();
            var lows = candles.Select(c => c.LowestPrice).ToList();

            // Calculate trendlines using linear regression
            var (upperSlope, upperIntercept) = CalculateTrendline(highs);
            var (lowerSlope, lowerIntercept) = CalculateTrendline(lows);

            // Check trendline convergence (triangle formation)
            bool isConverging = Math.Abs(upperSlope - lowerSlope) < 0.05m; // Small slope difference indicates compression
            if (!isConverging) return false;

            // Ensure price compression (low volatility before breakout)
            decimal stdDev = CalculateStandardDeviation(candles.Select(c => c.EndPrice).ToList());
            if (stdDev > 0.5m) return false; // Low volatility required for a valid breakout

            // Validate breakout (EndPrice should be above the upper trendline)
            decimal expectedUpper = upperIntercept + (candles.Count * upperSlope);
            bool breakout = testCandel.EndPrice > expectedUpper;

            // Confirm volume increase
            bool volumeSurge = testCandel.Volume > candles.Last().Volume * 1.5m; // 50% volume increase

            // RSI Confirmation (Above 55 suggests bullish momentum)
            decimal rsi = CalculateRSI(candles);
            bool strongMomentum = rsi > 55;

            return breakout && volumeSurge && strongMomentum;
        }



        //public decimal CalculateAveragePrice(List<Candel> candels)
        //{
        //    // Sum of average prices for each Candel
        //    decimal totalAveragePrice = 0;

        //    // Iterate through each Candel and calculate its average price
        //    foreach (var candel in candels)
        //    {
        //        decimal averagePrice = (candel.StartPrice + candel.HighestPrice + candel.LowestPrice + candel.EndPrice) / 4;
        //        totalAveragePrice += averagePrice;
        //    }

        //    // Calculate overall average price
        //    decimal overallAveragePrice = totalAveragePrice / candels.Count;

        //    return overallAveragePrice;
        //}

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

                        Candel EndCandel = CandelData.FirstOrDefault(c => c.OpenTime.TimeOfDay == new TimeSpan(15, 20, 0));

                        List<Candel>? dragonFlyDojiCandles = new List<Candel>();

                        if (CandelData != null)
                        {


                            //foreach (Candel testCandel in CandelData)
                            //{

                            //    //Candel candle2 = CandelData
                            //    //                .Where(c => c.OpenTime < testCandel.OpenTime) // Get only previous candles
                            //    //                .OrderByDescending(c => c.OpenTime) // Order in descending order
                            //    //                .Skip(5) // Skip the first previous candle
                            //    //                .FirstOrDefault(); // Get the second previous candle

                            //    //if (candle2 != null)
                            //    //{

                            //    //    List<Candel> CandelDataBeforeTestCandel = CandelData
                            //    //                       .Where(candel => candel.OpenTime < candle2.OpenTime)
                            //    //                       .OrderByDescending(c => c.OpenTime)  // Sort in descending order to get latest first
                            //    //                       .Take(10)  // Take the last 21 candles
                            //    //                       .OrderBy(c => c.OpenTime)  // Reorder them back in ascending order
                            //    //                       .ToList();

                            //    //    //List<Candel> CandelDataBeforeTestCandel = CandelData
                            //    //    //              .Where(candel => candel.OpenTime < testCandel.OpenTime)
                            //    //    //              .OrderBy(c => c.OpenTime)
                            //    //    //              .ToList();

                            //    //    bool isListinUptrend = IsListInUptrendAdvanced(CandelDataBeforeTestCandel);

                            //    //    bool exists = dragonFlyDojiCandles?.Any(c => c.Ticker == candle2.Ticker) ?? false;


                            //    //    if (
                            //    //       isListinUptrend
                            //    //       && (exists == false)
                            //    //       )
                            //    //    {
                            //    //        dragonFlyDojiCandles.Add(candle2);
                            //    //    }

                            //    //}



                            //    List<Candel> CandelDataBeforeTestCandel = CandelData
                            //                       .Where(candel => candel.OpenTime < testCandel.OpenTime)
                            //                       .OrderByDescending(c => c.OpenTime)  // Sort in descending order to get latest first
                            //                       .Take(20)  // Take the last 21 candles
                            //                       .OrderBy(c => c.OpenTime)  // Reorder them back in ascending order
                            //                       .ToList();


                            //    bool isListinUptrend = IsListInUptrendAdvanced(CandelDataBeforeTestCandel);

                            //    bool exists = dragonFlyDojiCandles?.Any(c => c.Ticker == testCandel.Ticker) ?? false;


                            //    if (
                            //       isListinUptrend
                            //       //&& (exists == false)
                            //       )
                            //    {
                            //        dragonFlyDojiCandles.Add(testCandel);
                            //    }




                            //}


                            //foreach (Candel testCandel in CandelData)
                            //{
                            //    if (
                            //        (testCandel.EndPrice == testCandel.HighestPrice)
                            //        &&
                            //        (testCandel.StartPrice == testCandel.LowestPrice)
                            //        //&&
                            //        //(testCandel.PriceChangePercentage >= 1.0m)
                            //        &&
                            //        (testCandel.EndPrice > testCandel.StartPrice)
                            //        )
                            //    {
                            //        dragonFlyDojiCandles.Add(testCandel);
                            //    }
                            //}



                            //foreach (Candel testCandel in CandelData)
                            //{


                            //    List<Candel> CandelDataBeforeTestCandel = CandelData
                            //                       .Where(candel => candel.OpenTime <= testCandel.OpenTime)
                            //                       .OrderByDescending(c => c.OpenTime)  // Sort in descending order to get latest first
                            //                       .Take(5)  // Take the last 21 candles
                            //                       .OrderBy(c => c.OpenTime)  // Reorder them back in ascending order
                            //                       .ToList();


                            //    bool isConditionVerified = CheckCandleConditions(CandelDataBeforeTestCandel);

                            //    bool hasMomentum = HasStrongMomentum(CandelDataBeforeTestCandel);

                            //    if (
                            //        (isConditionVerified == true)
                            //        && (hasMomentum == true)
                            //        )
                            //    {
                            //        dragonFlyDojiCandles.Add(testCandel);
                            //    }



                            //}


                            //foreach (Candel testCandel in CandelData)
                            //{


                            //    //List<Candel> CandelDataBeforeTestCandel = CandelData
                            //    //                   .Where(candel => candel.OpenTime <= testCandel.OpenTime)
                            //    //                   .OrderByDescending(c => c.OpenTime)  // Sort in descending order to get latest first
                            //    //                   .Take(21)  // Take the last 21 candles
                            //    //                   .OrderBy(c => c.OpenTime)  // Reorder them back in ascending order
                            //    //                   .ToList();

                            //    //bool isBuyingSignal = DetectStrongBuySignal(CandelDataBeforeTestCandel);

                            //    //if (
                            //    //    (isBuyingSignal == true)
                            //    //    )
                            //    //{
                            //    //    dragonFlyDojiCandles.Add(testCandel);
                            //    //}

                            //    List<Candel> CandelDataBeforeTestCandel = CandelData
                            //                .Where(candel => candel.OpenTime < testCandel.OpenTime)
                            //                .OrderByDescending(c => c.OpenTime)  // Sort in descending order to get latest first
                            //                .Take(10)  // Take the last 21 candles
                            //                .OrderBy(c => c.OpenTime)  // Reorder them back in ascending order
                            //                .ToList();

                            //    bool isBuyingSignal = DetectBullishTriangle(CandelDataBeforeTestCandel, testCandel);

                            //    if (
                            //        (isBuyingSignal == true)
                            //        )
                            //    {
                            //        dragonFlyDojiCandles.Add(testCandel);
                            //    }
                            //}

                            //foreach (Candel testCandel in CandelData)
                            //{


                            //    if (testCandel.OpenTime.TimeOfDay < new TimeSpan(9, 30, 0))
                            //    {
                            //        continue; // Skip the rest of this iteration and proceed to the next object
                            //    }


                            //    //List<Candel> CandelDataBeforeTestCandel = CandelData
                            //    //           .Where(candel => candel.OpenTime < testCandel.OpenTime)
                            //    //           .OrderBy(c => c.OpenTime)  // Reorder them back in ascending order
                            //    //           .ToList();

                            //    List<Candel> CandelDataBeforeTestCandel = CandelData
                            //                .Where(candel => candel.OpenTime < testCandel.OpenTime)
                            //                .OrderByDescending(c => c.OpenTime)  // Sort in descending order to get latest first
                            //                .Take(10)  // Take the last 21 candles
                            //                .OrderBy(c => c.OpenTime)  // Reorder them back in ascending order
                            //                .ToList();

                            //    decimal avgPrice = CalculateAveragePrice(CandelDataBeforeTestCandel);

                            //    bool islessthanavgprice = (testCandel.EndPrice >= avgPrice * 1.0025m);

                            //    if (islessthanavgprice == true)
                            //    {
                            //        dragonFlyDojiCandles.Add(testCandel);
                            //    }
                            //}



                            //foreach (Candel testCandel in CandelData)
                            //{


                            //    //if (testCandel.OpenTime.TimeOfDay < new TimeSpan(9, 30, 0))
                            //    //{
                            //    //    continue; // Skip the rest of this iteration and proceed to the next object
                            //    //}

                            //    List<Candel> CandelDataBeforeTestCandel = CandelData
                            //                .Where(candel => candel.OpenTime < testCandel.OpenTime)
                            //                .OrderByDescending(c => c.OpenTime)  // Sort in descending order to get latest first
                            //                .Take(10)  // Take the last 21 candles
                            //                .OrderBy(c => c.OpenTime)  // Reorder them back in ascending order
                            //                .ToList();

                            //    bool isbullishTriangle = DetectBullishTriangle(CandelDataBeforeTestCandel, testCandel);

                            //    if (isbullishTriangle == true)
                            //    {
                            //        dragonFlyDojiCandles.Add(testCandel);
                            //    }
                            //}



                            //foreach (Candel testCandel in CandelData)
                            //{

                            //    //if (testCandel.OpenTime.TimeOfDay > new TimeSpan(15, 00, 0))
                            //    //{
                            //    //    break;
                            //    //}

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



                            ////// INVERTED HAMMER WORKING ABOVE 90% accuracy 755 / 76  can take random 10 pred from it

                            //foreach (Candel testCandel in CandelData)
                            //{

                            //    try
                            //    {

                            //        decimal wickToBodyRatio = 2.0m;

                            //        decimal bodySize = Math.Abs(testCandel.EndPrice - testCandel.StartPrice);

                            //        decimal upperWickSize = testCandel.HighestPrice - Math.Max(testCandel.StartPrice, testCandel.EndPrice);

                            //        decimal lowerWickSize = Math.Min(testCandel.StartPrice, testCandel.EndPrice) - testCandel.LowestPrice;

                            //        bool longUpperWick = upperWickSize >= wickToBodyRatio * bodySize;

                            //        bool smallLowerWick = lowerWickSize <= 0.025m * bodySize;

                            //        bool smallBody = bodySize <= (testCandel.HighestPrice - testCandel.LowestPrice) * 0.2m;


                            //        Candel previousCandel = CandelData
                            //                    .Where(c => c.CloseTime < testCandel.CloseTime)
                            //                    .OrderByDescending(c => c.CloseTime)
                            //                    .FirstOrDefault();

                            //        Candel verificationCandel = CandelData
                            //                           .Where(c => c.CloseTime > testCandel.CloseTime)
                            //                           .OrderBy(c => c.CloseTime)
                            //                           .FirstOrDefault();







                            //        if (
                            //            longUpperWick
                            //            //&& smallLowerWick
                            //            && smallBody
                            //            && (testCandel.EndPrice > testCandel.StartPrice)
                            //            //&& (previousCandel != null && verificationCandel != null)
                            //            //&& (previousCandel.EndPrice < previousCandel.StartPrice)
                            //            && (verificationCandel != null)
                            //            && (verificationCandel.EndPrice > verificationCandel.StartPrice)
                            //            && (verificationCandel.EndPrice > testCandel.HighestPrice)

                            //            )
                            //        {
                            //            //dragonFlyDojiCandles.Add(testCandel);
                            //            dragonFlyDojiCandles.Add(verificationCandel);
                            //        }


                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        Console.WriteLine($"An error occurred: {ex.Message}");
                            //    }

                            //}


                            //// WORKING DRAGONFLY-DOJI

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


                            //    bool isVerificationCandelHighestPriceGreater = (verificationCandel != null) && (verificationCandel.HighestPrice > recentCandel.HighestPrice);

                            //    bool shortUpperShadow = (recentCandel.HighestPrice - Math.Max(recentCandel.StartPrice, recentCandel.EndPrice)) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;


                            //    List<Candel> CandelDataBeforeTestCandel = CandelData
                            //                       .Where(candel => candel.OpenTime < c.OpenTime)
                            //                       .OrderBy(c => c.OpenTime)  // Reorder them back in ascending order
                            //                       .ToList();

                            //    //var averageVolume = CandelDataBeforeTestCandel.Average(c => c.Volume);


                            //    // If all conditions match, then it's a Dragonfly Doji with high probability of upward movement
                            //    if (isDoji
                            //        && longLowerShadow
                            //        && smallBodyAtTop
                            //        //&& (c.Volume > averageVolume)
                            //        && (previousCandel != null && verificationCandel != null)
                            //        && (previousCandel.EndPrice < previousCandel.StartPrice)
                            //        && (verificationCandel.EndPrice > verificationCandel.StartPrice)
                            //        && (verificationCandel.EndPrice > c.HighestPrice)
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
                            //    ///

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
                            //        bool isFirstCandelBearish = BFfirst.EndPrice < BFfirst.StartPrice;

                            //        // Check if the current candle is bullish
                            //        bool isSecondCandelBullish = BFSecond.EndPrice > BFSecond.StartPrice;

                            //        // Check if the current candle's body engulfs the previous candle's body
                            //        bool isEngulfingBody =
                            //            BFSecond.StartPrice < BFfirst.EndPrice && // Current start below previous end
                            //            BFSecond.EndPrice > BFfirst.StartPrice;  // Current end above previous start

                            //        // Check if the current candle is larger (stronger) than the previous one
                            //        bool isSecondCandelLarge =
                            //            (BFSecond.EndPrice - BFSecond.StartPrice) >= (2 * (BFfirst.EndPrice - BFfirst.StartPrice)); // Current body at least twice as large as the previous one


                            //        if (
                            //         isFirstCandelBearish
                            //         && isSecondCandelBullish
                            //         && isEngulfingBody
                            //         && isSecondCandelLarge
                            //         && (BFVerification != null)
                            //         && (BFVerification.IsBullish.HasValue && BFVerification.IsBullish == true)
                            //         && (BFVerification.EndPrice > BFSecond.HighestPrice)
                            //         )
                            //        {
                            //            //dragonFlyDojiCandles.Add(BFSecond);
                            //            dragonFlyDojiCandles.Add(BFVerification);
                            //        }
                            //    }


                            //}



                            //foreach (Candel testCandel in CandelData)
                            //{

                            //    Candel candel1 = testCandel;
                            //    Candel candel2 = CandelData
                            //        .Where(c => c.OpenTime > candel1.OpenTime)
                            //        .OrderBy(c => c.OpenTime)
                            //        .FirstOrDefault();

                            //    Candel candel3 = null;

                            //    if (candel2 != null)
                            //    {
                            //        candel3 = CandelData
                            //            .Where(c => c.OpenTime > candel2.OpenTime)
                            //            .OrderBy(c => c.OpenTime)
                            //            .FirstOrDefault();
                            //    }

                            //    if (candel1 != null && candel2 != null && candel3 != null)
                            //    {

                            //        // 1. Check first candle is bearish
                            //        if ((bool)!candel1.IsBearish)
                            //            continue;

                            //        // 2. Check third candle is bullish
                            //        if ((bool)!candel3.IsBullish)
                            //            continue;

                            //        // 3. Check second candle has small body (body ≤ 20% of its total range)
                            //        decimal bodySizeC2 = Math.Abs(candel2.EndPrice - candel2.StartPrice);
                            //        decimal rangeC2 = candel2.HighestPrice - candel2.LowestPrice;
                            //        if (rangeC2 == 0 || (bodySizeC2 / rangeC2) > 0.2M)
                            //            continue;

                            //        // 4. Check second candle's upper and lower shadows are each ≥20% of range
                            //        decimal upperShadowC2 = candel2.HighestPrice - Math.Max(candel2.StartPrice, candel2.EndPrice);
                            //        decimal lowerShadowC2 = Math.Min(candel2.StartPrice, candel2.EndPrice) - candel2.LowestPrice;
                            //        if (upperShadowC2 / rangeC2 < 0.2M || lowerShadowC2 / rangeC2 < 0.2M)
                            //            continue;

                            //        // 5. Check gap down: Candle 2 opens below Candle 1's close with significant size (≥0.5% of C1's close)
                            //        decimal gapDown = candel1.EndPrice - candel2.StartPrice;
                            //        if (gapDown <= 0 || gapDown < candel1.EndPrice * 0.005M)
                            //            continue;

                            //        // 6. Check gap up: Candle 3 opens above Candle 2's close with significant size (≥0.5% of C2's close)
                            //        decimal gapUp = candel3.StartPrice - candel2.EndPrice;
                            //        if (gapUp <= 0 || gapUp < candel2.EndPrice * 0.005M)
                            //            continue;

                            //        // 7. Check Candle 3 closes above 75% of Candle 1's body
                            //        decimal c1Body = candel1.StartPrice - candel1.EndPrice;
                            //        decimal seventyFivePercentC1 = candel1.EndPrice + c1Body * 0.75M;
                            //        if (candel3.EndPrice <= seventyFivePercentC1)
                            //            continue;

                            //        // 8. Check Candle 3 closes above Candle 1's low
                            //        if (candel3.EndPrice <= candel1.LowestPrice)
                            //            continue;

                            //        // 9. Check Candle 1's body is ≥50% of its range and ≥2% of its start price
                            //        decimal c1Range = candel1.HighestPrice - candel1.LowestPrice;
                            //        if (c1Body < c1Range * 0.5M || c1Body < candel1.StartPrice * 0.02M)
                            //            continue;

                            //        // 10. Check Candle 3's body is ≥50% of its range and ≥1.5% of its start price
                            //        decimal c3Body = candel3.EndPrice - candel3.StartPrice;
                            //        decimal c3Range = candel3.HighestPrice - candel3.LowestPrice;
                            //        if (c3Body < c3Range * 0.5M || c3Body < candel3.StartPrice * 0.015M)
                            //            continue;

                            //        // 11. Check Candle 3's upper shadow is ≤20% of its range
                            //        decimal upperShadowC3 = candel3.HighestPrice - candel3.EndPrice;
                            //        if (upperShadowC3 / c3Range > 0.2M)
                            //            continue;

                            //        // 12. Check that the first and third candles have significant bodies
                            //        if (c1Body < candel1.StartPrice * 0.02M || c3Body < candel3.StartPrice * 0.015M)
                            //            continue;

                            //        dragonFlyDojiCandles.Add(candel3);





                            //        //// 1. Check first candle is bearish
                            //        //if (candel1.IsBearish != true)
                            //        //    continue;

                            //        //// 2. Check third candle is bullish
                            //        //if (candel3.IsBullish != true)
                            //        //    continue;

                            //        //// 3. Check second candle has small body (body ≤ 30% of its total range)
                            //        //decimal bodySizeC2 = Math.Abs(candel2.EndPrice - candel2.StartPrice);
                            //        //decimal rangeC2 = candel2.HighestPrice - candel2.LowestPrice;
                            //        //if (rangeC2 == 0 || (bodySizeC2 / rangeC2) > 0.3M)
                            //        //    continue;

                            //        //// 4. Check gap down: Candle 2 opens below Candle 1's close
                            //        //if (candel2.StartPrice >= candel1.EndPrice)
                            //        //    continue;

                            //        //// 5. Check gap up: Candle 3 opens above Candle 2's close
                            //        //if (candel3.StartPrice <= candel2.EndPrice)
                            //        //    continue;

                            //        //// 6. Check Candle 3 closes above midpoint of Candle 1's body
                            //        //decimal midpointC1 = (candel1.StartPrice + candel1.EndPrice) / 2;
                            //        //if (candel3.EndPrice <= midpointC1)
                            //        //    continue;

                            //        //// 7. Check significant bodies for Candle 1 and 3 (e.g., >1% of StartPrice)
                            //        //decimal c1Body = candel1.StartPrice - candel1.EndPrice; // Bearish body
                            //        //if (c1Body < candel1.StartPrice * 0.01M)
                            //        //    continue;

                            //        //decimal c3Body = candel3.EndPrice - candel3.StartPrice; // Bullish body
                            //        //if (c3Body < candel3.StartPrice * 0.01M)
                            //        //    continue;

                            //        //dragonFlyDojiCandles.Add(candel3);
                            //    }

                            //}



                            //foreach (Candel testCandel in CandelData)
                            //{

                            //    /// MORNING STAR

                            //    Candel candel1 = testCandel;
                            //    Candel candel2 = CandelData
                            //        .Where(c => c.OpenTime > candel1.OpenTime)
                            //        .OrderBy(c => c.OpenTime)
                            //        .FirstOrDefault();

                            //    Candel candel3 = null;

                            //    if (candel2 != null)
                            //    {
                            //        candel3 = CandelData
                            //            .Where(c => c.OpenTime > candel2.OpenTime)
                            //            .OrderBy(c => c.OpenTime)
                            //            .FirstOrDefault();
                            //    }

                            //    if (candel1 != null && candel2 != null && candel3 != null)
                            //    {

                            //        // Check wether the first candel is bearish
                            //        bool isFirstCandelqualified = false;

                            //        if (
                            //            candel1.IsBearish.HasValue 
                            //            && candel1.IsBearish == true
                            //            )
                            //        {
                            //            isFirstCandelqualified = true;
                            //        }




                            //        // Check wether second Candel is doji
                            //        bool isSecondCandelqualified = false;

                            //        // Define a threshold percentage for determining if the candle is a Doji
                            //        decimal thresholdPercentage = 0.1m; // Adjust this as needed (e.g., 0.1% of the range)

                            //        // Calculate the absolute difference between the open and close prices
                            //        decimal bodySize = Math.Abs(candel2.StartPrice - candel2.EndPrice);

                            //        // Calculate the total range of the candle (from low to high)
                            //        decimal totalRange = candel2.HighestPrice - candel2.LowestPrice;

                            //        // Check if the body size is small compared to the total range
                            //        if (totalRange == 0)
                            //            continue;

                            //        decimal bodySizePercentage = (bodySize / totalRange) * 100;

                            //        bool arePricesBelowtheStart = (candel1.StartPrice > candel2.HighestPrice);

                            //        // Return true if the body size percentage is smaller than the threshold, indicating a Doji candle
                            //        if(
                            //            bodySizePercentage <= thresholdPercentage
                            //            && arePricesBelowtheStart == true
                            //            )
                            //        {
                            //            isSecondCandelqualified = true;
                            //        }



                            //        // Check wether second Candel is doji
                            //        bool isThirdCandelqualified = false;

                            //        // A candle is bullish if the end price is higher than the start price
                            //        bool isThirdCandelBullish = candel3.EndPrice > candel3.StartPrice;

                            //        bool isThirdCandelEndPriceGreater = ((candel3.EndPrice >= candel2.HighestPrice));

                            //        bool isThirdCandelBiggerThanFirstCandel = ((candel3.EndPrice >= candel1.StartPrice));

                            //        if (
                            //            isThirdCandelBullish
                            //            && isThirdCandelEndPriceGreater
                            //            && isThirdCandelBiggerThanFirstCandel
                            //            )
                            //        {
                            //            isThirdCandelqualified = true;
                            //        }



                            //        if (
                            //            (isFirstCandelqualified == true)
                            //            && (isSecondCandelqualified == true)
                            //            && (isThirdCandelqualified == true)
                            //          )
                            //        {
                            //            dragonFlyDojiCandles.Add(candel3);
                            //        }


                            //    }


                            //}




                            //  THREE WHITE SOILDER WORKING GREAT 90% ACCURACY on 1000 days data  1622 / 168 

                            foreach (Candel testCandel in CandelData)
                            {

                                /// Three White Soilders

                                Candel candel1 = testCandel;
                                Candel candel2 = CandelData
                                    .Where(c => c.OpenTime > candel1.OpenTime)
                                    .OrderBy(c => c.OpenTime)
                                    .FirstOrDefault();

                                Candel candel3 = null;

                                if (candel2 != null)
                                {
                                    candel3 = CandelData
                                        .Where(c => c.OpenTime > candel2.OpenTime)
                                        .OrderBy(c => c.OpenTime)
                                        .FirstOrDefault();
                                }

                                if (candel1 != null && candel2 != null && candel3 != null)
                                {
                                    decimal Range1 = 0;
                                    decimal Range2 = 0;
                                    decimal Range3 = 0;


                                    // Check wether the first candel is bearish
                                    bool isFirstCandelqualified = false;

                                    Range1 = candel1.EndPrice - candel1.StartPrice;

                                    if (
                                        candel1.IsBullish.HasValue
                                        && candel1.IsBullish == true
                                        )
                                    {
                                        isFirstCandelqualified = true;
                                    }




                                    // Check wether second Candel is doji
                                    bool isSecondCandelqualified = false;

                                    Range2 = candel2.EndPrice - candel2.StartPrice;

                                    bool isAboveFirstCandel = candel2.StartPrice > candel1.StartPrice;

                                    if (
                                        candel2.IsBullish.HasValue
                                        && candel2.IsBullish == true
                                        && isAboveFirstCandel == true
                                        )
                                    {
                                        isSecondCandelqualified = true;
                                    }



                                    // Check wether second Candel is doji
                                    bool isThirdCandelqualified = false;

                                    Range3 = candel3.EndPrice - candel3.StartPrice;

                                    bool isAboveSecondCandel = candel3.StartPrice > candel2.StartPrice;

                                    if (
                                        candel3.IsBullish.HasValue
                                        && candel3.IsBullish == true
                                        && isAboveSecondCandel == true
                                        && (candel3.LowestPrice == candel3.StartPrice)
                                        )
                                    {
                                        isThirdCandelqualified = true;
                                    }



                                    if (
                                        (isFirstCandelqualified == true)
                                        && (isSecondCandelqualified == true)
                                        && (isThirdCandelqualified == true)
                                        && ((Range2 > Range1) && (Range3 > Range2))
                                      )
                                    {
                                        dragonFlyDojiCandles.Add(candel3);
                                    }


                                }


                            }



                        }

                        MasterList = CandelData;
                        MainMasterList.Add(MasterList);
                        DrafonFlyDojiCandels = dragonFlyDojiCandles;

                        if (dragonFlyDojiCandles.Count > 0)
                        {

                            foreach (Candel dojiCandle in dragonFlyDojiCandles)
                            {
                                Candel firstCandel = dojiCandle;

                                if (firstCandel == null) continue;

                                //var expectedPrice = firstCandel.EndPrice * 1.0008014m; 
                                //var expectedPrice = firstCandel.EndPrice * 1.001429m; // 1.429 R profit on 1000 R // 285 on 2 Lakh

                                decimal expectedPrice = 0; // 2.5 R profit on 1000 R //450 on 2Lakh

                                if (firstCandel != null)
                                {
                                    //expectedPrice = firstCandel.EndPrice * 1.000595m;
                                    //expectedPrice = firstCandel.EndPrice * 1.00061m;
                                    expectedPrice = firstCandel.EndPrice * 1.00065m;
                                    //expectedPrice = firstCandel.EndPrice * 1.01m; //  10 R profit on 1000 R //1995 on 2 lakh
                                    //expectedPrice = firstCandel.EndPrice * 1.005m; //  5 R profit on 1000 R //997 on 2lakh
                                    //expectedPrice = firstCandel.EndPrice * 1.0025m; // 2.5 R profit on 1000 R //450 on 2Lakh

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
                                else if (stopLoss == firstCandel.EndPrice - (firstCandel.EndPrice * 0.0025m))
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



                                /// RANGE LOGIC
                                Candel RANGE_HIGH = CandelDataAfterFirstCandel
                                                                  .OrderByDescending(c => c.HighestPrice)
                                                                  .FirstOrDefault();


                                List<decimal> decList = new List<decimal>();

                                foreach (Candel testCandel in CandelDataAfterFirstCandel)
                                {
                                    decList.Add(testCandel.HighestPrice);
                                }


                                bool isGreaterPriceFound = false;

                                foreach (decimal price in decList)
                                {
                                    if (price >= expectedPrice)
                                    {
                                        isGreaterPriceFound = true;
                                        break; // Exit the loop as we found a match
                                    }
                                }


                                Candel RangeProfit = new Candel();
                                bool isRangeProfitFound = false;

                                foreach (Candel testCandel in CandelDataAfterFirstCandel)
                                {
                                    decimal High = testCandel.HighestPrice;
                                    decimal Low = testCandel.LowestPrice;

                                    if (
                                        expectedPrice <= High
                                        )
                                    {
                                        RangeProfit = testCandel;
                                        isRangeProfitFound = true;
                                        break;
                                    }
                                }





                                if (isRangeProfitFound)
                                {
                                    List<Candel> CandelPair = new List<Candel>();

                                    CandelPair.Add(dojiCandle);
                                    CandelPair.Add(RangeProfit);
                                    CandelPair.Add(RANGE_HIGH);

                                    CorrectPredictionList.Add(CandelPair);
                                    MainCorrectPredictionList.Add(CorrectPredictionList);
                                }
                                else
                                {
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


            List<List<Candel>> combinedList = MainWrongPredictionList
                                     .Concat(MainCorrectPredictionList)
                                     .SelectMany(innerList => innerList)
                                     .Distinct()
                                     .ToList();

            List<Candel> selectedObject = SelectRandomCandelList(combinedList);


            // Check if selectedObject is present in extractedListWrongPredictions
            bool isInWrongPredictions = extractedListWrongPredictions
                .Any(innerList => innerList.SequenceEqual(selectedObject));

            // Check if selectedObject is present in extractedListCorrectPredictions
            bool isInCorrectPredictions = extractedListCorrectPredictions
                .Any(innerList => innerList.SequenceEqual(selectedObject));

            foreach (List<Candel> mainList in extractedListWrongPredictions)
            {
                Candel firstCandel = mainList[0];
                Candel secondCandel = mainList[1];

                decimal NoofStocks = 200000 / firstCandel.EndPrice;
                //decimal NoofStocks = 400000 / firstCandel.EndPrice;
                //decimal NoofStocks = 50000 / firstCandel.EndPrice;
                //decimal NoofStocks = 3391 / firstCandel.EndPrice;

                //MainLoss = MainLoss + (NoofStocks * (firstCandel.EndPrice - secondCandel.EndPrice)) + 117;

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

            var accuracy = ((double)extractedListCorrectPredictions.Count /
                            (extractedListCorrectPredictions.Count + extractedListWrongPredictions.Count)) * 100;

            return Ok(new
            {
                Net = MainProfit - MainLoss,

                //selectedObject,
                accuracy,
                isInCorrectPredictions,
                //isInWrongPredictions,

                //MainProfit,
                //MainLoss,
                //CorrectPred,
                //WrongPred,
                extractedListCorrectPredictions,
                extractedListWrongPredictions,
                MainMasterList
            });

        }

        public static List<Candel> SelectRandomCandelList(List<List<Candel>> combinedList)
        {
            if (combinedList == null || combinedList.Count == 0)
            {
                Console.WriteLine("Error: Combined list is empty or null.");
                return null;
            }

            Random random = new Random();
            HashSet<int> usedIndexes = new HashSet<int>();
            int maxAttempts = Math.Min(10, combinedList.Count); // Avoid infinite loops

            // Shuffle the list using Fisher-Yates shuffle
            combinedList = combinedList.OrderBy(_ => random.Next()).ToList();

            for (int i = 0; i < maxAttempts; i++)
            {
                int randomIndex = random.Next(combinedList.Count);

                if (!usedIndexes.Contains(randomIndex))
                {
                    usedIndexes.Add(randomIndex);
                    List<Candel> selectedList = combinedList[randomIndex];

                    if (selectedList != null && selectedList.Count > 0)
                    {
                        Console.WriteLine($"Selected List at Index {randomIndex}");
                        return selectedList;
                    }
                }
            }

            Console.WriteLine("Warning: Could not find a valid non-empty list after multiple attempts.");
            return null;
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
