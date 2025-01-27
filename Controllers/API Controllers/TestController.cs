using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OtpNet;
using StockLogger.Data;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Dragonfly_Doji;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        ////POST https://localhost:44364/api/Test/BulkTestMasterAPI
        //[HttpPost("BulkTestMasterAPI")]
        //public async Task<IActionResult> BulkTestMasterAPI([FromBody] BulkTestRequestModel request)
        //{
        //    var Tokenresponse = await _context.Token.FirstOrDefaultAsync();

        //    string authtoken = Tokenresponse.AuthToken;

        //    //string authtoken = await GetAuthorizationTokenAsync();

        //    List<List<List<Candel>>> MainCorrectPredictionList = new List<List<List<Candel>>>();
        //    List<List<List<Candel>>> MainWrongPredictionList = new List<List<List<Candel>>>();

        //    List<List<Candel>> MainMasterList = new List<List<Candel>>();

        //    decimal ConstForProfit = 0;


        //    decimal MainProfit = 0;
        //    decimal MainLoss = 0;

        //    // Check if the StartDate is a Saturday or Sunday
        //    if (request.StartDate.DayOfWeek == DayOfWeek.Saturday || request.StartDate.DayOfWeek == DayOfWeek.Sunday)
        //    {
        //        return Ok(new
        //        {
        //            Net = MainProfit - MainLoss,
        //            MainProfit,
        //            MainLoss,
        //            MainCorrectPredictionList,
        //            MainWrongPredictionList,
        //            MainMasterList
        //        });
        //    }

        //    foreach (var stock in _stocks)
        //    {
        //        List<Candel> MasterList = new List<Candel>();

        //        List<Candel> DrafonFlyDojiCandels = new List<Candel>();

        //        List<List<Candel>> CorrectPredictionList = new List<List<Candel>>();
        //        List<List<Candel>> WrongPredictionList = new List<List<Candel>>();

        //        List<Candel> CandelData = new List<Candel>();

        //        // Variables for profit and loss tracking
        //        decimal TotalProfit = 0;
        //        decimal NetLoss = 0;

        //        var token = authtoken;

        //        var requestBody = new
        //        {
        //            SymbolToken = stock.symboltoken,
        //            AuthorizationToken = token,
        //            StartDate = request.StartDate.ToString("o"), // ISO string format
        //            EndDate = request.EndDate.ToString("o") // ISO string format
        //        };

        //        var client = new HttpClient();
        //        var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
        //        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");


        //        var adjustedStartDate = request.StartDate.AddDays(-1);
        //        if (adjustedStartDate.DayOfWeek == DayOfWeek.Saturday)
        //            adjustedStartDate = adjustedStartDate.AddDays(-1); // Move to Friday
        //        else if (adjustedStartDate.DayOfWeek == DayOfWeek.Sunday)
        //            adjustedStartDate = adjustedStartDate.AddDays(-2); // Move to Friday

        //        var adjustedEndDate = request.EndDate.AddDays(-1);
        //        if (adjustedEndDate.DayOfWeek == DayOfWeek.Saturday)
        //            adjustedEndDate = adjustedEndDate.AddDays(-1); // Move to Friday
        //        else if (adjustedEndDate.DayOfWeek == DayOfWeek.Sunday)
        //            adjustedEndDate = adjustedEndDate.AddDays(-2); // Move to Friday

        //        var requestBodyforPrevousDayData = new
        //        {
        //            SymbolToken = stock.symboltoken,
        //            AuthorizationToken = token,
        //            StartDate = adjustedStartDate.ToString("o"), // Final adjusted date
        //            EndDate = adjustedEndDate.ToString("o")     // Final adjusted date
        //        };

        //        try
        //        {
        //            var response = await client.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest", content);

        //            if (response.IsSuccessStatusCode)
        //            {

        //                var responseData = await response.Content.ReadAsStringAsync();
        //                CandelData = JsonConvert.DeserializeObject<List<Candel>>(responseData);

        //                Candel EndCandel = CandelData.FirstOrDefault(c => c.OpenTime.TimeOfDay == new TimeSpan(15, 15, 0));

        //                List<Candel>? dragonFlyDojiCandles = new List<Candel>();

        //                if (CandelData != null)
        //                {

        //                    foreach (Candel testCandel in CandelData)
        //                    {
        //                        Candel firstCandel = testCandel;

        //                        // Calculate the body size
        //                        decimal bodySize = Math.Abs(firstCandel.EndPrice - firstCandel.StartPrice);

        //                        // Calculate the upper wick size
        //                        decimal upperWick = firstCandel.HighestPrice - Math.Max(firstCandel.StartPrice, firstCandel.EndPrice);

        //                        // Calculate the lower wick size
        //                        decimal lowerWick = Math.Min(firstCandel.StartPrice, firstCandel.EndPrice) - firstCandel.LowestPrice;

        //                        // Check if this candle matches the Shooting Star criteria
        //                        bool isShootingStar =
        //                            upperWick >= 2 * bodySize &&     // Upper wick is at least twice the body size
        //                            lowerWick <= bodySize * 0.1m &&  // Lower wick is negligible
        //                            bodySize > 0 &&                  // Non-zero body size (to avoid dojis)
        //                            firstCandel.EndPrice < firstCandel.StartPrice; // Indicates a bearish Shooting Star

        //                        if (isShootingStar)
        //                        {
        //                            dragonFlyDojiCandles.Add(firstCandel);
        //                        }

        //                    }


        //                }

        //                MasterList = CandelData;
        //                MainMasterList.Add(MasterList);
        //                DrafonFlyDojiCandels = dragonFlyDojiCandles;

        //                if (dragonFlyDojiCandles.Count > 0)
        //                {

        //                    foreach (Candel dojiCandle in dragonFlyDojiCandles)
        //                    {
        //                        Candel firstCandel = dojiCandle;

        //                        //var expectedPrice = firstCandel.EndPrice * 1.0008014m; 
        //                        //var expectedPrice = firstCandel.EndPrice * 1.001429m; // 1.429 R profit on 1000 R // 285 on 2 Lakh
        //                        //var expectedPrice = firstCandel.EndPrice * 1.005m; //  5 R profit on 1000 R //997 on 2lakh
        //                        //var expectedPrice = firstCandel.EndPrice * 1.01m;  //  10 R profit on 1000 R //1995 okkkn 2 lakh
        //                        //decimal expectedPrice = firstCandel.EndPrice * 1.0025m; // 2.5 R profit on 1000 R //450 on 2Lakh
        //                        decimal expectedPrice = 0; // 2.5 R profit on 1000 R //450 on 2Lakh

        //                        if (firstCandel != null)
        //                        {
        //                            expectedPrice = firstCandel.EndPrice - (firstCandel.EndPrice * 0.00065m);
        //                        }

        //                        decimal profitMargin = 0;

        //                        if (expectedPrice == firstCandel.EndPrice * 1.00065m)
        //                        {
        //                            profitMargin = 130 - 117;
        //                        }
        //                        else if (expectedPrice == firstCandel.EndPrice * 1.0008014m)
        //                        {
        //                            profitMargin = 160 - 117;
        //                        }
        //                        else if (expectedPrice == firstCandel.EndPrice * 1.0025m)
        //                        {
        //                            profitMargin = 450 - 117;
        //                        }
        //                        else if (expectedPrice == firstCandel.EndPrice * 1.01m)
        //                        {
        //                            profitMargin = 1995 - 117;
        //                        }
        //                        else if (expectedPrice == firstCandel.EndPrice * 1.005m)
        //                        {
        //                            profitMargin = 997 - 117;
        //                        }
        //                        else if (expectedPrice == firstCandel.EndPrice * 1.001429m)
        //                        {
        //                            profitMargin = 285 - 117;
        //                        }


        //                        ConstForProfit = profitMargin;

        //                        //var stopLoss = firstCandel.EndPrice - (firstCandel.EndPrice * 0.03m);// 5985 on 2 lakh
        //                        //var stopLoss = firstCandel.EndPrice - (firstCandel.EndPrice * 0.01m);// 2000 on 2 lakh
        //                        //var stopLoss = firstCandel.EndPrice - (firstCandel.EndPrice * 0.005m);// 997.5 on 2 lakh
        //                        var stopLoss = firstCandel.EndPrice - (firstCandel.EndPrice * 0.001429m); // 300 on 2 lakh


        //                        decimal lossMargin = 0;
        //                        if (stopLoss == firstCandel.EndPrice - (firstCandel.EndPrice * 0.001429m))
        //                        {
        //                            lossMargin = 300 + 117;
        //                        }
        //                        else if (stopLoss == firstCandel.EndPrice - (firstCandel.EndPrice * 0.005m))
        //                        {
        //                            lossMargin = 997 + 117;
        //                        }
        //                        else if (stopLoss == firstCandel.EndPrice - (firstCandel.EndPrice * 0.03m))
        //                        {
        //                            lossMargin = 5985 + 117;
        //                        }
        //                        else if (stopLoss == firstCandel.EndPrice - (firstCandel.EndPrice * 0.01m))
        //                        {
        //                            lossMargin = 2000 + 117;
        //                        }




        //                        List<Candel> CandelDataAfterFirstCandel = CandelData
        //                                 .Where(candel => candel.OpenTime > firstCandel.OpenTime).ToList();

        //                        // Lowest StartPrice
        //                        Candel candleWithLowestStartPrice = CandelDataAfterFirstCandel
        //                                                           .OrderBy(c => c.StartPrice)
        //                                                           .FirstOrDefault();

        //                        // Lowest HighestPrice
        //                        Candel candleWithLowestHighestPrice = CandelDataAfterFirstCandel
        //                                                            .OrderBy(c => c.HighestPrice)
        //                                                            .FirstOrDefault();

        //                        // Lowest LowestPrice
        //                        Candel candleWithLowestLowestPrice = CandelDataAfterFirstCandel
        //                                                           .OrderBy(c => c.LowestPrice)
        //                                                           .FirstOrDefault();

        //                        // Lowest EndPrice
        //                        Candel candleWithLowestEndPrice = CandelDataAfterFirstCandel
        //                                                         .OrderBy(c => c.EndPrice)
        //                                                         .FirstOrDefault();

        //                        Candel stoplossCandel = null;

        //                        stoplossCandel = CandelDataAfterFirstCandel
        //                                      .Where(c =>
        //                                          (c.HighestPrice <= stopLoss ||
        //                                           c.LowestPrice <= stopLoss ||
        //                                           c.EndPrice <= stopLoss ||
        //                                           c.StartPrice <= stopLoss))
        //                                      .OrderBy(c => c.OpenTime)
        //                                      .FirstOrDefault();


        //                        Candel profitCandel = null;


        //                        profitCandel = CandelDataAfterFirstCandel
        //                                      .Where(c =>
        //                                          (c.HighestPrice <= expectedPrice ||
        //                                           c.LowestPrice <= expectedPrice ||
        //                                           c.EndPrice <= expectedPrice ||
        //                                           c.StartPrice <=expectedPrice))
        //                                      .OrderBy(c => c.OpenTime)
        //                                      .FirstOrDefault();


        //                        Candel earliestCandle = new[]
        //                        {
        //                            candleWithLowestStartPrice,
        //                            candleWithLowestHighestPrice,
        //                            candleWithLowestLowestPrice,
        //                            candleWithLowestEndPrice,
        //                            profitCandel
        //                        }
        //                        .Where(c => c != null) // Ensure c is not null
        //                        .OrderBy(c => c.OpenTime)
        //                        .FirstOrDefault();


        //                        bool suceessFound = false;

        //                        if (
        //                               (candleWithLowestStartPrice != null && candleWithLowestStartPrice.HighestPrice <= expectedPrice)
        //                            || (candleWithLowestHighestPrice != null && candleWithLowestHighestPrice.StartPrice <= expectedPrice)
        //                            || (candleWithLowestLowestPrice != null && candleWithLowestLowestPrice.EndPrice <= expectedPrice)
        //                            || (candleWithLowestEndPrice != null && candleWithLowestEndPrice.LowestPrice <= expectedPrice)
        //                            || (profitCandel != null)
        //                            )
        //                        {
        //                            suceessFound = true;
        //                        }


        //                        /// RANGE LOGIC
        //                        Candel RANGE_LOW = CandelDataAfterFirstCandel
        //                                                          .OrderBy(c => c.LowestPrice)
        //                                                          .FirstOrDefault();



        //                        if ((RANGE_LOW != null && RANGE_LOW.LowestPrice <= expectedPrice) || (suceessFound == true))
        //                        {
        //                            List<Candel> CandelPair = new List<Candel>();

        //                            CandelPair.Add(dojiCandle);
        //                            CandelPair.Add(earliestCandle);

        //                            CorrectPredictionList.Add(CandelPair);
        //                            MainCorrectPredictionList.Add(CorrectPredictionList);
        //                        }
        //                        else
        //                        {

        //                            List<Candel> CandelPair = new List<Candel>();
        //                            CandelPair.Add(dojiCandle);
        //                            CandelPair.Add(EndCandel);

        //                            WrongPredictionList.Add(CandelPair);
        //                            MainWrongPredictionList.Add(WrongPredictionList);

        //                        }

        //                    }

        //                }

        //            }
        //            else
        //            {
        //                Console.WriteLine($"Error: {response.StatusCode}");
        //            }


        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"An error occurred: {ex.Message}");
        //        }





        //    }

        //    List<Candel> allCandelsOfCorrectPredictions = MainCorrectPredictionList
        //                             .SelectMany(outerList => outerList.SelectMany(innerList => innerList))
        //                             .ToList();


        //    // Assuming Candel is a predefined class
        //    List<List<Candel>> extractedListWrongPredictions = MainWrongPredictionList
        //                                      .SelectMany(innerList => innerList)
        //                                      .Distinct()
        //                                      .ToList();



        //    List<List<Candel>> extractedListCorrectPredictions = MainCorrectPredictionList
        //                                      .SelectMany(innerList => innerList)
        //                                      .Distinct()
        //                                      .ToList();



        //    foreach (List<Candel> mainList in extractedListWrongPredictions)
        //    {
        //        Candel firstCandel = mainList[0];
        //        Candel secondCandel = mainList[1];

        //        decimal NoofStocks = 200000 / firstCandel.EndPrice;
        //        //decimal NoofStocks = 50000 / firstCandel.EndPrice;
        //        //decimal NoofStocks = 3391 / firstCandel.EndPrice;

        //        MainLoss = MainLoss + (NoofStocks * (firstCandel.EndPrice - secondCandel.EndPrice)) + 117;

        //    }

        //    MainProfit = extractedListCorrectPredictions.Count * ConstForProfit;

        //    return Ok(new
        //    {
        //        Net = MainProfit - MainLoss,
        //        //MainProfit,
        //        //MainLoss,
        //        extractedListCorrectPredictions,
        //        extractedListWrongPredictions,
        //        MainMasterList
        //    });

        //}


        //POST https://localhost:44364/api/Test/BulkTestMasterAPI
        [HttpPost("BulkTestMasterAPI")]
        public async Task<IActionResult> BulkTestMasterAPI([FromBody] BulkTestRequestModel request)
        {
            var Tokenresponse = await _context.Token.FirstOrDefaultAsync();

            string authtoken = Tokenresponse.AuthToken;

            //string authtoken = await GetAuthorizationTokenAsync();

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

                    //var responseForPreviousDaysData = await client.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest", contentForPreviousDaysData);

                    if (response.IsSuccessStatusCode
                        //&& responseForPreviousDaysData.IsSuccessStatusCode
                        )
                    {

                        var responseData = await response.Content.ReadAsStringAsync();
                        CandelData = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                        //var responseDataForPreviousDaysData = await responseForPreviousDaysData.Content.ReadAsStringAsync();
                        //List<Candel> CandelDataPreviousDay = JsonConvert.DeserializeObject<List<Candel>>(responseDataForPreviousDaysData);

                        //Candel EndCandel = CandelData.LastOrDefault();

                        Candel EndCandel = CandelData.FirstOrDefault(c => c.OpenTime.TimeOfDay == new TimeSpan(15, 15, 0));

                        List<Candel>? dragonFlyDojiCandles = new List<Candel>();

                        if (CandelData != null)
                        {
                            // Dont use INVERTED HAMMER logic without stoploss


                            //dragonFlyDojiCandles = IdentifyDragonflyDojiCandles(CandelData , CandelDataPreviousDay);

                            //dragonFlyDojiCandles = IdentifyBullishEngulfingCandles(CandelData, CandelDataPreviousDay);
                            //dragonFlyDojiCandles = IdentifyInvertedHammerCandles(CandelData, CandelDataPreviousDay);


                            //////////////////////////////////////////////////////////////////////////////


                            foreach (Candel testCandel in CandelData)
                            {
                                // Get the next 10 candles after the current candle for analysis
                                List<Candel> next10Candles = CandelData
                                    .Where(c => c.OpenTime > testCandel.OpenTime)
                                    .OrderBy(c => c.OpenTime)
                                    .Take(10)
                                    .ToList();

                                if (next10Candles.Count < 10) continue; // Skip if not enough candles to analyze

                                List<Candel> leftCup = next10Candles.Take(5).ToList(); // Left part (cup decline)
                                List<Candel> rightCup = next10Candles.Skip(5).Take(5).ToList(); // Right part (cup rise and handle)

                                // LEFT SIDE IN DOWNTREND
                                bool isleftDownTrend = leftCup[4].EndPrice < leftCup[0].EndPrice;
                                // RIGHT SIDE IN UPTREND
                                bool isRigtUpTrend = rightCup[4].EndPrice > rightCup[0].EndPrice;

                                if(isleftDownTrend && isRigtUpTrend)
                                {
                                    dragonFlyDojiCandles.Add(next10Candles[9]);
                                }

                            }


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
                            //            && isVerificationCandelBullish
                            //            && isVerificationCandelHighGreater
                            //            && isverificationCandelVolumeGreater
                            //            )
                            //        {
                            //            dragonFlyDojiCandles.Add(verificationCandel);
                            //            //dragonFlyDojiCandles.Add(testCandel);
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

                            //    //Candel previousTenth = CandelData
                            //    //                     .Where(c => c.OpenTime < testCandel.OpenTime) // Select candles before the test candle
                            //    //                     .OrderByDescending(c => c.OpenTime)          // Sort in descending order of OpenTime
                            //    //                     .Skip(7)                                     // Skip the first 9 candles
                            //    //                     .FirstOrDefault();                           // Take the 10th one (or default if none exist)

                            //    //// Hammer must appear after a downtrend (additional logic to determine downtrend can be added)
                            //    //bool isInDowntrend = false; // Example threshold for downtrend check

                            //    //if ((previousTenth != null) && (testCandel.EndPrice < previousTenth.EndPrice))
                            //    //{
                            //    //    isInDowntrend = true;
                            //    //}

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
                            //        && isverificationCandelVolumeGreater
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
                                //var expectedPrice = firstCandel.EndPrice * 1.005m; //  5 R profit on 1000 R //997 on 2lakh
                                //var expectedPrice = firstCandel.EndPrice * 1.01m;  //  10 R profit on 1000 R //1995 okkkn 2 lakh
                                //decimal expectedPrice = firstCandel.EndPrice * 1.0025m; // 2.5 R profit on 1000 R //450 on 2Lakh
                                decimal expectedPrice = 0; // 2.5 R profit on 1000 R //450 on 2Lakh

                                if (firstCandel != null)
                                {
                                    //expectedPrice = firstCandel.EndPrice * 1.000595m;
                                    //expectedPrice = firstCandel.EndPrice * 1.00061m;
                                    expectedPrice = firstCandel.EndPrice * 1.00065m;
                                    //expectedPrice = firstCandel.EndPrice * 1.01m;
                                    //expectedPrice = firstCandel.EndPrice * 1.005m;
                                    //expectedPrice = firstCandel.EndPrice * 1.0025m;
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

                                ConstForProfit = profitMargin;

                                //var stopLoss = firstCandel.EndPrice - (firstCandel.EndPrice * 0.03m);// 5985 on 2 lakh
                                //var stopLoss = firstCandel.EndPrice - (firstCandel.EndPrice * 0.01m);// 2000 on 2 lakh
                                //var stopLoss = firstCandel.EndPrice - (firstCandel.EndPrice * 0.005m);// 997.5 on 2 lakh
                                var stopLoss = firstCandel.EndPrice - (firstCandel.EndPrice * 0.001429m); // 300 on 2 lakh


                                decimal lossMargin = 0;
                                if (stopLoss == firstCandel.EndPrice - (firstCandel.EndPrice * 0.001429m))
                                {
                                    lossMargin = 300 + 117;
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
                                         .Where(candel => candel.OpenTime > firstCandel.OpenTime).ToList();

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
                                              .Where(c =>
                                                  (c.HighestPrice <= stopLoss ||
                                                   c.LowestPrice <= stopLoss ||
                                                   c.EndPrice <= stopLoss ||
                                                   c.StartPrice <= stopLoss))
                                              .OrderBy(c => c.OpenTime)
                                              .FirstOrDefault();


                                Candel profitCandel = null;


                                profitCandel = CandelDataAfterFirstCandel
                                              .Where(c =>
                                                  (c.HighestPrice >= expectedPrice ||
                                                   c.LowestPrice >= expectedPrice ||
                                                   c.EndPrice >= expectedPrice ||
                                                   c.StartPrice >= expectedPrice))
                                              .OrderBy(c => c.OpenTime)
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




                                if(
                                    (highestCandel != null) 
                                    && (highestCandel.HighestPrice >= expectedPrice) || ((RANGE_HIGH != null && RANGE_HIGH.HighestPrice >= expectedPrice) || (suceessFound == true))
                                  )
                                {
                                    List<Candel> CandelPair = new List<Candel>();

                                    CandelPair.Add(dojiCandle);
                                    CandelPair.Add(earliestCandle);
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






            List<List<Candel>> TotalPredictions = new List<List<Candel>>();


            // TotalPredictions = MainWrongPredictionList
            //                          .SelectMany(innerList => innerList)
            //                          .Distinct()
            //                          .ToList();

            //TotalPredictions = MainCorrectPredictionList
            //                          .SelectMany(innerList => innerList)
            //                          .Distinct()
            //                          .ToList();

            TotalPredictions = MainWrongPredictionList
                       .SelectMany(innerList => innerList)
                       .Concat(MainCorrectPredictionList.SelectMany(innerList => innerList))
                       .Distinct()
                       .ToList();

            List<Candel> CPred = new List<Candel>();
            List<Candel> WPRed = new List<Candel>();

            List<Candel> TotalList = new List<Candel>();


            foreach (List<Candel> testList in TotalPredictions)
            {
                TotalList.Add(testList[0]);
            }

            foreach (List<Candel> testList in extractedListCorrectPredictions)
            {
                CPred.Add(testList[0]);
            }

            foreach (List<Candel> testList in extractedListWrongPredictions)
            {
                WPRed.Add(testList[0]);
            }



            List<Candel> CorrectPred = new List<Candel>();
            List<Candel> WrongPred = new List<Candel>();

            Candel referenceCandel = new Candel();
            List<Candel> referenceList = new List<Candel>();

            List<Candel> MasterList2 = new List<Candel>();


            //ORDER THE LIST
            TotalList = TotalList.OrderBy(candel => candel.OpenTime).ToList();

            //foreach (Candel testCandel in TotalList)
            if (TotalList != null)
            {
                Candel testCandel = TotalList[0];


                if (CPred.Any(c => c.Equals(testCandel)))
                {
                    CorrectPred.Add(testCandel);
                }
                else
                {
                    WrongPred.Add(testCandel);
                }


                referenceCandel = testCandel;

                    referenceList = TotalList;

                referenceList.RemoveAll(c => c.OpenTime <= referenceCandel.OpenTime);

                Candel nextCandel = referenceList
                                   .Where(c => c.OpenTime > testCandel.OpenTime)
                                   .OrderBy(c => c.OpenTime)
                                   .FirstOrDefault();


                    MasterList2.Add(testCandel);

                    referenceCandel = nextCandel;

                    int maxIterations = 160;
                    int iterationCount = 0;

                    while 
                    (nextCandel != TotalList.LastOrDefault()
                    && iterationCount < maxIterations
                    )
                    {
                        MasterList2.Add(nextCandel);
    
                        referenceList.RemoveAll(c => c.OpenTime <= referenceCandel.OpenTime);

                         Candel nextCandel2 = referenceList
                            .Where(c => c.OpenTime > referenceCandel.OpenTime)
                            .OrderBy(c => c.OpenTime)
                            .FirstOrDefault();

                        referenceCandel = nextCandel2;

                        nextCandel = referenceCandel;

                        if (CPred.Any(c => c.Equals(nextCandel)))
                        {
                            CorrectPred.Add(nextCandel);
                        }
                        else
                        {
                            WrongPred.Add(nextCandel);
                        }
                        //else if (WPRed.Any(c => c.Equals(nextCandel)))
                        //{
                        //    WrongPred.Add(nextCandel);
                        //}


                    iterationCount++;
                    }

            }



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






            //foreach (List<Candel> mainList in extractedListWrongPredictions)
            //{
            //    Candel firstCandel = mainList[0];
            //    Candel secondCandel = mainList[1];

            //    decimal NoofStocks = 200000 / firstCandel.EndPrice;
            //    //decimal NoofStocks = 50000 / firstCandel.EndPrice;
            //    //decimal NoofStocks = 3391 / firstCandel.EndPrice;

            //    MainLoss = MainLoss + (NoofStocks * (firstCandel.EndPrice - secondCandel.EndPrice)) + 117;

            //}

            //MainProfit = extractedListCorrectPredictions.Count * ConstForProfit;


            MainProfit = CorrectPred.Count * ConstForProfit;


            List<List<Candel>> WrongTotal = new List<List<Candel>>();

            foreach (Candel testCandel in WrongPred)
            {

                foreach (List<Candel> mainList in extractedListWrongPredictions)
                {
                    if(testCandel == mainList[0])
                    {
                        WrongTotal.Add(mainList);
                    }
                }

            }


            foreach (List<Candel> mainList in WrongTotal)
            {
                Candel firstCandel = mainList[0];
                Candel secondCandel = mainList[1];

                decimal NoofStocks = 200000 / firstCandel.EndPrice;
                //decimal NoofStocks = 50000 / firstCandel.EndPrice;
                //decimal NoofStocks = 3391 / firstCandel.EndPrice;

                MainLoss = MainLoss + (NoofStocks * (firstCandel.EndPrice - secondCandel.EndPrice)) + 117;

            }

            return Ok(new
            {
                Net = MainProfit - MainLoss,
                //MainProfit,
                //MainLoss,
                CorrectPred,
                WrongPred,
                extractedListCorrectPredictions,
                extractedListWrongPredictions,
                MainMasterList
            });

        }


        // Function to identify Dragonfly Doji candles

        private List<Candel> IdentifyInvertedHammerCandles(List<Candel> CandelData, List<Candel> CandelDataPreviousDay)
        {
            ConcurrentBag<Candel> dragonFlyDojiCandles = new ConcurrentBag<Candel>();

            //// Convert ConcurrentBag to a list, if needed
            List<Candel> result = dragonFlyDojiCandles.ToList();

            foreach (Candel testCandel in CandelData)
            {

                if (testCandel.OpenTime.TimeOfDay > new TimeSpan(11, 00, 0))
                {
                    break; // Skip the rest of this iteration and proceed to the next object
                }


                var diff = 5;



                /// INVERTED HAMMER


                // Define thresholds as a percentage (adjust based on sensitivity)
                const decimal bodyToWickRatio = 0.3m; // Body should be small relative to the wicks
                const decimal upperWickLengthRatio = 2m; // Upper wick should be at least twice the body
                const decimal lowerWickLimit = 0.2m; // Lower wick should be small/negligible
                decimal bodySize = Math.Abs(testCandel.EndPrice - testCandel.StartPrice);
                // Calculate body size, wick sizes, and ratios
                decimal IHbodySize = Math.Abs(testCandel.EndPrice - testCandel.StartPrice);
                decimal upperWickSize = testCandel.HighestPrice - Math.Max(testCandel.StartPrice, testCandel.EndPrice);
                decimal lowerWickSize = Math.Min(testCandel.StartPrice, testCandel.EndPrice) - testCandel.LowestPrice;

                decimal totalRange = testCandel.HighestPrice - testCandel.LowestPrice;

                // Ensure totalRange is not zero to avoid divide by zero exception
                decimal bodyRatio = totalRange != 0 ? bodySize / totalRange : 0;

                // Validate conditions for Inverted Hammer
                bool smallBody = bodyRatio <= bodyToWickRatio;
                bool longUpperWick = upperWickSize >= bodySize * upperWickLengthRatio;
                bool minimalLowerWick = lowerWickSize <= (totalRange * lowerWickLimit);


                bool IHisFit = false;

                Candel IHPreviousDayHighestPriceCandel = null;

                if (testCandel != null)
                {
                    IHPreviousDayHighestPriceCandel = CandelDataPreviousDay
                        .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
                        .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
                        .FirstOrDefault();                                                 // Take the first (highest)
                }

                if (IHPreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < IHPreviousDayHighestPriceCandel.HighestPrice)
                {
                    var percentageDifference = ((IHPreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

                    if (percentageDifference > diff)
                    {
                        IHisFit = true; // IHisFit is true
                    }
                }



                if (
                    smallBody
                    && longUpperWick
                    && minimalLowerWick
                    && IHisFit
                    )
                {
                    dragonFlyDojiCandles.Add(testCandel);
                }

            }



            return result;

        }



        private List<Candel> IdentifyBullishEngulfingCandles(List<Candel> CandelData, List<Candel> CandelDataPreviousDay)
        {
            ConcurrentBag<Candel> dragonFlyDojiCandles = new ConcurrentBag<Candel>();

            //// Convert ConcurrentBag to a list, if needed
            List<Candel> result = dragonFlyDojiCandles.ToList();



            foreach (Candel testCandel in CandelData)
            {
                if (testCandel.OpenTime.TimeOfDay > new TimeSpan(11, 00, 0))
                {
                    break; // Skip the rest of this iteration and proceed to the next object
                }


                var diff = 5;

                /// BULLISH ENGULFING
                Candel BFfirst = testCandel;
                Candel BFSecond = CandelData
                    .Where(c => c.OpenTime > BFfirst.OpenTime)
                    .OrderBy(c => c.OpenTime)
                    .FirstOrDefault();

                if (BFfirst != null && BFSecond != null)
                {

                    // Check if the previous candle is bearish
                    bool isPreviousBearish = BFfirst.EndPrice < BFfirst.StartPrice;

                    // Check if the current candle is bullish
                    bool isCurrentBullish = BFSecond.EndPrice > BFSecond.StartPrice;

                    // Check if the current candle's body engulfs the previous candle's body
                    bool isEngulfingBody =
                        BFSecond.StartPrice < BFfirst.EndPrice && // Current start below previous end
                        BFSecond.EndPrice > BFfirst.StartPrice;  // Current end above previous start

                    bool BFisFit = false;

                    Candel BFPreviousDayHighestPriceCandel = null;

                    if (testCandel != null)
                    {
                        BFPreviousDayHighestPriceCandel = CandelDataPreviousDay
                            .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
                            .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
                            .FirstOrDefault();                                                 // Take the first (highest)
                    }

                    if (BFPreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < BFPreviousDayHighestPriceCandel.HighestPrice)
                    {
                        var percentageDifference = ((BFPreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

                        if (percentageDifference > diff)
                        {
                            BFisFit = true; // IHisFit is true
                        }
                    }

                    if (
                     isPreviousBearish
                     && isCurrentBullish
                     && isEngulfingBody
                     && BFisFit
                     )
                    {
                        dragonFlyDojiCandles.Add(testCandel);
                    }
                }


            }





            return result;

        }




        private List<Candel> IdentifyDragonflyDojiCandles(List<Candel> CandelData , List<Candel> CandelDataPreviousDay)
        {

            //List<Candel> dragonFlyDojiCandles = new List<Candel>();

            //foreach (Candel c in CandelData)
            //{
            //    // Get the last (most recent) candle
            //    Candel latestCandel = c;

            //    Candel previousCandel = CandelData
            //                .Where(c => c.CloseTime < latestCandel.CloseTime)
            //                .OrderByDescending(c => c.CloseTime) // Ensures we get the closest one before recentCandel
            //                .FirstOrDefault();


            //    // Calculate the body size
            //    decimal bodySize = Math.Abs(latestCandel.EndPrice - latestCandel.StartPrice);

            //    // Calculate the lower shadow size
            //    decimal lowerShadowSize = latestCandel.StartPrice - latestCandel.LowestPrice;

            //    // Calculate the upper shadow size
            //    decimal upperShadowSize = latestCandel.HighestPrice - latestCandel.EndPrice;

            //    // Check if the candle is bullish
            //    bool isBullish = latestCandel.IsBullish == true &&
            //                     (latestCandel.EndPrice > (latestCandel.LowestPrice + latestCandel.HighestPrice) / 2);

            //    // Check if the body is small relative to the range
            //    bool smallBody = bodySize < (latestCandel.HighestPrice - latestCandel.LowestPrice) * 0.2m;

            //    // Check if the lower shadow is significantly longer than the body
            //    bool longLowerShadow = lowerShadowSize > (latestCandel.HighestPrice - latestCandel.LowestPrice) * 0.5m;

            //    // Check if the upper shadow is very small
            //    bool smallUpperShadow = upperShadowSize < bodySize * 0.1m;

            //    bool highVolume = false;

            //    if (previousCandel != null)
            //    {
            //        decimal previousVolume = previousCandel.Volume;
            //        // Optional: Add a volume check for additional confirmation (if volume data is available)
            //        highVolume = latestCandel.Volume > (previousVolume * 1.5m); // Modify this based on available data
            //    }

            //    Candel verificationCandel = CandelData
            //                       .Where(c => c.CloseTime > latestCandel.CloseTime)
            //                       .OrderBy(c => c.CloseTime) // Ensures we get the closest one
            //                       .FirstOrDefault();

            //    bool nextcandelbullish = false;

            //    if (verificationCandel != null)
            //    {
            //        if (verificationCandel.IsBullish == true)
            //        {
            //            nextcandelbullish = true;
            //        }
            //    }

            //    // Final check
            //    if (isBullish && smallBody && longLowerShadow && smallUpperShadow && highVolume && nextcandelbullish)
            //    {
            //        if (verificationCandel.OpenTime.TimeOfDay < new TimeSpan(11, 00, 0))
            //        {
            //            dragonFlyDojiCandles.Add(verificationCandel);
            //        }
            //    }


            //    //// Fetch the last and previous candles
            //    //Candel latestCandel = c;
            //    //Candel previousCandel = CandelData
            //    //    .Where(c => c.CloseTime < latestCandel.CloseTime)
            //    //    .OrderByDescending(c => c.CloseTime)
            //    //    .FirstOrDefault();

            //    //Candel verificationCandel = CandelData
            //    //                   .Where(c => c.CloseTime > latestCandel.CloseTime)
            //    //                   .OrderBy(c => c.CloseTime) // Ensures we get the closest one
            //    //                   .FirstOrDefault();

            //    //// Calculate key metrics
            //    //decimal bodySize = Math.Abs(latestCandel.EndPrice - latestCandel.StartPrice);
            //    //decimal range = latestCandel.HighestPrice - latestCandel.LowestPrice;
            //    //decimal lowerShadowSize = latestCandel.StartPrice - latestCandel.LowestPrice;
            //    //decimal upperShadowSize = latestCandel.HighestPrice - latestCandel.EndPrice;

            //    //// Define thresholds
            //    //bool isBullish = latestCandel.IsBullish == true &&
            //    //                 latestCandel.EndPrice > (latestCandel.LowestPrice + range / 2);
            //    //bool smallBody = bodySize < (range * 0.15m); // Tweaked threshold
            //    //bool longLowerShadow = lowerShadowSize > (range * 0.6m); // Increased relative size
            //    //bool smallUpperShadow = upperShadowSize < bodySize * 0.1m;

            //    //// High volume check
            //    //bool highVolume = false;
            //    //if (previousCandel != null)
            //    //{
            //    //    highVolume = latestCandel.Volume > (previousCandel.Volume * 1.7m); // Increased factor
            //    //}

            //    //// Price change confirmation
            //    //bool significantPriceChange = latestCandel.PriceChangePercentage > 0.005m; // Ensure meaningful move

            //    //bool nextcandelbullish = false;

            //    //if (verificationCandel != null)
            //    //{
            //    //    if (verificationCandel.IsBullish == true)
            //    //    {
            //    //        nextcandelbullish = true;
            //    //    }
            //    //}

            //    //// Combine conditions
            //    //if (isBullish
            //    //    && smallBody
            //    //    && longLowerShadow
            //    //    && smallUpperShadow
            //    //    && highVolume
            //    //    && significantPriceChange
            //    //    && nextcandelbullish
            //    //    )
            //    //{
            //    //    if (verificationCandel.OpenTime.TimeOfDay < new TimeSpan(11, 00, 0))
            //    //    {
            //    //        dragonFlyDojiCandles.Add(verificationCandel);
            //    //    }

            //    //}

            //}

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //foreach (Candel c in CandelData)
            //{
            //    // Fetch the last and previous candles
            //    Candel latestCandel = c;
            //    Candel previousCandel = CandelData
            //        .Where(c => c.CloseTime < latestCandel.CloseTime)
            //        .OrderByDescending(c => c.CloseTime)
            //        .FirstOrDefault();

            //    Candel verificationCandel = CandelData
            //                       .Where(c => c.CloseTime > latestCandel.CloseTime)
            //                       .OrderBy(c => c.CloseTime) // Ensures we get the closest one
            //                       .FirstOrDefault();

            //    // Calculate key metrics
            //    decimal bodySize = Math.Abs(latestCandel.EndPrice - latestCandel.StartPrice);
            //    decimal range = latestCandel.HighestPrice - latestCandel.LowestPrice;
            //    decimal lowerShadowSize = latestCandel.StartPrice - latestCandel.LowestPrice;
            //    decimal upperShadowSize = latestCandel.HighestPrice - latestCandel.EndPrice;

            //    // Define thresholds
            //    bool isBullish = latestCandel.IsBullish == true &&
            //                     latestCandel.EndPrice > (latestCandel.LowestPrice + range / 2);
            //    bool smallBody = bodySize < (range * 0.15m); // Tweaked threshold
            //    bool longLowerShadow = lowerShadowSize > (range * 0.6m); // Increased relative size
            //    bool smallUpperShadow = upperShadowSize < bodySize * 0.1m;

            //    // High volume check
            //    bool highVolume = false;
            //    if (previousCandel != null)
            //    {
            //        highVolume = latestCandel.Volume > (previousCandel.Volume * 1.7m); // Increased factor
            //    }

            //    // Price change confirmation
            //    bool significantPriceChange = latestCandel.PriceChangePercentage > 0.005m; // Ensure meaningful move

            //    bool nextcandelbullish = false;

            //    if (verificationCandel != null)
            //    {
            //        if (verificationCandel.IsBullish == true)
            //        {
            //            nextcandelbullish = true;
            //        }
            //    }

            //    // Combine conditions
            //    if (isBullish
            //        && smallBody
            //        && longLowerShadow
            //        && smallUpperShadow
            //        && highVolume
            //        && significantPriceChange
            //        && nextcandelbullish
            //        )
            //    {
            //        if (verificationCandel.OpenTime.TimeOfDay < new TimeSpan(10, 30, 0))
            //        {
            //            dragonFlyDojiCandles.Add(verificationCandel);
            //        }

            //    }
            //}




            //foreach (Candel c in CandelData)
            //{
            //        // Fetch the current, previous, and verification candles
            //        Candel latestCandel = c;
            //        Candel previousCandel = CandelData
            //            .Where(c => c.CloseTime < latestCandel.CloseTime)
            //            .OrderByDescending(c => c.CloseTime)
            //            .FirstOrDefault();

            //        Candel verificationCandel = CandelData
            //            .Where(c => c.CloseTime > latestCandel.CloseTime)
            //            .OrderBy(c => c.CloseTime)
            //            .FirstOrDefault();


            //        // Calculate key metrics
            //        decimal bodySize = Math.Abs(latestCandel.EndPrice - latestCandel.StartPrice);
            //        decimal range = latestCandel.HighestPrice - latestCandel.LowestPrice;
            //        decimal lowerShadowSize = Math.Min(latestCandel.StartPrice, latestCandel.EndPrice) - latestCandel.LowestPrice;
            //        decimal upperShadowSize = latestCandel.HighestPrice - Math.Max(latestCandel.StartPrice, latestCandel.EndPrice);

            //        // Define thresholds for dragonfly doji
            //        bool isDoji = bodySize <= (range * 0.05m); // Body is less than 5% of the range
            //        bool longLowerShadow = lowerShadowSize >= (range * 0.7m); // Lower shadow is 70% or more of the range
            //        bool minimalUpperShadow = upperShadowSize <= (range * 0.1m); // Upper shadow is 10% or less of the range

            //        // Confirm bullish context
            //        bool bullishContext = previousCandel != null && previousCandel.EndPrice > previousCandel.StartPrice; // Prior candle bullish

            //        // High volume check
            //        bool highVolume = false;
            //        if (previousCandel != null)
            //        {
            //            highVolume = latestCandel.Volume > (previousCandel.Volume * 1.5m); // Increased activity
            //        }

            //        // Price change confirmation
            //        bool nextCandleBullish = false;
            //        if (verificationCandel != null && verificationCandel.IsBullish.HasValue)
            //        {
            //            nextCandleBullish = verificationCandel.IsBullish.Value;
            //        }

            //        // Combine conditions for dragonfly doji
            //        if (
            //        isDoji
            //        && longLowerShadow
            //        && minimalUpperShadow
            //        && bullishContext
            //        && highVolume
            //        && nextCandleBullish
            //        )
            //        {
            //        if (verificationCandel != null && verificationCandel.OpenTime.TimeOfDay < new TimeSpan(11, 00, 0))
            //        {
            //            dragonFlyDojiCandles.Add(verificationCandel);
            //        }
            //}


            //}

            //foreach (Candel c in CandelData)
            //{
            //    // Fetch the current candle
            //    Candel latestCandel = c;

            //    // Fetch the previous and verification candles
            //    Candel previousCandel = CandelData
            //        .Where(c => c.CloseTime < latestCandel.CloseTime)
            //        .OrderByDescending(c => c.CloseTime)
            //        .FirstOrDefault();

            //    Candel verificationCandel = CandelData
            //        .Where(c => c.CloseTime > latestCandel.CloseTime)
            //        .OrderBy(c => c.CloseTime)
            //        .FirstOrDefault();

            //    // Calculate key metrics
            //    decimal bodySize = Math.Abs(latestCandel.EndPrice - latestCandel.StartPrice);
            //    decimal range = latestCandel.HighestPrice - latestCandel.LowestPrice;
            //    decimal lowerShadowSize = Math.Abs(latestCandel.StartPrice - latestCandel.LowestPrice);
            //    decimal upperShadowSize = Math.Abs(latestCandel.HighestPrice - latestCandel.EndPrice);

            //    // Define thresholds for dragonfly doji
            //    bool isDoji = bodySize < (range * 0.05m); // Very small or no body
            //    bool longLowerShadow = lowerShadowSize > (range * 0.7m); // Lower shadow occupies most of the range
            //    bool noUpperShadow = upperShadowSize < (range * 0.05m); // Minimal or no upper shadow

            //    // Check if it occurs at the bottom of a downtrend
            //    bool isAtDowntrend = previousCandel != null && latestCandel.LowestPrice < previousCandel.LowestPrice;

            //    // High volume confirmation
            //    bool highVolume = previousCandel != null && latestCandel.Volume > (previousCandel.Volume * 1.5m);

            //    // Price change confirmation
            //    bool significantPriceChange = latestCandel.PriceChangePercentage > 0.005m;

            //    // Verify if the next candle is bullish
            //    bool nextCandelBullish = verificationCandel != null && verificationCandel.IsBullish == true;

            //    // Combine conditions for dragonfly doji detection
            //    if (
            //        isDoji
            //        && longLowerShadow
            //        && noUpperShadow
            //        && isAtDowntrend
            //        && highVolume
            //        && significantPriceChange
            //        && nextCandelBullish
            //        )
            //    {
            //        if (verificationCandel != null && verificationCandel.OpenTime.TimeOfDay < new TimeSpan(11, 00, 0))
            //        {
            //            dragonFlyDojiCandles.Add(verificationCandel);
            //        }
            //    }
            //}




            //List<List<Candel>> MasterCandelList = new List<List<Candel>>();

            //for (int i = 0; i < CandelData.Count - 1; i++)
            //{
            //    List<Candel> sublist = new List<Candel>
            //    {
            //        CandelData[i],
            //        CandelData[i + 1]
            //    };

            //    MasterCandelList.Add(sublist);
            //}

            //foreach (List<Candel> candelList in MasterCandelList)
            //{
            //    if (candelList.Count == 2)
            //    {
            //        Candel firstCandle = candelList[0];
            //        Candel secondCandle = candelList[1];

            //        Candel verificationCandel = CandelData
            //                           .Where(c => c.CloseTime > secondCandle.CloseTime)
            //                           .OrderBy(c => c.CloseTime) // Ensures we get the closest one
            //                           .FirstOrDefault();

            //        // Check for Bullish Harami pattern
            //        bool isBearishFirstCandle = firstCandle.EndPrice < firstCandle.StartPrice; // First candle must be bearish (red)
            //        bool isBullishSecondCandle = secondCandle.EndPrice > secondCandle.StartPrice; // Second candle must be bullish (green)

            //        bool isContainedBody = (secondCandle.StartPrice > firstCandle.EndPrice) && (secondCandle.EndPrice < firstCandle.StartPrice); // Bullish candle inside the first candle's body

            //        bool isVerificationCandelBullish = false;

            //        if(verificationCandel != null)
            //        {
            //            if (verificationCandel.IsBullish.HasValue)
            //            {
            //                isVerificationCandelBullish = true;
            //            }
            //        }


            //        if (isBearishFirstCandle && isBullishSecondCandle && isContainedBody)
            //        {
            //            if (verificationCandel != null && verificationCandel.OpenTime.TimeOfDay < new TimeSpan(11, 00, 0))
            //            {
            //                dragonFlyDojiCandles.Add(verificationCandel);
            //            }
            //        }
            //    }
            //}



            ///////////////////////////////////////////////////////////////////////////////////////////////////////////










            //List<List<Candel>> MasterCandelList = new List<List<Candel>>();

            //for (int i = 0; i < CandelData.Count - 2; i++)
            //{
            //    List<Candel> sublist = new List<Candel>
            //    {
            //        CandelData[i],
            //        CandelData[i + 1],
            //        CandelData[i + 2]
            //    };

            //    MasterCandelList.Add(sublist);
            //}



            //List<Candel> dragonFlyDojiCandles = new List<Candel>();

            //foreach (List<Candel> candelList in MasterCandelList)
            //{

            //    // Get the last 3 candles from the list (the most recent 3)
            //    List<Candel> recentThreeCandles = candelList.OrderByDescending(c => c.CloseTime).Take(3).ToList();

            //    Candel latestCandel = recentThreeCandles.FirstOrDefault();

            //    // Check if all three candles are bullish
            //    bool allThreeBullish = recentThreeCandles.All(c => c.IsBullish == true);

            //    // Check if the three candles close higher than the previous one
            //    bool progressiveCloses = recentThreeCandles[0].EndPrice > recentThreeCandles[1].EndPrice
            //                                                            &&
            //                             recentThreeCandles[1].EndPrice > recentThreeCandles[2].EndPrice;

            //    // Check if the bodies of the candles are progressively larger
            //    bool increasingBodySize = (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice) > (recentThreeCandles[1].EndPrice - recentThreeCandles[1].StartPrice)
            //                                                                                                  &&
            //                              (recentThreeCandles[1].EndPrice - recentThreeCandles[1].StartPrice) > (recentThreeCandles[2].EndPrice - recentThreeCandles[2].StartPrice);

            //    // Check for small upper and lower shadows
            //    bool smallUpperShadow = (recentThreeCandles[0].HighestPrice - recentThreeCandles[0].EndPrice) < (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice);
            //    bool smallLowerShadow = (recentThreeCandles[0].StartPrice - recentThreeCandles[0].LowestPrice) < (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice);

            //    // Check if the body is at least 60% of the total range (strong body)
            //    decimal range = recentThreeCandles[0].HighestPrice - recentThreeCandles[0].LowestPrice;
            //    bool strongBodyRatio = range != 0 && (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice) / range > 0.6m;

            //    bool increasingVolume = recentThreeCandles[0].Volume > recentThreeCandles[1].Volume &&
            //            recentThreeCandles[1].Volume > recentThreeCandles[2].Volume;

            //    // Get the Candel where CloseTime is greater than latestCandel's CloseTime
            //    Candel verificationCandel = CandelData
            //        .Where(c => c.CloseTime > latestCandel.CloseTime)
            //        .OrderBy(c => c.CloseTime) // Ensures we get the closest one
            //        .FirstOrDefault();

            //    bool precedingCandelBullish = false;

            //    if(verificationCandel != null)
            //    {
            //        if (verificationCandel.IsBullish == true)
            //        {
            //            precedingCandelBullish = true;
            //        }
            //    }


            //    if (
            //        allThreeBullish
            //        && progressiveCloses
            //        && increasingBodySize
            //        && smallUpperShadow
            //        && smallLowerShadow
            //        && strongBodyRatio
            //        && increasingVolume
            //        && precedingCandelBullish
            //        )
            //    {
            //        dragonFlyDojiCandles.Add(verificationCandel);
            //    }



            //List<Candel> dragonFlyDojiCandles = new List<Candel>();

            //foreach (Candel c in CandelData)
            //{
            //    Candel recentCandel = c;

            //    Candel verificationCandel = CandelData
            //                       .Where(c => c.CloseTime > recentCandel.CloseTime)
            //                       .OrderBy(c => c.CloseTime) // Ensures we get the closest one
            //                       .FirstOrDefault();

            //    Candel previousCandel = CandelData
            //                .Where(c => c.CloseTime < recentCandel.CloseTime)
            //                .OrderByDescending(c => c.CloseTime) // Ensures we get the closest one before recentCandel
            //                .FirstOrDefault();

            //    //List<Candel> futureCandels = CandelData
            //    //              .Where(c => c.CloseTime > recentCandel.CloseTime)
            //    //              .OrderBy(c => c.CloseTime)
            //    //              .Take(3)
            //    //              .ToList();


            //    //Candel v1 = futureCandels[0];
            //    //Candel v2 = futureCandels[1];
            //    //Candel verificationCandel = futureCandels[1];

            //    // Check if it is a Doji with a small body
            //    bool isDoji = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;

            //    // Check for a long lower shadow (shadow size relative to the body)
            //    bool longLowerShadow = (recentCandel.StartPrice - recentCandel.LowestPrice) > 3 * (recentCandel.EndPrice - recentCandel.StartPrice);

            //    // The body of the candle should be small and at the top of the range
            //    bool smallBodyAtTop = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.3m;

            //    //// Preceding candle's trend should be bullish (for confirming upward momentum)
            //    ////bool precedingBullishTrend = CandelData.Where(x => x.CloseTime < recentCandel.OpenTime)
            //    ////                                       .OrderByDescending(x => x.CloseTime)
            //    ////                                       .Take(3)
            //    ////                                       .All(x => x.EndPrice > x.StartPrice); // At least the last 3 candles should be bullish


            //    //// Check for higher volume
            //    //bool higherVolume = recentCandel.Volume > CandelData.Average(x => x.Volume);

            //    //bool higherVolume = recentCandel.Volume > CandelData.TakeLast(10).Max(x => x.Volume) * 0.75m; // Volume above 75% of the max in last 10 candles
            //    bool higherVolume2 = recentCandel.Volume > CandelData.Average(x => x.Volume);


            //    // Find the index of the recentCandel
            //    int recentCandelIndex = CandelData.IndexOf(recentCandel);

            //    // Get the 10 candles immediately before the recentCandel
            //    var last10CandelsBefore = CandelData.Skip(recentCandelIndex - 10).Take(10);

            //    // Determine if the volume of recentCandel is higher than 75% of the max volume from the previous 10 candles
            //    bool higherVolume = recentCandel.Volume > last10CandelsBefore.Max(x => x.Volume) * 0.75m;


            //    ////The next candle should also be bullish for confirmation

            //    bool nextCandleBullish = false;

            //    if (verificationCandel != null)
            //    {
            //        if (verificationCandel.IsBullish == true)
            //        {
            //            nextCandleBullish = true;
            //        }
            //    }

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
            //        //&& higherVolume
            //        && higherVolume2
            //        && nextCandleBullish
            //        && previousCandelBearish
            //        )
            //    {
            //        if (verificationCandel.OpenTime.TimeOfDay < new TimeSpan(11, 00, 0))
            //        {
            //            dragonFlyDojiCandles.Add(recentCandel);
            //        }
            //    }

            //}



            //ConcurrentBag<Candel> dragonFlyDojiCandles = new ConcurrentBag<Candel>();

            //// Create and execute tasks for parallel processing
            //List<Task> tasks = new List<Task>();

            //foreach (Candel latestCandel in CandelData)
            //{
            //    tasks.Add(Task.Run(() =>
            //    {
            //        // Fetch the previous and next candles
            //        Candel previousCandel = CandelData
            //            .Where(c => c.CloseTime < latestCandel.CloseTime)
            //            .OrderByDescending(c => c.CloseTime)
            //            .FirstOrDefault();

            //        Candel verificationCandel = CandelData
            //            .Where(c => c.CloseTime > latestCandel.CloseTime)
            //            .OrderBy(c => c.CloseTime)
            //            .FirstOrDefault();

            //        // Calculate key metrics
            //        decimal bodySize = Math.Abs(latestCandel.EndPrice - latestCandel.StartPrice);
            //        decimal range = latestCandel.HighestPrice - latestCandel.LowestPrice;
            //        decimal lowerShadowSize = latestCandel.StartPrice - latestCandel.LowestPrice;
            //        decimal upperShadowSize = latestCandel.HighestPrice - latestCandel.EndPrice;

            //        // Define thresholds
            //        bool isBullish = latestCandel.IsBullish == true &&
            //                         latestCandel.EndPrice > (latestCandel.LowestPrice + range / 2);
            //        bool smallBody = bodySize < (range * 0.15m);
            //        bool longLowerShadow = lowerShadowSize > (range * 0.6m);
            //        bool smallUpperShadow = upperShadowSize < bodySize * 0.1m;

            //        // High volume check
            //        bool highVolume = false;
            //        if (previousCandel != null)
            //        {
            //            highVolume = latestCandel.Volume > (previousCandel.Volume * 1.7m);
            //        }

            //        // Price change confirmation
            //        bool significantPriceChange = latestCandel.PriceChangePercentage > 0.005m;

            //        bool nextCandelBullish = verificationCandel?.IsBullish == true;

            //        // Combine conditions
            //        if (isBullish
            //            && smallBody
            //            && longLowerShadow
            //            && smallUpperShadow
            //            && highVolume
            //            && significantPriceChange
            //            && nextCandelBullish)
            //        {
            //            if (verificationCandel?.OpenTime.TimeOfDay < new TimeSpan(10, 30, 0))
            //            {
            //                dragonFlyDojiCandles.Add(verificationCandel);
            //            }
            //        }
            //    }));
            //}

            //// Wait for all tasks to complete
            //Task.WaitAll(tasks.ToArray());





            ConcurrentBag<Candel> dragonFlyDojiCandles = new ConcurrentBag<Candel>();


            //List<List<Candel>> MasterCandelList = new List<List<Candel>>();

            //for (int i = 0; i < CandelData.Count - 1; i++)
            //{
            //    List<Candel> sublist = new List<Candel>
            //    {
            //        CandelData[i],
            //        CandelData[i + 1]
            //    };

            //    MasterCandelList.Add(sublist);
            //}


            //// Create and execute tasks for parallel processing
            //List<Task> tasks = new List<Task>();

            //foreach (List<Candel> candelList in MasterCandelList)
            //{
            //    tasks.Add(Task.Run(() =>
            //    {
            //        if (candelList.Count == 2)
            //        {
            //            Candel firstCandle = candelList[0];
            //            Candel secondCandle = candelList[1];

            //            Candel verificationCandel = CandelData
            //                               .Where(c => c.CloseTime > secondCandle.CloseTime)
            //                               .OrderBy(c => c.CloseTime) // Ensures we get the closest one
            //                               .FirstOrDefault();

            //            // Check for Bullish Harami pattern
            //            bool isBearishFirstCandle = firstCandle.EndPrice < firstCandle.StartPrice; // First candle must be bearish (red)
            //            bool isBullishSecondCandle = secondCandle.EndPrice > secondCandle.StartPrice; // Second candle must be bullish (green)

            //            bool isContainedBody = (secondCandle.StartPrice > firstCandle.EndPrice) && (secondCandle.EndPrice < firstCandle.StartPrice); // Bullish candle inside the first candle's body

            //            bool isVerificationCandelBullish = false;

            //            if (verificationCandel != null)
            //            {
            //                if (verificationCandel.IsBullish.HasValue)
            //                {
            //                    isVerificationCandelBullish = true;
            //                }
            //            }

            //            bool IsVerficationCandelEndPriceGreater = false;
            //            if (verificationCandel != null)
            //            {
            //                if (verificationCandel.EndPrice > firstCandle.EndPrice)
            //                {
            //                    IsVerficationCandelEndPriceGreater = true;
            //                }
            //            }

            //            // High volume check
            //            bool highVolume = false;
            //            if (secondCandle != null && verificationCandel != null)
            //            {
            //                highVolume = verificationCandel.Volume > (secondCandle.Volume * 1.7m);
            //            }

            //            Candel PreviousDayHighestPriceCandel = null;

            //            if(secondCandle != null)
            //            {
            //              PreviousDayHighestPriceCandel = CandelDataPreviousDay
            //                    .Where(c => c.OpenTime.TimeOfDay > secondCandle.OpenTime.TimeOfDay) // Compare only the time
            //                    .OrderByDescending(c => c.HighestPrice)                             // Order by HighestPrice descending
            //                    .FirstOrDefault();                                                  // Take the first (highest)
            //            }

            //            bool isFit = false;

            //            if (PreviousDayHighestPriceCandel != null && secondCandle != null && secondCandle.EndPrice < PreviousDayHighestPriceCandel.HighestPrice)
            //            {
            //                var percentageDifference = ((PreviousDayHighestPriceCandel.HighestPrice - secondCandle.EndPrice) / secondCandle.EndPrice) * 100;

            //                if (percentageDifference > 10)
            //                {
            //                    isFit = true;
            //                }
            //            }


            //            if (
            //            isBearishFirstCandle
            //            && isBullishSecondCandle
            //            && isContainedBody
            //            //&& highVolume
            //            && isFit
            //            //&& isVerificationCandelBullish
            //            //&& IsVerficationCandelEndPriceGreater
            //            )
            //            {
            //                //if (
            //                //verificationCandel != null
            //                //&& verificationCandel.OpenTime.TimeOfDay < new TimeSpan(11, 00, 0)
            //                //                                                                   )
            //                //{
            //                //    dragonFlyDojiCandles.Add(verificationCandel);
            //                //}

            //                if (
            //                secondCandle != null
            //                //&& secondCandle.OpenTime.TimeOfDay < new TimeSpan(11, 00, 0)
            //                                                       )
            //                {
            //                    dragonFlyDojiCandles.Add(secondCandle);
            //                }
            //            }
            //        }
            //    }));
            //}

            //// Wait for all tasks to complete
            //Task.WaitAll(tasks.ToArray());

            //// Convert ConcurrentBag to a list, if needed
            //List<Candel> result = dragonFlyDojiCandles.ToList();


            //foreach (Candel testCandel in CandelData)
            //{
            //    var diff = 5;

            //    /// DRAGON FLY DOJI

            //    // Tolerance level for considering "nearly equal" start and end prices
            //    decimal tolerance = 0.01m;

            //    // Check for price equality
            //    bool isPriceEqual = Math.Abs(testCandel.StartPrice - testCandel.EndPrice) <= tolerance;

            //    // Check for long lower shadow: Lowest price should be much lower than Start and End prices
            //    bool hasLongLowerShadow = testCandel.LowestPrice < Math.Min(testCandel.StartPrice, testCandel.EndPrice) - (Math.Max(testCandel.StartPrice, testCandel.EndPrice) - Math.Min(testCandel.StartPrice, testCandel.EndPrice)) * 2;

            //    // Check for small upper shadow: Highest price should be at or near Start/End price
            //    bool hasSmallUpperShadow = Math.Abs(testCandel.HighestPrice - testCandel.StartPrice) <= tolerance && Math.Abs(testCandel.HighestPrice - testCandel.EndPrice) <= tolerance;

            //    bool isFit = false;


            //    Candel PreviousDayHighestPriceCandel = null;

            //    if (testCandel != null)
            //    {
            //        PreviousDayHighestPriceCandel = CandelDataPreviousDay
            //            .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
            //            .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
            //            .FirstOrDefault();                                                 // Take the first (highest)
            //    }

            //    if (PreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < PreviousDayHighestPriceCandel.HighestPrice)
            //    {
            //        var percentageDifference = ((PreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

            //        if (percentageDifference > diff)
            //        {
            //            isFit = true; // IHisFit is true
            //        }
            //    }



            //    if (
            //        isPriceEqual == true
            //        && hasLongLowerShadow == true
            //        && hasSmallUpperShadow == true
            //        && isFit == true
            //      )
            //    {
            //        dragonFlyDojiCandles.Add(testCandel);
            //    }






            //    /// HAMMER


            //    // Calculate body size and shadows
            //    decimal bodySize = Math.Abs(testCandel.EndPrice - testCandel.StartPrice);
            //    decimal lowerShadow = Math.Abs(testCandel.StartPrice - testCandel.LowestPrice < testCandel.EndPrice - testCandel.LowestPrice ? testCandel.StartPrice - testCandel.LowestPrice : testCandel.EndPrice - testCandel.LowestPrice);
            //    decimal upperShadow = testCandel.HighestPrice - Math.Max(testCandel.StartPrice, testCandel.EndPrice);

            //    // Define thresholds (can be adjusted based on dataset and market conditions)
            //    decimal shadowToBodyRatio = 2.5m; // Lower shadow must be at least 2.5x body size
            //    decimal bodyToRangeRatio = 0.25m; // Body must be within 25% of the price range
            //    decimal minBodySize = 0.01m; // Avoid false positives for tiny candles

            //    // Check Hammer pattern conditions
            //    bool HammerhasLongLowerShadow = lowerShadow >= (bodySize * shadowToBodyRatio);
            //    bool HammerhasSmallUpperShadow = upperShadow < (bodySize * 0.5m);
            //    bool hasSmallBody = bodySize > 0 && bodySize / (testCandel.HighestPrice - testCandel.LowestPrice) <= bodyToRangeRatio;
            //    bool isHammer = hasLongLowerShadow && hasSmallUpperShadow && hasSmallBody && testCandel.StartPrice > testCandel.LowestPrice;

            //    bool HammerisFit = false;

            //    Candel HammerPreviousDayHighestPriceCandel = null;

            //    if (testCandel != null)
            //    {
            //        HammerPreviousDayHighestPriceCandel = CandelDataPreviousDay
            //            .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
            //            .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
            //            .FirstOrDefault();                                                 // Take the first (highest)
            //    }

            //    if (HammerPreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < HammerPreviousDayHighestPriceCandel.HighestPrice)
            //    {
            //        var percentageDifference = ((HammerPreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

            //        if (percentageDifference > diff)
            //        {
            //            HammerisFit = true; // IHisFit is true
            //        }
            //    }


            //    if (
            //        HammerhasLongLowerShadow
            //        && HammerhasSmallUpperShadow
            //        && hasSmallBody
            //        && isHammer
            //        && HammerisFit
            //        )
            //    {
            //        dragonFlyDojiCandles.Add(testCandel);
            //    }








            //    /// INVERTED HAMMER


            //    // Define thresholds as a percentage (adjust based on sensitivity)
            //    const decimal bodyToWickRatio = 0.3m; // Body should be small relative to the wicks
            //    const decimal upperWickLengthRatio = 2m; // Upper wick should be at least twice the body
            //    const decimal lowerWickLimit = 0.2m; // Lower wick should be small/negligible

            //    // Calculate body size, wick sizes, and ratios
            //    decimal IHbodySize = Math.Abs(testCandel.EndPrice - testCandel.StartPrice);
            //    decimal upperWickSize = testCandel.HighestPrice - Math.Max(testCandel.StartPrice, testCandel.EndPrice);
            //    decimal lowerWickSize = Math.Min(testCandel.StartPrice, testCandel.EndPrice) - testCandel.LowestPrice;

            //    // Check if body is small relative to the total candlestick size
            //    decimal totalRange = testCandel.HighestPrice - testCandel.LowestPrice;

            //    decimal bodyRatio = bodySize / totalRange;

            //    // Validate conditions for Inverted Hammer
            //    bool smallBody = bodyRatio <= bodyToWickRatio;
            //    bool longUpperWick = upperWickSize >= bodySize * upperWickLengthRatio;
            //    bool minimalLowerWick = lowerWickSize <= (totalRange * lowerWickLimit);


            //    bool IHisFit = false;

            //    Candel IHPreviousDayHighestPriceCandel = null;

            //    if (testCandel != null)
            //    {
            //        IHPreviousDayHighestPriceCandel = CandelDataPreviousDay
            //            .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
            //            .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
            //            .FirstOrDefault();                                                 // Take the first (highest)
            //    }

            //    if (IHPreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < IHPreviousDayHighestPriceCandel.HighestPrice)
            //    {
            //        var percentageDifference = ((IHPreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

            //        if (percentageDifference > diff)
            //        {
            //            IHisFit = true; // IHisFit is true
            //        }
            //    }



            //    if (
            //        smallBody
            //        && longUpperWick
            //        && minimalLowerWick
            //        && IHisFit
            //        )
            //    {
            //        dragonFlyDojiCandles.Add(testCandel);
            //    }






            //    /// BULLISH ENGULFING
            //    Candel BFfirst = testCandel;
            //    Candel BFSecond = CandelData
            //        .Where(c => c.OpenTime > BFfirst.OpenTime)
            //        .OrderBy(c => c.OpenTime)
            //        .FirstOrDefault();

            //    if(BFfirst != null && BFSecond != null)
            //    {

            //        // Check if the previous candle is bearish
            //        bool isPreviousBearish = BFfirst.EndPrice < BFfirst.StartPrice;

            //        // Check if the current candle is bullish
            //        bool isCurrentBullish = BFSecond.EndPrice > BFSecond.StartPrice;

            //        // Check if the current candle's body engulfs the previous candle's body
            //        bool isEngulfingBody =
            //            BFSecond.StartPrice < BFfirst.EndPrice && // Current start below previous end
            //            BFSecond.EndPrice > BFfirst.StartPrice;  // Current end above previous start

            //        bool BFisFit = false;

            //        Candel BFPreviousDayHighestPriceCandel = null;

            //        if (testCandel != null)
            //        {
            //            BFPreviousDayHighestPriceCandel = CandelDataPreviousDay
            //                .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
            //                .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
            //                .FirstOrDefault();                                                 // Take the first (highest)
            //        }

            //        if (BFPreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < BFPreviousDayHighestPriceCandel.HighestPrice)
            //        {
            //            var percentageDifference = ((BFPreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

            //            if (percentageDifference > diff)
            //            {
            //                BFisFit = true; // IHisFit is true
            //            }
            //        }

            //        if (
            //         isPreviousBearish
            //         && isCurrentBullish
            //         && isEngulfingBody
            //         && BFisFit
            //         )
            //        {
            //            dragonFlyDojiCandles.Add(testCandel);
            //        }





            //        /// TWEEZER BOTTOM
            //        Candel TBfirst = testCandel;
            //        Candel TBSecond = CandelData
            //            .Where(c => c.OpenTime > TBfirst.OpenTime)
            //            .OrderBy(c => c.OpenTime)
            //            .FirstOrDefault();

            //        if(TBfirst != null && TBSecond != null)
            //        {

            //            // Check if the first candle is bearish and the second is bullish
            //            bool isBearishBullish = TBfirst.EndPrice < TBfirst.StartPrice
            //                                    && TBSecond.EndPrice > TBSecond.StartPrice;

            //            // Check if the lowest prices of the candles are nearly equal
            //            bool hasMatchingLows = Math.Abs(TBfirst.LowestPrice - TBSecond.LowestPrice) <= 0.01m;

            //            bool TBisFit = false;

            //            Candel TBPreviousDayHighestPriceCandel = null;

            //            if (testCandel != null)
            //            {
            //                TBPreviousDayHighestPriceCandel = CandelDataPreviousDay
            //                    .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
            //                    .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
            //                    .FirstOrDefault();                                                 // Take the first (highest)
            //            }

            //            if (TBPreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < TBPreviousDayHighestPriceCandel.HighestPrice)
            //            {
            //                var percentageDifference = ((TBPreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

            //                if (percentageDifference > diff)
            //                {
            //                    TBisFit = true; // IHisFit is true
            //                }
            //            }

            //            if (
            //             isBearishBullish
            //             && hasMatchingLows
            //             && TBisFit
            //             )
            //            {
            //                dragonFlyDojiCandles.Add(testCandel);
            //            }

            //        }



            //    }

            //}


            // Create and execute tasks for parallel processing
            List<Task> tasks = new List<Task>();

            //foreach (Candel testCandel in CandelData)
            //{
            //    tasks.Add(Task.Run(() =>
            //    {
            //        var diff = 5;

            //        /// DRAGON FLY DOJI

            //        // Tolerance level for considering "nearly equal" start and end prices
            //        decimal tolerance = 0.01m;

            //        // Check for price equality
            //        bool isPriceEqual = Math.Abs(testCandel.StartPrice - testCandel.EndPrice) <= tolerance;

            //        // Check for long lower shadow: Lowest price should be much lower than Start and End prices
            //        bool hasLongLowerShadow = testCandel.LowestPrice < Math.Min(testCandel.StartPrice, testCandel.EndPrice) - (Math.Max(testCandel.StartPrice, testCandel.EndPrice) - Math.Min(testCandel.StartPrice, testCandel.EndPrice)) * 2;

            //        // Check for small upper shadow: Highest price should be at or near Start/End price
            //        bool hasSmallUpperShadow = Math.Abs(testCandel.HighestPrice - testCandel.StartPrice) <= tolerance && Math.Abs(testCandel.HighestPrice - testCandel.EndPrice) <= tolerance;

            //        bool isFit = false;


            //        Candel PreviousDayHighestPriceCandel = null;

            //        if (testCandel != null)
            //        {
            //            PreviousDayHighestPriceCandel = CandelDataPreviousDay
            //                .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
            //                .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
            //                .FirstOrDefault();                                                 // Take the first (highest)
            //        }

            //        if (PreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < PreviousDayHighestPriceCandel.HighestPrice)
            //        {
            //            var percentageDifference = ((PreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

            //            if (percentageDifference > diff)
            //            {
            //                isFit = true; // IHisFit is true
            //            }
            //        }



            //        if (
            //            isPriceEqual == true
            //            && hasLongLowerShadow == true
            //            && hasSmallUpperShadow == true
            //            && isFit == true
            //          )
            //        {
            //            dragonFlyDojiCandles.Add(testCandel);
            //        }






            //        /// HAMMER


            //        // Calculate body size and shadows
            //        decimal bodySize = Math.Abs(testCandel.EndPrice - testCandel.StartPrice);
            //        decimal lowerShadow = Math.Abs(testCandel.StartPrice - testCandel.LowestPrice < testCandel.EndPrice - testCandel.LowestPrice ? testCandel.StartPrice - testCandel.LowestPrice : testCandel.EndPrice - testCandel.LowestPrice);
            //        decimal upperShadow = testCandel.HighestPrice - Math.Max(testCandel.StartPrice, testCandel.EndPrice);

            //        // Define thresholds (can be adjusted based on dataset and market conditions)
            //        decimal shadowToBodyRatio = 2.5m; // Lower shadow must be at least 2.5x body size
            //        decimal bodyToRangeRatio = 0.25m; // Body must be within 25% of the price range
            //        decimal minBodySize = 0.01m; // Avoid false positives for tiny candles

            //        // Check Hammer pattern conditions
            //        bool HammerhasLongLowerShadow = lowerShadow >= (bodySize * shadowToBodyRatio);
            //        bool HammerhasSmallUpperShadow = upperShadow < (bodySize * 0.5m);
            //        bool hasSmallBody = bodySize > 0 && bodySize / (testCandel.HighestPrice - testCandel.LowestPrice) <= bodyToRangeRatio;
            //        bool isHammer = hasLongLowerShadow && hasSmallUpperShadow && hasSmallBody && testCandel.StartPrice > testCandel.LowestPrice;

            //        bool HammerisFit = false;

            //        Candel HammerPreviousDayHighestPriceCandel = null;

            //        if (testCandel != null)
            //        {
            //            HammerPreviousDayHighestPriceCandel = CandelDataPreviousDay
            //                .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
            //                .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
            //                .FirstOrDefault();                                                 // Take the first (highest)
            //        }

            //        if (HammerPreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < HammerPreviousDayHighestPriceCandel.HighestPrice)
            //        {
            //            var percentageDifference = ((HammerPreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

            //            if (percentageDifference > diff)
            //            {
            //                HammerisFit = true; // IHisFit is true
            //            }
            //        }


            //        if (
            //            HammerhasLongLowerShadow
            //            && HammerhasSmallUpperShadow
            //            && hasSmallBody
            //            && isHammer
            //            && HammerisFit
            //            )
            //        {
            //            dragonFlyDojiCandles.Add(testCandel);
            //        }








            //        /// INVERTED HAMMER


            //        // Define thresholds as a percentage (adjust based on sensitivity)
            //        const decimal bodyToWickRatio = 0.3m; // Body should be small relative to the wicks
            //        const decimal upperWickLengthRatio = 2m; // Upper wick should be at least twice the body
            //        const decimal lowerWickLimit = 0.2m; // Lower wick should be small/negligible

            //        // Calculate body size, wick sizes, and ratios
            //        decimal IHbodySize = Math.Abs(testCandel.EndPrice - testCandel.StartPrice);
            //        decimal upperWickSize = testCandel.HighestPrice - Math.Max(testCandel.StartPrice, testCandel.EndPrice);
            //        decimal lowerWickSize = Math.Min(testCandel.StartPrice, testCandel.EndPrice) - testCandel.LowestPrice;

            //        decimal totalRange = testCandel.HighestPrice - testCandel.LowestPrice;

            //        // Ensure totalRange is not zero to avoid divide by zero exception
            //        decimal bodyRatio = totalRange != 0 ? bodySize / totalRange : 0;

            //        // Validate conditions for Inverted Hammer
            //        bool smallBody = bodyRatio <= bodyToWickRatio;
            //        bool longUpperWick = upperWickSize >= bodySize * upperWickLengthRatio;
            //        bool minimalLowerWick = lowerWickSize <= (totalRange * lowerWickLimit);


            //        bool IHisFit = false;

            //        Candel IHPreviousDayHighestPriceCandel = null;

            //        if (testCandel != null)
            //        {
            //            IHPreviousDayHighestPriceCandel = CandelDataPreviousDay
            //                .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
            //                .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
            //                .FirstOrDefault();                                                 // Take the first (highest)
            //        }

            //        if (IHPreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < IHPreviousDayHighestPriceCandel.HighestPrice)
            //        {
            //            var percentageDifference = ((IHPreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

            //            if (percentageDifference > diff)
            //            {
            //                IHisFit = true; // IHisFit is true
            //            }
            //        }



            //        if (
            //            smallBody
            //            && longUpperWick
            //            && minimalLowerWick
            //            && IHisFit
            //            )
            //        {
            //            dragonFlyDojiCandles.Add(testCandel);
            //        }






            //        /// BULLISH ENGULFING
            //        Candel BFfirst = testCandel;
            //        Candel BFSecond = CandelData
            //            .Where(c => c.OpenTime > BFfirst.OpenTime)
            //            .OrderBy(c => c.OpenTime)
            //            .FirstOrDefault();

            //        if (BFfirst != null && BFSecond != null)
            //        {

            //            // Check if the previous candle is bearish
            //            bool isPreviousBearish = BFfirst.EndPrice < BFfirst.StartPrice;

            //            // Check if the current candle is bullish
            //            bool isCurrentBullish = BFSecond.EndPrice > BFSecond.StartPrice;

            //            // Check if the current candle's body engulfs the previous candle's body
            //            bool isEngulfingBody =
            //                BFSecond.StartPrice < BFfirst.EndPrice && // Current start below previous end
            //                BFSecond.EndPrice > BFfirst.StartPrice;  // Current end above previous start

            //            bool BFisFit = false;

            //            Candel BFPreviousDayHighestPriceCandel = null;

            //            if (testCandel != null)
            //            {
            //                BFPreviousDayHighestPriceCandel = CandelDataPreviousDay
            //                    .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
            //                    .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
            //                    .FirstOrDefault();                                                 // Take the first (highest)
            //            }

            //            if (BFPreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < BFPreviousDayHighestPriceCandel.HighestPrice)
            //            {
            //                var percentageDifference = ((BFPreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

            //                if (percentageDifference > diff)
            //                {
            //                    BFisFit = true; // IHisFit is true
            //                }
            //            }

            //            if (
            //             isPreviousBearish
            //             && isCurrentBullish
            //             && isEngulfingBody
            //             && BFisFit
            //             )
            //            {
            //                dragonFlyDojiCandles.Add(testCandel);
            //            }





            //            /// TWEEZER BOTTOM
            //            Candel TBfirst = testCandel;
            //            Candel TBSecond = CandelData
            //                .Where(c => c.OpenTime > TBfirst.OpenTime)
            //                .OrderBy(c => c.OpenTime)
            //                .FirstOrDefault();

            //            if (TBfirst != null && TBSecond != null)
            //            {

            //                // Check if the first candle is bearish and the second is bullish
            //                bool isBearishBullish = TBfirst.EndPrice < TBfirst.StartPrice
            //                                        && TBSecond.EndPrice > TBSecond.StartPrice;

            //                // Check if the lowest prices of the candles are nearly equal
            //                bool hasMatchingLows = Math.Abs(TBfirst.LowestPrice - TBSecond.LowestPrice) <= 0.01m;

            //                bool TBisFit = false;

            //                Candel TBPreviousDayHighestPriceCandel = null;

            //                if (testCandel != null)
            //                {
            //                    TBPreviousDayHighestPriceCandel = CandelDataPreviousDay
            //                        .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
            //                        .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
            //                        .FirstOrDefault();                                                 // Take the first (highest)
            //                }

            //                if (TBPreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < TBPreviousDayHighestPriceCandel.HighestPrice)
            //                {
            //                    var percentageDifference = ((TBPreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

            //                    if (percentageDifference > diff)
            //                    {
            //                        TBisFit = true; // IHisFit is true
            //                    }
            //                }

            //                if (
            //                 isBearishBullish
            //                 && hasMatchingLows
            //                 && TBisFit
            //                 )
            //                {
            //                    dragonFlyDojiCandles.Add(testCandel);
            //                }

            //            }



            //        }
            //    }));
            //}

            //// Wait for all tasks to complete
            //Task.WaitAll(tasks.ToArray());







            //foreach (Candel testCandel in CandelData)
            //{
            //    var diff = 5;

            //    /// DRAGON FLY DOJI

            //    // Tolerance level for considering "nearly equal" start and end prices
            //    decimal tolerance = 0.01m;

            //    // Check for price equality
            //    bool isPriceEqual = Math.Abs(testCandel.StartPrice - testCandel.EndPrice) <= tolerance;

            //    // Check for long lower shadow: Lowest price should be much lower than Start and End prices
            //    bool hasLongLowerShadow = testCandel.LowestPrice < Math.Min(testCandel.StartPrice, testCandel.EndPrice) - (Math.Max(testCandel.StartPrice, testCandel.EndPrice) - Math.Min(testCandel.StartPrice, testCandel.EndPrice)) * 2;

            //    // Check for small upper shadow: Highest price should be at or near Start/End price
            //    bool hasSmallUpperShadow = Math.Abs(testCandel.HighestPrice - testCandel.StartPrice) <= tolerance && Math.Abs(testCandel.HighestPrice - testCandel.EndPrice) <= tolerance;

            //    bool isFit = false;


            //    Candel PreviousDayHighestPriceCandel = null;

            //    if (testCandel != null)
            //    {
            //        PreviousDayHighestPriceCandel = CandelDataPreviousDay
            //            .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
            //            .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
            //            .FirstOrDefault();                                                 // Take the first (highest)
            //    }

            //    if (PreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < PreviousDayHighestPriceCandel.HighestPrice)
            //    {
            //        var percentageDifference = ((PreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

            //        if (percentageDifference > diff)
            //        {
            //            isFit = true; // IHisFit is true
            //        }
            //    }



            //    if (
            //        isPriceEqual == true
            //        && hasLongLowerShadow == true
            //        && hasSmallUpperShadow == true
            //        && isFit == true
            //      )
            //    {
            //        dragonFlyDojiCandles.Add(testCandel);
            //    }
            //}





            //foreach (Candel testCandel in CandelData)
            //{

            //    var diff = 5;

            //    /// HAMMER

            //    // Calculate body size and shadows
            //    decimal bodySize = Math.Abs(testCandel.EndPrice - testCandel.StartPrice);
            //    decimal lowerShadow = Math.Abs(testCandel.StartPrice - testCandel.LowestPrice < testCandel.EndPrice - testCandel.LowestPrice ? testCandel.StartPrice - testCandel.LowestPrice : testCandel.EndPrice - testCandel.LowestPrice);
            //    decimal upperShadow = testCandel.HighestPrice - Math.Max(testCandel.StartPrice, testCandel.EndPrice);

            //    // Define thresholds (can be adjusted based on dataset and market conditions)
            //    decimal shadowToBodyRatio = 2.5m; // Lower shadow must be at least 2.5x body size
            //    decimal bodyToRangeRatio = 0.25m; // Body must be within 25% of the price range
            //    decimal minBodySize = 0.01m; // Avoid false positives for tiny candles

            //    // Check Hammer pattern conditions
            //    bool hasLongLowerShadow = lowerShadow >= (bodySize * shadowToBodyRatio);
            //    bool hasSmallUpperShadow = upperShadow < (bodySize * 0.5m);
            //    bool hasSmallBody = bodySize > 0 && bodySize / (testCandel.HighestPrice - testCandel.LowestPrice) <= bodyToRangeRatio;
            //    bool isHammer = hasLongLowerShadow && hasSmallUpperShadow && hasSmallBody && testCandel.StartPrice > testCandel.LowestPrice;

            //    bool HammerisFit = false;

            //    Candel HammerPreviousDayHighestPriceCandel = null;

            //    if (testCandel != null)
            //    {
            //        HammerPreviousDayHighestPriceCandel = CandelDataPreviousDay
            //            .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
            //            .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
            //            .FirstOrDefault();                                                 // Take the first (highest)
            //    }

            //    if (HammerPreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < HammerPreviousDayHighestPriceCandel.HighestPrice)
            //    {
            //        var percentageDifference = ((HammerPreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

            //        if (percentageDifference > diff)
            //        {
            //            HammerisFit = true; // Hammer is a valid fit
            //        }
            //    }

            //    if (
            //        hasLongLowerShadow
            //        && hasSmallUpperShadow
            //        && hasSmallBody
            //        && isHammer
            //        && HammerisFit
            //        )
            //    {
            //        dragonFlyDojiCandles.Add(testCandel);
            //    }

            //}




            //foreach (Candel testCandel in CandelData)
            //{

            //    var diff = 5;



            //    /// INVERTED HAMMER


            //    // Define thresholds as a percentage (adjust based on sensitivity)
            //    const decimal bodyToWickRatio = 0.3m; // Body should be small relative to the wicks
            //    const decimal upperWickLengthRatio = 2m; // Upper wick should be at least twice the body
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


            //    bool IHisFit = false;

            //    Candel IHPreviousDayHighestPriceCandel = null;

            //    if (testCandel != null)
            //    {
            //        IHPreviousDayHighestPriceCandel = CandelDataPreviousDay
            //            .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
            //            .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
            //            .FirstOrDefault();                                                 // Take the first (highest)
            //    }

            //    if (IHPreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < IHPreviousDayHighestPriceCandel.HighestPrice)
            //    {
            //        var percentageDifference = ((IHPreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

            //        if (percentageDifference > diff)
            //        {
            //            IHisFit = true; // IHisFit is true
            //        }
            //    }



            //    if (
            //        smallBody
            //        && longUpperWick
            //        && minimalLowerWick
            //        && IHisFit
            //        )
            //    {
            //        dragonFlyDojiCandles.Add(testCandel);
            //    }

            //}



            foreach (Candel testCandel in CandelData)
            {
                if (testCandel.OpenTime.TimeOfDay > new TimeSpan(11, 00, 0))
                {
                    continue; // Skip the rest of this iteration and proceed to the next object
                }


                var diff = 5;

                /// BULLISH ENGULFING
                Candel BFfirst = testCandel;
                Candel BFSecond = CandelData
                    .Where(c => c.OpenTime > BFfirst.OpenTime)
                    .OrderBy(c => c.OpenTime)
                    .FirstOrDefault();

                if (BFfirst != null && BFSecond != null)
                {

                    // Check if the previous candle is bearish
                    bool isPreviousBearish = BFfirst.EndPrice < BFfirst.StartPrice;

                    // Check if the current candle is bullish
                    bool isCurrentBullish = BFSecond.EndPrice > BFSecond.StartPrice;

                    // Check if the current candle's body engulfs the previous candle's body
                    bool isEngulfingBody =
                        BFSecond.StartPrice < BFfirst.EndPrice && // Current start below previous end
                        BFSecond.EndPrice > BFfirst.StartPrice;  // Current end above previous start

                    bool BFisFit = false;

                    Candel BFPreviousDayHighestPriceCandel = null;

                    if (testCandel != null)
                    {
                        BFPreviousDayHighestPriceCandel = CandelDataPreviousDay
                            .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
                            .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
                            .FirstOrDefault();                                                 // Take the first (highest)
                    }

                    if (BFPreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < BFPreviousDayHighestPriceCandel.HighestPrice)
                    {
                        var percentageDifference = ((BFPreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

                        if (percentageDifference > diff)
                        {
                            BFisFit = true; // IHisFit is true
                        }
                    }

                    if (
                     isPreviousBearish
                     && isCurrentBullish
                     && isEngulfingBody
                     && BFisFit
                     )
                    {
                        dragonFlyDojiCandles.Add(testCandel);
                    }
                }


            }





            //foreach (Candel testCandel in CandelData)
            //{

            //    if (testCandel.OpenTime.TimeOfDay > new TimeSpan(11, 00, 0))
            //    {
            //        continue; // Skip the rest of this iteration and proceed to the next object
            //    }

            //    var diff = 5;


            //    /// TWEEZER BOTTOM
            //    Candel TBfirst = testCandel;
            //    Candel TBSecond = CandelData
            //        .Where(c => c.OpenTime > TBfirst.OpenTime)
            //        .OrderBy(c => c.OpenTime)
            //        .FirstOrDefault();

            //    if (TBfirst != null && TBSecond != null)
            //    {

            //        // Check if the first candle is bearish and the second is bullish
            //        bool isBearishBullish = TBfirst.EndPrice < TBfirst.StartPrice
            //                                && TBSecond.EndPrice > TBSecond.StartPrice;

            //        // Check if the lowest prices of the candles are nearly equal
            //        bool hasMatchingLows = Math.Abs(TBfirst.LowestPrice - TBSecond.LowestPrice) <= 0.01m;

            //        bool TBisFit = false;

            //        Candel TBPreviousDayHighestPriceCandel = null;

            //        if (testCandel != null)
            //        {
            //            TBPreviousDayHighestPriceCandel = CandelDataPreviousDay
            //                .Where(c => c.OpenTime.TimeOfDay > testCandel.OpenTime.TimeOfDay) // Compare only the time
            //                .OrderByDescending(c => c.HighestPrice)                            // Order by HighestPrice descending
            //                .FirstOrDefault();                                                 // Take the first (highest)
            //        }

            //        if (TBPreviousDayHighestPriceCandel != null && testCandel != null && testCandel.EndPrice < TBPreviousDayHighestPriceCandel.HighestPrice)
            //        {
            //            var percentageDifference = ((TBPreviousDayHighestPriceCandel.HighestPrice - testCandel.EndPrice) / testCandel.EndPrice) * 100;

            //            if (percentageDifference > diff)
            //            {
            //                TBisFit = true; // IHisFit is true
            //            }
            //        }

            //        if (
            //         isBearishBullish
            //         && hasMatchingLows
            //         && TBisFit
            //         )
            //        {
            //            dragonFlyDojiCandles.Add(TBSecond);
            //        }

            //    }

            //}


            //////Convert ConcurrentBag to a list, if needed
            //List<Candel> result = dragonFlyDojiCandles.ToList();









            //////Create and execute tasks for parallel processing

            //List < Task > tasks = new List<Task>();

            //foreach (Candel latestCandel in CandelData)
            //    {
            //        tasks.Add(Task.Run(() =>
            //        {
            //            // Fetch the previous and next candles
            //            Candel previousCandel = CandelData
            //                .Where(c => c.CloseTime < latestCandel.CloseTime)
            //                .OrderByDescending(c => c.CloseTime)
            //                .FirstOrDefault();

            //            Candel verificationCandel = CandelData
            //                .Where(c => c.CloseTime > latestCandel.CloseTime)
            //                .OrderBy(c => c.CloseTime)
            //                .FirstOrDefault();

            //            // Calculate key metrics
            //            decimal bodySize = Math.Abs(latestCandel.EndPrice - latestCandel.StartPrice);
            //            decimal range = latestCandel.HighestPrice - latestCandel.LowestPrice;
            //            decimal lowerShadowSize = latestCandel.StartPrice - latestCandel.LowestPrice;
            //            decimal upperShadowSize = latestCandel.HighestPrice - latestCandel.EndPrice;

            //            // Define thresholds
            //            bool isBullish = latestCandel.IsBullish == true &&
            //                             latestCandel.EndPrice > (latestCandel.LowestPrice + range / 2);
            //            bool smallBody = bodySize < (range * 0.15m);
            //            bool longLowerShadow = lowerShadowSize > (range * 0.6m);
            //            bool smallUpperShadow = upperShadowSize < bodySize * 0.1m;

            //            // High volume check
            //            bool highVolume = false;
            //            if (previousCandel != null)
            //            {
            //                highVolume = latestCandel.Volume > (previousCandel.Volume * 1.7m);
            //            }

            //            // Price change confirmation
            //            bool significantPriceChange = latestCandel.PriceChangePercentage > 0.005m;

            //            bool nextCandelBullish = verificationCandel?.IsBullish == true;

            //            // Combine conditions
            //            if (isBullish
            //                && smallBody
            //                && longLowerShadow
            //                && smallUpperShadow
            //                && highVolume
            //                && significantPriceChange
            //                && nextCandelBullish)
            //            {
            //                if (verificationCandel?.OpenTime.TimeOfDay < new TimeSpan(11, 00, 0))
            //                {
            //                    dragonFlyDojiCandles.Add(verificationCandel);
            //                }
            //            }
            //        }));
            //    }

            //// Wait for all tasks to complete
            //Task.WaitAll(tasks.ToArray());

            //// Convert ConcurrentBag to a list, if needed
            List<Candel> result = dragonFlyDojiCandles.ToList();


            return result;
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
