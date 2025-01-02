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
            if (candelList.Count < 1)
            {
                Console.WriteLine("The list must contain at least 2 candles.");
                return;
            }

            // Get the most recent candle
            Candel recentCandel = candelList.OrderByDescending(c => c.CloseTime).FirstOrDefault();

            // Check if it is a Doji
            bool isDoji = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.1m;

            // Check for a long lower shadow (the shadow should be at least twice the size of the body)
            bool longLowerShadow = (recentCandel.StartPrice - recentCandel.LowestPrice) > 2 * (recentCandel.EndPrice - recentCandel.StartPrice);

            // The body of the candle should be at the top of the range
            bool smallBodyAtTop = Math.Abs(recentCandel.StartPrice - recentCandel.EndPrice) < (recentCandel.HighestPrice - recentCandel.LowestPrice) * 0.3m;

            //// Check if the last candle is followed by a bullish candle (suggesting potential for a bullish trend)
            //bool bullishFollowUp = candelList.Count > 1 && candelList[1].IsBullish == true;

            // Combine all conditions to detect the Dragonfly Doji Bullish pattern
            //if (isDoji && longLowerShadow && smallBodyAtTop && bullishFollowUp)
            if (isDoji && longLowerShadow && smallBodyAtTop)
            {
                if (Range == 1)
                {
                    DragonflyDojiDb Payload = new DragonflyDojiDb
                    {
                        Ticker = recentCandel.Ticker,
                        TickerId = recentCandel.TickerId,
                        Exchange = recentCandel.Exchange,
                        IsDragonflyDojiDetected = true,
                        DetectionRange = 1,
                        DetectionTime = recentCandel.CloseTime,
                        DragonflyDojiCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/DragonflyDoji",
                                      new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 5)
                {
                    DragonflyDojiDb Payload = new DragonflyDojiDb
                    {
                        Ticker = recentCandel.Ticker,
                        TickerId = recentCandel.TickerId,
                        Exchange = recentCandel.Exchange,
                        IsDragonflyDojiDetected = true,
                        DetectionRange = 5,
                        DetectionTime = recentCandel.CloseTime,
                        DragonflyDojiCandels = null
                    };
                    if ((recentCandel.CloseTime.Minute - recentCandel.OpenTime.Minute) > 4)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/DragonflyDoji",
                                                              new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 10)
                {
                    DragonflyDojiDb Payload = new DragonflyDojiDb
                    {
                        Ticker = recentCandel.Ticker,
                        TickerId = recentCandel.TickerId,
                        Exchange = recentCandel.Exchange,
                        IsDragonflyDojiDetected = true,
                        DetectionRange = 10,
                        DetectionTime = recentCandel.CloseTime,
                        DragonflyDojiCandels = null
                    };

                    if ((recentCandel.CloseTime.Minute - recentCandel.OpenTime.Minute) > 9)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/DragonflyDoji",
                                                              new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 15)
                {
                    DragonflyDojiDb Payload = new DragonflyDojiDb
                    {
                        Ticker = recentCandel.Ticker,
                        TickerId = recentCandel.TickerId,
                        Exchange = recentCandel.Exchange,
                        IsDragonflyDojiDetected = true,
                        DetectionRange = 15,
                        DetectionTime = recentCandel.CloseTime,
                        DragonflyDojiCandels = null
                    };

                    if ((recentCandel.CloseTime.Minute - recentCandel.OpenTime.Minute) > 14)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/DragonflyDoji",
                                                              new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
            }
        }


        public async Task Analyze1MinCandelAsync(string ticker, HttpClient _httpClient, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/GetCandel?ticker={ticker}", stoppingToken);
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
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
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
