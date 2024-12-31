using Newtonsoft.Json;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Breakaway__Bullish_;
using StockLogger.Models.Stratergic_Models.Tweezer_Bottom;
using System.Text;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers
{
    public class BreakawayAnalyzer
    {
        public async void AnalyzerLogic(List<Candel> candelList, HttpClient _httpClient, int Range)
        {
            // Ensure there are at least 5 candles in the list
            if (candelList.Count < 5)
            {
                Console.WriteLine("The list must contain at least 5 candles.");
                return;
            }

            // Get the last (most recent) candle
            Candel latestCandel = candelList.OrderByDescending(c => c.CloseTime).First();

            if (latestCandel.CloseTime.Second < 58)
            {
                return;
            }

            // Get the last 5 candles (most recent)
            List<Candel> lastFiveCandles = candelList.OrderByDescending(c => c.CloseTime).Take(5).ToList();

            // Reverse the list for chronological order
            lastFiveCandles.Reverse();

            // Check the conditions for a bullish breakaway pattern
            bool firstCandleBearish = (bool)lastFiveCandles[0].IsBearish;
            bool secondCandleGapDown = lastFiveCandles[1].StartPrice < lastFiveCandles[0].EndPrice;

            bool thirdCandleIndecisive = Math.Abs(lastFiveCandles[2].EndPrice - lastFiveCandles[2].StartPrice) <
                                         (lastFiveCandles[2].HighestPrice - lastFiveCandles[2].LowestPrice) * 0.3m;

            bool fourthCandleIndecisive = Math.Abs(lastFiveCandles[3].EndPrice - lastFiveCandles[3].StartPrice) <
                                          (lastFiveCandles[3].HighestPrice - lastFiveCandles[3].LowestPrice) * 0.3m;

            bool fifthCandleBullish = (bool)lastFiveCandles[4].IsBullish;

            bool fifthCandleClosesWithinFirst = lastFiveCandles[4].EndPrice >= lastFiveCandles[0].StartPrice;

            // Analyze the pattern
            if (firstCandleBearish &&
                secondCandleGapDown &&
                thirdCandleIndecisive &&
                fourthCandleIndecisive &&
                fifthCandleBullish &&
                fifthCandleClosesWithinFirst)
            {
                if (Range == 1)
                {
                    BreakawayDb Payload = new BreakawayDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsBreakawayDetected = true,
                        DetectionRange = 1,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Breakaway",
                                        new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 5)
                {
                    BreakawayDb Payload = new BreakawayDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsBreakawayDetected = true,
                        DetectionRange = 5,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 4)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Breakaway",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 10)
                {
                    BreakawayDb Payload = new BreakawayDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsBreakawayDetected = true,
                        DetectionRange = 10,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 9)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Breakaway",
                                            new StringContent(JsonConvert.SerializeObject(Payload), Encoding.UTF8, "application/json"));
                    }
                }
                else if (Range == 15)
                {
                    BreakawayDb Payload = new BreakawayDb
                    {
                        Ticker = latestCandel.Ticker,
                        TickerId = latestCandel.TickerId,
                        Exchange = latestCandel.Exchange,
                        IsBreakawayDetected = true,
                        DetectionRange = 15,
                        DetectionTime = latestCandel.CloseTime,
                        Candels = null
                    };

                    if ((latestCandel.CloseTime.Minute - latestCandel.OpenTime.Minute) > 14)
                    {
                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Breakaway",
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
