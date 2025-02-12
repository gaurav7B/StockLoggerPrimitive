using Newtonsoft.Json;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Bullish_Harami;
using StockLogger.Models.Stratergic_Models.Dragonfly_Doji;
using System.Net.Http;
using System.Text;
using static StockLogger.Controllers.API_Controllers.BuySellController;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers
{
    public class DragonFlyDojiAnalyzer
    {

        public decimal CalculateSupportLevel(List<Candel> candelDataBeforeTestCandel)
        {
            // Ensure that the list is not null or empty
            if (candelDataBeforeTestCandel == null || !candelDataBeforeTestCandel.Any())
            {
                throw new ArgumentException("The candlestick data is empty or null.");
            }

            // Find the lowest price (support) from the list of candels
            decimal supportLevel = candelDataBeforeTestCandel.Min(candel => candel.LowestPrice);

            return supportLevel;
        }


        public async void DragonFlyDojiAnalyzerLogic(string symboltoken, List<Candel> CandelData , HttpClient _httpClient, int Range)
        {


            Candel recentCandel = CandelData.OrderByDescending(c => c.CloseTime).Skip(1).FirstOrDefault();
            Candel verificationCandel = CandelData.OrderByDescending(c => c.CloseTime).FirstOrDefault();



            /// LOGIC FOR IDENTIFYING DRAGONFLY DOJI CANDEL
            // Check if it is a Doji with a small body
            bool isDoji = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;

            // Check for a long lower shadow (shadow size relative to the body)
            bool longLowerShadow = (recentCandel.StartPrice - recentCandel.LowestPrice) > 3 * (recentCandel.EndPrice - recentCandel.StartPrice);

            // The body of the candle should be small and at the top of the range
            bool smallBodyAtTop = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.3m;

            bool shortUpperShadow = (recentCandel.HighestPrice - Math.Max(recentCandel.StartPrice, recentCandel.EndPrice)) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;



            // SUPPORT LEVEL CALCULATIONS
            List<Candel> CandelDataBeforeTestCandel = CandelData
                               .Where(candel => candel.OpenTime <= recentCandel.OpenTime)
                               .OrderBy(c => c.OpenTime)  // Reorder them back in ascending order
                               .ToList();

            // CHECK IF THE DOJI IS NEAR SUPPORT
            bool isDojiNearSupport = false;

            decimal tolerancePercentageForSupportLevel = 0.01m;

            decimal supportLevel = CalculateSupportLevel(CandelDataBeforeTestCandel);

            // Calculate the absolute difference between the support level and the recent candel's lowest price
            decimal priceDifference = Math.Abs(recentCandel.LowestPrice - supportLevel);

            // If the price difference is within the tolerance (percentage of the support level)
            decimal tolerance = (tolerancePercentageForSupportLevel / 100) * supportLevel;


            if (priceDifference <= tolerance)
            {
                isDojiNearSupport = true;
            }


            // If all conditions match, then it's a Dragonfly Doji with high probability of upward movement
            if (isDoji
                && longLowerShadow
                && smallBodyAtTop
                && shortUpperShadow
                && (isDojiNearSupport == true)
                && (recentCandel.EndPrice > recentCandel.StartPrice)

                && (verificationCandel != null)
                && (verificationCandel.EndPrice > verificationCandel.StartPrice)
                && (verificationCandel.HighestPrice > recentCandel.HighestPrice)
                )
            {
                // BUY API HERE
                BuyData buyData = new BuyData
                {
                    symboltoken = symboltoken,
                    tradingsymbol = verificationCandel.Ticker,
                    CurrentPrice = verificationCandel.EndPrice,
                    PreviousDaayEndPrice = 0
                };

                var jsonRequestBodyForbuyData = JsonConvert.SerializeObject(buyData);
                var contentForbuyData = new StringContent(jsonRequestBodyForbuyData, Encoding.UTF8, "application/json");

                var buyDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/buy", contentForbuyData);

                buyDataApiResponse.EnsureSuccessStatusCode();

                // WHEN THE BUY API RUNS SUCCESSFULLY
                // CALL THE SELL API
                // SELL API HERE
                if (buyDataApiResponse.IsSuccessStatusCode) // Ensures status is 200
                {
                    SellData sellData = new SellData
                    {
                        symboltoken = symboltoken,
                        tradingsymbol = verificationCandel.Ticker,
                        CurrentPrice = verificationCandel.EndPrice
                    };

                    var jsonRequestBodyForsellData = JsonConvert.SerializeObject(sellData);
                    var contentForsellData = new StringContent(jsonRequestBodyForsellData, Encoding.UTF8, "application/json");

                    var sellDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/sell", contentForsellData);
                }


                if (Range == 1)
                {
                    DragonflyDojiDb Payload = new DragonflyDojiDb
                    {
                        Ticker = verificationCandel.Ticker,
                        TickerId = verificationCandel.TickerId,
                        Exchange = verificationCandel.Exchange,
                        IsDragonflyDojiDetected = true,
                        DetectionRange = 1,
                        DetectionTime = verificationCandel.CloseTime,
                        DragonflyDojiCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/DragonflyDoji",
                                      new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                }
            }
        }


        public async Task Analyze1MinCandelAsync(string symboltoken, HttpClient _httpClient, CancellationToken stoppingToken , string authToken)
        {
            var symbolTokenValue = symboltoken; // Replace with the correct value from stock
            var token = authToken; // Replace with the actual token
            var startDate = DateTime.Now.AddHours(9).AddMinutes(15); // Example start date
            var endDate = DateTime.Now; // Example end date

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
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    DragonFlyDojiAnalyzerLogic(symboltoken , candels, _httpClient, 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {symboltoken}: {ex.Message}");
            }
        }

    }
}
