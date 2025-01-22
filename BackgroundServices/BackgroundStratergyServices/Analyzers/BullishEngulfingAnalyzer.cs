using Newtonsoft.Json;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Bullish_Engulfing;
using System.Text;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers
{
    public class BullishEngulfingAnalyzer
    {
        public async void BullishEngulfingAnalyzerLogic(List<Candel> candelList, HttpClient _httpClient, int Range)
        {
            // Ensure there are at least 2 candles in the list
            if (candelList.Count < 2)
            {
                Console.WriteLine("The list must contain at least 2 candles.");
                return;
            }

            // Get the last two candles (most recent and the previous one)
            List<Candel> recentTwoCandles = candelList.OrderByDescending(c => c.CloseTime).Take(2).ToList();

            Candel BFfirst = recentTwoCandles[1];
            Candel BFSecond = recentTwoCandles[0];

            Candel currentCandle = BFSecond;

            // Check if the previous candle is bearish
            bool isPreviousBearish = BFfirst.EndPrice < BFfirst.StartPrice;

            // Check if the current candle is bullish
            bool isCurrentBullish = BFSecond.EndPrice > BFSecond.StartPrice;

            // Check if the current candle's body engulfs the previous candle's body
            bool isEngulfingBody =
                BFSecond.StartPrice < BFfirst.EndPrice && // Current start below previous end
                BFSecond.EndPrice > BFfirst.StartPrice;  // Current end above previous start


            // Combine all conditions to detect the Bullish Engulfing pattern
            if
             (
              isPreviousBearish
              && isCurrentBullish
              && isEngulfingBody
              //&& BFisFit
              )
            {
                if (Range == 1)
                {
                    BullishEngulfingDb Payload = new BullishEngulfingDb
                    {
                        Ticker = currentCandle.Ticker,
                        TickerId = currentCandle.TickerId,
                        Exchange = currentCandle.Exchange,
                        IsBullishEngulfingDetected = true,
                        DetectionRange = 1,
                        DetectionTime = currentCandle.CloseTime,
                        BullishEngulfingCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/BullishEngulfing",
                                      new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 5)
                {
                    BullishEngulfingDb Payload = new BullishEngulfingDb
                    {
                        Ticker = currentCandle.Ticker,
                        TickerId = currentCandle.TickerId,
                        Exchange = currentCandle.Exchange,
                        IsBullishEngulfingDetected = true,
                        DetectionRange = 5,
                        DetectionTime = currentCandle.CloseTime,
                        BullishEngulfingCandels = null
                    };

                    if ((currentCandle.CloseTime.Minute - currentCandle.OpenTime.Minute) > 4)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/BullishEngulfing",
                                      new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 10)
                {
                    BullishEngulfingDb Payload = new BullishEngulfingDb
                    {
                        Ticker = currentCandle.Ticker,
                        TickerId = currentCandle.TickerId,
                        Exchange = currentCandle.Exchange,
                        IsBullishEngulfingDetected = true,
                        DetectionRange = 10,
                        DetectionTime = currentCandle.CloseTime,
                        BullishEngulfingCandels = null
                    };

                    if ((currentCandle.CloseTime.Minute - currentCandle.OpenTime.Minute) > 9)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/BullishEngulfing",
                                      new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 15)
                {
                    BullishEngulfingDb Payload = new BullishEngulfingDb
                    {
                        Ticker = currentCandle.Ticker,
                        TickerId = currentCandle.TickerId,
                        Exchange = currentCandle.Exchange,
                        IsBullishEngulfingDetected = true,
                        DetectionRange = 15,
                        DetectionTime = currentCandle.CloseTime,
                        BullishEngulfingCandels = null
                    };

                    if ((currentCandle.CloseTime.Minute - currentCandle.OpenTime.Minute) > 14)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/BullishEngulfing",
                                      new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
            }
            else
            {
                Console.WriteLine("No Bullish Engulfing pattern detected.");
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
                    BullishEngulfingAnalyzerLogic(candels, _httpClient, 1);
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
                    BullishEngulfingAnalyzerLogic(candels, _httpClient, 5);
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
                    BullishEngulfingAnalyzerLogic(candels, _httpClient, 10);
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
                    BullishEngulfingAnalyzerLogic(candels, _httpClient, 15);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }



    }
}
