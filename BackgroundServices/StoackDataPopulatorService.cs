using Newtonsoft.Json;
using StockLogger.BackgroundServices.Helper_methods;
using StockLogger.Controllers.API_Controllers;
using StockLogger.Models.Candel;
using System.Net.Http;
using System.Text;
using static StockLogger.BackgroundServices.ShortSeller;
using static StockLogger.Controllers.API_Controllers.BuySellController;

namespace StockLogger.BackgroundServices
{
    public class StoackDataPopulatorService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string Ticker, string SymbolToken, long Id)> _stocks;

        public StoackDataPopulatorService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _stocks = StockList2.GetStocks()
                        .Select(s => (s.Ticker, s.SymbolToken, s.Id))
                        .ToList();
        }

        public static decimal? GetFuturePriceForRSI70(List<Candel> candels, int period)
        {
            if (candels == null || candels.Count < period)
                return null; // Not enough data

            List<decimal> gains = new List<decimal>();
            List<decimal> losses = new List<decimal>();

            for (int i = candels.Count - period; i < candels.Count; i++)
            {
                decimal change = candels[i].EndPrice - candels[i].StartPrice;
                gains.Add(Math.Max(0, change));
                losses.Add(Math.Max(0, -change));
            }

            decimal averageGain = gains.Sum() / period;
            decimal averageLoss = losses.Sum() / period;

            // If averageLoss is 0, RSI will stay at 100
            if (averageLoss == 0)
                return candels.Last().EndPrice;

            // Target RS for RSI = 70
            decimal targetRS = 7m / 3m;

            // Equation: (averageGain + requiredGain/period) / averageLoss = targetRS
            // Solve for requiredGain
            decimal requiredGain = (targetRS * averageLoss - averageGain) * period;

            if (requiredGain < 0)
                return candels.Last().EndPrice; // Already above RSI 70

            decimal lastClose = candels.Last().EndPrice;
            decimal futurePrice = lastClose + requiredGain;

            return futurePrice;
        }

        DateTime PreviousDay = DateTime.Today.AddDays(-30);
        DateTime TestDay = DateTime.Today.AddDays(-1);

        private async Task ProcessStock((string Ticker, string SymbolToken, long Id) stock)
        {
            try
            {
                Console.Beep();

                var request = new StockRequestModified
                {
                    SymbolToken = stock.SymbolToken,
                    AuthorizationToken = "",
                    StartDate = PreviousDay,
                    EndDate = TestDay,
                    Duration = "1d"
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    "https://localhost:44364/api/AngelCandel/getCandleDataForTest5PaisaSeprated",
                    content);

                if (!response.IsSuccessStatusCode) return;

                var data = await response.Content.ReadAsStringAsync();
                var candleList = JsonConvert.DeserializeObject<List<Candel>>(data);

                if (candleList == null || !candleList.Any()) return;

                decimal? predictedPrice = GetFuturePriceForRSI70(candleList, 14);
                if (predictedPrice == null) return;

                var lastCandle = candleList.OrderByDescending(c => c.CloseTime).First();

                decimal expectedPrice = predictedPrice.Value * (1 - 0.0025m);
                int quantity = (int)((1000 * 5) / predictedPrice.Value);

                // جلوگیری duplicates
                if (GlobalData.ListOfPreOrders.Any(x => x.SymbolToken == stock.SymbolToken))
                    return;

                GlobalData.ListOfPreOrders.Add(new PreOrder
                {
                    Ticker = stock.Ticker,
                    SymbolToken = stock.SymbolToken,
                    PreviosDayClosingPrice = lastCandle.EndPrice,
                    PredictedHighPrice = predictedPrice.Value,
                    TargetPrice = expectedPrice,
                    Quantity = quantity,
                    PriceDiffrencePercentage =
                        ((predictedPrice.Value - lastCandle.EndPrice) / lastCandle.EndPrice) * 100
                });
            }
            catch
            {
                // ignore errors for retry pass
            }
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DateTime PreviousDay = DateTime.Today.AddDays(-30);
            DateTime TestDay = DateTime.Today.AddDays(-1);

            // Wait 30 seconds before starting
            Speaker.Speak("Waiting for 10 seconds before populating Stock data");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            using (var httpClient = new HttpClient())
            {
                GlobalData.isAllStockDataPopulated = false;
                // 1st pass
                Speaker.Speak("Initiating First loop to populate Stock data");
                foreach (var stock in _stocks)
                {
                    await ProcessStock(stock);
                }
                Speaker.Speak($"{GlobalData.ListOfPreOrders.Count} Data in the list after First loop");

                // find missing
                var processed = GlobalData.ListOfPreOrders.Select(x => x.SymbolToken).ToHashSet();

                var missing = _stocks.Where(s => !processed.Contains(s.SymbolToken)).ToList();

                // 2nd pass
                Speaker.Speak("Initiating Second loop to populate Stock data");
                foreach (var stock in missing)
                {
                    await ProcessStock(stock);
                }
                Speaker.Speak($"{GlobalData.ListOfPreOrders.Count} Data in the list after Second loop");

            }


            GlobalData.isAllStockDataPopulated = true;
            Speaker.Speak($"{GlobalData.ListOfPreOrders.Count} Data in the list");
        }
    }
}
