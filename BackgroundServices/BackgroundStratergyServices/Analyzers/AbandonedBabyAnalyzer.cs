using Newtonsoft.Json;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Breakaway__Bullish_;
using StockLogger.Models.Stratergic_Models.Bullish_Abandoned_Baby;
using System.Text;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers
{
    public class AbandonedBabyAnalyzer
    {
        public async void AnalyzerLogic(List<Candel> candelList, HttpClient _httpClient, int Range)
        {
            // Ensure there are at least 3 candles in the list
            if (candelList.Count < 3)
            {
                Console.WriteLine("The list must contain at least 3 candles.");
                return;
            }

            // Get the last (most recent) candle
            Candel latestCandel = candelList.OrderByDescending(c => c.CloseTime).First();

            if (latestCandel.CloseTime.Second < 58)
            {
                return;
            }

            // Get the last 3 candles from the list (most recent 3)
            List<Candel> recentThreeCandles = candelList.OrderByDescending(c => c.CloseTime).Take(3).ToList();

            Candel firstCandle = recentThreeCandles[2];
            Candel secondCandle = recentThreeCandles[1];
            Candel thirdCandle = recentThreeCandles[0];

            // Validate the first candle: bearish with a significant body
            bool isFirstCandleBearish = (firstCandle.IsBearish ?? false) &&
                                        (firstCandle.StartPrice - firstCandle.EndPrice) / (firstCandle.HighestPrice - firstCandle.LowestPrice) > 0.6m;

            // Validate the second candle: small body (doji/spinning top) with gaps
            bool isSecondCandleSmall = Math.Abs(secondCandle.StartPrice - secondCandle.EndPrice) <
                                       0.1m * (secondCandle.HighestPrice - secondCandle.LowestPrice);
            bool isSecondCandleGaps = secondCandle.HighestPrice < firstCandle.LowestPrice;

            // Validate the third candle: bullish with a significant body and gaps
            bool isThirdCandleBullish = (thirdCandle.IsBullish ?? false) &&
                                        (thirdCandle.EndPrice - thirdCandle.StartPrice) / (thirdCandle.HighestPrice - thirdCandle.LowestPrice) > 0.6m;
            bool isThirdCandleGaps = thirdCandle.LowestPrice > secondCandle.HighestPrice;

            // Check for pattern match
            if (isFirstCandleBearish && isSecondCandleSmall && isSecondCandleGaps && isThirdCandleBullish && isThirdCandleGaps)
            {
                if (Range == 1)
                {
                    AbandonedBabyDb Payload = new AbandonedBabyDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsAbandonedBabyDetected = true,
                        DetectionRange = 1,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/AbandonedBaby",
                                        new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 5)
                {
                    AbandonedBabyDb Payload = new AbandonedBabyDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsAbandonedBabyDetected = true,
                        DetectionRange = 5,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 4)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/AbandonedBaby",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 10)
                {
                    AbandonedBabyDb Payload = new AbandonedBabyDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsAbandonedBabyDetected = true,
                        DetectionRange = 10,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 9)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/AbandonedBaby",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 15)
                {
                    AbandonedBabyDb Payload = new AbandonedBabyDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsAbandonedBabyDetected = true,
                        DetectionRange = 15,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 14)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/AbandonedBaby",
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
