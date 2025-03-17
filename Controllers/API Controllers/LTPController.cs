using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Candel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockLogger.Controllers.API_Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LTPController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public LTPController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: api/LTP
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LTP>>> GetLTPs()
        {
            return await _context.LTP.ToListAsync();
        }

        // GET: api/LTP/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LTP>> GetLTP(long id)
        {
            var ltp = await _context.LTP.FindAsync(id);
            return ltp == null ? NotFound() : Ok(ltp);
        }

        // POST: api/LTP
        [HttpPost]
        public async Task<ActionResult<LTP>> PostLTP(LTP ltp)
        {
            if (ltp == null)
                return BadRequest("Invalid data.");

            _context.LTP.Add(ltp);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLTP), new { id = ltp.Id }, ltp);
        }


        // DELETE: api/LTP/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLTP(long id)
        {
            var ltp = await _context.LTP.FindAsync(id);
            if (ltp == null)
                return NotFound();

            _context.LTP.Remove(ltp);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
