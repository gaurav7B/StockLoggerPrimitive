using Newtonsoft.Json;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Hammer;
using System.Diagnostics.SymbolStore;
using System.Text;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers
{
    public class HammerAnalyzer
    {
        public async void HammerAnalyzerLogic(List<Candel> candelList, HttpClient _httpClient, int Range)
        {
            // Ensure there are at least 1 candle to check
            if (candelList.Count < 1)
            {
                Console.WriteLine("The list must contain at least 1 candle.");
                return;
            }

            // Get the last (most recent) candle
            Candel verificationCandel = candelList.OrderByDescending(c => c.CloseTime).First();
            Candel hammerCandel = candelList
                                    .OrderByDescending(c => c.CloseTime)
                                    .Skip(1) // Skip the first candle
                                    .First(); // Take the second one
            Candel previousCandel = candelList
                                    .OrderByDescending(c => c.CloseTime)
                                    .Skip(2) // Skip two candles
                                    .First(); // Take the second one
            Candel latestCandel = hammerCandel;

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

            // If all conditions are met, the pattern is a bullish hammer
            if (
                    isBullish
                    && smallBody
                    && longLowerShadow
                    && smallUpperShadow
                    && highVolume
                    && significantPriceChange
                    && nextcandelbullish
                )
            {
                if (Range == 1)
                {
                    HammerDb Payload = new HammerDb
                    {
                        Ticker = verificationCandel.Ticker,
                        TickerId = verificationCandel.TickerId,
                        Exchange = verificationCandel.Exchange,
                        IsHammerDetected = true,
                        DetectionRange = 1,
                        DetectionTime = verificationCandel.CloseTime,
                        DetectedPrice = verificationCandel.EndPrice,
                        ExpectedPrice = verificationCandel.EndPrice * 1.001429m,
                        HammerCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Hammer",
                                        new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 5)
                {
                    HammerDb Payload = new HammerDb
                    {
                        Ticker = verificationCandel.Ticker,
                        TickerId = verificationCandel.TickerId,
                        Exchange = verificationCandel.Exchange,
                        IsHammerDetected = true,
                        DetectionRange = 5,
                        DetectionTime = verificationCandel.CloseTime,
                        DetectedPrice = verificationCandel.EndPrice,
                        ExpectedPrice = verificationCandel.EndPrice * 1.001429m,
                        HammerCandels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 4)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Hammer",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 10)
                {
                    HammerDb Payload = new HammerDb
                    {
                        Ticker = verificationCandel.Ticker,
                        TickerId = verificationCandel.TickerId,
                        Exchange = verificationCandel.Exchange,
                        IsHammerDetected = true,
                        DetectionRange = 10,
                        DetectionTime = verificationCandel.CloseTime,
                        DetectedPrice = verificationCandel.EndPrice,
                        ExpectedPrice = verificationCandel.EndPrice * 1.001429m,
                        HammerCandels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 9)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Hammer",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 15)
                {
                    HammerDb Payload = new HammerDb
                    {
                        Ticker = verificationCandel.Ticker,
                        TickerId = verificationCandel.TickerId,
                        Exchange = verificationCandel.Exchange,
                        IsHammerDetected = true,
                        DetectionRange = 15,
                        DetectionTime = verificationCandel.CloseTime,
                        DetectedPrice = verificationCandel.EndPrice,
                        ExpectedPrice = verificationCandel.EndPrice * 1.001429m,
                        HammerCandels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 14)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Hammer",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
            }
            else
            {
                Console.WriteLine("No Hammer pattern detected.");
            }
        }




        public async Task Analyze1MinCandelAsync(string symboltoken, HttpClient _httpClient, CancellationToken stoppingToken, string authToken)
        {

            var symbolTokenValue = symboltoken; // Replace with the correct value from stock
            var token = authToken; // Replace with the actual token
            var startDate = DateTime.Now.AddMinutes(-5); // Example start date
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
                    HammerAnalyzerLogic(candels, _httpClient, 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {symboltoken}: {ex.Message}");
            }
        }

        public async Task Analyze5MinCandelAsync(string ticker, HttpClient _httpClient, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get5MinCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    HammerAnalyzerLogic(candels, _httpClient, 5);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }

        public async Task Analyze10MinCandelAsync(string ticker, HttpClient _httpClient, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get10MinCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    HammerAnalyzerLogic(candels, _httpClient, 10);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }

        public async Task Analyze15MinCandelAsync(string ticker, HttpClient _httpClient, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get15MinCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    HammerAnalyzerLogic(candels, _httpClient, 15);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }



    }
}
