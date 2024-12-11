using StockLogger.Models.Candel;
using StockLogger.Models.DTO;
using System.Diagnostics;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices
{
    public class _3WhiteSoildersService : BackgroundService
    {
        private readonly ILogger<_3WhiteSoildersService> _logger;
        private readonly HttpClient _httpClient;

        public _3WhiteSoildersService(ILogger<_3WhiteSoildersService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var stocks = (await GetStockTickerExchanges())?.Select(x => new { x.Id, x.Ticker, x.Exchange }).ToArray();

                var stockTasks = stocks.Select(stock => ThreeWhiteSoilderAnalyzer((int)stock.Id, stock.Ticker, stock.Exchange)); // Create tasks dynamically for each stock
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




        private async Task ThreeWhiteSoilderAnalyzer(int Id, string ticker, string exchange)
        {
            try
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching stock price for {ticker}.");
            }
        }
    }
}
