using Newtonsoft.Json;
using StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers;
using StockLogger.Models.Candel;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices
{
    public class MarubozuService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;
        private readonly MarubozuAnalyzer _analyzer;

        public MarubozuService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _analyzer = new MarubozuAnalyzer();

            // Fetch stocks from StockList
            _stocks = StockList.GetStocks();
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
                        await _analyzer.Analyze1MinCandelAsync(stock.ticker, _httpClient, stoppingToken);
                        await _analyzer.Analyze5MinCandelAsync(stock.ticker, _httpClient, stoppingToken);
                        await _analyzer.Analyze10MinCandelAsync(stock.ticker, _httpClient, stoppingToken);
                        await _analyzer.Analyze15MinCandelAsync(stock.ticker, _httpClient, stoppingToken);
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
