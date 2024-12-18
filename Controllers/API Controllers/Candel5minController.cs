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
    public class Candel5minController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public Candel5minController(StockLoggerDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<ActionResult<Candel5min>> CreateCandel(Candel5min candel)
        {
            var existingCandel = await _context.Candel5min
                .FirstOrDefaultAsync(c =>
                    c.StartPrice == candel.StartPrice &&
                    c.HighestPrice == candel.HighestPrice &&
                    c.LowestPrice == candel.LowestPrice &&
                    c.EndPrice == candel.EndPrice &&
                    c.OpenTime == candel.OpenTime &&
                    c.CloseTime == candel.CloseTime &&
                    c.Ticker == candel.Ticker &&
                    c.TickerId == candel.TickerId &&
                    c.Exchange == candel.Exchange
                );

            var matchingCandles = await _context.Candel5min
           .Where(c => c.Ticker == candel.Ticker
                    && c.CloseTime.Hour == DateTime.Now.Hour
                    && c.CloseTime.Minute == 59)
           .ToListAsync();

            if (matchingCandles.Count > 0)
            {
                return null;
            }

            if (existingCandel != null)
            {
                return null;
            }

            // If no duplicate, add the new candel
            _context.Candel5min.Add(candel);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCandel), new { id = candel.Id }, candel);
        }

        // READ: api/candel
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candel5min>>> GetCandels()
        {
            return await _context.Candel5min.ToListAsync();
        }

        [HttpGet("recentThree")]
        public async Task<ActionResult<IEnumerable<Candel5min>>> GetRecentCandels(string ticker, string exchange)
        {
            // Validate input
            if (string.IsNullOrEmpty(ticker) || string.IsNullOrEmpty(exchange))
            {
                return BadRequest("Ticker and Exchange are required.");
            }

            // Fetch the most recent 3 distinct candels based on CloseTime for the given ticker and exchange
            var recentCandels = await _context.Candel5min
                .Where(c => c.Ticker == ticker && c.Exchange == exchange)
                .GroupBy(c => c.OpenTime)
                .OrderByDescending(g => g.Key) // Ordering by CloseTime
                .Take(4) // Taking the most recent 3
                .Select(g => g.FirstOrDefault()) // Select the first (latest) candle in each CloseTime group
                .ToListAsync();

            // Check if data exists
            if (!recentCandels.Any())
            {
                return NotFound("No candels found for the specified Ticker and Exchange.");
            }

            return Ok(recentCandels);
        }



        //For CUP AND HANDEL 
        // READ: api/Candel5min/recentTen?ticker={ticker}&exchange={exchange}
        [HttpGet("recentTen")]
        public async Task<ActionResult<IEnumerable<Candel5min>>> GetRecentTenCandels(string ticker, string exchange)
        {
            // Validate input
            if (string.IsNullOrEmpty(ticker) || string.IsNullOrEmpty(exchange))
            {
                return BadRequest("Ticker and Exchange are required.");
            }

            // Fetch the most recent 3 candels for the given ticker and exchange
            var recentCandels = await _context.Candel5min
                .Where(c => c.Ticker == ticker && c.Exchange == exchange)
                .OrderByDescending(c => c.CloseTime)
                .Take(10)
                .ToListAsync();

            // Check if data exists
            if (!recentCandels.Any())
            {
                return NotFound("No candels found for the specified Ticker and Exchange.");
            }

            return Ok(recentCandels);
        }



        // READ (Single Item): api/Candel5min/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Candel5min>> GetCandel(long id)
        {
            var candel = await _context.Candel5min.FindAsync(id);

            if (candel == null)
            {
                return NotFound();
            }

            return candel;
        }

        // UPDATE: api/Candel5min/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCandel(long id, Candel5min candel)
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

        // DELETE: api/candel
        [HttpDelete]
        public async Task<IActionResult> DeleteCandel()
        {
            // Execute raw SQL to truncate the table
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Candel5min");

            return NoContent();
        }

        private bool CandelExists(long id)
        {
            return _context.Candel5min.Any(e => e.Id == id);
        }

    }
}
