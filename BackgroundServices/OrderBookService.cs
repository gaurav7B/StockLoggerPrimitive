using Newtonsoft.Json;
using StockLogger.Controllers.API_Controllers;
using StockLogger.Models.Candel;
using System.Net.Http;
using System.Text;
using static StockLogger.Controllers.API_Controllers.BuySellController;

namespace StockLogger.BackgroundServices
{
    public class OrderBookService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string Ticker, string SymbolToken, long Id)> _stocks;

        public OrderBookService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _stocks = StockList2.GetStocks()
                        .Select(s => (s.Ticker, s.SymbolToken, s.Id))
                        .ToList();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait 30 seconds before starting
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //var now = DateTime.Now.TimeOfDay;
                    //var marketOpen = new TimeSpan(9, 15, 0);
                    //var marketClose = new TimeSpan(16, 0, 0);

                    //if (now < marketOpen || now > marketClose)
                    //{
                    //    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken); // Don't spin too fast
                    //    continue;
                    //}

                    // Wait until 9:15:00 AM
                    var now = DateTime.Now;
                    var today915 = now.Date.AddHours(9).AddMinutes(16);
                    if (now < today915)
                    {
                        var delay = today915 - now;
                        Console.WriteLine($"⏳ Waiting {delay.TotalMinutes:F1} minutes until 9:15 AM...");
                        await Task.Delay(delay, stoppingToken);
                    }

                    Console.WriteLine("🚀 Starting OrderBookService at 9:15 AM...");

                    OrderBookResponse? orderBook = null;

                    try
                    {
                        var response = await _httpClient.GetAsync(
                            "https://localhost:44364/api/BuySell/GetOrderBook",
                            stoppingToken
                        );

                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"❌ GetOrderBook failed with status {response.StatusCode}");
                            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                            continue;
                        }

                        var content = await response.Content.ReadAsStringAsync(stoppingToken);
                        orderBook = JsonConvert.DeserializeObject<OrderBookResponse>(content);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("❌ Failed to fetch or parse OrderBook: " + ex.Message);
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        continue;
                    }

                    if (orderBook?.Data == null)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                        continue;
                    }

                    List<OrderData> completedOrders = orderBook.Data
                                                   .Where(d => d.Status.Equals("complete", StringComparison.OrdinalIgnoreCase))
                                                   .ToList();

                    var statuses = orderBook.Data
                        .Select(d => d.Status)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    foreach (var status in statuses)
                    {
                        Console.WriteLine(status);
                    }

                    foreach (var completedOrder in completedOrders)
                    {
                        try
                        {
                            decimal profitPercent = 0.0025m; // 0.25%
                            decimal completedPrice = Convert.ToDecimal(completedOrder.Price);
                            decimal expectedPrice = completedPrice * (1 - profitPercent);

                            var matchedStock = _stocks.FirstOrDefault(
                                s => s.Ticker.Equals(completedOrder.TradingSymbol, StringComparison.OrdinalIgnoreCase)
                            );

                            BuyDataMSI sellData = new BuyDataMSI
                            {
                                tradingsymbol = completedOrder.TradingSymbol,
                                symboltoken = matchedStock.SymbolToken,
                                CurrentPrice = expectedPrice,
                                TargetPrice = 0,
                                Quantity = completedOrder.Quantity
                            };

                            var json = JsonConvert.SerializeObject(sellData);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            List<OrderData> buyOrders = orderBook.Data
                                .Where(d =>
                                    d.TransactionType.Equals("BUY", StringComparison.OrdinalIgnoreCase) &&
                                    d.TradingSymbol.Equals(completedOrder.TradingSymbol, StringComparison.OrdinalIgnoreCase) &&
                                    d.Status.Equals("open", StringComparison.OrdinalIgnoreCase)
                                )
                                .ToList();

                            List<OrderData> executedBuyOrders = orderBook.Data
                                .Where(d =>
                                    d.TransactionType.Equals("BUY", StringComparison.OrdinalIgnoreCase) &&
                                    d.TradingSymbol.Equals(completedOrder.TradingSymbol, StringComparison.OrdinalIgnoreCase) &&
                                    d.Status.Equals("complete", StringComparison.OrdinalIgnoreCase)
                                )
                                .ToList();

                            if (buyOrders.Count == 0 && executedBuyOrders.Count == 0)
                            {
                               var response = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/BuyMSI", content);
                                
                                if (response.IsSuccessStatusCode)
                                    Console.WriteLine($"✅ BuyMSI success for {completedOrder.TradingSymbol}");
                                else
                                    Console.WriteLine($"❌ BuyMSI failed for {completedOrder.TradingSymbol}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Failed processing order {completedOrder.TradingSymbol}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected error in OrderBookService: " + ex.Message);
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

    }
}
