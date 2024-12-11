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

        //// CREATE: api/candel
        //[HttpPost]
        //public async Task<ActionResult<Candel>> CreateCandel(Candel candel)
        //{
        //    _context.Candel.Add(candel);
        //    await _context.SaveChangesAsync();
        //    return CreatedAtAction(nameof(GetCandel), new { id = candel.Id }, candel);
        //}

        // CREATE or UPDATE: api/candel
        [HttpPost]
        public async Task<ActionResult<Candel>> CreateOrUpdateCandel(Candel candel)
        {
            // Check if the Candel already exists based on unique fields (e.g., Ticker, OpenTime, CloseTime)
            var existingCandel = await _context.Candel
                .Where(c => c.Ticker == candel.Ticker && c.OpenTime == candel.OpenTime)
                .FirstOrDefaultAsync();

            if (existingCandel != null)
            {
                // If found, update the existing Candel with new data
                existingCandel.StartPrice = candel.StartPrice;
                existingCandel.HighestPrice = candel.HighestPrice;
                existingCandel.LowestPrice = candel.LowestPrice;
                existingCandel.EndPrice = candel.EndPrice;
                existingCandel.TickerId = candel.TickerId;
                existingCandel.Exchange = candel.Exchange;
                existingCandel.IsBullish = candel.IsBullish;
                existingCandel.IsBearish = candel.IsBearish;

                // Recalculate the price change and set the bull/bear status again
                existingCandel.SetPriceChange();
                existingCandel.SetBullBearStatus();

                // Save the changes
                await _context.SaveChangesAsync();

                //Return the updated Candel
                return Ok();
            }
            else
            {
                // If no existing Candel is found, add the new Candel
                _context.Candel.Add(candel);
                await _context.SaveChangesAsync();

                // Return the newly created Candel
                return CreatedAtAction(nameof(GetCandel), new { id = candel.Id }, candel);
            }
        }


        // READ: api/candel
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candel>>> GetCandels()
        {
            return await _context.Candel.ToListAsync();
        }

        // READ: api/candel/recent?ticker={ticker}&exchange={exchange}
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<Candel>>> GetRecentCandels(string ticker, string exchange)
        {
            // Validate input
            if (string.IsNullOrEmpty(ticker) || string.IsNullOrEmpty(exchange))
            {
                return BadRequest("Ticker and Exchange are required.");
            }

            // Fetch the most recent 3 candels for the given ticker and exchange
            var recentCandels = await _context.Candel
                .Where(c => c.Ticker == ticker && c.Exchange == exchange)
                .OrderByDescending(c => c.CloseTime)
                .Take(4)
                .ToListAsync();

            // Check if data exists
            if (!recentCandels.Any())
            {
                return NotFound("No candels found for the specified Ticker and Exchange.");
            }

            return Ok(recentCandels);
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

        // DELETE: api/candel
        [HttpDelete]
        public async Task<IActionResult> DeleteCandel()
        {
            // Execute raw SQL to truncate the table
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Candel");

            return NoContent();
        }

        private bool CandelExists(long id)
        {
            return _context.Candel.Any(e => e.Id == id);
        }
    }
}
