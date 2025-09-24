//using Microsoft.Extensions.Hosting;
//using Newtonsoft.Json;
//using StockLogger.Controllers.API_Controllers;
//using StockLogger.Models.Candel;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace StockLogger.BackgroundServices
//{
//    public class RateLimitTestService : BackgroundService
//    {
//        private readonly HttpClient _httpClient;
//        private readonly List<(string Ticker, string SymbolToken, long Id)> _stocks;
//        private readonly string _logFilePath = @"C:\Users\Client\Desktop\Rate_Limit\api_log.txt";

//        public RateLimitTestService(HttpClient httpClient)
//        {
//            _httpClient = httpClient;

//            _stocks = StockList2.GetStocks()
//                        .Select(s => (s.Ticker, s.SymbolToken, s.Id))
//                        .ToList();

//            // Ensure directory exists
//            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));
//        }

//        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        //{
//        //    while (!stoppingToken.IsCancellationRequested)
//        //    {
//        //        foreach (var stock in _stocks)
//        //        {
//        //            try
//        //            {
//        //                var stockRequest = new StockRequest
//        //                {
//        //                    SymbolToken = stock.SymbolToken,
//        //                    AuthorizationToken = "",
//        //                    StartDate = DateTime.Today.AddHours(9).AddMinutes(30),
//        //                    EndDate = DateTime.Now
//        //                };

//        //                var json = JsonConvert.SerializeObject(stockRequest);
//        //                var content = new StringContent(json, Encoding.UTF8, "application/json");

//        //                var requestTime = DateTime.Now;
//        //                HttpResponseMessage response = await _httpClient.PostAsync(
//        //                    "https://localhost:44364/api/AngelCandel/getCandleDataForTest5Paisa",
//        //                    content,
//        //                    stoppingToken);
//        //                var responseTime = DateTime.Now;

//        //                var duration = responseTime - requestTime;

//        //                string logLine = $"Ticker: {stock.Ticker}, " +
//        //                                 $"Ticker_Id: {stock.Id}, " +
//        //                                 $"Duration(ms): {duration.TotalMilliseconds}, " + // Duration in milliseconds with 3 decimal places
//        //                                 $"Duration(s): {duration.TotalSeconds:F3}, " +  // Duration in seconds with 3 decimal places
//        //                                 $"StatusCode: {(int)response.StatusCode}{Environment.NewLine}";

//        //                await File.AppendAllTextAsync(_logFilePath, logLine, stoppingToken);

//        //                Console.WriteLine(logLine.Trim());
//        //            }
//        //            catch (Exception ex)
//        //            {
//        //                string errorLog = $"Ticker: {stock.Ticker}, Error: {ex.Message}, Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}{Environment.NewLine}";
//        //                await File.AppendAllTextAsync(_logFilePath, errorLog, stoppingToken);
//        //                Console.WriteLine(errorLog.Trim());
//        //            }

//        //            // Optional delay to control request frequency
//        //            //await Task.Delay(100, stoppingToken);
//        //        }

//        //        // Optional: delay between full loops
//        //        //await Task.Delay(5000, stoppingToken);
//        //    }
//        //}


//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                // Run requests in parallel
//                await Parallel.ForEachAsync(_stocks, new ParallelOptions { MaxDegreeOfParallelism = 10, CancellationToken = stoppingToken }, async (stock, token) =>
//                {
//                    try
//                    {
//                        var stockRequest = new StockRequest
//                        {
//                            SymbolToken = stock.SymbolToken,
//                            AuthorizationToken = "",
//                            StartDate = DateTime.Today.AddHours(9).AddMinutes(30),
//                            EndDate = DateTime.Now
//                        };

//                        var json = JsonConvert.SerializeObject(stockRequest);
//                        var content = new StringContent(json, Encoding.UTF8, "application/json");

