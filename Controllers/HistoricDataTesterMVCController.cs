using Microsoft.AspNetCore.Mvc;
using StockLogger.Data;

namespace StockLogger.Controllers
{
    public class HistoricDataTesterMVCController : Controller
    {
        private readonly StockLoggerDbContext _context;

        public HistoricDataTesterMVCController(StockLoggerDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
