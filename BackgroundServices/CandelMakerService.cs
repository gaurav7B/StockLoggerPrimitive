using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StockLogger.Data;
using StockLogger.Models.Candel;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Text;
using static StockLogger.Controllers.API_Controllers.BuySellController;

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
            //_stocks = StockList.GetStocks();

            _stocks = StockList2.GetStocks()
                     .Select(s => (s.Ticker, s.Exchange, s.Name, s.Id, s.SymbolToken))
                     .ToList();
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

            ////// CODE TO POPULATE THE DB WITH END PRICES

            //foreach (var stock in _stocks)
            //{
            //    await Post1MinCandelToDb(stock.symboltoken, stoppingToken);
            //}

            //while (!stoppingToken.IsCancellationRequested)
            //{

            //    foreach (var stock in _stocks)
            //    {
            //        HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/candel", stoppingToken);
            //        response.EnsureSuccessStatusCode();

            //        string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

            //        List<Candel> CandelData = JsonConvert.DeserializeObject<List<Candel>>(responseData);

            //        Candel selectedCandel = CandelData.FirstOrDefault(c => c.Ticker == stock.ticker);

            //        if (selectedCandel == null)
            //        {
            //            await Post1MinCandelToDb(stock.symboltoken, stoppingToken);
            //        }

            //    }

            //}


            ////// 3% REDUCTION

            //////CODE TO FIND THE STOCKS WHICH ARE BELOW 3 % FROM PREVIOUS DAYS END PRICE
            while (!stoppingToken.IsCancellationRequested)
            {

                // Check if the current time is before 9:15 AM
                if (DateTime.Now.TimeOfDay < new TimeSpan(9, 15, 0))
                {
                    continue; // Skip to the next iteration
                }


                // Check if the current time is after 11:00 AM
                if (DateTime.Now.TimeOfDay > new TimeSpan(11, 00, 0))
                {
                    break; // break iteration
                }

                foreach (var stock in _stocks)
                {

                    // THIS PART FETCEHES THE END PRICE OF THE PREVIOUS DAYS STOCK
                    HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/candel", stoppingToken);
                    response.EnsureSuccessStatusCode();

                    string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

                    List<Candel> CandelData = JsonConvert.DeserializeObject<List<Candel>>(responseData);

                    Candel previousDayCandel = CandelData.FirstOrDefault(c => c.Ticker == stock.ticker);

                    decimal expectedPrice = previousDayCandel.EndPrice - (previousDayCandel.EndPrice * 0.02m);
                    //decimal expectedPrice = previousDayCandel.EndPrice - (previousDayCandel.EndPrice * 0.03m);



                    // THIS PART FETCHES THE LATEST PRICE OF THE STOCK
                    var requestBodyforCurrentDayData = new
                    {
                        SymbolToken = stock.symboltoken,
                        AuthorizationToken = "",
                        StartDate = DateTime.Now.AddHours(9).AddMinutes(15).ToString("o"), // Final adjusted date
                        EndDate = DateTime.Now.ToString("o")     // Final adjusted date
                    };

                    var jsonRequestBodyForCurrentDaysData = JsonConvert.SerializeObject(requestBodyforCurrentDayData);
                    var contentForCurrentDaysData = new StringContent(jsonRequestBodyForCurrentDaysData, Encoding.UTF8, "application/json");


                    var responseCurrent = await _httpClient.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest", contentForCurrentDaysData);

                    //responseCurrent.EnsureSuccessStatusCode();

                    string responseCurrentData = await responseCurrent.Content.ReadAsStringAsync(stoppingToken);

                    List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseCurrentData);

                    Candel LastCandel = candels.LastOrDefault();


                    List<Candel> ExtractedCandelData = new List<Candel>();


                    //  THIS PART COMPARES THE LATEST PRICE WITH THE EXPECTED PRICE
                    if (
                        LastCandel != null
                        && previousDayCandel != null
                        && LastCandel.EndPrice <= expectedPrice
                        )
                    {
                        // BUY API HERE
                        BuyData buyData = new BuyData
                        {
                            symboltoken = stock.symboltoken,
                            tradingsymbol = stock.ticker,
                            CurrentPrice = LastCandel.EndPrice,
                            PreviousDaayEndPrice = previousDayCandel.EndPrice,
                        };

                        var jsonRequestBodyForbuyData = JsonConvert.SerializeObject(buyData);
                        var contentForbuyData = new StringContent(jsonRequestBodyForbuyData, Encoding.UTF8, "application/json");

                        var buyDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/buy", contentForbuyData);

                        buyDataApiResponse.EnsureSuccessStatusCode();

                        // WHEN THE BUY API RUNS SUCCESSFULLY
                        // CALL THE SELL API
                        // SELL API HERE
                        if (buyDataApiResponse.IsSuccessStatusCode) // Ensures status is 200
                        {
                            SellData sellData = new SellData
                            {
                                symboltoken = stock.symboltoken,
                                tradingsymbol = stock.ticker,
                                CurrentPrice = LastCandel.EndPrice
                            };

                            var jsonRequestBodyForsellData = JsonConvert.SerializeObject(sellData);
                            var contentForsellData = new StringContent(jsonRequestBodyForsellData, Encoding.UTF8, "application/json");

                            var sellDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/sell", contentForsellData);
                        }

                        ExtractedCandelData.Add(LastCandel);

                    }


                }

            }





            //// CODE TO FIND THE STOCKS WHICH ARE BELOW 3% FROM PREVIOUS DAYS END PRICE
            //while (!stoppingToken.IsCancellationRequested)
            //{

            //    foreach (var stock in _stocks)
            //    {

            //        // THIS PART FETCEHES THE END PRICE OF THE PREVIOUS DAYS STOCK
            //        HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/candel", stoppingToken);
            //        response.EnsureSuccessStatusCode();

            //        string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

            //        List<Candel> CandelData = JsonConvert.DeserializeObject<List<Candel>>(responseData);

            //        Candel previousDayCandel = CandelData.FirstOrDefault(c => c.Ticker == stock.ticker);

            //        decimal expectedPrice = previousDayCandel.EndPrice - (previousDayCandel.EndPrice * 0.03m);



            //        // THIS PART FETCHES THE LATEST PRICE OF THE STOCK
            //        var requestBodyforCurrentDayData = new
            //        {
            //            SymbolToken = stock.symboltoken,
            //            AuthorizationToken = "",
            //            StartDate = DateTime.Now.AddHours(9).AddMinutes(15).ToString("o"), // Final adjusted date
            //            EndDate = DateTime.Now.ToString("o")     // Final adjusted date
            //        };

            //        var jsonRequestBodyForCurrentDaysData = JsonConvert.SerializeObject(requestBodyforCurrentDayData);
            //        var contentForCurrentDaysData = new StringContent(jsonRequestBodyForCurrentDaysData, Encoding.UTF8, "application/json");


            //        var responseCurrent = await _httpClient.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest", contentForCurrentDaysData);

            //        responseCurrent.EnsureSuccessStatusCode();

            //        string responseCurrentData = await responseCurrent.Content.ReadAsStringAsync(stoppingToken);

            //        List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseCurrentData);

            //        Candel LastCandel = candels.LastOrDefault();


            //        List<Candel> ExtractedCandelData = new List<Candel>();


            //        //  THIS PART COMPARES THE LATEST PRICE WITH THE EXPECTED PRICE
            //        if (
            //            LastCandel != null
            //            && LastCandel.EndPrice <= expectedPrice
            //            )
            //        {
            //            // BUY API HERE
            //            BuyData buyData = new BuyData
            //            {
            //                symboltoken = stock.symboltoken,
            //                tradingsymbol = stock.ticker,
            //                CurrentPrice = LastCandel.EndPrice,
            //                PreviousDaayEndPrice = previousDayCandel.EndPrice,
            //            };

            //            var jsonRequestBodyForbuyData = JsonConvert.SerializeObject(buyData);
            //            var contentForbuyData = new StringContent(jsonRequestBodyForbuyData, Encoding.UTF8, "application/json");

            //            var buyDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/buy", contentForbuyData);

            //            ExtractedCandelData.Add(LastCandel);

            //        }


            //    }

            //}



            //// CODE TO POPULATE THE DB WITH END PRICES

            //foreach (var stock in _stocks)
            //{
            //    await Post5MinCandelToDb(stock.symboltoken, stoppingToken);

            //    BuyData buyData = new BuyData
            //    {
            //        symboltoken = stock.symboltoken,
            //        tradingsymbol = stock.ticker,
            //        CurrentPrice = 0
            //    };

            //    var jsonRequestBodyForbuyData = JsonConvert.SerializeObject(buyData);
            //    var contentForbuyData = new StringContent(jsonRequestBodyForbuyData, Encoding.UTF8, "application/json");

            //    var buyDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/buy", contentForbuyData);


            //}

            //while (!stoppingToken.IsCancellationRequested)
            //{

            //    foreach (var stock in _stocks)
            //    {
            //        HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:44364/api/candel", stoppingToken);
            //        response.EnsureSuccessStatusCode();

            //        string responseData = await response.Content.ReadAsStringAsync(stoppingToken);

            //        List<Candel> CandelData = JsonConvert.DeserializeObject<List<Candel>>(responseData);

            //        if(CandelData.Count == 162)
            //        {
            //            break;
            //        }

            //        Candel selectedCandel = CandelData.FirstOrDefault(c => c.Ticker == stock.ticker);

            //        if (selectedCandel == null)
            //        {
            //            await Post1MinCandelToDb(stock.symboltoken, stoppingToken);

            //            BuyData buyData = new BuyData
            //            {
            //                symboltoken = stock.symboltoken,
            //                tradingsymbol = stock.ticker,
            //                CurrentPrice = 0,
            //            };

            //            var jsonRequestBodyForbuyData = JsonConvert.SerializeObject(buyData);
            //            var contentForbuyData = new StringContent(jsonRequestBodyForbuyData, Encoding.UTF8, "application/json");

            //            var buyDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/buy", contentForbuyData);
            //        }

            //    }

            //}



            //List<double> iterationTimes = new();

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    var stopwatch = Stopwatch.StartNew();

            //    var tasks = _stocks.Select(stock => Task.Run(async () =>
            //    {
            //        try
            //        {
            //            await Post1MinCandelToDb(stock.symboltoken, stoppingToken);
            //            //await Post5MinCandelToDb(stock.ticker, stoppingToken);
            //            //await Post10MinCandelToDb(stock.ticker, stoppingToken);
            //            //await Post15MinCandelToDb(stock.ticker, stoppingToken);
            //        }
            //        catch (Exception ex)
            //        {
            //        }
            //    }, stoppingToken));



            //    // Wait for all tasks to complete.
            //    await Task.WhenAll(tasks);

            //    stopwatch.Stop();
            //    iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);

            //    // Trigger garbage collection periodically
            //    GC.Collect();
            //    GC.WaitForPendingFinalizers();

            //    //foreach (var stock in _stocks)
            //    //{
            //    //    await Post1MinCandelToDb(stock.symboltoken, stoppingToken);
            //    //}

            //}

        }

    }

}
