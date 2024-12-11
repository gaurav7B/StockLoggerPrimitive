using StockLogger.BackgroundServices.BackgroundStratergyServices.HelperMethods;
using StockLogger.Helpers;
using StockLogger.Models.Candel;
using StockLogger.Models.DTO;
using System.Diagnostics;

namespace StockLogger.BackgroundServices.BackgroundStratergyServices
{
    public class _3WhiteSoildersService : BackgroundService
    {
        private readonly ILogger<_3WhiteSoildersService> _logger;
        private readonly HttpClient _httpClient;
        private readonly GetTickerExchangeHelperMethode _GetTickerExchangeHelperMethode;


        public _3WhiteSoildersService(ILogger<_3WhiteSoildersService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            _GetTickerExchangeHelperMethode = new GetTickerExchangeHelperMethode();
        }



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var stocks = (await _GetTickerExchangeHelperMethode.GetStockTickerExchanges())?.Select(x => new { x.Id, x.Ticker, x.Exchange }).ToArray();

                var stockTasks = stocks.Select(stock => ThreeWhiteSoilderAnalyzer((int)stock.Id, stock.Ticker, stock.Exchange)); // Create tasks dynamically for each stock
                await Task.WhenAll(stockTasks); // Process all tasks in parallel
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
