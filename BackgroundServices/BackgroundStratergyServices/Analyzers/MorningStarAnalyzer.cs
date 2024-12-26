using Newtonsoft.Json;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Inverted_Hammer;
using StockLogger.Models.Stratergic_Models.Morning_Star;
using System.Net.Http;
using System.Text;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers
{
    public class MorningStarAnalyzer
    {
        public async void MorningStarAnalyzerLogic(List<Candel> candelList, HttpClient _httpClient, int Range)
        {
            // Ensure there are at least 3 candles in the list
            if (candelList.Count < 3)
            {
                Console.WriteLine("The list must contain at least 3 candles.");
                return;
            }

            // Get the last 3 candles (most recent)
            List<Candel> recentThreeCandles = candelList.OrderByDescending(c => c.CloseTime).Take(3).ToList();

            // Get the latest candel (top-most from the recentThreeCandles list)
            Candel latestCandel = recentThreeCandles.FirstOrDefault();

            if (latestCandel.CloseTime.Second < 59)
            {
                return;
            }

            // Condition 1: The first candle should be bearish with a large body
            bool firstBearish = recentThreeCandles[2].IsBearish == true &&
                                (recentThreeCandles[2].StartPrice - recentThreeCandles[2].EndPrice) >
                                (recentThreeCandles[2].HighestPrice - recentThreeCandles[2].LowestPrice) * 0.6m;

            // Condition 2: The second candle should be a small-bodied candle (indecision)
            decimal secondBodySize = Math.Abs(recentThreeCandles[1].EndPrice - recentThreeCandles[1].StartPrice);
            decimal secondRange = recentThreeCandles[1].HighestPrice - recentThreeCandles[1].LowestPrice;
            bool secondIndecision = secondRange != 0 && (secondBodySize / secondRange) <= 0.3m;

            // Condition 3: The third candle should be bullish with a large body
            bool thirdBullish = recentThreeCandles[0].IsBullish == true &&
                                (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice) >
                                (recentThreeCandles[0].HighestPrice - recentThreeCandles[0].LowestPrice) * 0.6m;

            // Condition 4: The third candle should close above the midpoint of the first candle
            decimal firstMidpoint = (recentThreeCandles[2].StartPrice + recentThreeCandles[2].EndPrice) / 2;
            bool thirdClosesAboveMidpoint = recentThreeCandles[0].EndPrice > firstMidpoint;

            // Combine all conditions to detect the Morning Star pattern
            if (firstBearish && secondIndecision && thirdBullish && thirdClosesAboveMidpoint)
            {
                if (Range == 1)
                {
                    MorningStarDb Payload = new MorningStarDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsMorningStarDetected = true,
                        DetectionRange = 1,
                        DetectionTime = latestCandel.CloseTime,
                        MorningStarCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/MorningStarDb",
                                      new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 5)
                {
                    MorningStarDb Payload = new MorningStarDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsMorningStarDetected = true,
                        DetectionRange = 5,
                        DetectionTime = latestCandel.CloseTime,
                        MorningStarCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/MorningStarDb",
                                      new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 10)
                {
                    MorningStarDb Payload = new MorningStarDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsMorningStarDetected = true,
                        DetectionRange = 10,
                        DetectionTime = latestCandel.CloseTime,
                        MorningStarCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/MorningStarDb",
                                      new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 15)
                {
                    MorningStarDb Payload = new MorningStarDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsMorningStarDetected = true,
                        DetectionRange = 15,
                        DetectionTime = latestCandel.CloseTime,
                        MorningStarCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/MorningStarDb",
                                      new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                }
            }
            else
            {
                Console.WriteLine("No Morning Star pattern detected.");
            }
        }


        public async Task AnalyzeMorningStarAsync(string ticker, HttpClient _httpClient, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/GetCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    MorningStarAnalyzerLogic(candels, _httpClient , 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }

        public async Task Analyze5MinCandelMorningStarAsync(string ticker, HttpClient _httpClient, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get5MinCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    MorningStarAnalyzerLogic(candels, _httpClient , 5);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }

        public async Task Analyze10MinCandelMorningStarAsync(string ticker, HttpClient _httpClient, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get10MinCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    MorningStarAnalyzerLogic(candels, _httpClient , 10);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }

        public async Task Analyze15MinCandelMorningStarAsync(string ticker, HttpClient _httpClient, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get15MinCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    MorningStarAnalyzerLogic(candels, _httpClient , 15);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }



    }
}
