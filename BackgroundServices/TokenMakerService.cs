using Newtonsoft.Json;
using StockLogger.Controllers.API_Controllers;
using StockLogger.Models.Candel;
using System.Text;
using System.Net.Http;

namespace StockLogger.BackgroundServices
{
    public class TokenMakerService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string Ticker, string SymbolToken, long Id)> _stocks;
        private readonly string _logFilePath = @"C:\Users\Client\Desktop\Rate_Limit\api_log.txt";

        // Semaphore to synchronize file writes
        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        public TokenMakerService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _stocks = StockList2.GetStocks()
                        .Select(s => (s.Ticker, s.SymbolToken, s.Id))
                        .ToList();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1️ Get Access Token first
                    var tokenResponse = await _httpClient.PostAsync(
                        "https://localhost:44364/api/AngelCandel/getAccessToken5Paisa",
                        null,
                        stoppingToken
                    );

                    var content = await tokenResponse.Content.ReadAsStringAsync(stoppingToken);

                }
                catch (Exception ex)
                {
                }

                // Wait for 20 minutes before next execution
                await Task.Delay(TimeSpan.FromMinutes(20), stoppingToken);
            }
        }
    }
}
