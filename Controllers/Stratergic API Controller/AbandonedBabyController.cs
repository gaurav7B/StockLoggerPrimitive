using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models.Bullish_Abandoned_Baby;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AbandonedBabyController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public AbandonedBabyController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/Breakaway
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AbandonedBabyDb>>> GetFromDb()
        {
            return await _context.AbandonedBabyDb
                .Include(t => t.Candels) // Eager load the navigation property
                .ToListAsync();
        }

        // POST: https://localhost:44364/api/Breakaway
        [HttpPost]
        public async Task<ActionResult<AbandonedBabyDb>> PostToDb(AbandonedBabyDb payload)
        {
            _context.AbandonedBabyDb.Add(payload);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
