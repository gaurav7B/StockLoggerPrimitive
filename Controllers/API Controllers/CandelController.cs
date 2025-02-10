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

        //POST https://localhost:44364/api/Candel
        //[HttpPost]
        //public async Task<ActionResult> CreateCandel(Candel candel)
        //{
        //    // If no duplicate, add the new candel
        //    _context.Candel.Add(candel);
        //    await _context.SaveChangesAsync();
        //    return Ok();
        //}

        //POST https://localhost:44364/api/Candel
        [HttpPost]
        public async Task<ActionResult> CreateCandel(Candel candel)
        {
            // Check if the same candel already exists in the database
            var existingCandel = await _context.Candel
                .Where(c => c.OpenTime == candel.OpenTime && c.EndPrice == candel.EndPrice)
                .FirstOrDefaultAsync();

            if (existingCandel != null)
            {
                // Return a response indicating the candel already exists
                return Ok();
            }

            // If no duplicate, add the new candel
            _context.Candel.Add(candel);
            await _context.SaveChangesAsync();

            return Ok();
        }


        // READ: api/candel
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candel>>> GetCandels()
        {
            return await _context.Candel.ToListAsync();
        }

        // GET: api/Candel/getByTimeAndTicker
        [HttpGet("getByTimeAndTicker")]
        public async Task<ActionResult<List<Candel>>> GetCandelByTimeAndTicker(DateTime dateTime, string ticker, long tickerId)
        {
            // Extract hour and minute from the input dateTime
            int inputHour = dateTime.Hour;
            int inputMinute = dateTime.Minute + 1;
            int inputMinute2 = dateTime.Minute + 2;

            // Initialize the list to avoid null reference exception
            List<Candel> ReportCandelList = new List<Candel>();

            // Query the database for the first candel that matches the hour, minute, ticker, and tickerId
            var candel0 = await _context.Candel
                .Where(c =>
                    c.Ticker == ticker &&
                    c.TickerId == tickerId &&
                    (c.OpenTime.Hour == inputHour) &&
                    c.OpenTime.Minute == dateTime.Minute
                )
                .FirstOrDefaultAsync();

            // Add the found candles to the list (if they are not null)
            if (candel0 != null)
                ReportCandelList.Add(candel0);

            // Query the database for the first candel that matches the hour, minute, ticker, and tickerId
            var candel1 = await _context.Candel
                .Where(c =>
                    c.Ticker == ticker &&
                    c.TickerId == tickerId &&
                    (c.OpenTime.Hour == inputHour || c.OpenTime.Hour == inputHour + 1) &&
                    c.OpenTime.Minute == inputMinute
                )
                .FirstOrDefaultAsync();

            // Add the found candles to the list (if they are not null)
            if (candel1 != null)
                ReportCandelList.Add(candel1);

            // Query the database for the second candel that matches the hour, minute, ticker, and tickerId
            var candel2 = await _context.Candel
                .Where(c =>
                    c.Ticker == ticker &&
                    c.TickerId == tickerId &&
                    (c.OpenTime.Hour == inputHour || c.OpenTime.Hour == inputHour + 1) &&
                    c.OpenTime.Minute == inputMinute2
                )
                .FirstOrDefaultAsync();

            if (candel2 != null)
                ReportCandelList.Add(candel2);

            // Return the list of found candels
            return Ok(ReportCandelList);
        }


        [HttpGet("GetbyTicker")]
        public async Task<ActionResult<IEnumerable<Candel>>> GetRecentCandels(string ticker)
        {
            // Validate input
            if (string.IsNullOrEmpty(ticker))
            {
                return BadRequest("Ticker and Exchange are required.");
            }

            var currentTime = DateTime.UtcNow; // Use DateTime.Now if you are working in local time.

            var result = await _context.Candel
                .Where(c => c.Ticker == ticker)
                .OrderByDescending(c => c.CloseTime)
                .ToListAsync();

            // Remove the first element if its CloseTime is greater than the current time.
            if (result.Count > 0 && result[0].CloseTime > currentTime)
            {
                result.RemoveAt(0);
            }


            // Check if data exists
            if (!result.Any())
            {
                return NotFound("No candels found for the specified Ticker and Exchange.");
            }

            return Ok(result);
        }



        //For CUP AND HANDEL 
        // READ: api/candel/recentTen?ticker={ticker}&exchange={exchange}
        [HttpGet("recentTen")]
        public async Task<ActionResult<IEnumerable<Candel>>> GetRecentTenCandels(string ticker, string exchange)
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
                .Take(10)
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
