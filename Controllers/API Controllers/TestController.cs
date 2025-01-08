using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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

            // Fetch stocks from StockList
            _stocks = StockList.GetStocks();
        }

        //// POST https://localhost:44364/api/Test/DragonFly
        //[HttpPost("DragonFly")]
        //public async Task<IActionResult> DragonFly(dynamic CandelData)
        //{
        //    return Ok(CandelData);  // Correct usage of Ok()
        //}

        // Define the PriceData class
        public class PriceData
        {
            public int Count { get; set; }
            public Candel DetectedCandel { get; set; }
            public Candel FutureCandel { get; set; }
        }

        public class CandelPair
        {
            public Candel First { get; set; }
            public Candel Second { get; set; }

            public CandelPair(Candel first, Candel second)
            {
                First = first;
                Second = second;
            }
        }

        //POST https://localhost:44364/api/Test/TestMasterAPI
        [HttpPost("TestMasterAPI")]
        public async Task<IActionResult> TestMasterAPI()
        {
            string authtoken = await GetAuthorizationTokenAsync();

            List<List<Candel>> MasterList = new List<List<Candel>>();

            List<Candel> DrafonFlyDojiCandels = new List<Candel>();

            foreach (var stock in _stocks)
            {
                List<Candel> CandelData = new List<Candel>();

                var symbolTokenValue = stock.symboltoken; // Replace with the correct value from stock
                var token = authtoken; // Replace with the actual token
                var startDate = DateTime.UtcNow.AddDays(-20); // Example start date
                var endDate = DateTime.UtcNow; // Example end date

                var requestBody = new
                {
                    SymbolToken = symbolTokenValue,
                    AuthorizationToken = token,
                    StartDate = startDate.ToString("o"), // ISO string format
                    EndDate = endDate.ToString("o") // ISO string format
                };

                var client = new HttpClient();
                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync("https://localhost:44364/api/AngelCandel/getCandleData", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        CandelData = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                        MasterList.Add(CandelData);
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

            foreach(List<Candel> c in MasterList)
            {
                List<Candel> dragonFlyDojiCandles = IdentifyDragonflyDojiCandles(c);
                foreach(Candel candel in dragonFlyDojiCandles)
                {
                    DrafonFlyDojiCandels.Add(candel);

                }
            }

            DragonFlyDojiResponse LR = CreateDragonFlyDojiResponse(MasterList);

            return Ok();
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



        // Function to identify Dragonfly Doji candles
        private List<Candel> IdentifyDragonflyDojiCandles(List<Candel> CandelData)
        {
            List<Candel> dragonFlyDojiCandles = new List<Candel>();

            foreach (var c in CandelData)
            {
                Candel recentCandel = c;

                // Check if it is a Doji with a small body
                bool isDoji = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;

                // Check for a long lower shadow (shadow size relative to the body)
                bool longLowerShadow = (recentCandel.StartPrice - recentCandel.LowestPrice) > 3 * (recentCandel.EndPrice - recentCandel.StartPrice);

                // The body of the candle should be small and at the top of the range
                bool smallBodyAtTop = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.3m;

                // Preceding candle's trend should be bullish (for confirming upward momentum)
                bool precedingBullishTrend = CandelData.Where(x => x.CloseTime < recentCandel.OpenTime)
                                                       .OrderByDescending(x => x.CloseTime)
                                                       .Take(3)
                                                       .All(x => x.EndPrice > x.StartPrice); // At least the last 3 candles should be bullish

                // Check for higher volume
                bool higherVolume = recentCandel.Volume > CandelData.Average(x => x.Volume);

                // The next candle should also be bullish for confirmation
                bool nextCandleBullish = CandelData.Where(x => x.OpenTime > recentCandel.CloseTime)
                                                   .OrderBy(x => x.OpenTime)
                                                   .FirstOrDefault()?.EndPrice > recentCandel.EndPrice;

                Candel verificationCandel = CandelData.Where(x => x.OpenTime > recentCandel.CloseTime)
                                                   .OrderBy(x => x.OpenTime)
                                                   .FirstOrDefault();

                // If all conditions match, then it's a Dragonfly Doji with high probability of upward movement
                if (isDoji && longLowerShadow && smallBodyAtTop && precedingBullishTrend && higherVolume && nextCandleBullish)
                {
                    dragonFlyDojiCandles.Add(verificationCandel);
                }
            }

            return dragonFlyDojiCandles;
        }



        // POST https://localhost:44364/api/Test/DragonFly
        [HttpPost("DragonFly")]
        public async Task<IActionResult> DragonFly(List<Candel> CandelData)
        {
            List<Candel> DrafonFlyDojiCandels = new List<Candel>();
            List<Candel> CorrectPredictedCandels = new List<Candel>();
            List<Candel> WrongPredictedCandels = new List<Candel>();

            List<Candel> LossList = new List<Candel>();

            List<Candel> MisleniousList = new List<Candel>();

            List<CandelPair> Logger = new List<CandelPair>();



            foreach (var c in CandelData)
            {
                Candel recentCandel = c;

                //// Check if it is a Doji with a very small body compared to the entire range
                //bool isDoji = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;

                //// Check for a long lower shadow (shadow size relative to the body)
                //bool longLowerShadow = (recentCandel.StartPrice - recentCandel.LowestPrice) > 3 * (recentCandel.EndPrice - recentCandel.StartPrice);

                //// The body should be small and placed near the top of the range for Dragonfly Doji
                //bool smallBodyAtTop = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.2m
                //                      && recentCandel.StartPrice < recentCandel.HighestPrice * 0.6m;

                // Check if it is a Doji with a small body
                bool isDoji = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;

                // Check for a long lower shadow (shadow size relative to the body)
                bool longLowerShadow = (recentCandel.StartPrice - recentCandel.LowestPrice) > 3 * (recentCandel.EndPrice - recentCandel.StartPrice);

                // The body of the candle should be small and at the top of the range
                bool smallBodyAtTop = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.3m;

                // Preceding candles should confirm a more substantial bullish trend (use a larger window of candles)
                bool precedingBullishTrend = CandelData.Where(x => x.CloseTime < recentCandel.OpenTime)
                                                       .OrderByDescending(x => x.CloseTime)
                                                       .Take(5)
                                                       .All(x => x.EndPrice > x.StartPrice); // At least the last 5 candles should be bullish

                //// Check for significant volume compared to the highest volume in the last 10 candles
                //bool higherVolume = recentCandel.Volume > CandelData.TakeLast(10).Max(x => x.Volume) * 0.75m; // Volume above 75% of the max in last 10 candles

                // Check for higher volume
                bool higherVolume = recentCandel.Volume > CandelData.Average(x => x.Volume);

                // The next candle should be strongly bullish for confirmation
                bool nextCandleBullish = CandelData.Where(x => x.OpenTime > recentCandel.CloseTime)
                                                   .OrderBy(x => x.OpenTime)
                                                   .FirstOrDefault()?.EndPrice > recentCandel.EndPrice
                                        && CandelData.Where(x => x.OpenTime > recentCandel.CloseTime)
                                                     .OrderBy(x => x.OpenTime)
                                                     .FirstOrDefault()?.EndPrice > recentCandel.StartPrice;

                // Ensure the recent candle follows a significant downtrend before confirming reversal
                bool isAfterDowntrend = CandelData.Where(x => x.CloseTime < recentCandel.OpenTime)
                                                  .OrderByDescending(x => x.CloseTime)
                                                  .Take(5)
                                                  .All(x => x.EndPrice < x.StartPrice); // Last 5 candles should be bearish

                Candel verificationCandel = CandelData.Where(x => x.OpenTime > recentCandel.CloseTime)
                                                      .OrderBy(x => x.OpenTime)
                                                      .FirstOrDefault();

                // If all conditions match, then it's a Dragonfly Doji with high probability of upward movement
                if (isDoji && longLowerShadow && smallBodyAtTop && precedingBullishTrend && higherVolume && nextCandleBullish && isAfterDowntrend)
                {
                    DrafonFlyDojiCandels.Add(verificationCandel);
                }
            }

            //foreach (var c in CandelData)
            //{
            //    Candel recentCandel = c;

            //    // Check if it is a Doji with a small body
            //    bool isDoji = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;

            //    // Check for a long lower shadow (shadow size relative to the body)
            //    bool longLowerShadow = (recentCandel.StartPrice - recentCandel.LowestPrice) > 3 * (recentCandel.EndPrice - recentCandel.StartPrice);

            //    // The body of the candle should be small and at the top of the range
            //    bool smallBodyAtTop = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.3m;

            //    // Preceding candle's trend should be bullish (for confirming upward momentum)
            //    bool precedingBullishTrend = CandelData.Where(x => x.CloseTime < recentCandel.OpenTime)
            //                                           .OrderByDescending(x => x.CloseTime)
            //                                           .Take(3)
            //                                           .All(x => x.EndPrice > x.StartPrice); // At least the last 3 candles should be bullish

            //    // Check for higher volume
            //    bool higherVolume = recentCandel.Volume > CandelData.Average(x => x.Volume);

            //    // The next candle should also be bullish for confirmation
            //    bool nextCandleBullish = CandelData.Where(x => x.OpenTime > recentCandel.CloseTime)
            //                                       .OrderBy(x => x.OpenTime)
            //                                       .FirstOrDefault()?.EndPrice > recentCandel.EndPrice;

            //    Candel verificationCandel = CandelData.Where(x => x.OpenTime > recentCandel.CloseTime)
            //                                       .OrderBy(x => x.OpenTime)
            //                                       .FirstOrDefault();

            //    // If all conditions match, then it's a Dragonfly Doji with high probability of upward movement
            //    if (isDoji && longLowerShadow && smallBodyAtTop && precedingBullishTrend && higherVolume && nextCandleBullish)
            //    {
            //        DrafonFlyDojiCandels.Add(verificationCandel);
            //    }
            //}

            //foreach (var c in CandelData)
            //{
            //    Candel recentCandel = c;

            //    //// Check if it is a Doji with a very small body compared to the entire range
            //    //bool isDoji = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;

            //    //// Check for a long lower shadow (shadow size relative to the body)
            //    //bool longLowerShadow = (recentCandel.StartPrice - recentCandel.LowestPrice) > 3 * (recentCandel.EndPrice - recentCandel.StartPrice);

            //    //// The body should be small and placed near the top of the range for Dragonfly Doji
            //    //bool smallBodyAtTop = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.2m
            //    //                      && recentCandel.StartPrice < recentCandel.HighestPrice * 0.6m;

            //    // Check if it is a Doji with a small body
            //    bool isDoji = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;

            //    // Check for a long lower shadow (shadow size relative to the body)
            //    bool longLowerShadow = (recentCandel.StartPrice - recentCandel.LowestPrice) > 3 * (recentCandel.EndPrice - recentCandel.StartPrice);

            //    // The body of the candle should be small and at the top of the range
            //    bool smallBodyAtTop = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.3m;

            //    // Preceding candles should confirm a more substantial bullish trend (use a larger window of candles)
            //    bool precedingBullishTrend = CandelData.Where(x => x.CloseTime < recentCandel.OpenTime)
            //                                           .OrderByDescending(x => x.CloseTime)
            //                                           .Take(5)
            //                                           .All(x => x.EndPrice > x.StartPrice); // At least the last 5 candles should be bullish

            //    //// Check for significant volume compared to the highest volume in the last 10 candles
            //    //bool higherVolume = recentCandel.Volume > CandelData.TakeLast(10).Max(x => x.Volume) * 0.75m; // Volume above 75% of the max in last 10 candles

            //    // Check for higher volume
            //    bool higherVolume = recentCandel.Volume > CandelData.Average(x => x.Volume);

            //    // The next candle should be strongly bullish for confirmation
            //    bool nextCandleBullish = CandelData.Where(x => x.OpenTime > recentCandel.CloseTime)
            //                                       .OrderBy(x => x.OpenTime)
            //                                       .FirstOrDefault()?.EndPrice > recentCandel.EndPrice
            //                            && CandelData.Where(x => x.OpenTime > recentCandel.CloseTime)
            //                                         .OrderBy(x => x.OpenTime)
            //                                         .FirstOrDefault()?.EndPrice > recentCandel.StartPrice;

            //    // Ensure the recent candle follows a significant downtrend before confirming reversal
            //    bool isAfterDowntrend = CandelData.Where(x => x.CloseTime < recentCandel.OpenTime)
            //                                      .OrderByDescending(x => x.CloseTime)
            //                                      .Take(5)
            //                                      .All(x => x.EndPrice < x.StartPrice); // Last 5 candles should be bearish

            //    Candel verificationCandel = CandelData.Where(x => x.OpenTime > recentCandel.CloseTime)
            //                                          .OrderBy(x => x.OpenTime)
            //                                          .FirstOrDefault();

            //    // If all conditions match, then it's a Dragonfly Doji with high probability of upward movement
            //    if (isDoji && longLowerShadow && smallBodyAtTop && precedingBullishTrend && higherVolume && nextCandleBullish && isAfterDowntrend)
            //    {
            //        DrafonFlyDojiCandels.Add(verificationCandel);
            //    }
            //}


            decimal loss = 0;
            decimal profit = 0;
            List<PriceData> PriceIncreasedAfterCount = new List<PriceData>();


            foreach (Candel DetectedCandels in DrafonFlyDojiCandels)
            {

                // Find all objects where CloseTime > DetectedCandels.CloseTime
                List<Candel> filteredCandels = CandelData
                                              .Where(c => c.CloseTime > DetectedCandels.CloseTime)
                                              .OrderBy(c => c.CloseTime)
                                              .ToList();

                Candel nextCandel = filteredCandels[0];
                Candel nextCandel2 = filteredCandels[1];
                Candel nextCandel3 = filteredCandels[2];


                List<Candel> FurtherCandel = new List<Candel>();

                if (nextCandel == null || nextCandel2 == null || nextCandel3 == null)
                {
                    MisleniousList.Add(DetectedCandels);
                }


                if (nextCandel.EndPrice > DetectedCandels.EndPrice)
                {
                    CorrectPredictedCandels.Add(DetectedCandels);

                    Logger.Add(new CandelPair(DetectedCandels, nextCandel));

                    decimal calculatedProfit = (nextCandel.EndPrice - DetectedCandels.EndPrice);
                    profit = profit + calculatedProfit;
                }
                else if (nextCandel2.EndPrice > DetectedCandels.EndPrice)
                {
                    CorrectPredictedCandels.Add(DetectedCandels);

                    Logger.Add(new CandelPair(DetectedCandels, nextCandel2));

                    decimal calculatedProfit = (nextCandel2.EndPrice - DetectedCandels.EndPrice);
                    profit = profit + calculatedProfit;
                }
                else if (nextCandel3.EndPrice > DetectedCandels.EndPrice)
                {
                    CorrectPredictedCandels.Add(DetectedCandels);

                    Logger.Add(new CandelPair(DetectedCandels, nextCandel3));

                    decimal calculatedProfit = (nextCandel3.EndPrice - DetectedCandels.EndPrice);
                    profit = profit + calculatedProfit;
                }
                else if ((nextCandel.EndPrice < DetectedCandels.EndPrice) || (nextCandel2.EndPrice < DetectedCandels.EndPrice) || (nextCandel3.EndPrice < DetectedCandels.EndPrice))
                {

                    if (nextCandel.EndPrice < DetectedCandels.EndPrice)
                    {

                        if (nextCandel2.EndPrice < DetectedCandels.EndPrice)
                        {
                            if (nextCandel3.EndPrice < DetectedCandels.EndPrice)
                            {
                                WrongPredictedCandels.Add(DetectedCandels);
                                LossList.Add(nextCandel3);

                                Logger.Add(new CandelPair(DetectedCandels, nextCandel3));

                                decimal calculatedloss = (DetectedCandels.EndPrice - nextCandel3.EndPrice);
                                loss = loss + calculatedloss;
                            }
                            else
                            {
                                WrongPredictedCandels.Add(DetectedCandels);
                                LossList.Add(nextCandel2);

                                Logger.Add(new CandelPair(DetectedCandels, nextCandel2));

                                decimal calculatedloss = (DetectedCandels.EndPrice - nextCandel2.EndPrice);
                                loss = loss + calculatedloss;
                            }
                        }
                        else
                        {
                            WrongPredictedCandels.Add(DetectedCandels);
                            LossList.Add(nextCandel);

                            Logger.Add(new CandelPair(DetectedCandels, nextCandel));

                            decimal calculatedloss = (DetectedCandels.EndPrice - nextCandel.EndPrice);
                            loss = loss + calculatedloss;
                        }

                    }
                }
                else
                {
                    MisleniousList.Add(DetectedCandels);
                }

            }

            // Find the objects in PriceIncreasedAfterCount where DetectedCandel matches any Candel from WrongPredictedCandels
            var matchingPriceData = PriceIncreasedAfterCount
                .Where(priceData => WrongPredictedCandels
                                    .Any(wrongCandel => priceData.DetectedCandel.Equals(wrongCandel)))
                .ToList();

            // Find the objects in PriceIncreasedAfterCount where DetectedCandel does NOT match any Candel from WrongPredictedCandels
            var nonMatchingPriceData = PriceIncreasedAfterCount
                .Where(priceData => !WrongPredictedCandels
                                    .Any(wrongCandel => priceData.DetectedCandel.Equals(wrongCandel)))
                .ToList();






            // Get the object(s) in DrafonFlyDojiCandels that are not present in the other two lists
            var missingObjects = DrafonFlyDojiCandels
                .Where(candle => !CorrectPredictedCandels.Contains(candle) && !WrongPredictedCandels.Contains(candle))
                .ToList();

            foreach (var c in missingObjects)
            {
                MisleniousList.Add(c);
            }

            decimal sumEndPrice = DrafonFlyDojiCandels.Sum(candel => candel.EndPrice);
            decimal correctPredictionsSum = CorrectPredictedCandels.Sum(candel => candel.EndPrice);
            decimal wrongPredictionsSum = WrongPredictedCandels.Sum(candel => candel.EndPrice);
            decimal MissleniousSum = WrongPredictedCandels.Sum(candel => candel.EndPrice);

            // Calculate accuracy using the helper method
            double accuracy = CalculateAccuracy(DrafonFlyDojiCandels.Count, CorrectPredictedCandels.Count);


            // Return an anonymous object containing the lists
            return Ok(new
            {
                profit,
                loss,

                Accuracy = accuracy,

                netprofit = profit - loss,

                Logger,

                CorrectPredictedCandels,
                DrafonFlyDojiCandels,
                WrongPredictedCandels,

                MisleniousList,

            });
        }

        public class DragonFlyDojiResponse
        {
            public List<List<Candel>> MasterCorrectPrediction { get; set; }
            public List<List<Candel>> MasterWrongPrediction { get; set; }
            public List<List<CandelPair>> CorrectPredictionLogger { get; set; }
            public List<List<CandelPair>> WrongpredictionLogger { get; set; }
        }

        // Function to identify Dragonfly Doji candles
        private DragonFlyDojiResponse CreateDragonFlyDojiResponse(List<List<Candel>> MasterList)
        {
            List<LogicResponse> logicResponse = new List<LogicResponse>();

            List<List<Candel>> DojiList = new List<List<Candel>>();

            List<List<Candel>> MasterCorrectPrediction = new List<List<Candel>>();
            List<List<Candel>> MasterWrongprediction = new List<List<Candel>>();
            List<List<CandelPair>> MasterCorrectPredictionLogger = new List<List<CandelPair>>();
            List<List<CandelPair>> MasterWrongPredictionLogger = new List<List<CandelPair>>();


            foreach (List<Candel> CandelData in MasterList)
            {
                List<Candel> dragonFlyDojiCandles = new List<Candel>();

                List<Candel> CorrectPrediction = new List<Candel>();
                List<Candel> Wrongprediction = new List<Candel>();
                List<CandelPair> CorrectPredictionLogger = new List<CandelPair>();
                List<CandelPair> WrongPredictionLogger = new List<CandelPair>();

                foreach (var c in CandelData)
                {
                    //Candel recentCandel = c;


                    //// Check if it is a Doji with a small body
                    //bool isDoji = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;

                    //// Check for a long lower shadow (shadow size relative to the body)
                    //bool longLowerShadow = (recentCandel.StartPrice - recentCandel.LowestPrice) > 3 * (recentCandel.EndPrice - recentCandel.StartPrice);

                    //// The body of the candle should be small and at the top of the range
                    //bool smallBodyAtTop = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.3m;

                    //// Preceding candle's trend should be bullish (for confirming upward momentum)
                    //bool precedingBullishTrend = CandelData.Where(x => x.CloseTime < recentCandel.OpenTime)
                    //                                       .OrderByDescending(x => x.CloseTime)
                    //                                       .Take(3)
                    //                                       .All(x => x.EndPrice > x.StartPrice); // At least the last 3 candles should be bullish

                    //// Check for higher volume
                    //bool higherVolume = recentCandel.Volume > CandelData.Average(x => x.Volume);

                    //// The next candle should also be bullish for confirmation
                    //bool nextCandleBullish = CandelData.Where(x => x.OpenTime > recentCandel.CloseTime)
                    //                                   .OrderBy(x => x.OpenTime)
                    //                                   .FirstOrDefault()?.EndPrice > recentCandel.EndPrice;

                    //Candel verificationCandel = CandelData.Where(x => x.OpenTime > recentCandel.CloseTime)
                    //                                   .OrderBy(x => x.OpenTime)
                    //                                   .FirstOrDefault();

                    //// If all conditions match, then it's a Dragonfly Doji with high probability of upward movement
                    //if (isDoji && longLowerShadow && smallBodyAtTop && precedingBullishTrend && higherVolume && nextCandleBullish)
                    //{

                    Candel recentCandel = c;

                    // Check if it is a Doji with a small body
                    bool isDoji = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;

                    // Check for a long lower shadow (shadow size relative to the body)
                    bool longLowerShadow = (recentCandel.StartPrice - recentCandel.LowestPrice) > 3 * (recentCandel.EndPrice - recentCandel.StartPrice);

                    // The body of the candle should be small and at the top of the range
                    bool smallBodyAtTop = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.3m;

                    // Preceding candle's trend should be bullish (for confirming upward momentum)
                    bool precedingBullishTrend = CandelData.Where(x => x.CloseTime < recentCandel.OpenTime)
                                                           .OrderByDescending(x => x.CloseTime)
                                                           .Take(3)
                                                           .All(x => x.EndPrice > x.StartPrice); // At least the last 3 candles should be bullish

                    // Check for higher volume
                    bool higherVolume = recentCandel.Volume > CandelData.Average(x => x.Volume);

                    //bool higherVolume = recentCandel.Volume > CandelData.TakeLast(10).Max(x => x.Volume) * 0.75m; // Volume above 75% of the max in last 10 candles


                    // The next candle should also be bullish for confirmation
                    bool nextCandleBullish = CandelData.Where(x => x.OpenTime > recentCandel.CloseTime)
                                                       .OrderBy(x => x.OpenTime)
                                                       .FirstOrDefault()?.EndPrice > recentCandel.EndPrice;

                    Candel verificationCandel = CandelData.Where(x => x.OpenTime > recentCandel.CloseTime)
                                                       .OrderBy(x => x.OpenTime)
                                                       .FirstOrDefault();

                    // If all conditions match, then it's a Dragonfly Doji with high probability of upward movement
                    if (isDoji && longLowerShadow && smallBodyAtTop && precedingBullishTrend && higherVolume && nextCandleBullish)
                    {

                        dragonFlyDojiCandles.Add(verificationCandel);

                        List<Candel> filteredCandels = CandelData
                              .Where(c => c.CloseTime > verificationCandel.CloseTime)
                              .OrderBy(c => c.CloseTime)
                              .ToList();

                        Candel nextCandel = filteredCandels[0];
                        Candel nextCandel2 = filteredCandels[1];
                        Candel nextCandel3 = filteredCandels[2];


                        if (nextCandel != null && nextCandel2 != null && nextCandel3 != null)
                        {
                            if(nextCandel.EndPrice > verificationCandel.EndPrice)
                            {
                                CorrectPrediction.Add(nextCandel);
                                CorrectPredictionLogger.Add(new CandelPair(verificationCandel, nextCandel));
                            }
                            else if(nextCandel2.EndPrice > verificationCandel.EndPrice)
                            {
                                CorrectPrediction.Add(nextCandel2);
                                CorrectPredictionLogger.Add(new CandelPair(verificationCandel, nextCandel2));
                            }
                            else if(nextCandel3.EndPrice > verificationCandel.EndPrice)
                            {
                                CorrectPrediction.Add(nextCandel3);
                                CorrectPredictionLogger.Add(new CandelPair(verificationCandel, nextCandel3));
                            }
                            else
                            {
                                if(nextCandel.EndPrice < verificationCandel.EndPrice)
                                {
                                    if(nextCandel2.EndPrice < verificationCandel.EndPrice)
                                    {
                                        if(nextCandel3.EndPrice < verificationCandel.EndPrice)
                                        {
                                            Wrongprediction.Add(verificationCandel);
                                            WrongPredictionLogger.Add(new CandelPair(verificationCandel, nextCandel3));
                                        }
                                    }
                                }
                            }
                        }

                    }
                }

                DojiList.Add(dragonFlyDojiCandles);
                MasterCorrectPrediction.Add(CorrectPrediction);
                MasterWrongprediction.Add(Wrongprediction);
                MasterCorrectPredictionLogger.Add(CorrectPredictionLogger);
                MasterWrongPredictionLogger.Add(WrongPredictionLogger);


            }



            return new DragonFlyDojiResponse
            {
                MasterCorrectPrediction = MasterCorrectPrediction,
                MasterWrongPrediction = MasterWrongprediction,
                CorrectPredictionLogger = MasterCorrectPredictionLogger,
                WrongpredictionLogger = MasterWrongPredictionLogger

            };
        }


        public class LogicResponse
        {
            public decimal profit { get; set; }
            public decimal loss { get; set; }
            public decimal accuracy { get; set; }
            public decimal netprofit { get; set; }

            public CandelPair Logger { get; set; }
            public List<Candel> CorrectPredictedCandels { get; set; }
            public List<Candel> DrafonFlyDojiCandels { get; set; }
            public List<Candel> WrongPredictedCandels { get; set; }

            public List<Candel> MisleniousList { get; set; }

        }

        // Helper method to calculate accuracy
        private double CalculateAccuracy(int totalPredictions, int correctPredictions)
        {
            if (totalPredictions == 0)
                return 0;

            double accuracy = ((double)correctPredictions / totalPredictions) * 100;
            return Math.Round(accuracy, 2); // Round the result to 2 decimal places
        }












    }
}
