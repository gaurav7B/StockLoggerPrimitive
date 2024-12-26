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
    public class MorningStarService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string ticker, string exchange, string name, long id)> _stocks;
        private readonly MorningStarAnalyzer _morningStarAnalyzer;

        public MorningStarService(HttpClient httpClient)
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
