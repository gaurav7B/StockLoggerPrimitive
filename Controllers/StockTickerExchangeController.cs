using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Candel;
using StockLogger.Models.DTO;

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

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<StockTickerExchange>>> GetStockTickerExchanges()
        {
            return await _context.StockTickerExchanges.ToListAsync();
        }

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

        [HttpPost("Create")]
        public async Task<ActionResult<StockTickerExchange>> PostStockTickerExchange(StockTickerExchangeDto stockTickerExchangeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingStockTickerExchange = await _context.StockTickerExchanges
                .FirstOrDefaultAsync(e => e.Ticker == stockTickerExchangeDto.Ticker);

            if (existingStockTickerExchange != null)
            {
                return Conflict(new { message = "Ticker already exists" });
            }

            var stockTickerExchange = new StockTickerExchange
            {
                Ticker = stockTickerExchangeDto.Ticker,
                Exchange = stockTickerExchangeDto.Exchange
            };

            _context.StockTickerExchanges.Add(stockTickerExchange);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStockTickerExchange", new { id = stockTickerExchange.Id }, stockTickerExchange);
        }



        [HttpPut("Update/{id}")]
        public async Task<IActionResult> PutStockTickerExchange(int id, StockTickerExchangeDto stockTickerExchangeDto)
        {

            var existingStockTickerExchange = await _context.StockTickerExchanges.FindAsync(id);
            if (existingStockTickerExchange == null)
            {
                return NotFound();
            }

            _context.Entry(existingStockTickerExchange).CurrentValues.SetValues(stockTickerExchangeDto);

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
