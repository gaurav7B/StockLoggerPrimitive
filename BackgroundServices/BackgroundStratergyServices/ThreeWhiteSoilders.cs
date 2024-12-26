using Newtonsoft.Json;
using StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models;
using StockLogger.Models.Stratergic_Models.Inverted_Hammer;
using System.Diagnostics;
using System.Net.Http;
using System.Text;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices
{
    public class ThreeWhiteSoilders : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string ticker, string exchange, string name, long id)> _stocks;
        public List<List<Candel>> MasterCandelListFor1MinCandel3WS = new List<List<Candel>>();
        public List<List<Candel>> MasterCandelListFor5MinCandel3WS = new List<List<Candel>>();
        public List<List<Candel>> MasterCandelListFor10MinCandel3WS = new List<List<Candel>>();
        public List<List<Candel>> MasterCandelListFor15MinCandel3WS = new List<List<Candel>>();

        private readonly MorningStarAnalyzer _morningStarAnalyzer;

        public ThreeWhiteSoilders(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _morningStarAnalyzer = new MorningStarAnalyzer();

            _stocks = new List<(string, string, string, long)>
            {
                ("INFY", "NSE", "Infosys", 1),
                ("RELIANCE", "NSE", "Reliance Industries", 2),
                ("TCS", "NSE", "Tata Consultancy Services", 3),
                ("HDFCBANK", "NSE", "HDFC Bank", 4),
                ("ICICIBANK", "NSE", "ICICI Bank", 5),
                ("HINDUNILVR", "NSE", "Hindustan Unilever", 6),
                ("ITC", "NSE", "ITC Limited", 7),
                ("KOTAKBANK", "NSE", "Kotak Mahindra Bank", 8),
                ("LT", "NSE", "Larsen & Toubro", 9),
                ("SBIN", "NSE", "State Bank of India", 10),
                ("AXISBANK", "NSE", "Axis Bank", 11),
                ("BAJFINANCE", "NSE", "Bajaj Finance", 12),
                ("BHARTIARTL", "NSE", "Bharti Airtel", 13),
                ("HCLTECH", "NSE", "HCL Technologies", 14),
                ("ASIANPAINT", "NSE", "Asian Paints", 15),
                ("DMART", "NSE", "Avenue Supermarts", 16),
                ("MARUTI", "NSE", "Maruti Suzuki India", 17),
                ("SUNPHARMA", "NSE", "Sun Pharmaceutical Industries", 18),
                ("NTPC", "NSE", "NTPC Limited", 19),
                ("TITAN", "NSE", "Titan Company", 20),
            };
        }

        public async void ThreeWhiteSoilderAnalyzer(List<Candel> candelList, int Range)
        {
            Candel CandelSample;

            // Ensure there are at least 4 candles in the list
            if (candelList.Count < 4)
            {
                Console.WriteLine("The list must contain at least 4 candles.");
                return;
            }

            // Get the last 3 candles from the list (the most recent 3)
            List<Candel> recentThreeCandles = candelList.OrderByDescending(c => c.CloseTime).Take(3).ToList();

            Candel latestCandel = recentThreeCandles.FirstOrDefault();

            if (latestCandel.CloseTime.Second < 59)
            {
                return;
            }

            // Check if all three candles are bullish
            bool allThreeBullish = recentThreeCandles.All(c => c.IsBullish == true);

            // Check if the three candles close higher than the previous one
            bool progressiveCloses = recentThreeCandles[0].EndPrice > recentThreeCandles[1].EndPrice
                                                                    &&
                                     recentThreeCandles[1].EndPrice > recentThreeCandles[2].EndPrice;

            // Check if the bodies of the candles are progressively larger
            bool increasingBodySize = (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice) > (recentThreeCandles[1].EndPrice - recentThreeCandles[1].StartPrice)
                                                                                                          &&
                                      (recentThreeCandles[1].EndPrice - recentThreeCandles[1].StartPrice) > (recentThreeCandles[2].EndPrice - recentThreeCandles[2].StartPrice);

            // Check for small upper and lower shadows
            bool smallUpperShadow = (recentThreeCandles[0].HighestPrice - recentThreeCandles[0].EndPrice) < (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice);
            bool smallLowerShadow = (recentThreeCandles[0].StartPrice - recentThreeCandles[0].LowestPrice) < (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice);

            // Check if the body is at least 60% of the total range (strong body)
            decimal range = recentThreeCandles[0].HighestPrice - recentThreeCandles[0].LowestPrice;
            bool strongBodyRatio = range != 0 && (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice) / range > 0.6m;

            // Check the 4th previous candle for a potential downtrend or neutral pattern
            bool priorConsolidationOrBearish = candelList.Count > 3 &&
                                               (candelList[3].IsBearish == true || candelList[3].IsBullish == false);

            CandelSample = recentThreeCandles[0];

            // Combine all conditions to detect the Three White Soldiers pattern
            //if (CandelSample != null)
            if (allThreeBullish && progressiveCloses && increasingBodySize && priorConsolidationOrBearish)
            //if (allThreeBullish && progressiveCloses && increasingBodySize && smallUpperShadow && smallLowerShadow &&
            //strongBodyRatio && priorConsolidationOrBearish)
            {
                if (Range == 1)
                {
                    MasterCandelListFor1MinCandel3WS.Add(candelList);
                    ThreeWhiteSoilderDb TWSPayload = new ThreeWhiteSoilderDb
                    {
                        Ticker = CandelSample.Ticker,
                        TickerId = CandelSample.TickerId,
                        Exchange = CandelSample.Exchange,
                        IsThreeWhiteSoilderDetected = true,
                        DetectionRange = 1,
                        DetectionTime = CandelSample.CloseTime,
                        ThreeWhiteSoilderCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/ThreeWhiteSoilderDb",
                                      new StringContent(JsonConvert.SerializeObject(TWSPayload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 5)
                {
                    MasterCandelListFor5MinCandel3WS.Add(candelList);
                    ThreeWhiteSoilderDb TWSPayload = new ThreeWhiteSoilderDb
                    {
                        Ticker = CandelSample.Ticker,
                        TickerId = CandelSample.TickerId,
                        Exchange = CandelSample.Exchange,
                        IsThreeWhiteSoilderDetected = true,
                        DetectionRange = 5,
                        DetectionTime = CandelSample.CloseTime,
                        ThreeWhiteSoilderCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/ThreeWhiteSoilderDb",
                                      new StringContent(JsonConvert.SerializeObject(TWSPayload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 10)
                {
                    MasterCandelListFor10MinCandel3WS.Add(candelList);
                    ThreeWhiteSoilderDb TWSPayload = new ThreeWhiteSoilderDb
                    {
                        Ticker = CandelSample.Ticker,
                        TickerId = CandelSample.TickerId,
                        Exchange = CandelSample.Exchange,
                        IsThreeWhiteSoilderDetected = true,
                        DetectionRange = 10,
                        DetectionTime = CandelSample.CloseTime,
                        ThreeWhiteSoilderCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/ThreeWhiteSoilderDb",
                                      new StringContent(JsonConvert.SerializeObject(TWSPayload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 15)
                {
                    MasterCandelListFor15MinCandel3WS.Add(candelList);
                    ThreeWhiteSoilderDb TWSPayload = new ThreeWhiteSoilderDb
                    {
                        Ticker = CandelSample.Ticker,
                        TickerId = CandelSample.TickerId,
                        Exchange = CandelSample.Exchange,
                        IsThreeWhiteSoilderDetected = true,
                        DetectionRange = 15,
                        DetectionTime = CandelSample.CloseTime,
                        ThreeWhiteSoilderCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/ThreeWhiteSoilderDb",
                                      new StringContent(JsonConvert.SerializeObject(TWSPayload), Encoding.UTF8, "application/json"));
                }
            }
            else
            {
                Console.WriteLine("Three White Soldiers Pattern Not Detected");
            }
        }

        public async void InvertedHammerAnalyzer(List<Candel> candelList, int Range)
        {
            // Ensure there is at least 1 candle in the list for detection
            if (candelList.Count < 1)
            {
                Console.WriteLine("The list must contain at least 1 candle.");
                return;
            }

            // Get the most recent candle
            Candel recentCandle = candelList.OrderByDescending(c => c.CloseTime).FirstOrDefault();

            if (recentCandle == null)
            {
                Console.WriteLine("No candle data available.");
                return;
            }

            if(recentCandle.CloseTime.Second < 59)
            {
                return;
            }

            // Calculate the real body and the shadows
            decimal realBody = Math.Abs(recentCandle.EndPrice - recentCandle.StartPrice);
            decimal upperShadow = recentCandle.HighestPrice - Math.Max(recentCandle.EndPrice, recentCandle.StartPrice);
            decimal lowerShadow = Math.Min(recentCandle.EndPrice, recentCandle.StartPrice) - recentCandle.LowestPrice;
            decimal range = recentCandle.HighestPrice - recentCandle.LowestPrice;

            // Check conditions for Inverted Hammer
            bool smallRealBody = range > 0 && (realBody / range) <= 0.3m; // Real body is at most 30% of the range
            bool longUpperShadow = upperShadow > 2 * realBody;            // Upper shadow at least twice the real body
            bool minimalLowerShadow = lowerShadow < realBody;            // Lower shadow is minimal

            // Check prior candles for downtrend or neutral pattern
            bool priorDowntrendOrConsolidation = candelList.Count > 1 &&
                                                 candelList.Skip(1)
                                                           .Take(Math.Min(Range, candelList.Count - 1))
                                                           .All(c => c.IsBearish.GetValueOrDefault() || !c.IsBullish.GetValueOrDefault());

            // Combine conditions to detect the Inverted Hammer
            //if (CandelSample != null)
            if (smallRealBody && longUpperShadow && minimalLowerShadow && priorDowntrendOrConsolidation)
            {
                if (Range == 1)
                {
                    MasterCandelListFor1MinCandel3WS.Add(candelList);
                    InvertedHammerDb IMPayload = new InvertedHammerDb
                    {
                        Ticker = recentCandle.Ticker,
                        TickerId = recentCandle.TickerId,
                        Exchange = recentCandle.Exchange,
                        IsInvertedHammerDetected = true,
                        DetectionRange = 1,
                        DetectionTime = recentCandle.CloseTime,
                        InvertedHammerCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/InvertedHammerDb",
                                      new StringContent(JsonConvert.SerializeObject(IMPayload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 5)
                {
                    MasterCandelListFor5MinCandel3WS.Add(candelList);
                    InvertedHammerDb IMPayload = new InvertedHammerDb
                    {
                        Ticker = recentCandle.Ticker,
                        TickerId = recentCandle.TickerId,
                        Exchange = recentCandle.Exchange,
                        IsInvertedHammerDetected = true,
                        DetectionRange = 5,
                        DetectionTime = recentCandle.CloseTime,
                        InvertedHammerCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/InvertedHammerDb",
                                      new StringContent(JsonConvert.SerializeObject(IMPayload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 10)
                {
                    MasterCandelListFor10MinCandel3WS.Add(candelList);
                    InvertedHammerDb IMPayload = new InvertedHammerDb
                    {
                        Ticker = recentCandle.Ticker,
                        TickerId = recentCandle.TickerId,
                        Exchange = recentCandle.Exchange,
                        IsInvertedHammerDetected = true,
                        DetectionRange = 10,
                        DetectionTime = recentCandle.CloseTime,
                        InvertedHammerCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/InvertedHammerDb",
                                      new StringContent(JsonConvert.SerializeObject(IMPayload), Encoding.UTF8, "application/json"));
                }
                else if (Range == 15)
                {
                    MasterCandelListFor15MinCandel3WS.Add(candelList);
                    InvertedHammerDb IMPayload = new InvertedHammerDb
                    {
                        Ticker = recentCandle.Ticker,
                        TickerId = recentCandle.TickerId,
                        Exchange = recentCandle.Exchange,
                        IsInvertedHammerDetected = true,
                        DetectionRange = 15,
                        DetectionTime = recentCandle.CloseTime,
                        InvertedHammerCandels = null
                    };

                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/InvertedHammerDb",
                                      new StringContent(JsonConvert.SerializeObject(IMPayload), Encoding.UTF8, "application/json"));
                }
            }
            else
            {
                Console.WriteLine("No Inverted Hammer pattern detected.");
            }
        }


        //public async void MorningStarAnalyzer(List<Candel> candelList, int Range)
        //{
        //    // Ensure there are at least 3 candles in the list
        //    if (candelList.Count < 3)
        //    {
        //        Console.WriteLine("The list must contain at least 3 candles.");
        //        return;
        //    }

        //    // Get the last 3 candles (most recent)
        //    List<Candel> recentThreeCandles = candelList.OrderByDescending(c => c.CloseTime).Take(3).ToList();

        //    // Get the latest candel (top-most from the recentThreeCandles list)
        //    Candel latestCandel = recentThreeCandles.FirstOrDefault();

        //    if(latestCandel.CloseTime.Second < 58)
        //    {
        //        return;
        //    }

        //    // Condition 1: The first candle should be bearish with a large body
        //    bool firstBearish = recentThreeCandles[2].IsBearish == true &&
        //                        (recentThreeCandles[2].StartPrice - recentThreeCandles[2].EndPrice) >
        //                        (recentThreeCandles[2].HighestPrice - recentThreeCandles[2].LowestPrice) * 0.6m;

        //    // Condition 2: The second candle should be a small-bodied candle (indecision)
        //    decimal secondBodySize = Math.Abs(recentThreeCandles[1].EndPrice - recentThreeCandles[1].StartPrice);
        //    decimal secondRange = recentThreeCandles[1].HighestPrice - recentThreeCandles[1].LowestPrice;
        //    bool secondIndecision = secondBodySize / secondRange <= 0.3m;

        //    // Condition 3: The third candle should be bullish with a large body
        //    bool thirdBullish = recentThreeCandles[0].IsBullish == true &&
        //                        (recentThreeCandles[0].EndPrice - recentThreeCandles[0].StartPrice) >
        //                        (recentThreeCandles[0].HighestPrice - recentThreeCandles[0].LowestPrice) * 0.6m;

        //    // Condition 4: The third candle should close above the midpoint of the first candle
        //    decimal firstMidpoint = (recentThreeCandles[2].StartPrice + recentThreeCandles[2].EndPrice) / 2;
        //    bool thirdClosesAboveMidpoint = recentThreeCandles[0].EndPrice > firstMidpoint;

        //    // Combine all conditions to detect the Morning Star pattern
        //    if (firstBearish && secondIndecision && thirdBullish && thirdClosesAboveMidpoint)
        //    {
        //        Console.WriteLine("Morning Star pattern detected!");
        //    }
        //    else
        //    {
        //        Console.WriteLine("No Morning Star pattern detected.");
        //    }
        //}


        private async Task AnalyzeThreeWhiteSoldiersAsync(string ticker, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/GetCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    var lastFourCandels = candels.TakeLast(4).ToList();
                    ThreeWhiteSoilderAnalyzer(lastFourCandels, 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }

        private async Task AnalyzeThreeWhite5MinCandelSoldiersAsync(string ticker, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get5MinCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    var lastFourCandels = candels.TakeLast(4).ToList();
                    ThreeWhiteSoilderAnalyzer(lastFourCandels, 5);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }

        private async Task AnalyzeThreeWhite10MinCandelSoldiersAsync(string ticker, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get10MinCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    var lastFourCandels = candels.TakeLast(4).ToList();
                    ThreeWhiteSoilderAnalyzer(lastFourCandels, 10);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }

        private async Task AnalyzeThreeWhite15MinCandelSoldiersAsync(string ticker, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get15MinCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    var lastFourCandels = candels.TakeLast(4).ToList();
                    ThreeWhiteSoilderAnalyzer(lastFourCandels, 15);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }


        private async Task AnalyzeInvertedHammerAsync(string ticker, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/GetCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    var lastFourCandels = candels.TakeLast(4).ToList();
                    InvertedHammerAnalyzer(lastFourCandels, 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }

        private async Task Analyze5MinCandelInvertedHammerAsync(string ticker, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get5MinCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    var lastFourCandels = candels.TakeLast(4).ToList();
                    InvertedHammerAnalyzer(lastFourCandels, 5);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }

        private async Task Analyze10MinCandelInvertedHammerAsync(string ticker, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get10MinCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    var lastFourCandels = candels.TakeLast(4).ToList();
                    InvertedHammerAnalyzer(lastFourCandels, 10);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }

        private async Task Analyze15MinCandelInvertedHammerAsync(string ticker, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get15MinCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                if (candels != null)
                {
                    var lastFourCandels = candels.TakeLast(4).ToList();
                    InvertedHammerAnalyzer(lastFourCandels, 15);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            List<double> iterationTimes = new();

            while (!stoppingToken.IsCancellationRequested)
            {
                var stopwatch = Stopwatch.StartNew();

                var tasks = _stocks.Select(stock => Task.Run(async () =>
                {
                    try
                    {
                        await AnalyzeThreeWhiteSoldiersAsync(stock.ticker, stoppingToken);
                        await AnalyzeThreeWhite5MinCandelSoldiersAsync(stock.ticker, stoppingToken);
                        await AnalyzeThreeWhite10MinCandelSoldiersAsync(stock.ticker, stoppingToken);
                        await AnalyzeThreeWhite15MinCandelSoldiersAsync(stock.ticker, stoppingToken);

                        await AnalyzeInvertedHammerAsync(stock.ticker, stoppingToken);
                        await Analyze5MinCandelInvertedHammerAsync(stock.ticker, stoppingToken);
                        await Analyze10MinCandelInvertedHammerAsync(stock.ticker, stoppingToken);
                        await Analyze15MinCandelInvertedHammerAsync(stock.ticker, stoppingToken);

                    }
                    catch (Exception ex)
                    {
                    }
                }, stoppingToken));



                // Wait for all tasks to complete.
                await Task.WhenAll(tasks);

                stopwatch.Stop();
                iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);

                // Trigger garbage collection periodically
                GC.Collect();
                GC.WaitForPendingFinalizers();

            }

        }

    }
}
