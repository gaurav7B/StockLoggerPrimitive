using Microsoft.AspNetCore.Mvc;

namespace StockLogger.Controllers
{
    public class DashboardMVCController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
