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

namespace StockLogger.Controllers.API_Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestDownTrendController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;

        public TestDownTrendController(StockLoggerDbContext context)
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


        /////////////////////////////////////////////////////

        // TO_CHECK_UPTREND


        // FOR_10_CANDELS

        //public bool IsInDowntrend(List<Candel> candelData, int confirmationCount = 2)
        //{
        //    if (candelData == null || candelData.Count < 10)
        //        return false;

        //    var ordered = candelData.OrderBy(c => c.OpenTime).ToList();
        //    var recentCandles = ordered.TakeLast(10).ToList(); // Use last 10 candles

        //    // Adaptive period calculation
        //    int shortPeriod = Math.Max(3, recentCandles.Count / 3);
        //    int mediumPeriod = Math.Max(5, recentCandles.Count / 2);

        //    bool shortTerm = CheckDowntrendPriceStructure(recentCandles.TakeLast(shortPeriod).ToList());
        //    bool mediumTerm = CheckDowntrendPriceStructure(recentCandles.TakeLast(mediumPeriod).ToList());

        //    // Simplified moving average setup
        //    var maFast = CalculateSMA(ordered, Math.Min(5, ordered.Count));
        //    var maSlow = CalculateSMA(ordered, Math.Min(10, ordered.Count));

        //    bool priceBelowMA = recentCandles.Last().EndPrice < maFast.Last()
        //                      && maFast.Last() < maSlow.Last();

        //    bool volumeIncreasing = CheckVolumeTrend(recentCandles);
        //    bool momentumNegative = CheckDowntrendMomentum(recentCandles);
        //    bool trendlineDownward = CalculateTrendlineSlope(recentCandles) < 0;

        //    // Simplified scoring system
        //    int score = 0;
        //    score += shortTerm ? 3 : 0;
        //    score += mediumTerm ? 4 : 0;
        //    score += priceBelowMA ? 3 : 0;
        //    score += volumeIncreasing ? 2 : 0;
        //    score += momentumNegative ? 3 : 0;
        //    score += trendlineDownward ? 2 : 0;

        //    // Volume-based confirmation
        //    decimal avgVolume = CalculateAverageVolume(ordered, Math.Min(5, ordered.Count));
        //    int bearishConfirmation = ordered.TakeLast(confirmationCount)
        //        .Count(c => (c.IsBearish ?? false) && c.Volume > avgVolume);

        //    return score >= 10 && bearishConfirmation >= confirmationCount;
        //}

        //private bool CheckDowntrendPriceStructure(List<Candel> candles)
        //{
        //    // Allow 1 exception in short sequences
        //    int exceptionsAllowed = candles.Count <= 5 ? 1 : 0;
        //    int exceptions = 0;

        //    for (int i = 1; i < candles.Count; i++)
        //    {
        //        if (candles[i].HighestPrice >= candles[i - 1].HighestPrice ||
        //            candles[i].LowestPrice >= candles[i - 1].LowestPrice)
        //        {
        //            if (++exceptions > exceptionsAllowed)
        //                return false;
        //        }
        //    }
        //    return true;
        //}


        // FOR_5_CANDELS 

        public bool IsInDowntrend(List<Candel> candelData, int confirmationCount = 2)
        {
            // Check for at least 5 candles
            if (candelData == null || candelData.Count < 5)
                return false;

            var ordered = candelData.OrderBy(c => c.OpenTime).ToList();
            var recentCandles = ordered.TakeLast(5).ToList(); // Use last 5 candles

            // Adjusted adaptive periods for 5-candle setup
            int shortPeriod = Math.Max(2, recentCandles.Count / 3);  // 5/3 ≈ 1.67 → 2
            int mediumPeriod = Math.Max(3, recentCandles.Count / 2); // 5/2 = 2.5 → 3

            bool shortTerm = CheckDowntrendPriceStructure(recentCandles.TakeLast(shortPeriod).ToList());
            bool mediumTerm = CheckDowntrendPriceStructure(recentCandles.TakeLast(mediumPeriod).ToList());

            // Adjusted moving average periods for smaller dataset
            int maFastPeriod = Math.Min(3, ordered.Count);
            int maSlowPeriod = Math.Min(5, ordered.Count);
            var maFast = CalculateSMA(ordered, maFastPeriod);
            var maSlow = CalculateSMA(ordered, maSlowPeriod);

            bool priceBelowMA = recentCandles.Last().EndPrice < maFast.Last()
                              && maFast.Last() < maSlow.Last();

            bool volumeIncreasing = CheckVolumeTrend(recentCandles);
            bool momentumNegative = CheckDowntrendMomentum(recentCandles);
            bool trendlineDownward = CalculateTrendlineSlope(recentCandles) < 0;

            // Adjusted scoring weights for smaller dataset
            int score = 0;
            score += shortTerm ? 2 : 0;       // Reduced weight for shorter term
            score += mediumTerm ? 3 : 0;      // Adjusted medium term weight
            score += priceBelowMA ? 3 : 0;
            score += volumeIncreasing ? 2 : 0;
            score += momentumNegative ? 2 : 0; // Reduced weight for single-point momentum
            score += trendlineDownward ? 2 : 0;

            // Volume-based confirmation with adjusted average calculation
            decimal avgVolume = CalculateAverageVolume(ordered, Math.Min(3, ordered.Count));
            int bearishConfirmation = ordered.TakeLast(confirmationCount)
                .Count(c => (c.IsBearish ?? false) && c.Volume > avgVolume);

            // Adjusted threshold for smaller dataset
            return score >= 8 && bearishConfirmation >= confirmationCount;
        }

        // Other helper methods remain the same but with contextual awareness for smaller datasets

        private bool CheckDowntrendPriceStructure(List<Candel> candles)
        {
            // More tolerant exceptions for small datasets
            int exceptionsAllowed = candles.Count <= 3 ? 1 : 0;
            int exceptions = 0;

            for (int i = 1; i < candles.Count; i++)
            {
                if (candles[i].HighestPrice >= candles[i - 1].HighestPrice ||
                    candles[i].LowestPrice >= candles[i - 1].LowestPrice)
                {
                    if (++exceptions > exceptionsAllowed)
                        return false;
                }
            }
            return true;
        }

        private List<decimal> CalculateSMA(List<Candel> data, int period)
        {
            if (period < 1) period = 1;
            return data.Select((c, i) =>
                i >= period - 1
                ? data.Skip(Math.Max(0, i - period + 1)).Take(period).Average(c => c.EndPrice)
                : 0m).ToList();
        }

        private decimal CalculateRSI(decimal[] closes, int period = 5)
        {
            if (closes.Length < period + 1) return 50m;

            decimal avgGain = closes.TakeLast(period).Where((c, i) => i > 0 && c > closes[i - 1]).Average();
            decimal avgLoss = closes.TakeLast(period).Where((c, i) => i > 0 && c < closes[i - 1]).Average();

            if (avgLoss == 0) return 100m;
            decimal rs = avgGain / avgLoss;
            return 100 - (100 / (1 + rs));
        }

        private decimal CalculateAverageVolume(List<Candel> data, int period)
        {
            return data.TakeLast(Math.Min(period, data.Count)).Average(c => c.Volume);
        }

        private decimal CalculateTrendlineSlope(List<Candel> candles)
        {
            if (candles.Count < 2) return 0m;

            decimal sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            int n = candles.Count;

            for (int i = 0; i < n; i++)
            {
                decimal x = i;
                decimal y = candles[i].EndPrice;
                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            decimal denominator = n * sumX2 - sumX * sumX;
            return denominator == 0 ? 0m : (n * sumXY - sumX * sumY) / denominator;
        }

        private bool CheckVolumeTrend(List<Candel> candles)
        {
            if (candles.Count < 3) return false;
            decimal currentVolume = candles.Last().Volume;
            decimal prevVolume = candles[candles.Count - 2].Volume;
            return currentVolume > prevVolume;
        }

        private bool CheckDowntrendMomentum(List<Candel> candles)
        {
            if (candles.Count < 3) return false;
            decimal currentClose = candles.Last().EndPrice;
            decimal prevClose = candles[candles.Count - 2].EndPrice;
            return currentClose < prevClose;
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

        public decimal CalculateAveragePrice(List<Candel> candels)
        {
            // Sum of average prices for each Candel
            decimal totalAveragePrice = 0;

            // Iterate through each Candel and calculate its average price
            foreach (var candel in candels)
            {
                decimal averagePrice = (candel.StartPrice + candel.HighestPrice + candel.LowestPrice + candel.EndPrice) / 4;
                totalAveragePrice += averagePrice;
            }

            // Calculate overall average price
            decimal overallAveragePrice = totalAveragePrice / candels.Count;

            return overallAveragePrice;
        }


        //POST https://localhost:44364/api/TestDownTrend/BulkTestMasterAPI
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


                            //    //Downtrend 5 Candel vs 10 Candel comparison

                            //    /////////////////////////////////////////

                            //    //5 Candel

                            //    //924 / 70
                            //    //1135 / 125


                            //    /////////////////////////////////////////

                            //    //10 Candel

                            //    //902 / 69
                            //    //1187 / 128


                            //    //if (testCandel.OpenTime.TimeOfDay > new TimeSpan(15, 00, 0))
                            //    //{
                            //    //    break;
                            //    //}

                            //    //List<Candel> CandelDataBeforeTestCandel = CandelData
                            //    //                    .Where(candel => candel.OpenTime < testCandel.OpenTime)
                            //    //                    .OrderByDescending(c => c.OpenTime)  // Sort in descending order to get latest first
                            //    //                    .Take(10)  // Take the last 21 candles
                            //    //                    .OrderBy(c => c.OpenTime)  // Reorder them back in ascending order
                            //    //                    .ToList();


                            //    //bool isListinDowntrend = IsInDowntrend(CandelDataBeforeTestCandel);

                            //    //bool exists = dragonFlyDojiCandles?.Any(c => c.Ticker == testCandel.Ticker) ?? false;


                            //    //if (
                            //    //   isListinDowntrend
                            //    //   //&& (exists == false)
                            //    //   )
                            //    //{
                            //    //    dragonFlyDojiCandles.Add(testCandel);
                            //    //}


                            //    if (testCandel.OpenTime.TimeOfDay > new TimeSpan(14, 00, 0))
                            //    {
                            //        break;
                            //    }

                            //    List<Candel> CandelDataBeforeTestCandel = CandelData
                            //                        .Where(candel => candel.OpenTime < testCandel.OpenTime)
                            //                        .OrderByDescending(c => c.OpenTime)  // Sort in descending order to get latest first
                            //                        .Take(5)  // Take the last 21 candles
                            //                        .OrderBy(c => c.OpenTime)  // Reorder them back in ascending order
                            //                        .ToList();


                            //    bool isListinDowntrend = IsInDowntrend(CandelDataBeforeTestCandel);

                            //    bool exists = dragonFlyDojiCandles?.Any(c => c.Ticker == testCandel.Ticker) ?? false;


                            //    if (
                            //       isListinDowntrend
                            //       //&& (exists == false)
                            //       )
                            //    {
                            //        dragonFlyDojiCandles.Add(testCandel);
                            //    }




                            //}



                            foreach (Candel testCandel in CandelData)
                            {

                                //List<Candel> CandelDataBeforeTestCandel = CandelData
                                //                    .Where(candel => candel.OpenTime <= testCandel.OpenTime)
                                //                    .OrderByDescending(c => c.OpenTime)
                                //                    .Take(3)
                                //                    .OrderBy(c => c.OpenTime)
                                //                    .ToList();

                                //Candel candel1 = CandelDataBeforeTestCandel[0];
                                //Candel candel2 = CandelDataBeforeTestCandel[1];
                                //Candel candel3 = CandelDataBeforeTestCandel[2];


                                List<Candel> CandelDataBeforeTestCandel = CandelData
                                                    .Where(candel => candel.OpenTime < testCandel.OpenTime)
                                                    .OrderByDescending(c => c.OpenTime)  // Sort in descending order to get latest first
                                                    .Take(5)  // Take the last 21 candles
                                                    .OrderBy(c => c.OpenTime)  // Reorder them back in ascending order
                                                    .ToList();


                                bool isListinDowntrend = IsInDowntrend(CandelDataBeforeTestCandel);

                                //if ((testCandel.EndPrice > testCandel.StartPrice) && (isListinDowntrend == true)) // Bullish Candel
                                //{
                                //    if ((testCandel.StartPrice == testCandel.LowestPrice))
                                //    {
                                //        dragonFlyDojiCandles.Add(testCandel);
                                //    }
                                //}
                                //else if (testCandel.EndPrice < testCandel.StartPrice && (isListinDowntrend == true)) // Bearish Candel
                                //{
                                //    if ((testCandel.EndPrice == testCandel.LowestPrice))
                                //    {
                                //        dragonFlyDojiCandles.Add(testCandel);
                                //    }
                                //}


                                if(isListinDowntrend)
                                {
                                    dragonFlyDojiCandles.Add(testCandel);
                                }


                                //if ((testCandel.EndPrice > testCandel.StartPrice)) // Bullish Candel
                                //{
                                //    if ((testCandel.StartPrice == testCandel.LowestPrice))
                                //    {
                                //        dragonFlyDojiCandles.Add(testCandel);
                                //    }
                                //}
                                //else if (testCandel.EndPrice < testCandel.StartPrice) // Bearish Candel
                                //{
                                //    if ((testCandel.EndPrice == testCandel.LowestPrice))
                                //    {
                                //        dragonFlyDojiCandles.Add(testCandel);
                                //    }
                                //}

                                //if ((DetectThreeBlackCrows(CandelDataBeforeTestCandel)) == true)
                                //{
                                //    dragonFlyDojiCandles.Add(testCandel);
                                //}


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

                                decimal expectedPrice = 0; // 2.5 R profit on 1000 R //450 on 2Lakh

                                if (firstCandel != null)
                                {
                                    //expectedPrice = firstCandel.EndPrice * 1.000595m;
                                    //expectedPrice = firstCandel.EndPrice * 1.00061m;
                                    expectedPrice = firstCandel.EndPrice - (firstCandel.EndPrice * 0.00065m);
                                    //firstCandel.EndPrice - (firstCandel.EndPrice * 0.01m); //  10 R profit on 1000 R //1995 on 2 lakh
                                    //expectedPrice = firstCandel.EndPrice - (firstCandel.EndPrice * 0.005m); //  5 R profit on 1000 R //997 on 2lakh
                                    //expectedPrice = firstCandel.EndPrice - (firstCandel.EndPrice * 0.0025m); // 2.5 R profit on 1000 R //450 on 2Lakh

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


                                ConstForProfit = profitMargin;

                                List<Candel> CandelDataAfterFirstCandel = CandelData
                                           .Where(candel => candel.OpenTime >= firstCandel.OpenTime)
                                           .OrderBy(c => c.OpenTime)
                                           .ToList();
                   

                                /// RANGE LOGIC
                                Candel RANGE_LOW = CandelDataAfterFirstCandel
                                                                  .OrderBy(c => c.LowestPrice)
                                                                  .FirstOrDefault();


                                List<decimal> decList = new List<decimal>();

                                foreach (Candel testCandel in CandelDataAfterFirstCandel)
                                {
                                    decList.Add(testCandel.LowestPrice);
                                }


                                bool isLowestPriceFound = false;

                                foreach (decimal price in decList)
                                {
                                    if (price <= expectedPrice)
                                    {
                                        isLowestPriceFound = true;
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
                                        //expectedPrice >= Low 
                                        //&&
                                        Low <= expectedPrice
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
                                    CandelPair.Add(RANGE_LOW);

                                    CorrectPredictionList.Add(CandelPair);
                                    MainCorrectPredictionList.Add(CorrectPredictionList);
                                }
                                else
                                {
                                    List<Candel> CandelPair = new List<Candel>();
                                    CandelPair.Add(dojiCandle);
                                    CandelPair.Add(EndCandel);
                                    CandelPair.Add(RANGE_LOW);

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
