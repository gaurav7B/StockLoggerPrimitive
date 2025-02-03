using Newtonsoft.Json;
using StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers;
using StockLogger.Models.Candel;
using System.Diagnostics;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices
{
    public class Uptrend_Service : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;
        private readonly UptrendAnalyzer _analyzer;

        public Uptrend_Service(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _analyzer = new UptrendAnalyzer();

            // Fetch stocks from StockList
            //_stocks = StockList2.GetStocks();

            _stocks = StockList2.GetStocks()
                     .Select(s => (s.Ticker, s.Exchange, s.Name, s.Id, s.SymbolToken))
                     .ToList();
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string authToken = null;

            List<double> iterationTimes = new();

            Random random = new();

            while (!stoppingToken.IsCancellationRequested)
            {
                //var stopwatch = Stopwatch.StartNew();

                ////// Fisher-Yates shuffle for better efficiency
                ////for (int i = _stocks.Count - 1; i > 0; i--)
                ////{
                ////    int j = random.Next(0, i + 1);
                ////    (_stocks[i], _stocks[j]) = (_stocks[j], _stocks[i]); // Swap elements
                ////}


                //var tasks = _stocks.Select(stock => Task.Run(async () =>
                //    {
                //        try
                //        {
                //            await _analyzer.Analyze1MinCandelAsync(stock.symboltoken, _httpClient, stoppingToken, authToken);
                //        }
                //        catch (Exception ex)
                //        {
                //        }
                //    }, stoppingToken));



                //// Wait for all tasks to complete.
                //await Task.WhenAll(tasks);

                //stopwatch.Stop();
                //iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);

                //// Trigger garbage collection periodically
                //GC.Collect();
                //GC.WaitForPendingFinalizers();

                foreach (var stock in _stocks)
                {
                    await _analyzer.Analyze1MinCandelAsync(stock.symboltoken, _httpClient, stoppingToken, authToken);
                }

            }

        }
    }
}
