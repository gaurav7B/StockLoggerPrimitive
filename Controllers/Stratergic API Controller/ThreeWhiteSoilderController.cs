using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThreeWhiteSoilderController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public ThreeWhiteSoilderController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/ThreeWhiteSoilder
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ThreeWhiteSoilderDb>>> GetThreeWhiteSoilderDbs()
        {
            return await _context.ThreeWhiteSoilderDbs
                .Include(t => t.ThreeWhiteSoilderCandels) // Eager load the navigation property
                .ToListAsync();
        }

        // GET: api/ThreeWhiteSoilder/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ThreeWhiteSoilderDb>> GetThreeWhiteSoilderDb(long id)
        {
            var threeWhiteSoilderDb = await _context.ThreeWhiteSoilderDbs
                .Include(t => t.ThreeWhiteSoilderCandels) // Eager load the navigation property
                .FirstOrDefaultAsync(t => t.Id == id);

            if (threeWhiteSoilderDb == null)
            {
                return NotFound();
            }

            return threeWhiteSoilderDb;
        }

        // POST: api/ThreeWhiteSoilder
        [HttpPost]
        public async Task<ActionResult<ThreeWhiteSoilderDb>> PostThreeWhiteSoilderDb(ThreeWhiteSoilderDb threeWhiteSoilderDb)
        {
            _context.ThreeWhiteSoilderDbs.Add(threeWhiteSoilderDb);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetThreeWhiteSoilderDb", new { id = threeWhiteSoilderDb.Id }, threeWhiteSoilderDb);
        }

        // PUT: api/ThreeWhiteSoilder/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutThreeWhiteSoilderDb(long id, ThreeWhiteSoilderDb threeWhiteSoilderDb)
        {
            if (id != threeWhiteSoilderDb.Id)
            {
                return BadRequest();
            }

            _context.Entry(threeWhiteSoilderDb).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ThreeWhiteSoilderDbExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ThreeWhiteSoilder/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteThreeWhiteSoilderDb(long id)
        {
            var threeWhiteSoilderDb = await _context.ThreeWhiteSoilderDbs.FindAsync(id);
            if (threeWhiteSoilderDb == null)
            {
                return NotFound();
            }

            _context.ThreeWhiteSoilderDbs.Remove(threeWhiteSoilderDb);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ThreeWhiteSoilderDbExists(long id)
        {
            return _context.ThreeWhiteSoilderDbs.Any(e => e.Id == id);
        }
    }
}
