using Microsoft.AspNetCore.Mvc;

namespace StockLogger.Controllers
{
    public class DBCandels : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
