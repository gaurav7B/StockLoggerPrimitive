using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models.Bullish_Engulfing;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BullishEngulfingController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public BullishEngulfingController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/BullishEngulfing
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BullishEngulfingDb>>> GetFromDb()
        {
            return await _context.BullishEngulfingDb
                .Include(t => t.BullishEngulfingCandels) // Eager load the navigation property
                .ToListAsync();
        }

        // POST: https://localhost:44364/api/BullishEngulfing
        [HttpPost]
        public async Task<ActionResult<BullishEngulfingDb>> PostToDb(BullishEngulfingDb payload)
        {
            _context.BullishEngulfingDb.Add(payload);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
