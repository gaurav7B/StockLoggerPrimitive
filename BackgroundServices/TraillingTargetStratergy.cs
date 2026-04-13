using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Newtonsoft.Json;
using StockLogger.Controllers.API_Controllers;
using StockLogger.Models.Candel;
using System.Drawing;
using System.Net.Http;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static StockLogger.Controllers.API_Controllers.BuySellController;

namespace StockLogger.BackgroundServices
{
    public class TraillingTargetStratergy : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly List<(string Ticker, string SymbolToken, long Id)> _stocks;

        public TraillingTargetStratergy(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _stocks = StockList2.GetStocks()
                        .Select(s => (s.Ticker, s.SymbolToken, s.Id))
                        .ToList();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait 30 seconds before starting
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Calcualte ATR for previous day for high voletile stocks
                    // Select the stock with highest ATR
                    // BUY and SELL at once with two diffrent accounts set stoploss at 1% for BUY and SELL
                    // after BUY and SELL is complete set GHOST trailing targets at 1% 

                    // GHOST trailing targets means take the LTP of stock every sec 
                    // you will place the target order when the price goes below 1% for SELL scenario
                    // you will place the target order when the price goes above 1% for BUY scenario
                    // and as the price changes keep updating your target order

                    // You will get unique order id's for this target order use it for traking the orders
                    // once its status becomes complete Then repete the process again

                    //🧮 Example with numbers(short side)
                    //Entry: ₹100
                    //Price drops to ₹99 → (1 % fall) → you activate trailing target.
                    //Choose trail% = 0.8 %.
                    //Target = ₹99 × 1.008 = ₹99.792
                    //If price continues to fall to ₹98.5 → update target to 98.5 × 1.008 = ₹99.288
                    //Keep updating every second(or every tick).
                    //Exit only when price crosses above that updated target.


                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected error in OrderBookService: " + ex.Message);
                }

                //await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

    }
}
