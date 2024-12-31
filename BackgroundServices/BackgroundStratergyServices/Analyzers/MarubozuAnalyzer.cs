using Newtonsoft.Json;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Marubozu__Bullish_;
using System.Text;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers
{
    public class MarubozuAnalyzer
    {
        public async void AnalyzerLogic(List<Candel> candelList, HttpClient _httpClient, int Range)
        {
            // Ensure there is at least one candle in the list
            if (candelList.Count == 0)
            {
                Console.WriteLine("The list must contain at least one candle.");
                return;
            }

            // Get the last (most recent) candle
            Candel latestCandel = candelList.OrderByDescending(c => c.CloseTime).First();

            if (latestCandel.CloseTime.Second < 58)
            {
                return;
            }

            // Get the most recent candle
            Candel latestCandle = candelList.OrderByDescending(c => c.CloseTime).FirstOrDefault();

            // Check for Bullish Marubozu characteristics

            // 1. Ensure the candle is bullish
            bool isBullish = (bool)latestCandle.IsBullish;

            // 2. Small or non-existent upper shadow
            bool smallUpperShadow = (latestCandle.HighestPrice - latestCandle.EndPrice) <
                                    (latestCandle.EndPrice - latestCandle.StartPrice) * 0.1m; // Adjust threshold as needed

            // 3. Small or non-existent lower shadow
            bool smallLowerShadow = (latestCandle.StartPrice - latestCandle.LowestPrice) <
                                    (latestCandle.EndPrice - latestCandle.StartPrice) * 0.1m; // Adjust threshold as needed

            // 4. A strong body (dominant price movement)
            decimal totalRange = latestCandle.HighestPrice - latestCandle.LowestPrice;
            bool strongBody = totalRange != 0 &&
                              (latestCandle.EndPrice - latestCandle.StartPrice) / totalRange > 0.9m;

            // Analyze pattern
            if (isBullish && smallUpperShadow && smallLowerShadow && strongBody)
            {
                if (Range == 1)
                {
                    MarubozuDb Payload = new MarubozuDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsMarubozuDetected = true,
                        DetectionRange = 1,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Marubozu",
                                        new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 5)
                {
                    MarubozuDb Payload = new MarubozuDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsMarubozuDetected = true,
                        DetectionRange = 5,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 4)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Marubozu",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 10)
                {
                    MarubozuDb Payload = new MarubozuDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsMarubozuDetected = true,
                        DetectionRange = 10,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 9)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Marubozu",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 15)
                {
                    MarubozuDb Payload = new MarubozuDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsMarubozuDetected = true,
                        DetectionRange = 15,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 14)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Marubozu",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
            }
            else
            {
                Console.WriteLine("Tweezer Bottom pattern not detected.");
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
                    AnalyzerLogic(candels, _httpClient, 1);
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
                    AnalyzerLogic(candels, _httpClient, 5);
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
                    AnalyzerLogic(candels, _httpClient, 10);
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
                    AnalyzerLogic(candels, _httpClient, 15);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }

    }
}
