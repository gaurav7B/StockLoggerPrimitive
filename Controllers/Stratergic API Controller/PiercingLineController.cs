using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models.Piercing_Line;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class PiercingLineController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public PiercingLineController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/PiercingLine
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PiercingLineDb>>> GetFromDb()
        {
            return await _context.PiercingLineDb
                .Include(t => t.PiercingLineCandels) // Eager load the navigation property
                .ToListAsync();
        }

        // POST: https://localhost:44364/api/PiercingLine
        [HttpPost]
        public async Task<ActionResult<PiercingLineDb>> PostToDb(PiercingLineDb payload)
        {
            _context.PiercingLineDb.Add(payload);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
