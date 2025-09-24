using Newtonsoft.Json;
using StockLogger.Controllers.API_Controllers;
using StockLogger.Models.Candel;
using System.Text;

namespace StockLogger.BackgroundServices
{
    public class StoreDataService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;

        public StoreDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _stocks = StockList2.GetStocks()
                     .Select(s => (s.Ticker, s.Exchange, s.Name, s.Id, s.SymbolToken))
                     .ToList();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
                while (!stoppingToken.IsCancellationRequested)
                {
                    //foreach (var stock in _stocks)
                    //{
                    //    try
                    //    {
                    //        var stockRequest = new StockRequest
                    //        {
                    //            SymbolToken = stock.symboltoken,
                    //            AuthorizationToken = "",
                    //            StartDate = DateTime.Today.AddHours(9).AddMinutes(30),
                    //            EndDate = DateTime.Now.Date
                    //        };

                    //        var json = JsonConvert.SerializeObject(stockRequest);
                    //        var content = new StringContent(json, Encoding.UTF8, "application/json");

                    //        HttpResponseMessage response = await _httpClient.PostAsync(
                    //            $"https://localhost:44364/api/AngelCandel/getCandleDataForTest5Paisa", content, stoppingToken);

                    //        if (!response.IsSuccessStatusCode)
                    //            continue;

                    //        string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                    //        List<Candel> candelData = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                    //        if (candelData != null && candelData.Any())
                    //        {
                    //            var postContent = new StringContent(
                    //                JsonConvert.SerializeObject(candelData), // serialize the whole list
                    //                Encoding.UTF8,
                    //                "application/json"
                    //            );

                    //            var postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Candel/CandelList", postContent, stoppingToken);
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Console.WriteLine($"Error processing {stock.ticker}: {ex.Message}");
                    //    }
                    //}

                    await Parallel.ForEachAsync(_stocks, new ParallelOptions { MaxDegreeOfParallelism = 500 }, async (stock, stoppingToken) =>
                    {
                        try
                        {
                            var stockRequest = new StockRequest
                            {
                                SymbolToken = stock.symboltoken,
                                AuthorizationToken = "",
                                StartDate = DateTime.Today.AddHours(9).AddMinutes(30),
                                EndDate = DateTime.Now.Date
                            };

                            var json = JsonConvert.SerializeObject(stockRequest);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            HttpResponseMessage response = await _httpClient.PostAsync(
                                $"https://localhost:44364/api/AngelCandel/getCandleDataForTest5Paisa", content, stoppingToken);

                            if (!response.IsSuccessStatusCode)
                                return; // Use 'return' instead of 'continue' in Parallel.ForEachAsync

                            string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                            List<Candel> candelData = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                            if (candelData != null && candelData.Any())
                            {
                                var postContent = new StringContent(
                                    JsonConvert.SerializeObject(candelData),
                                    Encoding.UTF8,
                                    "application/json"
                                );

                                var postResponse = await _httpClient.PostAsync("https://localhost:44364/api/Candel/CandelList", postContent, stoppingToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing {stock.ticker}: {ex.Message}");
                        }
                    });
                }

        }
    }
}
