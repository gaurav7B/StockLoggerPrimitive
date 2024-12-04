using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Candel;

namespace StockLogger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockTickerExchangeController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public StockTickerExchangeController(StockLoggerDbContext context)
        {
            _context = context;
        }

        //http://localhost:44364/api/StockTickerExchange/GetAll
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<StockTickerExchange>>> GetStockTickerExchanges()
        {
            return await _context.StockTickerExchanges.ToListAsync();
        }

        //http://localhost:44364/api/StockTickerExchange/GetStockTickerExchange/{id}
        [HttpGet("GetStockTickerExchange/{id}")]
        public async Task<ActionResult<StockTickerExchange>> GetStockTickerExchange(int id)
        {
            var stockTickerExchange = await _context.StockTickerExchanges.FindAsync(id);

            if (stockTickerExchange == null)
            {
                return NotFound();
            }

            return Ok(stockTickerExchange);
        }

        //http://localhost:44364/api/StockTickerExchange/Create
        [HttpPost("Create")]
        public async Task<ActionResult<StockTickerExchange>> PostStockTickerExchange(StockTickerExchange stockTickerExchange)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.StockTickerExchanges.Add(stockTickerExchange);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStockTickerExchange", new { id = stockTickerExchange.Id }, stockTickerExchange);
        }

        //http://localhost:44364/api/StockTickerExchange/Update/{id}
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> PutStockTickerExchange(int id, StockTickerExchange stockTickerExchange)
        {
            if (id != stockTickerExchange.Id)
            {
                return BadRequest("ID in the route does not match ID in the body.");
            }

            var existingStockTickerExchange = await _context.StockTickerExchanges.FindAsync(id);
            if (existingStockTickerExchange == null)
            {
                return NotFound();
            }

            _context.Entry(existingStockTickerExchange).CurrentValues.SetValues(stockTickerExchange);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockTickerExchangeExists(id))
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


        //http://localhost:44364/api/StockTickerExchange/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteStockTickerExchange(int id)
        {
            var stockTickerExchange = await _context.StockTickerExchanges.FindAsync(id);
            if (stockTickerExchange == null)
            {
                return NotFound();
            }

            _context.StockTickerExchanges.Remove(stockTickerExchange);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StockTickerExchangeExists(int id)
        {
            return _context.StockTickerExchanges.Any(e => e.Id == id);
        }
    }
}
