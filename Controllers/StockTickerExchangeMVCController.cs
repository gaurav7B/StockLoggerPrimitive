using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Candel;
using StockLogger.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockLogger.Controllers
{
    public class StockTickerExchangeMVCController : Controller
    {
        private readonly StockLoggerDbContext _context;

        public StockTickerExchangeMVCController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: /StockTickerExchange/TickerList
        public async Task<IActionResult> TickerList()
        {
            // Fetching the list of tickers from the database
            var stockTickerExchanges = await _context.StockTickerExchanges.ToListAsync();
            return View(stockTickerExchanges);
        }

        public async Task<IActionResult> AddTicker()
        {
            return View();
        }

    }
}
