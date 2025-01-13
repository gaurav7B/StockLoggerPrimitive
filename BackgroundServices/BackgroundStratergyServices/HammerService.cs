using Newtonsoft.Json;
using StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers;
using StockLogger.Models.Candel;
using System.Diagnostics;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices
{
    public class HammerService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;
        private readonly HammerAnalyzer _analyzer;

        public HammerService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _analyzer = new HammerAnalyzer();

            // Fetch stocks from StockList
            _stocks = StockList.GetStocks();
        }

        private async Task<string> FetchAuthTokenAsync(CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("https://localhost:44364/api/Token", stoppingToken);
                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                dynamic data = JsonConvert.DeserializeObject(responseData);
                return data?.authToken;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string authToken = null;

            List<double> iterationTimes = new();

            while (!stoppingToken.IsCancellationRequested)
            {
                var stopwatch = Stopwatch.StartNew();

                // Fetch token before the service starts
                authToken = await FetchAuthTokenAsync(stoppingToken);

                if(authToken != null)
                {
                    var tasks = _stocks.Select(stock => Task.Run(async () =>
                    {
                        try
                        {
                            await _analyzer.Analyze1MinCandelAsync(stock.symboltoken, _httpClient, stoppingToken, authToken);
                            //await _analyzer.Analyze5MinCandelAsync(stock.ticker, _httpClient, stoppingToken);
                            //await _analyzer.Analyze10MinCandelAsync(stock.ticker, _httpClient, stoppingToken);
                            //await _analyzer.Analyze15MinCandelAsync(stock.ticker, _httpClient, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                        }
                    }, stoppingToken));



                    // Wait for all tasks to complete.
                    await Task.WhenAll(tasks);
                }

                stopwatch.Stop();
                iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);

                // Trigger garbage collection periodically
                GC.Collect();
                GC.WaitForPendingFinalizers();

            }

        }
    }
}
