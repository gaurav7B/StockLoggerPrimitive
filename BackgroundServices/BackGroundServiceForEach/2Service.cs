using Azure;
using Newtonsoft.Json;
using Polly;
using StockLogger.Models.Candel;
using StockLogger.Models.DTO;
using System.Diagnostics;
using System.Text.Json;

namespace StockLogger.BackgroundServices.BackGroundServiceForEach
{
    public class _2Service : BackgroundService
    {
        private readonly ILogger<_2Service> _logger;
        private readonly HttpClient _httpClient;
        private bool _isRunning = true;

        public _2Service(ILogger<_2Service> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var stocks = (await GetStockTickerExchanges())?.Select(x => new { x.Id, x.Ticker, x.Exchange }).Where(x => x.Id >= 3 && x.Id <= 4).ToArray();

                var stockTasks = stocks.Select(stock => CandelMaker((int)stock.Id, stock.Ticker, stock.Exchange)); // Create tasks dynamically for each stock
                await Task.WhenAll(stockTasks); // Process all tasks in parallel
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

        //Grouping by 1 minute
        public async Task<List<List<StockPricePerSec>>> GroupStockPricesByMinute(StockPricePerSec[] stockPrices)
        {
            // Group by minute (rounded down to the minute)
            var groupedStockPrices = stockPrices
                .GroupBy(sp => new DateTime(sp.StockDateTime.Year, sp.StockDateTime.Month, sp.StockDateTime.Day, sp.StockDateTime.Hour, sp.StockDateTime.Minute, 0))  // Round down to the minute
                .OrderBy(g => g.Key)  // Sort by datetime
                .ToList();

            int groupedStockPricesCount = groupedStockPrices.Count;

            // Create a list of lists (child arrays) for each group
            List<List<StockPricePerSec>> result = new List<List<StockPricePerSec>>();

            foreach (var group in groupedStockPrices)
            {
                result.Add(group.ToList());  // Add each group as a child array
            }

            return result;
        }

        // Grouping by 5 minutes
        public async Task<List<List<StockPricePerSec>>> GroupStockPricesByFiveMinutes(StockPricePerSec[] stockPrices)
        {
            // Group by 5-minute interval (rounding down to nearest 5 minutes)
            var groupedStockPrices = stockPrices
                .GroupBy(sp => new DateTime(
                    sp.StockDateTime.Year,
                    sp.StockDateTime.Month,
                    sp.StockDateTime.Day,
                    sp.StockDateTime.Hour,
                    sp.StockDateTime.Minute / 5 * 5,  // Round down to the nearest 5 minutes
                    0)) // Reset the seconds and milliseconds
                .OrderBy(g => g.Key)  // Sort by datetime
                .ToList();

            int groupedStockPricesCount = groupedStockPrices.Count;

            // Create a list of lists (child arrays) for each group
            List<List<StockPricePerSec>> result = new List<List<StockPricePerSec>>();

            foreach (var group in groupedStockPrices)
            {
                result.Add(group.ToList());  // Add each group as a child array
            }

            return result;
        }

        public static StockPricePerSec GetHighestStockPrice(List<StockPricePerSec> stockList)
        {
            // Get the object with the highest StockPrice
            var highestStockPrice = stockList.OrderByDescending(stock => stock.StockPrice).FirstOrDefault();

            return highestStockPrice;
        }

        public static StockPricePerSec GetLowestStockPrice(List<StockPricePerSec> stockList)
        {
            // Get the object with the lowest StockPrice
            var lowestStockPrice = stockList.OrderBy(stock => stock.StockPrice).FirstOrDefault();

            return lowestStockPrice;
        }


        public static StockPricePerSec GetFirstStockPrice(List<StockPricePerSec> stockList)
        {
            // Get the first object in the list
            var firstStockPrice = stockList.FirstOrDefault();

            return firstStockPrice;
        }

        public async Task<StockPricePerSec> GetLastStockPrice(List<StockPricePerSec> stockList, string tickerForAPI, DateTime lastDateTime)
        {

            // Convert DateTime to the desired format
            string formattedDateTime = lastDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
            string encodedTicker = Uri.EscapeDataString(tickerForAPI);

            var url = $"https://localhost:44364/api/StockPricePerSec/by-datetime2?datetime={formattedDateTime}&ticker={encodedTicker}";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (lastDateTime.Second == 58)
                {
                    DateTime currentTime = DateTime.Now;
                    int currentSecond = currentTime.Second;

                    // Calculate remaining seconds until the 59th second
                    int remainingSeconds = 59 - currentSecond;

                    // Wait until the 59th second
                    //await Task.Delay(TimeSpan.FromSeconds(remainingSeconds));

                    response = await _httpClient.GetAsync(url);
                    jsonResponse = await response.Content.ReadAsStringAsync();
                }
                while (jsonResponse == "[]")
                {
                    try
                    {
                        response = await _httpClient.GetAsync(url);
                        jsonResponse = await response.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        var defaultLastStockPriceInException = stockList.LastOrDefault();
                        return defaultLastStockPriceInException;
                    }
                }
                var stockPriceList = JsonConvert.DeserializeObject<List<StockPricePerSec>>(jsonResponse);
                var StockPriceData59 = stockPriceList?.FirstOrDefault();
                if (StockPriceData59 != null)
                {
                    return StockPriceData59;
                }
            }
            var defaultLastStockPrice = stockList.LastOrDefault();

            return defaultLastStockPrice;
        }


