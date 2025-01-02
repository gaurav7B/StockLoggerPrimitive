using StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers;
using StockLogger.Models.Candel;
using System.Diagnostics;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices
{
    public class MorningStarService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;
        private readonly MorningStarAnalyzer _morningStarAnalyzer;

        public MorningStarService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _morningStarAnalyzer = new MorningStarAnalyzer();

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
                        await _morningStarAnalyzer.AnalyzeMorningStarAsync(stock.ticker, _httpClient, stoppingToken);
                        await _morningStarAnalyzer.Analyze5MinCandelMorningStarAsync(stock.ticker, _httpClient, stoppingToken);
                        await _morningStarAnalyzer.Analyze10MinCandelMorningStarAsync(stock.ticker, _httpClient, stoppingToken);
                        await _morningStarAnalyzer.Analyze15MinCandelMorningStarAsync(stock.ticker, _httpClient, stoppingToken);
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
