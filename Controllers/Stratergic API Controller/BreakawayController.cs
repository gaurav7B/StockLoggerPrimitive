using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models.Breakaway__Bullish_;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BreakawayController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public BreakawayController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/Breakaway
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BreakawayDb>>> GetFromDb()
        {
            return await _context.BreakawayDb
                .Include(t => t.Candels) // Eager load the navigation property
                .ToListAsync();
        }

        // POST: https://localhost:44364/api/Breakaway
        [HttpPost]
        public async Task<ActionResult<BreakawayDb>> PostToDb(BreakawayDb payload)
        {
            _context.BreakawayDb.Add(payload);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
