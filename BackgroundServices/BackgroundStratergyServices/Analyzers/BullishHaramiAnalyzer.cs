using Newtonsoft.Json;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Bullish_Engulfing;
using StockLogger.Models.Stratergic_Models.Bullish_Harami;
using System.Text;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers
{
    public class BullishHaramiAnalyzer
    {
        public async void BullishHaramiAnalyzerLogic(List<Candel> candelList, HttpClient _httpClient, int Range)
        {
            // Ensure there are at least 2 candles in the list
            if (candelList.Count < 2)
            {
                Console.WriteLine("The list must contain at least 2 candles.");
                return;
            }

            // Get the last 2 candles from the list (the most recent 2)
            List<Candel> recentTwoCandles = candelList.OrderByDescending(c => c.CloseTime).Take(2).ToList();

            Candel firstCandle = recentTwoCandles[1]; // The larger, previous candle
            Candel secondCandle = recentTwoCandles[0]; // The smaller, current candle

            Candel currentCandle = recentTwoCandles[0];

            if (currentCandle.CloseTime.Second < 59)
            {
                return;
            }

            // Check if the first candle is bearish
            bool firstCandleBearish = firstCandle.IsBearish == true;

            // Check if the second candle is bullish
            bool secondCandleBullish = secondCandle.IsBullish == true;

            // Check if the second candle's body is within the first candle's body
            bool secondCandleWithinFirstCandle = secondCandle.StartPrice > firstCandle.EndPrice &&
                                                 secondCandle.EndPrice < firstCandle.StartPrice;

            // Combine all conditions to detect the Bullish Harami pattern
            if (firstCandleBearish && secondCandleBullish && secondCandleWithinFirstCandle)
            {
                if (Range == 1)
                {
                    BullishHaramiDb Payload = new BullishHaramiDb
                    {
                        Ticker = currentCandle.Ticker,
                        TickerId = currentCandle.TickerId,
                        Exchange = currentCandle.Exchange,
                        IsBullishHaramiDetected = true,
                        DetectionRange = 1,
                        DetectionTime = currentCandle.CloseTime,
                        BullishHaramiCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/BullishHarami",
                                        new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 5)
                {
                    BullishHaramiDb Payload = new BullishHaramiDb
                    {
                        Ticker = currentCandle.Ticker,
                        TickerId = currentCandle.TickerId,
                        Exchange = currentCandle.Exchange,
                        IsBullishHaramiDetected = true,
                        DetectionRange = 5,
                        DetectionTime = currentCandle.CloseTime,
                        BullishHaramiCandels = null
                    };

                    if ((currentCandle.CloseTime.Minute - currentCandle.OpenTime.Minute) > 4)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/BullishHarami",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 10)
                {
                    BullishHaramiDb Payload = new BullishHaramiDb
                    {
                        Ticker = currentCandle.Ticker,
                        TickerId = currentCandle.TickerId,
                        Exchange = currentCandle.Exchange,
                        IsBullishHaramiDetected = true,
                        DetectionRange = 10,
                        DetectionTime = currentCandle.CloseTime,
                        BullishHaramiCandels = null
                    };

                    if ((currentCandle.CloseTime.Minute - currentCandle.OpenTime.Minute) > 9)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/BullishHarami",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 15)
                {
                    BullishHaramiDb Payload = new BullishHaramiDb
                    {
                        Ticker = currentCandle.Ticker,
                        TickerId = currentCandle.TickerId,
                        Exchange = currentCandle.Exchange,
                        IsBullishHaramiDetected = true,
                        DetectionRange = 15,
                        DetectionTime = currentCandle.CloseTime,
                        BullishHaramiCandels = null
                    };

                    if ((currentCandle.CloseTime.Minute - currentCandle.OpenTime.Minute) > 14)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/BullishHarami",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
            }
            else
            {
                Console.WriteLine("No Bullish Harami pattern detected.");
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
                    BullishHaramiAnalyzerLogic(candels, _httpClient, 1);
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
                    BullishHaramiAnalyzerLogic(candels, _httpClient, 5);
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
                    BullishHaramiAnalyzerLogic(candels, _httpClient, 10);
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
                    BullishHaramiAnalyzerLogic(candels, _httpClient, 15);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }


    }
}
