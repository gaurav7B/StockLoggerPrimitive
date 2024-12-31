using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models.Bullish_Belt_Hold;
using StockLogger.Models.Stratergic_Models.Rising_Sun;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class RisingSunController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public RisingSunController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/RisingSun
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RisingSunDb>>> GetFromDb()
        {
            return await _context.RisingSunDb
                .Include(t => t.Candels) // Eager load the navigation property
                .ToListAsync();
        }

        // POST: https://localhost:44364/api/RisingSun
        [HttpPost]
        public async Task<ActionResult<RisingSunDb>> PostToDb(RisingSunDb payload)
        {
            _context.RisingSunDb.Add(payload);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
