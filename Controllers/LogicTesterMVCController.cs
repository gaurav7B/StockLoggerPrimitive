using Microsoft.AspNetCore.Mvc;

namespace StockLogger.Controllers
{
    public class LogicTesterMVCController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
