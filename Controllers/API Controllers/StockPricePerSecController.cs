using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Candel;

namespace StockLogger.Controllers.API_Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockPricePerSecController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public StockPricePerSecController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: api/StockPricePerSec
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockPricePerSec>>> GetStockPrices()
        {
            return await _context.StockPricePerSec.ToListAsync();
        }

        // GET: api/StockPricePerSec/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StockPricePerSec>> GetStockPrice(long id)
        {
            var stockPrice = await _context.StockPricePerSec.FindAsync(id);

            if (stockPrice == null)
            {
                return NotFound();
            }

            return stockPrice;
        }

        // POST: api/StockPricePerSec
        [HttpPost("PostStockPricePerSec")]
        public async Task<ActionResult<StockPricePerSec>> PostStockPricePerSec(StockPricePerSec stockPrice)
        {
            _context.StockPricePerSec.Add(stockPrice);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStockPrice), new { id = stockPrice.Id }, stockPrice);
        }

        // PUT: api/StockPricePerSec/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStockPrice(long id, StockPricePerSec stockPrice)
        {
            if (id != stockPrice.Id)
            {
                return BadRequest();
            }

            _context.Entry(stockPrice).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockPriceExists(id))
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

        // DELETE: api/StockPricePerSec/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStockPrice(long id)
        {
            var stockPrice = await _context.StockPricePerSec.FindAsync(id);
            if (stockPrice == null)
            {
                return NotFound();
            }

            _context.StockPricePerSec.Remove(stockPrice);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StockPriceExists(long id)
        {
            return _context.StockPricePerSec.Any(e => e.Id == id);
        }
    }
}
