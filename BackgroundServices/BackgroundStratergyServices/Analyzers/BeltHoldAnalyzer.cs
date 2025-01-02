using Newtonsoft.Json;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Breakaway__Bullish_;
using StockLogger.Models.Stratergic_Models.Bullish_Belt_Hold;
using System.Text;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers
{
    public class BeltHoldAnalyzer
    {
        public async void AnalyzerLogic(List<Candel> candelList, HttpClient _httpClient, int Range)
        {
            // Ensure there is at least one candle to analyze
            if (candelList.Count < 2)
            {
                Console.WriteLine("The list must contain at least 2 candles.");
                return;
            }

            // Get the last (most recent) candle
            Candel latestCandel = candelList.OrderByDescending(c => c.CloseTime).First();

            // Get the last two candles: most recent and the one before
            Candel latestCandle = candelList.OrderByDescending(c => c.CloseTime).FirstOrDefault();
            Candel previousCandle = candelList.OrderByDescending(c => c.CloseTime).Skip(1).FirstOrDefault();

            // Check if the latest candle is bullish
            bool isBullish = (latestCandle.IsBullish ?? false);

            // Check if the latest candle opened at or near its low (minimal lower shadow)
            bool minimalLowerShadow = (latestCandle.StartPrice - latestCandle.LowestPrice) <=
                                      0.02m * (latestCandle.HighestPrice - latestCandle.LowestPrice);

            // Check if the latest candle's body is relatively large
            bool largeBody = (latestCandle.EndPrice - latestCandle.StartPrice) >=
                             0.7m * (latestCandle.HighestPrice - latestCandle.LowestPrice);

            // Check if the previous candle's trend is bearish or neutral
            bool isPreviousBearish = previousCandle.IsBearish ?? false;

            // Check for pattern match
            if (isBullish && minimalLowerShadow && largeBody && isPreviousBearish)
            {
                if (Range == 1)
                {
                    BeltHoldDb Payload = new BeltHoldDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsBeltHoldDetected = true,
                        DetectionRange = 1,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/BeltHold",
                                        new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 5)
                {
                    BeltHoldDb Payload = new BeltHoldDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsBeltHoldDetected = true,
                        DetectionRange = 5,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 4)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/BeltHold",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 10)
                {
                    BeltHoldDb Payload = new BeltHoldDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsBeltHoldDetected = true,
                        DetectionRange = 10,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 9)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/BeltHold",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 15)
                {
                    BeltHoldDb Payload = new BeltHoldDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsBeltHoldDetected = true,
                        DetectionRange = 15,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 14)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/BeltHold",
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
