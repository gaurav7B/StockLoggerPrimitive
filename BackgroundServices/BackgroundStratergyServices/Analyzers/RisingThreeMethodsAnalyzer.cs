using Newtonsoft.Json;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Rising_Three_Methods;
using System.Text;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers
{
    public class RisingThreeMethodsAnalyzer
    {
        public async void RisingThreeMethodsAnalyzerLogic(List<Candel> candelList, HttpClient _httpClient, int Range)
        {
            // Ensure there are at least 5 candles in the list
            if (candelList.Count < 5)
            {
                Console.WriteLine("The list must contain at least 5 candles.");
                return;
            }

            // Get the last (most recent) candle
            Candel latestCandel = candelList.OrderByDescending(c => c.CloseTime).First();

            // Get the last 5 candles from the list
            List<Candel> lastFiveCandles = candelList.OrderByDescending(c => c.CloseTime).Take(5).ToList();

            // Identify each candle for clarity
            Candel firstBullishCandle = lastFiveCandles[4]; // First candle
            List<Candel> middleThreeCandles = lastFiveCandles.GetRange(1, 3); // Middle three candles
            Candel finalBullishCandle = lastFiveCandles[0]; // Last candle

            // Check if the first candle is strongly bullish
            bool firstBullish = firstBullishCandle.IsBullish == true;

            // Check if the three middle candles are bearish or neutral and stay within the range of the first bullish candle
            bool middleCandlesWithinRange = middleThreeCandles.All(c =>
                c.HighestPrice <= firstBullishCandle.HighestPrice &&
                c.LowestPrice >= firstBullishCandle.LowestPrice &&
                (c.IsBearish == true || c.IsBullish == null)
            );

            // Check if the final candle is strongly bullish and closes above the first candle's close
            bool finalBullish = finalBullishCandle.IsBullish == true &&
                                finalBullishCandle.EndPrice > firstBullishCandle.EndPrice;

            // Combine all conditions to detect the Rising Three Methods pattern
            if (firstBullish && middleCandlesWithinRange && finalBullish)
            {
                if (Range == 1)
                {
                    RisingThreeMethodsDb Payload = new RisingThreeMethodsDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsRisingThreeMethodsDetected = true,
                        DetectionRange = 1,
                        DetectionTime = latestCandel.CloseTime,
                        RisingThreeMethodsCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/RisingThreeMethods",
                                        new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 5)
                {
                    RisingThreeMethodsDb Payload = new RisingThreeMethodsDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsRisingThreeMethodsDetected = true,
                        DetectionRange = 5,
                        DetectionTime = latestCandel.CloseTime,
                        RisingThreeMethodsCandels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 4)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/RisingThreeMethods",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 10)
                {
                    RisingThreeMethodsDb Payload = new RisingThreeMethodsDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsRisingThreeMethodsDetected = true,
                        DetectionRange = 10,
                        DetectionTime = latestCandel.CloseTime,
                        RisingThreeMethodsCandels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 9)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/RisingThreeMethods",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 15)
                {
                    RisingThreeMethodsDb Payload = new RisingThreeMethodsDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsRisingThreeMethodsDetected = true,
                        DetectionRange = 15,
                        DetectionTime = latestCandel.CloseTime,
                        RisingThreeMethodsCandels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 14)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/RisingThreeMethods",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
            }
            else
            {
                Console.WriteLine("Rising Three Methods pattern not detected.");
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
                    RisingThreeMethodsAnalyzerLogic(candels, _httpClient, 1);
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
                    RisingThreeMethodsAnalyzerLogic(candels, _httpClient, 5);
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
                    RisingThreeMethodsAnalyzerLogic(candels, _httpClient, 10);
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
                    RisingThreeMethodsAnalyzerLogic(candels, _httpClient, 15);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }




    }
}