        // ADD 1 MINUTE CANDEL TO DATABASE
        public async void AddCandelToDatabase(List<List<StockPricePerSec>> soretedArray)
        {
            StockPricePerSec firstStockPrice = null;
            StockPricePerSec highestStockPrice = null;
            StockPricePerSec lowestStockPrice = null;
            StockPricePerSec lastStockPrice = null;
            string ticker = null;
            long Id = 0;
            string exchange = null;


            // Assuming each inner list represents stock prices for a specific ticker or interval
            if (soretedArray == null || soretedArray.Count == 0)
            {
                return;
            }

            // Extracting the last list from the sorted array
            var lastList = soretedArray.Last();

            // Get the last object in the stockList
            var lastStock = lastList.Last();

            var lastDateTime = lastStock.StockDateTime;
            var tickerForAPI = lastStock.Ticker;
            var exchangeForAPI = "NSE";

            // Declare a new variable to store the last two lists
            List<List<StockPricePerSec>> lastTwoLists = new List<List<StockPricePerSec>>();

            // Check if there are at least two lists in soretedArray
            if (soretedArray.Count >= 2)
            {
                List<StockPricePerSec> lastSecondobj = new List<StockPricePerSec>();

                DateTime lastSecondobjDateTime = DateTime.Now;

                List<Candel> CL = new List<Candel>();
                var Recent3candels = await _httpClient.GetAsync($"https://localhost:44364/api/candel/recentThree?ticker={tickerForAPI}&exchange={exchangeForAPI}");

                if (Recent3candels.IsSuccessStatusCode)
                {
                    // Deserialize the response body into a list of Candel objects
                    CL = await Recent3candels.Content.ReadAsAsync<List<Candel>>();
                }
                // Add the last two lists
                lastTwoLists.Add(soretedArray[soretedArray.Count - 2]);
                lastTwoLists.Add(soretedArray[soretedArray.Count - 1]);

                lastSecondobj = soretedArray[soretedArray.Count - 2];

                Candel result = CL.FirstOrDefault(c => c.OpenTime == lastSecondobjDateTime);

                if (result == null)
                {
                    if (lastSecondobj.LastOrDefault().StockDateTime.Second == 59)
                    {
                        var CandelInspectorPayLoad = new Candel
                        {
                            StartPrice = lastSecondobj.FirstOrDefault().StockPrice,
                            HighestPrice = lastSecondobj.OrderByDescending(stock => stock.StockPrice).FirstOrDefault().StockPrice,
                            LowestPrice = lastSecondobj.OrderBy(stock => stock.StockPrice).FirstOrDefault().StockPrice,
                            EndPrice = lastSecondobj.LastOrDefault().StockPrice,

                            OpenTime = lastSecondobj.FirstOrDefault().StockDateTime,
                            CloseTime = lastSecondobj.LastOrDefault().StockDateTime,

                            Ticker = lastSecondobj.FirstOrDefault().Ticker,
                            TickerId = lastSecondobj.FirstOrDefault().TickerId,
                            Exchange = exchangeForAPI,
                        };

                        CandelInspectorPayLoad.SetBullBearStatus();
                        CandelInspectorPayLoad.SetPriceChange();

                        await _httpClient.PostAsJsonAsync("https://localhost:44364/api/Candel", CandelInspectorPayLoad);

                    }
                }
            }

            ////Check if the second of the StockDateTime is 58
            if (lastStock.StockDateTime.Second < 58)
            {
                return;
            }

            List<Candel> CandelList = new List<Candel>();

            firstStockPrice = GetFirstStockPrice(lastList);
            highestStockPrice = GetHighestStockPrice(lastList);
            lowestStockPrice = GetLowestStockPrice(lastList);
            lastStockPrice = await GetLastStockPrice(lastList, tickerForAPI, lastDateTime);

            var CandelPayLoad = new Candel
            {
                StartPrice = firstStockPrice.StockPrice,
                HighestPrice = highestStockPrice.StockPrice,
                LowestPrice = lowestStockPrice.StockPrice,
                EndPrice = lastStockPrice.StockPrice,

                OpenTime = firstStockPrice.StockDateTime,
                CloseTime = lastStockPrice.StockDateTime,

                Ticker = firstStockPrice.Ticker,
                TickerId = firstStockPrice.TickerId,
                Exchange = "NSE",
            };

            // Set the BullBear status based on your logic
            CandelPayLoad.SetBullBearStatus();
            CandelPayLoad.SetPriceChange();

            CandelList.Add(CandelPayLoad);

            await _httpClient.PostAsJsonAsync("https://localhost:44364/api/Candel", CandelPayLoad);

            firstStockPrice = null;
            highestStockPrice = null;
            lowestStockPrice = null;
            lastStockPrice = null;
            ticker = null;
            Id = 0;
            exchange = null;
        }


