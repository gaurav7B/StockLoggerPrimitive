using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models.Bullish_Belt_Hold;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BeltHoldController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public BeltHoldController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/BeltHold
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BeltHoldDb>>> GetFromDb()
        {
            return await _context.BeltHoldDb
                .Include(t => t.Candels) // Eager load the navigation property
                .ToListAsync();
        }

        // POST: https://localhost:44364/api/BeltHold
        [HttpPost]
        public async Task<ActionResult<BeltHoldDb>> PostToDb(BeltHoldDb payload)
        {
            _context.BeltHoldDb.Add(payload);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
