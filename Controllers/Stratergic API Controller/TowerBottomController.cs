using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models.Bullish_Belt_Hold;
using StockLogger.Models.Stratergic_Models.Tower_Bottom;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TowerBottomController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public TowerBottomController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/BeltHold
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TowerBottomDb>>> GetFromDb()
        {
            return await _context.TowerBottomDb
                .Include(t => t.Candels) // Eager load the navigation property
                .ToListAsync();
        }

        // POST: https://localhost:44364/api/BeltHold
        [HttpPost]
        public async Task<ActionResult<TowerBottomDb>> PostToDb(TowerBottomDb payload)
        {
            _context.TowerBottomDb.Add(payload);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
