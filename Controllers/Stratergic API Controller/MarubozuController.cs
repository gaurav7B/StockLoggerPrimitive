using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models.Bullish_Belt_Hold;
using StockLogger.Models.Stratergic_Models.Marubozu__Bullish_;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarubozuController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public MarubozuController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/Marubozu
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MarubozuDb>>> GetFromDb()
        {
            return await _context.MarubozuDb
                .Include(t => t.Candels) // Eager load the navigation property
                .ToListAsync();
        }

        // POST: https://localhost:44364/api/Marubozu
        [HttpPost]
        public async Task<ActionResult<MarubozuDb>> PostToDb(MarubozuDb payload)
        {
            _context.MarubozuDb.Add(payload);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
