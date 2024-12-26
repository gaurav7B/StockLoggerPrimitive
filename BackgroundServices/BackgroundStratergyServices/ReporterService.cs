using Newtonsoft.Json;
using StockLogger.BackgroundServices.BackgroundStratergyServices.Analyzers;
using StockLogger.Models.Candel;
using StockLogger.Models.Stratergic_Models.Hammer;
using System.Diagnostics;
using System.Net.Http;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices
{
    public class ReporterService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string ticker, string exchange, string name, long id)> _stocks;

        public ReporterService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Fetch stocks from StockList
            _stocks = StockList.GetStocks();
        }

        public async Task HammerReportAnalyzer(string ticker, HttpClient _httpClient, CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage HammerResponse = await _httpClient.GetAsync($"https://localhost:44364/api/Hammer", stoppingToken);
                HammerResponse.EnsureSuccessStatusCode();

                string responseData = await HammerResponse.Content.ReadAsStringAsync(stoppingToken);

                List<HammerDb> HammerReport = JsonConvert.DeserializeObject<List<HammerDb>>(responseData);

                if (HammerReport.Count != 0)
                {
                    foreach(HammerDb hammer in HammerReport)
                    {
                        if(hammer.DetectionRange == 1)
                        {

                        }
                        else if(hammer.DetectionRange == 5)
                        {

                        }
                        else if (hammer.DetectionRange == 10)
                        {

                        }
                        else if (hammer.DetectionRange == 15)
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                var stopwatch = Stopwatch.StartNew();

                var tasks = _stocks.Select(stock => Task.Run(async () =>
                {
                    try
                    {
                        await HammerReportAnalyzer(stock.ticker, _httpClient, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                    }
                }, stoppingToken));



                // Wait for all tasks to complete.
                await Task.WhenAll(tasks);
                stopwatch.Stop();

                // Trigger garbage collection periodically
                GC.Collect();
                GC.WaitForPendingFinalizers();

            }

        }
    }
}
