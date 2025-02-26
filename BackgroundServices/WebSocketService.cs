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
    public class WebSocketService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string ticker, string exchange, string name, long id, string symboltoken)> _stocks;


        public WebSocketService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Fetch stocks from StockList
            //_stocks = StockList.GetStocks();

            _stocks = StockList2.GetStocks()
                     .Select(s => (s.Ticker, s.Exchange, s.Name, s.Id, s.SymbolToken))
                     .ToList();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

        }

    }

}