        private async Task CandelMaker(int Id, string ticker, string exchange)
        {
            try
            {

                List<List<StockPricePerSec>> soretedArray = new List<List<StockPricePerSec>>();
                List<List<StockPricePerSec>> soretedArray5Minutes = new List<List<StockPricePerSec>>();
                // List to hold all the generated Candels
                List<Candel> candlesList = new List<Candel>();
                List<StockPricePerSec> StockPricePerSecList = new List<StockPricePerSec>();


                var stopwatch = new Stopwatch();

                while (_isRunning)
                {
                    stopwatch.Restart();

                    try
                    {
                        // Make API request
                        var response = await _httpClient.GetAsync($"https://localhost:44364/Stock/GetStockPriceByTickerFor_2Service?ticker={ticker}&exchange={exchange}");

                        if (response.IsSuccessStatusCode)
                        {
                            // Parse JSON response
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            var stockData = System.Text.Json.JsonSerializer.Deserialize<StockDataDto>(jsonResponse);

                            if (stockData != null)
                            {
                                var StockPerSecPayload = new StockPricePerSec
                                {
                                    Ticker = ticker,
                                    TickerId = Id,
                                    StockDateTime = stockData.time,
                                    StockPrice = stockData.price,
                                };

                                StockPricePerSecList.Add(StockPerSecPayload);

                                soretedArray = await GroupStockPricesByMinute(StockPricePerSecList.ToArray());
                                soretedArray5Minutes = await GroupStockPricesByFiveMinutes(StockPricePerSecList.ToArray());

                                AddCandelToDatabase(soretedArray);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Failed to fetch data. Status code: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }

                    // Adjust delay to maintain 10-millisecond intervals
                    stopwatch.Stop();
                    var delay = Math.Max(0, 10 - (int)stopwatch.ElapsedMilliseconds); // Delay set to 10 milliseconds
                    await Task.Delay(delay);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching stock price for {ticker}.");
            }
        }


    }
}
