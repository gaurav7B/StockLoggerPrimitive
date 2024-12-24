using Newtonsoft.Json;
using StockLogger.Models.Candel;
using System.Diagnostics;

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

        public ThreeWhiteSoilders(HttpClient httpClient)
        {
            _httpClient = httpClient;

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
            // Ensure there are at least 4 candles in the list
            if (candelList.Count < 4)
            {
                Console.WriteLine("The list must contain at least 4 candles.");
                return;
            }

            // Get the last 3 candles from the list (the most recent 3)
            List<Candel> recentThreeCandles = candelList.OrderByDescending(c => c.CloseTime).Take(3).ToList();

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

            // Combine all conditions to detect the Three White Soldiers pattern
            if (allThreeBullish && progressiveCloses && increasingBodySize && smallUpperShadow && smallLowerShadow &&
                strongBodyRatio && priorConsolidationOrBearish)
            {
                if (Range == 1)
                {
                    MasterCandelListFor1MinCandel3WS.Add(candelList);
                }
                else if (Range == 5)
                {
                    MasterCandelListFor5MinCandel3WS.Add(candelList);
                }
                else if (Range == 10)
                {
                    MasterCandelListFor10MinCandel3WS.Add(candelList);
                }
                else if (Range == 15)
                {
                    MasterCandelListFor15MinCandel3WS.Add(candelList);
                }
            }
            else
            {
                Console.WriteLine("Three White Soldiers Pattern Not Detected");
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

                        //FOR 1 MIN CANDEL 3WS ANALYZER

                        // Call the API and get the response as a string
                        HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/GetCandel?ticker={stock.ticker}", stoppingToken);
                        response.EnsureSuccessStatusCode();

                        string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                        // Deserialize the JSON response into a list of Candel objects
                        List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                        // Store the candles in a variable (for future use)
                        if (candels != null)
                        {
                            var lastFourCandels = candels.TakeLast(4).ToList();
                            ThreeWhiteSoilderAnalyzer(lastFourCandels, 1);
                        }




                        //FOR 5 MIN CANDEL 3WS ANALYZER

                        // Call the API and get the response as a string
                        HttpResponseMessage response5 = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get5MinCandel?ticker={stock.ticker}", stoppingToken);
                        response5.EnsureSuccessStatusCode();
                        string responseData5 = await response5.Content.ReadAsStringAsync(stoppingToken);
                        // Deserialize the JSON response into a list of Candel objects
                        List<Candel> candels5 = JsonConvert.DeserializeObject<List<Candel>>(responseData5);
                        // Store the candles in a variable (for future use)
                        if (candels5 != null)
                        {
                            var lastFourCandels = candels5.TakeLast(4).ToList();
                            ThreeWhiteSoilderAnalyzer(lastFourCandels, 5);
                        }



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
