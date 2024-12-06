using Microsoft.AspNetCore.Mvc;
using StockLogger.Data;

namespace StockLogger.Controllers
{
    public class _3WSMVCController : Controller
    {
        private readonly StockLoggerDbContext _context;

        public _3WSMVCController(StockLoggerDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
