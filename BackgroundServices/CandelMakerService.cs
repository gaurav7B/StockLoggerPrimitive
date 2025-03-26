using Azure;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skender.Stock.Indicators;
using StockLogger.Data;
using StockLogger.Models.Candel;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Text;
using static StockLogger.Controllers.API_Controllers.BuySellController;
using static StockLogger.Controllers.API_Controllers.TestDownTrendController;

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


        public decimal? CalculateLatestRSI(List<Candel> CandelBeforeTestCandel)
        {
            // Check for valid input
            if (CandelBeforeTestCandel == null || CandelBeforeTestCandel.Count < 15)
                return null;

            List<decimal> closes = CandelBeforeTestCandel.Select(c => c.EndPrice).ToList();
            List<decimal> deltas = new List<decimal>();

            for (int i = 1; i < closes.Count; i++)
            {
                deltas.Add(closes[i] - closes[i - 1]);
            }

            List<decimal> gains = new List<decimal>();
            List<decimal> losses = new List<decimal>();

            foreach (decimal delta in deltas)
            {
                if (delta > 0)
                {
                    gains.Add(delta);
                    losses.Add(0);
                }
                else if (delta < 0)
                {
                    gains.Add(0);
                    losses.Add(-delta);
                }
                else
                {
                    gains.Add(0);
                    losses.Add(0);
                }
            }

            decimal avgGain = gains.Take(14).Average();
            decimal avgLoss = losses.Take(14).Average();

            decimal latestRSI = 0m;

            if (avgLoss == 0)
            {
                latestRSI = 100m;
            }
            else
            {
                decimal rs = avgGain / avgLoss;
                latestRSI = 100 - (100 / (1 + rs));
            }

            for (int i = 14; i < deltas.Count; i++)
            {
                decimal currentGain = gains[i];
                decimal currentLoss = losses[i];

                avgGain = (avgGain * 13 + currentGain) / 14;
                avgLoss = (avgLoss * 13 + currentLoss) / 14;

                if (avgLoss == 0)
                {
                    latestRSI = 100m;
                }
                else
                {
                    decimal rs = avgGain / avgLoss;
                    latestRSI = 100 - (100 / (1 + rs));
                }
            }

            return latestRSI;
        }


        ///// MFI
        public decimal? CalculateMFI(List<Candel> InputList, int period = 14)
        {
            // Check for valid input
            if (InputList == null || InputList.Count < period)
                return null;

            List<decimal> positiveFlow = new List<decimal>();
            List<decimal> negativeFlow = new List<decimal>();

            for (int i = 1; i < InputList.Count; i++)
            {
                var typicalPrice = (InputList[i].HighestPrice + InputList[i].LowestPrice + InputList[i].EndPrice) / 3;
                var moneyFlow = typicalPrice * InputList[i].Volume;

                var prevTypicalPrice = (InputList[i - 1].HighestPrice + InputList[i - 1].LowestPrice + InputList[i - 1].EndPrice) / 3;

                if (typicalPrice > prevTypicalPrice)
                    positiveFlow.Add(moneyFlow);
                else if (typicalPrice < prevTypicalPrice)
                    negativeFlow.Add(moneyFlow);
                else
                {
                    positiveFlow.Add(0);
                    negativeFlow.Add(0);
                }
            }

            int startIndex = Math.Max(0, positiveFlow.Count - period);
            decimal positiveSum = positiveFlow.Skip(startIndex).Take(period).Sum();
            decimal negativeSum = negativeFlow.Skip(startIndex).Take(period).Sum();

            if (negativeSum == 0)
                return 100; // If no negative money flow, MFI is 100

            decimal moneyFlowRatio = positiveSum / negativeSum;
            decimal MFI = 100 - (100 / (1 + moneyFlowRatio));

            return MFI;
        }


        ///// WILLIAMSR

        public static decimal? CalculateWilliamsR(List<Candel> inputList)
        {
            int period = 14;

            // Check for valid input
            if (inputList == null || inputList.Count < period)
                return null;

            // Extract the last 'period' candles from the input list
            var window = inputList.Skip(inputList.Count - period).Take(period).ToList();

            // Calculate highest high and lowest low in the window
            decimal maxHigh = window.Max(c => c.HighestPrice);
            decimal minLow = window.Min(c => c.LowestPrice);
            decimal close = inputList.Last().EndPrice; // Closing price of the latest candle

            // Handle division by zero case (flat line)
            decimal denominator = maxHigh - minLow;
            if (denominator == 0)
                return 0;

            // Compute Williams %R
            decimal williamsR = ((maxHigh - close) / denominator) * -100;
            return williamsR;
        }


        //// CCI Commodity Channel Index

        public static decimal? CalculateCCI(List<Candel> inputList, int period = 20)
        {
            if (inputList == null || inputList.Count < period)
                return null;

            List<decimal> typicalPrices = inputList
                .Skip(inputList.Count - period)
                .Select(c => (c.HighestPrice + c.LowestPrice + c.EndPrice) / 3)
                .ToList();

            decimal sma = typicalPrices.Average();

            decimal meanDeviation = typicalPrices.Average(tp => Math.Abs(tp - sma));

            if (meanDeviation == 0)
                return 0;

            decimal currentTp = typicalPrices.Last();
            decimal cciValue = (currentTp - sma) / (0.015m * meanDeviation);

            return cciValue;
        }

        public static decimal? CalculateStochasticOscillator(List<Candel> inputList, int period = 14)
        {
            if (inputList == null || inputList.Count < period)
                return null;

            // Get the last 'period' candles
            var recentCandles = inputList.TakeLast(period).ToList();

            // Calculate the Highest High and Lowest Low over the period
            decimal highestHigh = recentCandles.Max(c => c.HighestPrice);
            decimal lowestLow = recentCandles.Min(c => c.LowestPrice);

            // Get the closing price of the most recent candle
            decimal currentClose = recentCandles.Last().EndPrice;

            // Calculate Stochastic Oscillator (%K)
            decimal stochastic = ((currentClose - lowestLow) / (highestHigh - lowestLow)) * 100;

            return stochastic;
        }

        public List<(decimal? Rsi, Candel Candel)> CalculateRSI(List<Candel> inputList, int period = 14)
        {
            List<(decimal? Rsi, Candel Candel)> result = new List<(decimal? Rsi, Candel Candel)>();

            if (inputList == null || inputList.Count < period + 1)
            {
                // Not enough data to calculate RSI
                foreach (var candel in inputList)
                {
                    result.Add((null, candel));
                }
                return result;
            }

            // Extract EndPrices
            List<decimal> endPrices = inputList.Select(c => c.EndPrice).ToList();

            // Calculate deltas
            List<decimal> deltas = new List<decimal>();
            for (int i = 1; i < endPrices.Count; i++)
            {
                deltas.Add(endPrices[i] - endPrices[i - 1]);
            }

            // Separate into gains and losses
            List<decimal> gains = new List<decimal>();
            List<decimal> losses = new List<decimal>();
            foreach (decimal delta in deltas)
            {
                gains.Add(delta > 0 ? delta : 0);
                losses.Add(delta < 0 ? -delta : 0);
            }

            // Calculate initial averages
            decimal avgGain = gains.Take(period).Average();
            decimal avgLoss = losses.Take(period).Average();

            // Handle the case where avgLoss is zero to avoid division by zero
            decimal rs = avgLoss == 0 ? 0 : avgGain / avgLoss;
            decimal rsi = avgLoss == 0 ? 100m : 100m - (100m / (1 + rs));

            // Add nulls for the first 'period' entries
            for (int i = 0; i < period; i++)
            {
                result.Add((null, inputList[i]));
            }

            // Add the first RSI for the (period)th Candel
            result.Add((rsi, inputList[period]));

            // Calculate subsequent RSIs
            for (int i = period; i < gains.Count; i++)
            {
                avgGain = (avgGain * (period - 1) + gains[i]) / period;
                avgLoss = (avgLoss * (period - 1) + losses[i]) / period;

                if (avgLoss == 0)
                {
                    rsi = 100m;
                }
                else
                {
                    rs = avgGain / avgLoss;
                    rsi = 100m - (100m / (1 + rs));
                }

                result.Add((rsi, inputList[i + 1])); // Pair RSI with the next Candel
            }

            // Fill the remaining entries with nulls if necessary
            while (result.Count < inputList.Count)
            {
                result.Add((null, inputList[result.Count])); // Pair remaining nulls with Candels
            }

            return result;
        }

        public List<RSICandel> ConvertToRSICandelList(List<(decimal? Rsi, Candel Candel)> rsiCandelList)
        {
            return rsiCandelList.Select(item => new RSICandel
            {
                Id = item.Candel.Id,
                StartPrice = item.Candel.StartPrice,
                HighestPrice = item.Candel.HighestPrice,
                LowestPrice = item.Candel.LowestPrice,
                EndPrice = item.Candel.EndPrice,
                OpenTime = item.Candel.OpenTime,
                CloseTime = item.Candel.CloseTime,
                IsBullish = item.Candel.IsBullish,
                IsBearish = item.Candel.IsBearish,
                Ticker = item.Candel.Ticker,
                TickerId = item.Candel.TickerId,
                Exchange = item.Candel.Exchange,
                PriceChange = item.Candel.PriceChange,
                PriceChangePercentage = item.Candel.PriceChangePercentage,
                Volume = item.Candel.Volume,
                RSI = item.Rsi // Assign the RSI value
            }).ToList();
        }

        public class ApiResponse
        {
            public bool Status { get; set; }
            public string Message { get; set; }
            public string ErrorCode { get; set; }
            public OrderData Data { get; set; }
        }

        public class OrderData
        {
            public string Script { get; set; }
            public string OrderId { get; set; }
            public string UniqueOrderId { get; set; }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            //// CODE TO POPULATE THE DB WITH END PRICES

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

            //        try
            //        {

            //            // THIS PART FETCHES THE LATEST PRICE OF THE STOCK
            //            var requestBodyforCurrentDayData = new
            //            {
            //                SymbolToken = stock.symboltoken,
            //                AuthorizationToken = "",
            //                StartDate = DateTime.Now.AddHours(9).AddMinutes(15).ToString("o"), // Final adjusted date
            //                EndDate = DateTime.Now.ToString("o")     // Final adjusted date
            //            };

            //            var jsonRequestBodyForCurrentDaysData = JsonConvert.SerializeObject(requestBodyforCurrentDayData);
            //            var contentForCurrentDaysData = new StringContent(jsonRequestBodyForCurrentDaysData, Encoding.UTF8, "application/json");


            //            var responseCurrent = await _httpClient.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest", contentForCurrentDaysData);

            //            //responseCurrent.EnsureSuccessStatusCode();

            //            string responseCurrentData = await responseCurrent.Content.ReadAsStringAsync(stoppingToken);

            //            List<Candel> candels = JsonConvert.DeserializeObject<List<Candel>>(responseCurrentData);

            //            Candel LastCandel = candels.LastOrDefault();

            //            List<Candel> ExtractedCandelData = new List<Candel>();


            //            //  THIS PART COMPARES THE LATEST PRICE WITH THE EXPECTED PRICE
            //            if (
            //                LastCandel != null
            //                && previousDayCandel != null
            //                && LastCandel.EndPrice <= expectedPrice
            //                )
            //            {
            //                // BUY API HERE
            //                BuyData buyData = new BuyData
            //                {
            //                    symboltoken = stock.symboltoken,
            //                    tradingsymbol = stock.ticker,
            //                    CurrentPrice = LastCandel.EndPrice,
            //                    PreviousDaayEndPrice = previousDayCandel.EndPrice,
            //                };

            //                var jsonRequestBodyForbuyData = JsonConvert.SerializeObject(buyData);
            //                var contentForbuyData = new StringContent(jsonRequestBodyForbuyData, Encoding.UTF8, "application/json");

            //                var buyDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/buy", contentForbuyData);

            //                buyDataApiResponse.EnsureSuccessStatusCode();

            //                // WHEN THE BUY API RUNS SUCCESSFULLY
            //                // CALL THE SELL API
            //                // SELL API HERE
            //                if (buyDataApiResponse.IsSuccessStatusCode) // Ensures status is 200
            //                {
            //                    SellData sellData = new SellData
            //                    {
            //                        symboltoken = stock.symboltoken,
            //                        tradingsymbol = stock.ticker,
            //                        CurrentPrice = LastCandel.EndPrice
            //                    };

            //                    var jsonRequestBodyForsellData = JsonConvert.SerializeObject(sellData);
            //                    var contentForsellData = new StringContent(jsonRequestBodyForsellData, Encoding.UTF8, "application/json");

            //                    var sellDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/sell", contentForsellData);
            //                }

            //                ExtractedCandelData.Add(LastCandel);

            //            }

            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(ex);
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

            //            // THIS PART FETCHES THE LATEST PRICE OF THE STOCK
            //            var requestBodyforCurrentDayData = new
            //            {
            //                SymbolToken = stock.symboltoken,
            //                AuthorizationToken = "",
            //                StartDate = DateTime.Now.AddHours(9).AddMinutes(15).ToString("o"), // Final adjusted date
            //                EndDate = DateTime.Now.ToString("o")     // Final adjusted date
            //            };

            //            var jsonRequestBodyForCurrentDaysData = JsonConvert.SerializeObject(requestBodyforCurrentDayData);
            //            var contentForCurrentDaysData = new StringContent(jsonRequestBodyForCurrentDaysData, Encoding.UTF8, "application/json");


            //            var responseCurrent = await _httpClient.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest", contentForCurrentDaysData);

            //            string responseCurrentData = await responseCurrent.Content.ReadAsStringAsync(stoppingToken);

            //            List<Candel> CandelData = JsonConvert.DeserializeObject<List<Candel>>(responseCurrentData);

            //            List<Candel> TotalList = CandelData
            //                                .OrderBy(c => c.OpenTime)
            //                                .ToList();

            //            Candel testCandel = TotalList.LastOrDefault();


            //            List<(decimal? Rsi, Candel Candel)> RSIData = CalculateRSI(TotalList);

            //            decimal? CurrentRSI = null;

            //            if (RSIData != null)
            //            {
            //                foreach (var data in RSIData)
            //                {
            //                    if (data.Candel == testCandel)
            //                    {
            //                        CurrentRSI = data.Rsi;
            //                    }
            //                }
            //            }

            //            //  THIS PART COMPARES THE LATEST PRICE WITH THE EXPECTED PRICE
            //            if (
            //                testCandel != null
            //                && ((CurrentRSI != null) && (CurrentRSI > 90))
            //                )
            //            {
            //                // BUY API HERE
            //                BuyData buyData = new BuyData
            //                {
            //                    symboltoken = stock.symboltoken,
            //                    tradingsymbol = stock.ticker,
            //                    CurrentPrice = testCandel.EndPrice,
            //                    PreviousDaayEndPrice = testCandel.EndPrice,
            //                };

            //                var jsonRequestBodyForbuyData = JsonConvert.SerializeObject(buyData);
            //                var contentForbuyData = new StringContent(jsonRequestBodyForbuyData, Encoding.UTF8, "application/json");

            //                var buyDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/buy", contentForbuyData);

            //                string responsdata = await buyDataApiResponse.Content.ReadAsStringAsync(stoppingToken);

            //                ApiResponse ApiResponseData = JsonConvert.DeserializeObject<ApiResponse>(responsdata);


            //                //buyDataApiResponse.EnsureSuccessStatusCode();

            //                // WHEN THE BUY API RUNS SUCCESSFULLY
            //                // CALL THE SELL API
            //                // SELL API HERE
            //                if (ApiResponseData.Status != false) // Ensures status is 200
            //                {
            //                    SellData sellData = new SellData
            //                    {
            //                        symboltoken = stock.symboltoken,
            //                        tradingsymbol = stock.ticker,
            //                        CurrentPrice = testCandel.EndPrice
            //                    };

            //                    var jsonRequestBodyForsellData = JsonConvert.SerializeObject(sellData);
            //                    var contentForsellData = new StringContent(jsonRequestBodyForsellData, Encoding.UTF8, "application/json");

            //                    var sellDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/sell", contentForsellData);
            //                }

            //            }

            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(ex.ToString());
            //        }
            //    }, stoppingToken));



            //    // Wait for all tasks to complete.
            //    await Task.WhenAll(tasks);

            //    stopwatch.Stop();
            //    iterationTimes.Add(stopwatch.Elapsed.TotalMilliseconds);

            //    // Trigger garbage collection periodically
            //    GC.Collect();
            //    GC.WaitForPendingFinalizers();

            //}


            ////// PERCENT PRICE CHANGE STRATERGY > 90 ////////////////////

            //while (!stoppingToken.IsCancellationRequested)
            //{

            //    foreach (var stock in _stocks)
            //    {
            //        try
            //        {

            //            // THIS PART FETCHES THE LATEST PRICE OF THE STOCK
            //            var requestBodyforCurrentDayData = new
            //            {
            //                SymbolToken = stock.symboltoken,
            //                AuthorizationToken = "",
            //                StartDate = DateTime.Now.AddHours(9).AddMinutes(15).ToString("o"), // Final adjusted date
            //                EndDate = DateTime.Now.ToString("o")     // Final adjusted date
            //            };

            //            var jsonRequestBodyForCurrentDaysData = JsonConvert.SerializeObject(requestBodyforCurrentDayData);
            //            var contentForCurrentDaysData = new StringContent(jsonRequestBodyForCurrentDaysData, Encoding.UTF8, "application/json");


            //            var responseCurrent = await _httpClient.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest5Paisa", contentForCurrentDaysData);

            //            string responseCurrentData = await responseCurrent.Content.ReadAsStringAsync(stoppingToken);

            //            List<Candel> CandelData = JsonConvert.DeserializeObject<List<Candel>>(responseCurrentData);

            //            List<Candel> TotalList = CandelData.OrderBy(c => c.OpenTime).ToList();


            //            // MAIN LOGIC
            //            Candel testCandel = TotalList.LastOrDefault();

            //            decimal startPrice = testCandel.StartPrice;
            //            decimal highestPrice = testCandel.EndPrice;

            //            decimal percentageChange = ((highestPrice - startPrice) / startPrice) * 100;

            //            //  THIS PART EXECUTES SELL AND BUY API IF CONDITIONS ARE MET
            //            if (
            //                testCandel != null
            //                && (testCandel.EndPrice > testCandel.StartPrice)
            //                && (percentageChange * 100 > 90)
            //                )
            //            {
            //                // BUY API HERE
            //                BuyData buyData = new BuyData
            //                {
            //                    symboltoken = stock.symboltoken,
            //                    tradingsymbol = stock.ticker,
            //                    CurrentPrice = testCandel.EndPrice,
            //                    PreviousDaayEndPrice = testCandel.EndPrice,
            //                };

            //                var jsonRequestBodyForbuyData = JsonConvert.SerializeObject(buyData);
            //                var contentForbuyData = new StringContent(jsonRequestBodyForbuyData, Encoding.UTF8, "application/json");

            //                var buyDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/buy", contentForbuyData);

            //                string responsdata = await buyDataApiResponse.Content.ReadAsStringAsync(stoppingToken);

            //                ApiResponse ApiResponseData = JsonConvert.DeserializeObject<ApiResponse>(responsdata);


            //                // WHEN THE BUY API RUNS SUCCESSFULLY
            //                // CALL THE SELL API
            //                // SELL API HERE
            //                if (ApiResponseData.Message == "SUCCESS") // Ensures status is 200
            //                {
            //                    SellData sellData = new SellData
            //                    {
            //                        symboltoken = stock.symboltoken,
            //                        tradingsymbol = stock.ticker,
            //                        CurrentPrice = testCandel.EndPrice,
            //                        UniqueOrderId = ApiResponseData.Data.UniqueOrderId
            //                    };

            //                    var jsonRequestBodyForsellData = JsonConvert.SerializeObject(sellData);
            //                    var contentForsellData = new StringContent(jsonRequestBodyForsellData, Encoding.UTF8, "application/json");

            //                    var sellDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/sell", contentForsellData);
            //                }

            //            }

            //        }
            //        catch (Exception ex)
            //        {
            //            Console.Write(ex);
            //        }




            //    }
            //}



            while (!stoppingToken.IsCancellationRequested)
            {

                foreach (var stock in _stocks)
                {
                    try
                    {

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


                        var responseCurrent = await _httpClient.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest5Paisa", contentForCurrentDaysData);

                        string responseCurrentData = await responseCurrent.Content.ReadAsStringAsync(stoppingToken);

                        List<Candel> CandelData = JsonConvert.DeserializeObject<List<Candel>>(responseCurrentData);

                        List<Candel> TotalList = CandelData.OrderBy(c => c.OpenTime).ToList();

                        Candel testCandel = TotalList.LastOrDefault();

                        List<(decimal? Rsi, Candel Candel)> RSIData = CalculateRSI(TotalList);

                        decimal? CurrentRSI = null;

                        if (RSIData != null)
                        {
                            foreach (var data in RSIData)
                            {
                                if (data.Candel == testCandel)
                                {
                                    CurrentRSI = data.Rsi;
                                }
                            }
                        }

                        //List<RSICandel> RsiCandelList = ConvertToRSICandelList(RSIData);

                        //RSICandel secondLastRSICandel = RsiCandelList.OrderByDescending(c => c.OpenTime).Skip(1).First();
                        //RSICandel currentRSICandel = RsiCandelList.LastOrDefault();




                        // Sample stock data
                        List<Quote> stockData = new List<Quote>();
                        foreach (var candel in TotalList)
                        {
                            Quote quote = new Quote
                            {
                                Date = candel.OpenTime,
                                Open = candel.StartPrice,
                                High = candel.HighestPrice,
                                Low = candel.LowestPrice,
                                Close = candel.EndPrice,
                                Volume = candel.Volume,
                            };
                            stockData.Add(quote);
                        }

                        IEnumerable<BollingerBandsResult> bollingerBandsResult = stockData.GetBollingerBands(TotalList.Count, 4);
                        BollingerBandsResult currentBollingerBandsResult = bollingerBandsResult.LastOrDefault();


                        if (
                            (CurrentRSI > 70)
                            //(testCandel.EndPrice > (decimal)currentBollingerBandsResult.UpperBand)
                            )
                        {
                            // BUY API HERE
                            BuyData buyData = new BuyData
                            {
                                symboltoken = stock.symboltoken,
                                tradingsymbol = stock.ticker,
                                CurrentPrice = testCandel.EndPrice,
                                PreviousDaayEndPrice = testCandel.EndPrice,
                            };

                            var jsonRequestBodyForbuyData = JsonConvert.SerializeObject(buyData);
                            var contentForbuyData = new StringContent(jsonRequestBodyForbuyData, Encoding.UTF8, "application/json");

                            var buyDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/buy", contentForbuyData);

                            string responsdata = await buyDataApiResponse.Content.ReadAsStringAsync(stoppingToken);

                            ApiResponse ApiResponseData = JsonConvert.DeserializeObject<ApiResponse>(responsdata);


                            // WHEN THE BUY API RUNS SUCCESSFULLY
                            // CALL THE SELL API
                            // SELL API HERE
                            if (ApiResponseData.Message == "SUCCESS") // Ensures status is 200
                            {
                                SellData sellData = new SellData
                                {
                                    symboltoken = stock.symboltoken,
                                    tradingsymbol = stock.ticker,
                                    CurrentPrice = testCandel.EndPrice,
                                    UniqueOrderId = ApiResponseData.Data.UniqueOrderId
                                };

                                var jsonRequestBodyForsellData = JsonConvert.SerializeObject(sellData);
                                var contentForsellData = new StringContent(jsonRequestBodyForsellData, Encoding.UTF8, "application/json");

                                var sellDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/sell", contentForsellData);
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex);
                    }




                }
            }


            ////// 3% REDUCTION

            //// CODE TO FIND THE STOCKS WHICH ARE BELOW 3% FROM PREVIOUS DAYS END PRICE
            //while (!stoppingToken.IsCancellationRequested)
            //{

            //    foreach (var stock in _stocks)
            //    {
            //        try
            //        {

            //            // THIS PART FETCHES THE LATEST PRICE OF THE STOCK
            //            var requestBodyforCurrentDayData = new
            //            {
            //                SymbolToken = stock.symboltoken,
            //                AuthorizationToken = "",
            //                StartDate = DateTime.Now.AddHours(9).AddMinutes(15).ToString("o"), // Final adjusted date
            //                EndDate = DateTime.Now.ToString("o")     // Final adjusted date
            //            };

            //            var jsonRequestBodyForCurrentDaysData = JsonConvert.SerializeObject(requestBodyforCurrentDayData);
            //            var contentForCurrentDaysData = new StringContent(jsonRequestBodyForCurrentDaysData, Encoding.UTF8, "application/json");


            //            //var responseCurrent = await _httpClient.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest", contentForCurrentDaysData);
            //            var responseCurrent = await _httpClient.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest5Paisa", contentForCurrentDaysData);

            //            string responseCurrentData = await responseCurrent.Content.ReadAsStringAsync(stoppingToken);

            //            List<Candel> CandelData = JsonConvert.DeserializeObject<List<Candel>>(responseCurrentData);

            //            List<Candel> TotalList = CandelData
            //                                .OrderBy(c => c.OpenTime)
            //                                .ToList();

            //            Candel testCandel = TotalList.LastOrDefault();

            //            if (testCandel == null)
            //            {
            //                continue;
            //            }

            //            List<(decimal? Rsi, Candel Candel)> RSIData = CalculateRSI(TotalList);

            //            decimal? CurrentRSI = null;

            //            if (RSIData != null)
            //            {
            //                foreach (var data in RSIData)
            //                {
            //                    if (data.Candel == testCandel)
            //                    {
            //                        CurrentRSI = data.Rsi;
            //                    }
            //                }
            //            }

            //            List<Candel> PreviousData = CandelData
            //                        .Where(candel => candel.OpenTime < testCandel.OpenTime)
            //                        .OrderBy(c => c.OpenTime)
            //                        .ToList();

            //            bool isAnyHigher = PreviousData.Any(candel => candel.HighestPrice > testCandel.EndPrice);

            //            //  THIS PART COMPARES THE LATEST PRICE WITH THE EXPECTED PRICE
            //            if (
            //                testCandel != null
            //                && ((CurrentRSI != null) && (CurrentRSI > 70))
            //                && (isAnyHigher == false)
            //                )
            //            {
            //                // BUY API HERE
            //                BuyData buyData = new BuyData
            //                {
            //                    symboltoken = stock.symboltoken,
            //                    tradingsymbol = stock.ticker,
            //                    CurrentPrice = testCandel.EndPrice,
            //                    PreviousDaayEndPrice = testCandel.EndPrice,
            //                };

            //                var jsonRequestBodyForbuyData = JsonConvert.SerializeObject(buyData);
            //                var contentForbuyData = new StringContent(jsonRequestBodyForbuyData, Encoding.UTF8, "application/json");

            //                var buyDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/buy", contentForbuyData);

            //                string responsdata = await buyDataApiResponse.Content.ReadAsStringAsync(stoppingToken);

            //                ApiResponse ApiResponseData = JsonConvert.DeserializeObject<ApiResponse>(responsdata);


            //                // WHEN THE BUY API RUNS SUCCESSFULLY
            //                // CALL THE SELL API
            //                // SELL API HERE
            //                if (ApiResponseData.Message == "SUCCESS") // Ensures status is 200
            //                {
            //                    SellData sellData = new SellData
            //                    {
            //                        symboltoken = stock.symboltoken,
            //                        tradingsymbol = stock.ticker,
            //                        CurrentPrice = testCandel.EndPrice,
            //                        UniqueOrderId = ApiResponseData.Data.UniqueOrderId
            //                    };

            //                    var jsonRequestBodyForsellData = JsonConvert.SerializeObject(sellData);
            //                    var contentForsellData = new StringContent(jsonRequestBodyForsellData, Encoding.UTF8, "application/json");

            //                    var sellDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/sell", contentForsellData);
            //                }

            //            }

            //        }
            //        catch (Exception ex)
            //        {
            //            Console.Write(ex);
            //        }




            //    }
            //}









            ////// DRAGONFLYDOJI

            //// CODE TO FIND THE STOCKS WHICH ARE BELOW 3% FROM PREVIOUS DAYS END PRICE
            //while (!stoppingToken.IsCancellationRequested)
            //{

            //    foreach (var stock in _stocks)
            //    {

            //        try
            //        {

            //            // THIS PART FETCHES THE LATEST PRICE OF THE STOCK
            //            var requestBodyforCurrentDayData = new
            //            {
            //                SymbolToken = stock.symboltoken,
            //                AuthorizationToken = "",
            //                StartDate = DateTime.Now.AddHours(9).AddMinutes(15).ToString("o"), // Final adjusted date
            //                EndDate = DateTime.Now.ToString("o")     // Final adjusted date
            //            };

            //            var jsonRequestBodyForCurrentDaysData = JsonConvert.SerializeObject(requestBodyforCurrentDayData);
            //            var contentForCurrentDaysData = new StringContent(jsonRequestBodyForCurrentDaysData, Encoding.UTF8, "application/json");


            //            var responseCurrent = await _httpClient.PostAsync("https://localhost:44364/api/AngelCandel/getCandleDataForTest", contentForCurrentDaysData);

            //            //responseCurrent.EnsureSuccessStatusCode();

            //            string responseCurrentData = await responseCurrent.Content.ReadAsStringAsync(stoppingToken);

            //            List<Candel> Candels = JsonConvert.DeserializeObject<List<Candel>>(responseCurrentData);

            //            List<Candel> CandelData = Candels.OrderBy(c => c.OpenTime).ToList();

            //            Candel testCandel = CandelData.LastOrDefault();


            //            List<Candel> ListForRSI = CandelData
            //                                .Where(candel => candel.OpenTime <= testCandel.OpenTime)
            //                                .OrderByDescending(c => c.OpenTime)
            //                                .Take(15)
            //                                .OrderBy(c => c.OpenTime)
            //                                .ToList();

            //            List<Candel> ListForMFI = CandelData
            //                                .Where(candel => candel.OpenTime <= testCandel.OpenTime)
            //                                .OrderByDescending(c => c.OpenTime)
            //                                .Take(14)
            //                                .OrderBy(c => c.OpenTime)
            //                                .ToList();

            //            List<Candel> ListForCCI = CandelData
            //                                .Where(candel => candel.OpenTime <= testCandel.OpenTime)
            //                                .OrderByDescending(c => c.OpenTime)
            //                                .Take(20)
            //                                .OrderBy(c => c.OpenTime)
            //                                .ToList();

            //            decimal? RSI = CalculateLatestRSI(ListForRSI);

            //            decimal? MFI = CalculateMFI(ListForMFI);

            //            decimal? WILLIAMSR = CalculateWilliamsR(ListForMFI);

            //            decimal? CCI = CalculateCCI(ListForCCI);

            //            decimal? SO = CalculateStochasticOscillator(ListForMFI);


            //            List<Candel> ExtractedCandelData = new List<Candel>();


            //            //  THIS PART COMPARES THE LATEST PRICE WITH THE EXPECTED PRICE
            //            if (
            //                (
            //                        ((RSI != null) && (RSI > 70))
            //                        && ((SO != null) && (SO > 80))
            //                        && ((MFI != null) && (MFI > 80))

            //                        && ((WILLIAMSR != null) && (WILLIAMSR > -20) && (WILLIAMSR <= 0))
            //                        && ((CCI != null) && (CCI > 100))

            //                ) // RSI > 70 STOCK IS OVERBOUGHT HIGH SELLING PRESSURE
            //               )
            //            {
            //                // BUY API HERE
            //                BuyData buyData = new BuyData
            //                {
            //                    symboltoken = stock.symboltoken,
            //                    tradingsymbol = stock.ticker,
            //                    CurrentPrice = testCandel.EndPrice,
            //                    PreviousDaayEndPrice = 0,
            //                };

            //                var jsonRequestBodyForbuyData = JsonConvert.SerializeObject(buyData);
            //                var contentForbuyData = new StringContent(jsonRequestBodyForbuyData, Encoding.UTF8, "application/json");

            //                var buyDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/buy", contentForbuyData);

            //                buyDataApiResponse.EnsureSuccessStatusCode();

            //                // WHEN THE BUY API RUNS SUCCESSFULLY
            //                // CALL THE SELL API
            //                // SELL API HERE
            //                if (buyDataApiResponse.IsSuccessStatusCode) // Ensures status is 200
            //                {
            //                    SellData sellData = new SellData
            //                    {
            //                        symboltoken = stock.symboltoken,
            //                        tradingsymbol = stock.ticker,
            //                        CurrentPrice = testCandel.EndPrice
            //                    };

            //                    var jsonRequestBodyForsellData = JsonConvert.SerializeObject(sellData);
            //                    var contentForsellData = new StringContent(jsonRequestBodyForsellData, Encoding.UTF8, "application/json");

            //                    var sellDataApiResponse = await _httpClient.PostAsync("https://localhost:44364/api/BuySell/sell", contentForsellData);
            //                }

            //                ExtractedCandelData.Add(testCandel);

            //            }

            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(ex);
            //        }

            //    }

            //}





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
