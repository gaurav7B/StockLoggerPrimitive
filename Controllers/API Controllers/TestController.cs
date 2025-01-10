using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OtpNet;
using StockLogger.Data;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Dragonfly_Doji;
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
            var stocks = StockList.GetStocks();
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


            decimal MainProfit = 0;
            decimal MainLoss = 0;


            foreach (var stock in _stocks)
            {
                List<Candel> MasterList = new List<Candel>();

                List<Candel> DrafonFlyDojiCandels = new List<Candel>();

                List<List<Candel>> AnalyzerList = new List<List<Candel>>();

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

                try
                {
                    var response = await client.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest", content);

                    if (response.IsSuccessStatusCode)
                    {

                        var responseData = await response.Content.ReadAsStringAsync();
                        CandelData = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                        Candel EndCandel = CandelData.LastOrDefault();


                        List<Candel> dragonFlyDojiCandles = IdentifyDragonflyDojiCandles(CandelData);

                        MasterList = CandelData;
                        MainMasterList.Add(MasterList);
                        DrafonFlyDojiCandels = dragonFlyDojiCandles;

                        if (DrafonFlyDojiCandels.Count > 0)
                        {
                            // Iterating through each Dragonfly Doji Candle
                            foreach (Candel dojiCandle in dragonFlyDojiCandles)
                            {
                                // Get the index of the Dragonfly Doji candle in the full list
                                int dojiIndex = CandelData.IndexOf(dojiCandle);

                                // List to store the next 5 candles
                                List<Candel> nextFiveCandles = new List<Candel>();

                                nextFiveCandles.Add(dojiCandle);

                                // Check if there are at least 5 more candles after the identified Dragonfly Doji candle 6
                                for (int i = dojiIndex + 1; i < dojiIndex + 6 && i < CandelData.Count; i++)
                                {
                                    nextFiveCandles.Add(CandelData[i]);
                                }

                                // Add the list of next 5 candles to the AnalyzerList
                                AnalyzerList.Add(nextFiveCandles);
                            }

                            foreach (List<Candel> detectedCandelList in AnalyzerList)
                            {
                                Candel firstCandel = detectedCandelList[0];
                                Candel lastCandel = detectedCandelList[5];//5

                                bool isCorrectPrediction = detectedCandelList.Skip(1).Any(c => c.EndPrice > firstCandel.EndPrice + (decimal)1);

                                Candel candelThatSatisfiesCondition = detectedCandelList.Skip(1).FirstOrDefault(c => c.EndPrice > firstCandel.EndPrice + (decimal)1);
                                //Candel candelThatSatisfiesHighestPriceCondition = detectedCandelList.FirstOrDefault(c => c.HighestPrice > firstCandel.EndPrice);
                                //Candel candelThatSatisfiesHighestPriceCondition = detectedCandelList.FirstOrDefault(c => c.HighestPrice > firstCandel.EndPrice * (decimal)1.5);
                                Candel candelThatSatisfiesHighestPriceCondition = detectedCandelList.Skip(1).FirstOrDefault(c => c.HighestPrice > (firstCandel.EndPrice + (decimal)1));


                                if (isCorrectPrediction)
                                {
                                    CorrectPredictionList.Add(detectedCandelList);
                                    MainCorrectPredictionList.Add(CorrectPredictionList);

                                    TotalProfit = TotalProfit + (candelThatSatisfiesCondition.EndPrice - firstCandel.EndPrice);
                                    MainProfit = MainProfit + (candelThatSatisfiesCondition.EndPrice - firstCandel.EndPrice);
                                }
                                else if(isCorrectPrediction == false  && candelThatSatisfiesHighestPriceCondition != null)
                                {
                                    CorrectPredictionList.Add(detectedCandelList);
                                    MainCorrectPredictionList.Add(CorrectPredictionList);

                                    TotalProfit = TotalProfit + (candelThatSatisfiesHighestPriceCondition.HighestPrice - firstCandel.EndPrice);
                                    MainProfit = MainProfit + (candelThatSatisfiesHighestPriceCondition.HighestPrice - firstCandel.EndPrice);
                                }
                                else
                                {
                                    //WrongPredictionList.Add(detectedCandelList);
                                    //MainWrongPredictionList.Add(WrongPredictionList);
                                    //NetLoss = NetLoss + (firstCandel.EndPrice - lastCandel.EndPrice);
                                    //MainLoss = MainLoss + (firstCandel.EndPrice - lastCandel.EndPrice);


                                    foreach (var detectedCandel in detectedCandelList)
                                    {
                                        //Find the candel meeting the criteria
                                        var matchingCandel = CandelData
                                            .Where(c => c.EndPrice > detectedCandel.EndPrice && c.OpenTime > detectedCandel.OpenTime)
                                            .FirstOrDefault();

                                        Candel matchingCandel2 = CandelData.FirstOrDefault(c => c.HighestPrice > (firstCandel.EndPrice + (decimal)1) && c.OpenTime > detectedCandel.OpenTime);


                                        if (matchingCandel != null)
                                        {
                                            CorrectPredictionList.Add(detectedCandelList);

                                            MainCorrectPredictionList.Add(CorrectPredictionList);

                                            TotalProfit = TotalProfit + (matchingCandel.EndPrice - detectedCandel.EndPrice);
                                            MainProfit = MainProfit + (matchingCandel.EndPrice - detectedCandel.EndPrice);
                                        }else if(matchingCandel2 != null)
                                        {
                                            CorrectPredictionList.Add(detectedCandelList);

                                            MainCorrectPredictionList.Add(CorrectPredictionList);

                                            TotalProfit = TotalProfit + (matchingCandel2.HighestPrice - detectedCandel.EndPrice);
                                            MainProfit = MainProfit + (matchingCandel2.HighestPrice - detectedCandel.EndPrice);
                                        }
                                        else
                                        {
                                            Candel matchingCandel3 = CandelData.FirstOrDefault(c => c.HighestPrice > (firstCandel.EndPrice) && c.OpenTime > detectedCandel.OpenTime);

                                            if (matchingCandel3 != null)
                                            {
                                                CorrectPredictionList.Add(detectedCandelList);

                                                MainCorrectPredictionList.Add(CorrectPredictionList);

                                                TotalProfit = TotalProfit + (matchingCandel3.HighestPrice - detectedCandel.EndPrice);
                                                MainProfit = MainProfit + (matchingCandel3.HighestPrice - detectedCandel.EndPrice);
                                            }
                                            else if (EndCandel.EndPrice > firstCandel.EndPrice)
                                            {
                                                CorrectPredictionList.Add(detectedCandelList);

                                                MainCorrectPredictionList.Add(CorrectPredictionList);

                                                TotalProfit = TotalProfit + (EndCandel.EndPrice - firstCandel.EndPrice);
                                                MainProfit = MainProfit + (EndCandel.EndPrice - firstCandel.EndPrice);
                                            }
                                            else
                                            {
                                                WrongPredictionList.Add(detectedCandelList);
                                                MainWrongPredictionList.Add(WrongPredictionList);
                                                NetLoss = NetLoss + (firstCandel.EndPrice - EndCandel.EndPrice);
                                                MainLoss = MainLoss + (firstCandel.EndPrice - EndCandel.EndPrice);
                                            }


                                        }
                                    }

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


        // Function to identify Dragonfly Doji candles
        private List<Candel> IdentifyDragonflyDojiCandles(List<Candel> CandelData)
        {

            List<Candel> dragonFlyDojiCandles = new List<Candel>();

            foreach (Candel c in CandelData)
            {
                //// Get the last (most recent) candle
                //Candel latestCandel = c;

                //Candel previousCandel = CandelData
                //            .Where(c => c.CloseTime < latestCandel.CloseTime)
                //            .OrderByDescending(c => c.CloseTime) // Ensures we get the closest one before recentCandel
                //            .FirstOrDefault();


                //// Calculate the body size
                //decimal bodySize = Math.Abs(latestCandel.EndPrice - latestCandel.StartPrice);

                //// Calculate the lower shadow size
                //decimal lowerShadowSize = latestCandel.StartPrice - latestCandel.LowestPrice;

                //// Calculate the upper shadow size
                //decimal upperShadowSize = latestCandel.HighestPrice - latestCandel.EndPrice;

                //// Check if the candle is bullish
                //bool isBullish = latestCandel.IsBullish == true &&
                //                 (latestCandel.EndPrice > (latestCandel.LowestPrice + latestCandel.HighestPrice) / 2);

                //// Check if the body is small relative to the range
                //bool smallBody = bodySize < (latestCandel.HighestPrice - latestCandel.LowestPrice) * 0.2m;

                //// Check if the lower shadow is significantly longer than the body
                //bool longLowerShadow = lowerShadowSize > (latestCandel.HighestPrice - latestCandel.LowestPrice) * 0.5m;

                //// Check if the upper shadow is very small
                //bool smallUpperShadow = upperShadowSize < bodySize * 0.1m;

                //bool highVolume = false;

                //if (previousCandel != null)
                //{
                //    decimal previousVolume = previousCandel.Volume;
                //    // Optional: Add a volume check for additional confirmation (if volume data is available)
                //    highVolume = latestCandel.Volume > (previousVolume * 1.5m); // Modify this based on available data
                //}

                //Candel verificationCandel = CandelData
                //                   .Where(c => c.CloseTime > latestCandel.CloseTime)
                //                   .OrderBy(c => c.CloseTime) // Ensures we get the closest one
                //                   .FirstOrDefault();

                //bool nextcandelbullish = false;

                //if (verificationCandel != null)
                //{
                //    if (verificationCandel.IsBullish == true)
                //    {
                //        nextcandelbullish = true;
                //    }
                //}

                //// Final check
                //if (isBullish && smallBody && longLowerShadow && smallUpperShadow && highVolume && nextcandelbullish)
                //{
                //    dragonFlyDojiCandles.Add(verificationCandel);
                //}


                // Fetch the last and previous candles
                Candel latestCandel = c;
                Candel previousCandel = CandelData
                    .Where(c => c.CloseTime < latestCandel.CloseTime)
                    .OrderByDescending(c => c.CloseTime)
                    .FirstOrDefault();

                Candel verificationCandel = CandelData
                                   .Where(c => c.CloseTime > latestCandel.CloseTime)
                                   .OrderBy(c => c.CloseTime) // Ensures we get the closest one
                                   .FirstOrDefault();

                // Calculate key metrics
                decimal bodySize = Math.Abs(latestCandel.EndPrice - latestCandel.StartPrice);
                decimal range = latestCandel.HighestPrice - latestCandel.LowestPrice;
                decimal lowerShadowSize = latestCandel.StartPrice - latestCandel.LowestPrice;
                decimal upperShadowSize = latestCandel.HighestPrice - latestCandel.EndPrice;

                // Define thresholds
                bool isBullish = latestCandel.IsBullish == true &&
                                 latestCandel.EndPrice > (latestCandel.LowestPrice + range / 2);
                bool smallBody = bodySize < (range * 0.15m); // Tweaked threshold
                bool longLowerShadow = lowerShadowSize > (range * 0.6m); // Increased relative size
                bool smallUpperShadow = upperShadowSize < bodySize * 0.1m;

                // High volume check
                bool highVolume = false;
                if (previousCandel != null)
                {
                    highVolume = latestCandel.Volume > (previousCandel.Volume * 1.7m); // Increased factor
                }

                // Price change confirmation
                bool significantPriceChange = latestCandel.PriceChangePercentage > 0.005m; // Ensure meaningful move

                bool nextcandelbullish = false;

                if (verificationCandel != null)
                {
                    if (verificationCandel.IsBullish == true)
                    {
                        nextcandelbullish = true;
                    }
                }

                // Combine conditions
                if (isBullish && smallBody && longLowerShadow && smallUpperShadow && highVolume && significantPriceChange
                    && nextcandelbullish
                    )
                {
                    dragonFlyDojiCandles.Add(verificationCandel);

                }

            }


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


            //    bool precedingBullishTrend = false;
            //    if(verificationCandel != null)
            //    {
            //        if (verificationCandel.IsBullish == true)
            //        {
            //            precedingBullishTrend = true;
            //        }
            //    }


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
            //        && precedingBullishTrend
            //        //&& higherVolume
            //        && higherVolume2
            //        && nextCandleBullish
            //        && previousCandelBearish
            //        )
            //    {
            //        dragonFlyDojiCandles.Add(verificationCandel);
            //    }
            //}

            return dragonFlyDojiCandles;
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
