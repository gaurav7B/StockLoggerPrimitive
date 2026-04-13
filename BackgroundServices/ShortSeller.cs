//using Azure.Core;
//using Newtonsoft.Json;
//using StockLogger.BackgroundServices.Helper_methods;
//using StockLogger.Controllers.API_Controllers;
//using StockLogger.Models.Candel;
//using System.Net.Http;
//using System.Text;
//using static StockLogger.Controllers.API_Controllers.BuySellController;
//using static StockLogger.Controllers.API_Controllers.TestShortSellController;

//namespace StockLogger.BackgroundServices
//{
//    public class ShortSeller : BackgroundService
//    {
//        private readonly HttpClient _httpClient;
//        private readonly List<(string Ticker, string SymbolToken, long Id)> _stocks;
//        private readonly string _logFilePath = @"C:\Users\Client\Desktop\Rate_Limit\api_log.txt";

//        // Semaphore to synchronize file writes
//        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

//        public ShortSeller(HttpClient httpClient)
//        {
//            _httpClient = httpClient;

//            _stocks = StockList2.GetStocks()
//                        .Select(s => (s.Ticker, s.SymbolToken, s.Id))
//                        .ToList();
//        }

//        public static decimal? GetFuturePriceForRSI70(List<Candel> candels, int period)
//        {
//            if (candels == null || candels.Count < period)
//                return null; // Not enough data

//            List<decimal> gains = new List<decimal>();
//            List<decimal> losses = new List<decimal>();

//            for (int i = candels.Count - period; i < candels.Count; i++)
//            {
//                decimal change = candels[i].EndPrice - candels[i].StartPrice;
//                gains.Add(Math.Max(0, change));
//                losses.Add(Math.Max(0, -change));
//            }

//            decimal averageGain = gains.Sum() / period;
//            decimal averageLoss = losses.Sum() / period;

//            // If averageLoss is 0, RSI will stay at 100
//            if (averageLoss == 0)
//                return candels.Last().EndPrice;

//            // Target RS for RSI = 70
//            decimal targetRS = 7m / 3m;

//            // Equation: (averageGain + requiredGain/period) / averageLoss = targetRS
//            // Solve for requiredGain
//            decimal requiredGain = (targetRS * averageLoss - averageGain) * period;

//            if (requiredGain < 0)
//                return candels.Last().EndPrice; // Already above RSI 70

//            decimal lastClose = candels.Last().EndPrice;
//            decimal futurePrice = lastClose + requiredGain;

//            return futurePrice;
//        }


//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            // Wait 30 seconds before starting
//            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                //var now = DateTime.Now.TimeOfDay;
//                //var marketOpen = new TimeSpan(9, 15, 0);
//                //var marketClose = new TimeSpan(16, 0, 0);

//                //if (now < marketOpen || now > marketClose)
//                //{
//                //    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken); // Don't spin too fast
//                //    continue;
//                //}

//                // Wait until 9:15:00 AM
//                var now = DateTime.Now;
//                var today915 = now.Date.AddHours(9).AddMinutes(15);
//                if (now < today915)
//                {
//                    var delay = today915 - now;
//                    Console.WriteLine($"⏳ Waiting {delay.TotalMinutes:F1} minutes until 9:15 AM...");
//                    await Task.Delay(delay, stoppingToken);
//                }

//                Console.WriteLine("🚀 Starting OrderBookService at 9:15 AM...");

//                DateTime PreviousDay = DateTime.Today.AddDays(-30);
//                DateTime TestDay = DateTime.Today.AddDays(-1);


//                using (var httpClient = new HttpClient())
//                {
//                    foreach (var stock in _stocks)
//                    {
//                        // Create request object (map from your BulkTestRequestModel if needed)
//                        var stockRequestForPrevious14Days = new StockRequestModified
//                        {
//                            SymbolToken = stock.SymbolToken,  // coming from BulkTestRequestModel
//                            AuthorizationToken = "",            // set if required
//                            StartDate = PreviousDay,
//                            EndDate = TestDay,
//                            Duration = "1d"
//                        };

//                        // Convert request object to JSON
//                        var jsonPreviousDay = JsonConvert.SerializeObject(stockRequestForPrevious14Days);
//                        var contentPreviousDay = new StringContent(jsonPreviousDay, Encoding.UTF8, "application/json");

//                        // Call internal API
//                        var responsePrevious14Days = await httpClient.PostAsync(
//                            "https://localhost:44364/api/AngelCandel/getCandleDataForTest5PaisaSeprated",
//                            contentPreviousDay);

//                        if (responsePrevious14Days.IsSuccessStatusCode)
//                        {

//                            var responsePrevious14DayData = await responsePrevious14Days.Content.ReadAsStringAsync();
//                            List<Candel> Previous14DayList = JsonConvert.DeserializeObject<List<Candel>>(responsePrevious14DayData);

