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
    public class Candel15minController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public Candel15minController(StockLoggerDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> CreateCandel(Candel15min candel)
        {
            var existingCandel = await _context.Candel15min
                .FirstOrDefaultAsync(c =>
                    c.OpenTime == candel.OpenTime &&
                    c.Ticker == candel.Ticker
                );

            if (existingCandel != null)
            {
                // Update the existing candel with new data
                existingCandel.StartPrice = candel.StartPrice;
                existingCandel.HighestPrice = candel.HighestPrice;
                existingCandel.LowestPrice = candel.LowestPrice;
                existingCandel.EndPrice = candel.EndPrice;
                existingCandel.OpenTime = candel.OpenTime;
                existingCandel.CloseTime = candel.CloseTime;
                existingCandel.Ticker = candel.Ticker;
                existingCandel.TickerId = candel.TickerId;
                existingCandel.Exchange = candel.Exchange;
                existingCandel.IsBullish = candel.IsBullish;
                existingCandel.IsBearish = candel.IsBearish;
                existingCandel.PriceChange = candel.PriceChange;
                existingCandel.PriceChangePercentage = candel.PriceChangePercentage;

                // Save the changes to the database
                await _context.SaveChangesAsync();

                return Ok(); // Return the updated candel
            }

            // If no duplicate, add the new candel
            _context.Candel15min.Add(candel);
            await _context.SaveChangesAsync();
            return Ok();
        }


        // READ: api/candel
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candel15min>>> GetCandels()
        {
            return await _context.Candel15min.ToListAsync();
        }

        [HttpGet("recentThree")]
        public async Task<ActionResult<IEnumerable<Candel15min>>> GetRecentCandels(string ticker, string exchange)
        {
            // Validate input
            if (string.IsNullOrEmpty(ticker) || string.IsNullOrEmpty(exchange))
            {
                return BadRequest("Ticker and Exchange are required.");
            }

            // Fetch the most recent 3 distinct candels based on CloseTime for the given ticker and exchange
            var recentCandels = await _context.Candel15min
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
        // READ: api/Candel15min/recentTen?ticker={ticker}&exchange={exchange}
        [HttpGet("recentTen")]
        public async Task<ActionResult<IEnumerable<Candel15min>>> GetRecentTenCandels(string ticker, string exchange)
        {
            // Validate input
            if (string.IsNullOrEmpty(ticker) || string.IsNullOrEmpty(exchange))
            {
                return BadRequest("Ticker and Exchange are required.");
            }

            // Fetch the most recent 3 candels for the given ticker and exchange
            var recentCandels = await _context.Candel15min
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



        // READ (Single Item): api/Candel15min/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Candel15min>> GetCandel(long id)
        {
            var candel = await _context.Candel15min.FindAsync(id);

            if (candel == null)
            {
                return NotFound();
            }

            return candel;
        }

        // UPDATE: api/Candel15min/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCandel(long id, Candel15min candel)
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
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Candel15min");

            return NoContent();
        }

        private bool CandelExists(long id)
        {
            return _context.Candel15min.Any(e => e.Id == id);
        }

    }
}
