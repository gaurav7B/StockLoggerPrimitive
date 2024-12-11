using StockLogger.Models.Candel;
using StockLogger.Models.DTO;
using System.Web.WebPages;

namespace StockLogger.BackgroundServices
{
    public class CandelMakerService : BackgroundService
    {
        private readonly ILogger<CandelMakerService> _logger;
        private readonly HttpClient _httpClient;

        public CandelMakerService(ILogger<CandelMakerService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        var stocks = (await GetStockTickerExchanges())?.Select(x => new { x.Id, x.Ticker, x.Exchange }).ToArray();

        //        var stockTasks = stocks.Select(stock => CandelMaker((int)stock.Id, stock.Ticker, stock.Exchange)); // Create tasks dynamically for each stock
        //        await Task.WhenAll(stockTasks); // Process all tasks in parallel
        //        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Delay of 1 Min before the next iteration
        //    }
        //}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool isFirstIteration = true;  // Flag to check if it's the first iteration

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!isFirstIteration)
                {
                    var stocks = (await GetStockTickerExchanges())?.Select(x => new { x.Id, x.Ticker, x.Exchange }).ToArray();

                    var stockTasks = stocks.Select(stock => CandelMaker((int)stock.Id, stock.Ticker, stock.Exchange)); // Create tasks dynamically for each stock
                    await Task.WhenAll(stockTasks); // Process all tasks in parallel
                }

                isFirstIteration = false; // Mark the first iteration as complete
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Delay of 1 Min before the next iteration
            }
        }




        // Method to fetch stock TICKER and EXCHANGE data from the API
        private async Task<IEnumerable<StockTickerExchange>> GetStockTickerExchanges()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:44364/api/StockTickerExchange/GetStockTickerExchanges");

                if (response.IsSuccessStatusCode)
                {
                    var stockTickerExchanges = await response.Content.ReadAsAsync<IEnumerable<StockTickerExchange>>();
                    return stockTickerExchanges;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stock ticker exchanges.");
                return null;
            }
        }

        public async Task<List<List<StockPricePerSec>>> GroupStockPricesByMinute(StockPricePerSec[] stockPrices)
        {
            // Group by minute (rounded down to the minute)
            var groupedStockPrices = stockPrices
                .GroupBy(sp => new DateTime(sp.StockDateTime.Year, sp.StockDateTime.Month, sp.StockDateTime.Day, sp.StockDateTime.Hour, sp.StockDateTime.Minute, 0))  // Round down to the minute
                .OrderBy(g => g.Key)  // Sort by datetime
                .ToList();

            // Create a list of lists (child arrays) for each group
            List<List<StockPricePerSec>> result = new List<List<StockPricePerSec>>();

            foreach (var group in groupedStockPrices)
            {
                result.Add(group.ToList());  // Add each group as a child array
            }

            return result;
        }

        private async Task CandelMaker(int Id, string ticker, string exchange)
        {
            try
            {

                dynamic stockPrices;
                dynamic soretedArray;
                DateTime startTime = DateTime.Today.AddHours(9).AddMinutes(30); // 9:30 AM today
                DateTime endTime = startTime.AddDays(1); // 9:30 AM tomorrow

                StockPricePerSec highestStockPrice = null;
                StockPricePerSec lowestStockPrice = null;
                StockPricePerSec firstStockPrice = null;
                StockPricePerSec lastStockPrice = null;

                // List to hold all the generated Candels
                List<Candel> candlesList = new List<Candel>();

                try
                {
                    var response = await _httpClient.GetAsync($"https://localhost:44364/api/StockPricePerSec/ByDateRange?ticker={ticker}&startDate={startTime:yyyy-MM-dd HH:mm:ss.fff}&endDate={endTime:yyyy-MM-dd HH:mm:ss.fff}");


                    if (response.IsSuccessStatusCode)
                    {
                        stockPrices = await response.Content.ReadFromJsonAsync<StockPricePerSec[]>();

                        soretedArray = await GroupStockPricesByMinute(stockPrices);  // Use await here to get the actual result


                        // Iterate through the main array
                        foreach (var subArray in soretedArray)
                        {
                            // Iterate through each sub-array
                            foreach (StockPricePerSec stock in subArray)
                            {
                                // First stock price (for the first stock of each sub-array)
                                if (firstStockPrice == null)
                                {
                                    firstStockPrice = stock;
                                }

                                // Last stock price (for the last stock of each sub-array)
                                lastStockPrice = stock;

                                // Highest stock price (check if current stock is higher)
                                if (highestStockPrice == null || stock.StockPrice > highestStockPrice.StockPrice)
                                {
                                    highestStockPrice = stock;
                                }

                                // Lowest stock price (check if current stock is lower)
                                if (lowestStockPrice == null || stock.StockPrice < lowestStockPrice.StockPrice)
                                {
                                    lowestStockPrice = stock;
                                }

                            }

                            //// After collecting the data, create the Candel payload
                            var CandelPayload = new Candel
                            {
                                StartPrice = firstStockPrice.StockPrice,  // Use the first price from the list or 0 if empty
                                HighestPrice = highestStockPrice.StockPrice, // Get the highest price from the list
                                LowestPrice = lowestStockPrice.StockPrice,  // Get the lowest price from the list
                                EndPrice = lastStockPrice.StockPrice,     // Use the last price from the list or 0 if empty

                                OpenTime = firstStockPrice.StockDateTime,
                                CloseTime = lastStockPrice.StockDateTime,

                                Ticker = ticker,
                                TickerId = Id,
                                Exchange = exchange,
                            };

                            //// Set the BullBear status based on your logic
                            CandelPayload.SetBullBearStatus();
                            CandelPayload.SetPriceChange();

                            // Add the created Candel object to the list
                            candlesList.Add(CandelPayload);

                            ////// Send the POST request to save the data in the database
                            //var postResponse = await _httpClient.PostAsJsonAsync("https://localhost:44364/api/Candel", CandelPayload);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while fetching stock price.");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching stock price for {ticker}.");
            }
        }


    }
}