//                            // **<-- ADD THIS NULL/EMPTY CHECK TO PREVENT THE CRASH -->**
//                            if (Previous14DayList == null || !Previous14DayList.Any())
//                            {
//                                continue; // Skip the rest of the loop for this stock
//                            }

//                            decimal? PredictedRSIPrice = GetFuturePriceForRSI70(Previous14DayList, 14);

//                            // Skip if no valid RSI price was predicted
//                            if (PredictedRSIPrice == null)
//                            {
//                                Console.WriteLine($"⚠️ Skipping {stock.Ticker} — PredictedRSIPrice is null (insufficient data)");
//                                continue;
//                            }

//                            decimal profitPercent = 0.0025m; // 0.25% less than predicted price
//                            decimal expectedPrice = (decimal)(PredictedRSIPrice * (1 - profitPercent));

//                            decimal amount = 1000 * 5;

//                            int quantity = (int)(amount / PredictedRSIPrice);

//                            SellData sellData = new SellData
//                            {
//                                tradingsymbol = stock.Ticker,
//                                symboltoken = stock.SymbolToken,
//                                CurrentPrice = (decimal)PredictedRSIPrice,
//                                TargetPrice = expectedPrice,
//                                Quantity = quantity
//                            };


//                            // 🔹 Call the ShortSellMSI API here
//                            string apiUrl = "https://localhost:44364/api/BuySell/ShortSellMSI";

//                            var json = JsonConvert.SerializeObject(sellData);
//                            var content = new StringContent(json, Encoding.UTF8, "application/json");

//                            using (var client = new HttpClient())
//                            {
//                                var response = await client.PostAsync(apiUrl, content);

//                                if (response.IsSuccessStatusCode)
//                                {
//                                    Console.WriteLine($"✅ ShortSellMSI success for {stock.Ticker}");
//                                }
//                                else
//                                {
//                                    string errorMsg = await response.Content.ReadAsStringAsync();
//                                    Console.WriteLine($"❌ ShortSellMSI failed for {stock.Ticker}: {response.StatusCode} - {errorMsg}");
//                                }
//                            }

//                        }
//                        else
//                        {

//                            Console.WriteLine("Error Ocurred in Short Seller Service");
//                        }
//                        //await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
//                    }
//                }
//            }
//        }
//    }
//}




using Azure.Core;
using Newtonsoft.Json;
using StockLogger.BackgroundServices.Helper_methods;
using StockLogger.Controllers.API_Controllers;
using StockLogger.Models.Candel;
using System.Net.Http;
using System.Text;
using static StockLogger.Controllers.API_Controllers.BuySellController;
using static StockLogger.Controllers.API_Controllers.TestShortSellController;

