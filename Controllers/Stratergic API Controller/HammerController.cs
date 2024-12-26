using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models.Hammer;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class HammerController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public HammerController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/Hammer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HammerDb>>> GetFromDb()
        {
            return await _context.HammerDb
                .Include(t => t.HammerCandels) // Eager load the navigation property
                .ToListAsync();
        }

        // POST: https://localhost:44364/api/Hammer
        [HttpPost]
        public async Task<ActionResult<HammerDb>> PostToDb(HammerDb payload)
        {
            _context.HammerDb.Add(payload);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
