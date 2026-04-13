using Newtonsoft.Json;
using StockLogger.BackgroundServices.Helper_methods;
using StockLogger.Controllers.API_Controllers;
using StockLogger.Models.Candel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using static StockLogger.Controllers.API_Controllers.BuySellController;

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
                    string accessTokenContent = string.Empty;
                    string loginContent = string.Empty;

                    try
                    {
                        var tokenResponse = await _httpClient.PostAsync(
                            "https://localhost:44364/api/AngelCandel/getAccessToken5Paisa",
                            null,
                            stoppingToken
                        );
                        Console.WriteLine("tokenResponse" , tokenResponse);
                        accessTokenContent = await tokenResponse.Content.ReadAsStringAsync(stoppingToken);
                        Speaker.Speak("Angel One login successful");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ex_5PAISA", ex);
                    }

                    try
                    {
                        var angelResponse = await _httpClient.PostAsync(
                            "https://localhost:44364/api/AngelCandel/login2",
                            null,
                            stoppingToken
                        );
                        Console.WriteLine("angelResponse", angelResponse);
                        loginContent = await angelResponse.Content.ReadAsStringAsync(stoppingToken);
                        Speaker.Speak("5 Paisa login successful");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ex_ANGELONE", ex);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("ex", ex);
                }

                // Always wait for 20 minutes before next execution, even after failure
                await Task.Delay(TimeSpan.FromMinutes(20), stoppingToken);
            }
        }
    }
}
