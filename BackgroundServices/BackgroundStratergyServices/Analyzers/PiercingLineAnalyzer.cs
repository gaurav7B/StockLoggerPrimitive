using Newtonsoft.Json;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Hammer;
using StockLogger.Models.Stratergic_Models.Piercing_Line;
using System.Text;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers
{
    public class PiercingLineAnalyzer
    {
        public async void PiercingLineAnalyzerLogic(List<Candel> candelList, HttpClient _httpClient, int Range)
        {
            // Ensure there are at least 2 candles in the list
            if (candelList.Count < 2)
            {
                Console.WriteLine("The list must contain at least 2 candles.");
                return;
            }

            // Get the last 2 candles
            List<Candel> recentTwoCandles = candelList.OrderByDescending(c => c.CloseTime).Take(2).ToList();

            Candel firstCandle = recentTwoCandles[1];
            Candel secondCandle = recentTwoCandles[0];

            // Get the last (most recent) candle
            Candel latestCandel = candelList.OrderByDescending(c => c.CloseTime).First();

            if (latestCandel.CloseTime.Second < 58)
            {
                return;
            }

            // Check conditions for Piercing Line pattern
            bool firstCandleBearish = firstCandle.IsBearish == true;
            bool secondCandleBullish = secondCandle.IsBullish == true;
            bool secondOpensBelowFirstClose = secondCandle.StartPrice < firstCandle.EndPrice;
            bool secondClosesMoreThanHalfway = secondCandle.EndPrice > (firstCandle.StartPrice + firstCandle.EndPrice) / 2;

            // Combine all conditions
            if (firstCandleBearish && secondCandleBullish && secondOpensBelowFirstClose && secondClosesMoreThanHalfway)
            {
                if (Range == 1)
                {
                    PiercingLineDb Payload = new PiercingLineDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsPiercingLineDetected = true,
                        DetectionRange = 1,
                        DetectionTime = latestCandel.CloseTime,
                        PiercingLineCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/PiercingLine",
                                        new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 5)
                {
                    PiercingLineDb Payload = new PiercingLineDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsPiercingLineDetected = true,
                        DetectionRange = 5,
                        DetectionTime = latestCandel.CloseTime,
                        PiercingLineCandels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 4)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/PiercingLine",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 10)
                {
                    PiercingLineDb Payload = new PiercingLineDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsPiercingLineDetected = true,
                        DetectionRange = 10,
                        DetectionTime = latestCandel.CloseTime,
                        PiercingLineCandels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 9)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/PiercingLine",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 15)
                {
                    PiercingLineDb Payload = new PiercingLineDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsPiercingLineDetected = true,
                        DetectionRange = 15,
                        DetectionTime = latestCandel.CloseTime,
                        PiercingLineCandels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 14)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/PiercingLine",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
            }
            else
            {
                Console.WriteLine("No Piercing Line pattern detected.");
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
                    PiercingLineAnalyzerLogic(candels, _httpClient, 1);
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
                    PiercingLineAnalyzerLogic(candels, _httpClient, 5);
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
                    PiercingLineAnalyzerLogic(candels, _httpClient, 10);
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
                    PiercingLineAnalyzerLogic(candels, _httpClient, 15);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }


    }
}
