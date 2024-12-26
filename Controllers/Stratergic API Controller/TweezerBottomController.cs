using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models.Tweezer_Bottom;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TweezerBottomController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public TweezerBottomController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/TweezerBottom
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TweezerBottomDb>>> GetFromDb()
        {
            return await _context.TweezerBottomDb
                .Include(t => t.TweezerBottomCandels) // Eager load the navigation property
                .ToListAsync();
        }

        // POST: https://localhost:44364/api/TweezerBottom
        [HttpPost]
        public async Task<ActionResult<TweezerBottomDb>> PostToDb(TweezerBottomDb payload)
        {
            _context.TweezerBottomDb.Add(payload);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
