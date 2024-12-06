using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models;
using StockLogger.Models.Candel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockLogger.Controllers.API_Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandelController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public CandelController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // CREATE: api/candel
        [HttpPost]
        public async Task<ActionResult<Candel>> CreateCandel(Candel candel)
        {
            _context.Candel.Add(candel);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCandel), new { id = candel.Id }, candel);
        }

        // READ: api/candel
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candel>>> GetCandels()
        {
            return await _context.Candel.ToListAsync();
        }

        // READ (Single Item): api/candel/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Candel>> GetCandel(long id)
        {
            var candel = await _context.Candel.FindAsync(id);

            if (candel == null)
            {
                return NotFound();
            }

            return candel;
        }

        // UPDATE: api/candel/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCandel(long id, Candel candel)
        {
            if (id != candel.Id)
            {
                return BadRequest();
            }

            _context.Entry(candel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CandelExists(id))
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

        // DELETE: api/candel/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCandel(long id)
        {
            var candel = await _context.Candel.FindAsync(id);
            if (candel == null)
            {
                return NotFound();
            }

            _context.Candel.Remove(candel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CandelExists(long id)
        {
            return _context.Candel.Any(e => e.Id == id);
        }
    }
}