//                        var requestTime = DateTime.Now;
//                        HttpResponseMessage response = await _httpClient.PostAsync(
//                            "https://localhost:44364/api/AngelCandel/getCandleDataForTest5Paisa",
//                            content,
//                            token);
//                        var responseTime = DateTime.Now;

//                        var duration = responseTime - requestTime;

//                        string logLine = $"Ticker: {stock.Ticker}, " +
//                                         $"Ticker_Id: {stock.Id}, " +
//                                         $"Duration(ms): {duration.TotalMilliseconds:F3}, " +
//                                         $"Duration(s): {duration.TotalSeconds:F3}, " +
//                                         $"StatusCode: {(int)response.StatusCode}{Environment.NewLine}";

//                        await File.AppendAllTextAsync(_logFilePath, logLine, token);
//                        Console.WriteLine(logLine.Trim());
//                    }
//                    catch (Exception ex)
//                    {
//                        string errorLog = $"Ticker: {stock.Ticker}, Error: {ex.Message}, Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}{Environment.NewLine}";
//                        await File.AppendAllTextAsync(_logFilePath, errorLog, token);
//                        Console.WriteLine(errorLog.Trim());
//                    }
//                });

//                // Optional delay between loops
//                // await Task.Delay(5000, stoppingToken);
//            }
//        }





//    }
//}



using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using StockLogger.Controllers.API_Controllers;
using StockLogger.Models.Candel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockLogger.BackgroundServices
{
    public class RateLimitTestService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string Ticker, string SymbolToken, long Id)> _stocks;
        private readonly string _logFilePath = @"C:\Users\Client\Desktop\Rate_Limit\api_log.txt";

        // Semaphore to synchronize file writes
        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        public RateLimitTestService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _stocks = StockList2.GetStocks()
                        .Select(s => (s.Ticker, s.SymbolToken, s.Id))
                        .ToList();

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait 30 seconds before starting
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Parallel.ForEachAsync(
                    _stocks,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 500, // Adjust based on API limits
                        CancellationToken = stoppingToken
                    },
                    async (stock, token) =>
                    {
                        try
                        {
                            var stockRequest = new StockRequest
                            {
                                SymbolToken = stock.SymbolToken,
                                AuthorizationToken = "",
                                StartDate = DateTime.Today.AddHours(9).AddMinutes(30),
                                EndDate = DateTime.Now
                            };

                            var json = JsonConvert.SerializeObject(stockRequest);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            var requestTime = DateTime.Now;
                            HttpResponseMessage response = await _httpClient.PostAsync(
                                "https://localhost:44364/api/AngelCandel/getCandleDataForTest5PaisaSeprated",
                                content,
                                token);
                            var responseTime = DateTime.Now;

                            var duration = responseTime - requestTime;

                            string logLine = $"Ticker: {stock.Ticker}, " +
                                             $"Ticker_Id: {stock.Id}, " +
                                             $"requestTime: {requestTime}, " +
                                             $"Duration(ms): {duration.TotalMilliseconds:F3}, " +
                                             $"Duration(s): {duration.TotalSeconds:F3}, " +
                                             $"StatusCode: {(int)response.StatusCode}{Environment.NewLine}";

                            // Use semaphore to prevent simultaneous file access
                            await _fileLock.WaitAsync(token);
                            try
                            {
                                await File.AppendAllTextAsync(_logFilePath, logLine, token);
                            }
                            finally
                            {
                                _fileLock.Release();
                            }

                            Console.WriteLine(logLine.Trim());
                        }
                        catch (Exception ex)
                        {
                            string errorLog = $"Ticker: {stock.Ticker}, Error: {ex.Message}, Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}{Environment.NewLine}";

                            await _fileLock.WaitAsync(token);
                            try
                            {
                                await File.AppendAllTextAsync(_logFilePath, errorLog, token);
                            }
                            finally
                            {
                                _fileLock.Release();
                            }

                            Console.WriteLine(errorLog.Trim());
                        }
                    });
                // Wait 1 minute before next run
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
