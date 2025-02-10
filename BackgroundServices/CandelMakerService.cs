using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StockLogger.Models.Candel;
using System.Diagnostics;
using System.Text;

namespace StockLogger.BackgroundServices
{
    public class CandelMakerService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;

        public CandelMakerService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Fetch stocks from StockList
            _stocks = StockList.GetStocks();
        }

        private async Task Post1MinCandelToDb(string symboltoken, CancellationToken stoppingToken)
        {
            try
            {

                var adjustedStartDate = DateTime.Now.AddDays(-1);
                if (adjustedStartDate.DayOfWeek == DayOfWeek.Saturday)
                    adjustedStartDate = adjustedStartDate.AddDays(-1); // Move to Friday
                else if (adjustedStartDate.DayOfWeek == DayOfWeek.Sunday)
                    adjustedStartDate = adjustedStartDate.AddDays(-2); // Move to Friday

                var adjustedEndDate = DateTime.Now.AddDays(-1);
                if (adjustedEndDate.DayOfWeek == DayOfWeek.Saturday)
                    adjustedEndDate = adjustedEndDate.AddDays(-1); // Move to Friday
                else if (adjustedEndDate.DayOfWeek == DayOfWeek.Sunday)
                    adjustedEndDate = adjustedEndDate.AddDays(-2); // Move to Friday

                var requestBodyforPrevousDayData = new
                {
                    SymbolToken = symboltoken,
                    AuthorizationToken = "",
                    StartDate = adjustedStartDate.ToString("o"), // Final adjusted date
                    EndDate = adjustedEndDate.ToString("o")     // Final adjusted date
                };

                var jsonRequestBodyForPreviousDaysData = JsonConvert.SerializeObject(requestBodyforPrevousDayData);
                var contentForPreviousDaysData = new StringContent(jsonRequestBodyForPreviousDaysData, Encoding.UTF8, "application/json");


                var responsePrevious = await _httpClient.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest", contentForPreviousDaysData);

                responsePrevious.EnsureSuccessStatusCode();

                string responseData = await responsePrevious.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                Candel LastCandel = candels.LastOrDefault();

                if(LastCandel != null)
                {
                    HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Candel",
                                          new StringContent(JsonConvert.SerializeObject(LastCandel), Encoding.UTF8, "application/json"),
                                          stoppingToken);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ex.Message}");
            }
        }

        private async Task Post5MinCandelToDb(string ticker, CancellationToken stoppingToken)
        {

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get5MinCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                List<Candel> lastTwoCandels = candels.TakeLast(2).ToList();

                if (candels != null)
                {
                    foreach (var candel in lastTwoCandels)
                    {

                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Candel5min",
                                                                                        new StringContent(JsonConvert.SerializeObject(candel), Encoding.UTF8, "application/json"),
                                                                                        stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }

        private async Task Post10MinCandelToDb(string ticker, CancellationToken stoppingToken)
        {

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get10MinCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                List<Candel> lastTwoCandels = candels.TakeLast(2).ToList();

                if (candels != null)
                {
                    foreach (var candel in lastTwoCandels)
                    {

                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Candel10min",
                                                                                        new StringContent(JsonConvert.SerializeObject(candel), Encoding.UTF8, "application/json"),
                                                                                        stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing ticker {ticker}: {ex.Message}");
            }
        }

        private async Task Post15MinCandelToDb(string ticker, CancellationToken stoppingToken)
        {

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/Get15MinCandel?ticker={ticker}", stoppingToken);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                List<Candel> lastTwoCandels = candels.TakeLast(2).ToList();

                if (candels != null)
                {
                    foreach (var candel in lastTwoCandels)
                    {

                        HttpResponseMessage postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Candel15min",
                                                                                        new StringContent(JsonConvert.SerializeObject(candel), Encoding.UTF8, "application/json"),
                                                                                        stoppingToken);
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

            List<double> iterationTimes = new();

            while (!stoppingToken.IsCancellationRequested)
            {
                //var stopwatch = Stopwatch.StartNew();

                //var tasks = _stocks.Select(stock => Task.Run(async () =>
                //{
                //    try
                //    {
                //        await Post1MinCandelToDb(stock.symboltoken, stoppingToken);
                //        //await Post5MinCandelToDb(stock.ticker, stoppingToken);
                //        //await Post10MinCandelToDb(stock.ticker, stoppingToken);
                //        //await Post15MinCandelToDb(stock.ticker, stoppingToken);
                //    }
                //    catch (Exception ex)
                //    {
                //    }
                //}, stoppingToken));



                //// Wait for all tasks to complete.
                //await Task.WhenAll(tasks);

                //stopwatch.Stop();
                //iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);

                //// Trigger garbage collection periodically
                //GC.Collect();
                //GC.WaitForPendingFinalizers();

                foreach (var stock in _stocks)
                {
                    await Post1MinCandelToDb(stock.symboltoken, stoppingToken);
                }

            }

        }

    }

}