namespace StockLogger.BackgroundServices
{
    public class ShortSeller : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string Ticker, string SymbolToken, long Id)> _stocks;
        private readonly string _logFilePath = @"C:\Users\Client\Desktop\Rate_Limit\api_log.txt";

        // Semaphore to synchronize file writes
        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        public ShortSeller(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _stocks = StockList2.GetStocks()
                        .Select(s => (s.Ticker, s.SymbolToken, s.Id))
                        .ToList();
        }

        public class PreOrder
        {
            public string Ticker { get; set; }
            public string SymbolToken { get; set; }
            public decimal PreviosDayClosingPrice { get; set; }
            public decimal PredictedHighPrice { get; set; }
            public decimal PriceDiffrencePercentage { get; set; }
            public decimal TargetPrice { get; set; }
            public int Quantity { get; set; }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            DateTime PreviousDay = DateTime.Today.AddDays(-30);
            DateTime TestDay = DateTime.Today.AddDays(-1);

            List<PreOrder> ListOfPreOrders = new List<PreOrder>();

            // Wait 30 seconds before starting
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {


                ////Random shuffeling
                //ListOfPreOrders = ListOfPreOrders
                //                 .OrderBy(x => Guid.NewGuid())
                //                 .ToList();

                // Wait until 9:15:00 AM
                var now = DateTime.Now;
                var today915 = now.Date.AddHours(9).AddMinutes(15);
                if (now < today915)
                {
                    var delay = today915 - now;
                    Console.WriteLine($"⏳ Waiting {delay.TotalMinutes:F1} minutes until 9:15 AM...");
                    Speaker.Speak("Waiting to buy stocks as data ia populating");
                    await Task.Delay(delay, stoppingToken);
                }
                
                Console.WriteLine("🚀 Starting OrderBookService at 9:15 AM...");

                using (var httpClient = new HttpClient())
                {
                    Speaker.Speak("starting to buy stocks");

                    ListOfPreOrders = GlobalData.ListOfPreOrders;
                    foreach (var order in ListOfPreOrders)
                    {
                        SellData sellData = new SellData
                        {
                            tradingsymbol = order.Ticker,
                            symboltoken = order.SymbolToken,
                            CurrentPrice = order.PredictedHighPrice,
                            TargetPrice = order.TargetPrice,
                            Quantity = order.Quantity
                        };

                        // 🔹 Call the ShortSellMSI API here
                        string apiUrl = "https://localhost:44364/api/BuySell/ShortSellMSI";

                        var json = JsonConvert.SerializeObject(sellData);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        using (var client = new HttpClient())
                        {
                            var response = await client.PostAsync(apiUrl, content);

                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"✅ ShortSellMSI success for {order.Ticker}");
                            }
                            else
                            {
                                string errorMsg = await response.Content.ReadAsStringAsync();
                                Console.WriteLine($"❌ ShortSellMSI failed for {order.Ticker}: {response.StatusCode} - {errorMsg}");
                            }
                        }
                    }
                    //-------------------------------------------------------------------
                    //int callsPerSecond = 10;
                    //int delayBetweenCalls = 1000 / callsPerSecond; // 100ms delay between calls

                    //foreach (var order in ListOfPreOrders)
                    //{
                    //    SellData sellData = new SellData
                    //    {
                    //        tradingsymbol = order.Ticker,
                    //        symboltoken = order.SymbolToken,
                    //        CurrentPrice = order.PredictedHighPrice,
                    //        TargetPrice = order.TargetPrice,
                    //        Quantity = order.Quantity
                    //    };

                    //    string apiUrl = "https://localhost:44364/api/BuySell/ShortSellMSI";
                    //    var json = JsonConvert.SerializeObject(sellData);
                    //    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    //    using (var client = new HttpClient())
                    //    {
                    //        var response = await client.PostAsync(apiUrl, content);

                    //        if (response.IsSuccessStatusCode)
                    //        {
                    //            Console.WriteLine($"✅ ShortSellMSI success for {order.Ticker}");
                    //        }
                    //        else
                    //        {
                    //            string errorMsg = await response.Content.ReadAsStringAsync();
                    //            Console.WriteLine($"❌ ShortSellMSI failed for {order.Ticker}: {response.StatusCode} - {errorMsg}");
                    //        }
                    //    }

                    //    // wait before next call to maintain 10/sec rate
                    //    await Task.Delay(delayBetweenCalls);
                    //}

                    //--------------------------------------------------------------
                }


                //    const int RATE_LIMIT = 20; // max 20 calls per second
                //    const int DELAY_MS = 1000; // 1 second delay between batches

                //    string apiUrl = "https://localhost:44364/api/BuySell/ShortSellMSI";

                //    using (var httpClient = new HttpClient())
                //    {
                //        int totalOrders = ListOfPreOrders.Count;
                //        int processed = 0;

                //        while (processed < totalOrders)
                //        {
                //            // Take a batch of up to 20 orders
                //            var batch = ListOfPreOrders.Skip(processed).Take(RATE_LIMIT).ToList();

                //            // Prepare tasks for parallel sending
                //            var tasks = batch.Select(async order =>
                //            {
                //                try
                //                {
                //                    SellData sellData = new SellData
                //                    {
                //                        tradingsymbol = order.Ticker,
                //                        symboltoken = order.SymbolToken,
                //                        CurrentPrice = order.PredictedHighPrice,
                //                        TargetPrice = order.TargetPrice,
                //                        Quantity = order.Quantity
                //                    };

                //                    var json = JsonConvert.SerializeObject(sellData);
                //                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                //                    var response = await httpClient.PostAsync(apiUrl, content);

                //                    if (response.IsSuccessStatusCode)
                //                    {
                //                        Console.WriteLine($"✅ ShortSellMSI success for {order.Ticker}");
                //                    }
                //                    else
                //                    {
                //                        string errorMsg = await response.Content.ReadAsStringAsync();
                //                        Console.WriteLine($"❌ ShortSellMSI failed for {order.Ticker}: {response.StatusCode} - {errorMsg}");
                //                    }
                //                }
                //                catch (Exception ex)
                //                {
                //                    Console.WriteLine($"⚠️ Error for {order.Ticker}: {ex.Message}");
                //                }
                //            });

                //            // Wait for all 20 requests in this batch to finish
                //            await Task.WhenAll(tasks);

                //            processed += batch.Count;

                //            if (processed < totalOrders)
                //            {
                //                // Respect rate limit: wait 1 second before next batch
                //                await Task.Delay(DELAY_MS);
                //            }
                //        }

                //        Console.WriteLine("✅ All ShortSellMSI requests completed successfully.");
                //    }

            }
        }
    }
}
