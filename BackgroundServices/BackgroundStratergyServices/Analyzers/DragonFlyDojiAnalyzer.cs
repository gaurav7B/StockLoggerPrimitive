using Newtonsoft.Json;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Bullish_Harami;
using StockLogger.Models.Stratergic_Models.Dragonfly_Doji;
using System.Net.Http;
using System.Text;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers
{
    public class DragonFlyDojiAnalyzer
    {
        public async void DragonFlyDojiAnalyzerLogic(List<Candel> candelList , HttpClient _httpClient, int Range)
        {
            // Ensure there are at least 2 candles in the list
            if (candelList.Count < 2)
            {
                Console.WriteLine("The list must contain at least 2 candles.");
                return;
            }


            Candel verificationCandel = candelList.OrderByDescending(c => c.CloseTime).FirstOrDefault();
            Candel dojicandel = candelList.OrderByDescending(c => c.CloseTime).Skip(1).FirstOrDefault();


            // Check if it is a Doji with a small body
            bool isDoji = Math.Abs(dojicandel.StartPrice - dojicandel.EndPrice) < (dojicandel.HighestPrice - dojicandel.LowestPrice) * 0.1m;

            // Check for a long lower shadow (shadow size relative to the body)
            bool longLowerShadow = (dojicandel.StartPrice - dojicandel.LowestPrice) > 3 * (dojicandel.EndPrice - dojicandel.StartPrice);

            // The body of the candle should be small and at the top of the range
            bool smallBodyAtTop = Math.Abs(dojicandel.StartPrice - dojicandel.EndPrice) < (dojicandel.HighestPrice - dojicandel.LowestPrice) * 0.3m;

            //// Preceding candle's trend should be bullish (for confirming upward momentum)
            //bool precedingBullishTrend = candelList.Where(x => x.CloseTime < dojicandel.OpenTime)
            //                                       .OrderByDescending(x => x.CloseTime)
            //                                       .Take(3)
            //                                       .All(x => x.EndPrice > x.StartPrice); // At least the last 3 candles should be bullish

            ////// Check for higher volume
            ////bool higherVolume = recentCandel.Volume > CandelData.Average(x => x.Volume);

            //bool higherVolume = dojicandel.Volume > candelList.TakeLast(10).Max(x => x.Volume) * 0.75m; // Volume above 75% of the max in last 10 candles


            //// The next candle should also be bullish for confirmation
            //bool nextCandleBullish = candelList.Where(x => x.OpenTime > dojicandel.CloseTime)
            //                                   .OrderBy(x => x.OpenTime)
            //                                   .FirstOrDefault()?.EndPrice > dojicandel.EndPrice;

            ////Candel verificationCandel = candelList.Where(x => x.OpenTime > recentCandel.CloseTime)
            ////                                   .OrderBy(x => x.OpenTime)
            ////                                   .FirstOrDefault();

            // If all conditions match, then it's a Dragonfly Doji with high probability of upward movement
            if (isDoji && longLowerShadow && smallBodyAtTop
                //&& precedingBullishTrend && higherVolume
                //&& nextCandleBullish
                )
            {
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
                else if (Range == 5)
                {
                    DragonflyDojiDb Payload = new DragonflyDojiDb
                    {
                        Ticker = verificationCandel.Ticker,
                        TickerId = verificationCandel.TickerId,
                        Exchange = verificationCandel.Exchange,
                        IsDragonflyDojiDetected = true,
                        DetectionRange = 5,
                        DetectionTime = verificationCandel.CloseTime,
                        DragonflyDojiCandels = null
                    };
                    if ((verificationCandel.CloseTime.Minute - verificationCandel.OpenTime.Minute) > 4)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/DragonflyDoji",
                                                              new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 10)
                {
                    DragonflyDojiDb Payload = new DragonflyDojiDb
                    {
                        Ticker = verificationCandel.Ticker,
                        TickerId = verificationCandel.TickerId,
                        Exchange = verificationCandel.Exchange,
                        IsDragonflyDojiDetected = true,
                        DetectionRange = 10,
                        DetectionTime = verificationCandel.CloseTime,
                        DragonflyDojiCandels = null
                    };

                    if ((verificationCandel.CloseTime.Minute - verificationCandel.OpenTime.Minute) > 9)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/DragonflyDoji",
                                                              new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 15)
                {
                    DragonflyDojiDb Payload = new DragonflyDojiDb
                    {
                        Ticker = verificationCandel.Ticker,
                        TickerId = verificationCandel.TickerId,
                        Exchange = verificationCandel.Exchange,
                        IsDragonflyDojiDetected = true,
                        DetectionRange = 15,
                        DetectionTime = verificationCandel.CloseTime,
                        DragonflyDojiCandels = null
                    };

                    if ((verificationCandel.CloseTime.Minute - verificationCandel.OpenTime.Minute) > 14)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/DragonflyDoji",
                                                              new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
            }
        }


        public async Task Analyze1MinCandelAsync(string symboltoken, HttpClient _httpClient, CancellationToken stoppingToken , string authToken)
        {
            var symbolTokenValue = symboltoken; // Replace with the correct value from stock
            var token = authToken; // Replace with the actual token
            var startDate = DateTime.Now.AddMinutes(-3); // Example start date
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
                    DragonFlyDojiAnalyzerLogic(candels, _httpClient, 1);
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
                    DragonFlyDojiAnalyzerLogic(candels, _httpClient, 5);
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
                    DragonFlyDojiAnalyzerLogic(candels, _httpClient, 10);
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
                    DragonFlyDojiAnalyzerLogic(candels, _httpClient, 15);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }


    }
}
