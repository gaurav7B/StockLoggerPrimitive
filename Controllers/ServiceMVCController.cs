using Microsoft.AspNetCore.Mvc;
using StockLogger.Data;

namespace StockLogger.Controllers
{

    public class ServiceMVCController : Controller
    {

        private readonly StockLoggerDbContext _context;

        public ServiceMVCController(StockLoggerDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
