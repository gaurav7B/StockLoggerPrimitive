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

        // GET: https://localhost:44364/api/StockPricePerSec
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockPricePerSec>>> GetStockPrices()
        {
            return await _context.StockPricePerSec.ToListAsync();
        }

        // GET: https://localhost:44364/api/StockPricePerSec/GetForCandel?ticker=INFY
        [HttpGet("GetForCandel")]
        public async Task<ActionResult<IEnumerable<List<Candel>>>> MakeCandel([FromQuery] string ticker)
        {
            // Fetch data from the database based on the ticker
            var query = _context.StockPricePerSec
                                .Where(sp => sp.Ticker == ticker)
                                .OrderBy(sp => sp.StockDateTime); // Ensure the data is sorted by time

            var result = await query.ToListAsync();

            // Grouping stock prices by minute (rounded to the nearest minute)
            var groupedResult = result
                                .GroupBy(sp => new DateTime(sp.StockDateTime.Year, sp.StockDateTime.Month, sp.StockDateTime.Day, sp.StockDateTime.Hour, sp.StockDateTime.Minute, 0))
                                .Select(g => g.ToList()) // Create a list for each group
                                .ToList();

            StockPricePerSec firstStockPrice = null;
            StockPricePerSec highestStockPrice = null;
            StockPricePerSec lowestStockPrice = null;
            StockPricePerSec lastStockPrice = null;

            List<Candel> CandelList = new List<Candel>();

            if (groupedResult != null)
            {
                foreach (List<StockPricePerSec> sp in groupedResult)
                {
                    firstStockPrice = GetFirstStockPrice(sp);
                    highestStockPrice = GetHighestStockPrice(sp);
                    lowestStockPrice = GetLowestStockPrice(sp);
                    lastStockPrice = GetLastStockPrice(sp);

                    var CandelPayLoad = new Candel
                    {
                        StartPrice = firstStockPrice.StockPrice,  // Use the first price from the list
                        HighestPrice = highestStockPrice.StockPrice, // Get the highest price from the list
                        LowestPrice = lowestStockPrice.StockPrice,  // Get the lowest price from the list
                        EndPrice = lastStockPrice.StockPrice,     // Use the last price from the list

                        OpenTime = firstStockPrice.StockDateTime,
                        CloseTime = lastStockPrice.StockDateTime,

                        Ticker = firstStockPrice.Ticker,
                        TickerId = firstStockPrice.TickerId,
                        Exchange = "NSE",
                    };

                    // Set the BullBear status based on your logic
                    CandelPayLoad.SetBullBearStatus();
                    CandelPayLoad.SetPriceChange();

                    CandelList.Add(CandelPayLoad);
                }

            }

            // Return the list of lists containing stock prices per minute
            return Ok(CandelList);
        }


        public static StockPricePerSec GetHighestStockPrice(List<StockPricePerSec> stockList)
        {
            // Get the object with the highest StockPrice
            var highestStockPrice = stockList.OrderByDescending(stock => stock.StockPrice).FirstOrDefault();

            return highestStockPrice;
        }

        public static StockPricePerSec GetLowestStockPrice(List<StockPricePerSec> stockList)
        {
            // Get the object with the lowest StockPrice
            var lowestStockPrice = stockList.OrderBy(stock => stock.StockPrice).FirstOrDefault();

            return lowestStockPrice;
        }


        public static StockPricePerSec GetFirstStockPrice(List<StockPricePerSec> stockList)
        {
            // Get the first object in the list
            var firstStockPrice = stockList.FirstOrDefault();

            return firstStockPrice;
        }

        public static StockPricePerSec GetLastStockPrice(List<StockPricePerSec> stockList)
        {
            // Get the last object in the list
            var lastStockPrice = stockList.LastOrDefault();

            return lastStockPrice;
        }


        [HttpGet("by-datetime1")]
        public async Task<ActionResult<IEnumerable<StockPricePerSec>>> GetStockPricesByDateTimeAndTicker(
            [FromQuery] DateTime datetime,
            [FromQuery] string ticker)
        {
            Console.WriteLine($"Received datetime: {datetime}, Ticker: {ticker}"); // Log for debugging

            var targetDate = datetime.Date;
            var targetHour = datetime.Hour;
            var targetMinute = datetime.Minute;

            var query = _context.StockPricePerSec
                .Where(sp => sp.StockDateTime.Date == targetDate &&
                             sp.StockDateTime.Hour == targetHour &&
                             sp.StockDateTime.Minute == targetMinute &&
                             sp.StockDateTime.Second == 59);

            // Apply ticker filter if provided
            if (!string.IsNullOrEmpty(ticker))
            {
                query = query.Where(sp => sp.Ticker == ticker);
            }

            var result = await query.ToListAsync();

            return Ok(result);
        }

        [HttpGet("by-datetime2")]
        public async Task<ActionResult<IEnumerable<StockPricePerSec>>> GetStockPricesByDateTimeAndTicker2(
    [FromQuery] DateTime datetime,
    [FromQuery] string ticker)
        {
            Console.WriteLine($"Received datetime: {datetime}, Ticker: {ticker}"); // Log for debugging

            var targetDate = datetime.Date;
            var targetHour = datetime.Hour;
            var targetMinute = datetime.Minute;

            var query = _context.StockPricePerSec
                .Where(sp => sp.StockDateTime.Date == targetDate &&
                             sp.StockDateTime.Hour == targetHour &&
                             sp.StockDateTime.Minute == targetMinute &&
                             sp.StockDateTime.Second == 59);

            // Apply ticker filter if provided
            if (!string.IsNullOrEmpty(ticker))
            {
                query = query.Where(sp => sp.Ticker == ticker);
            }

            var result = await query.ToListAsync();

            return Ok(result);
        }

        [HttpGet("by-datetime3")]
        public async Task<ActionResult<IEnumerable<StockPricePerSec>>> GetStockPricesByDateTimeAndTicker3(
    [FromQuery] DateTime datetime,
    [FromQuery] string ticker)
        {
            Console.WriteLine($"Received datetime: {datetime}, Ticker: {ticker}"); // Log for debugging

            var targetDate = datetime.Date;
            var targetHour = datetime.Hour;
            var targetMinute = datetime.Minute;

            var query = _context.StockPricePerSec
                .Where(sp => sp.StockDateTime.Date == targetDate &&
                             sp.StockDateTime.Hour == targetHour &&
                             sp.StockDateTime.Minute == targetMinute &&
                             sp.StockDateTime.Second == 59);

            // Apply ticker filter if provided
            if (!string.IsNullOrEmpty(ticker))
            {
                query = query.Where(sp => sp.Ticker == ticker);
            }

            var result = await query.ToListAsync();

            return Ok(result);
        }

        [HttpGet("by-datetime4")]
        public async Task<ActionResult<IEnumerable<StockPricePerSec>>> GetStockPricesByDateTimeAndTicker4(
    [FromQuery] DateTime datetime,
    [FromQuery] string ticker)
        {
            Console.WriteLine($"Received datetime: {datetime}, Ticker: {ticker}"); // Log for debugging

            var targetDate = datetime.Date;
            var targetHour = datetime.Hour;
            var targetMinute = datetime.Minute;

            var query = _context.StockPricePerSec
                .Where(sp => sp.StockDateTime.Date == targetDate &&
                             sp.StockDateTime.Hour == targetHour &&
                             sp.StockDateTime.Minute == targetMinute &&
                             sp.StockDateTime.Second == 59);

            // Apply ticker filter if provided
            if (!string.IsNullOrEmpty(ticker))
            {
                query = query.Where(sp => sp.Ticker == ticker);
            }

            var result = await query.ToListAsync();

            return Ok(result);
        }



        // GET: https://localhost:44364/api/StockPricePerSec/ByDateRange?ticker=INFY&startDate=2024-12-09 10:30:40.9553290&endDate=2024-12-09 10:38:59.0339196
        [HttpGet("ByDateRange")]
        public async Task<ActionResult<IEnumerable<StockPricePerSec>>> GetStockPricesByDateRange(
            string ticker, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(ticker) || startDate == default || endDate == default)
            {
                return BadRequest("Invalid input parameters. Ensure ticker, startDate, and endDate are provided.");
            }

            var stockPrices = await _context.StockPricePerSec
                .Where(sp => sp.Ticker == ticker && sp.StockDateTime >= startDate && sp.StockDateTime <= endDate)
                .ToListAsync();

            if (!stockPrices.Any())
            {
                return NotFound($"No stock prices found for ticker {ticker} between {startDate} and {endDate}.");
            }

            return Ok(stockPrices);
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

        // DELETE: api/StockPricePerSec
        [HttpDelete]
        public async Task<IActionResult> TruncateStockPrice()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE StockPricePerSec");
                return NoContent();
            }
            catch (Exception ex)
            {
                // Handle any errors (e.g., logging)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private bool StockPriceExists(long id)
        {
            return _context.StockPricePerSec.Any(e => e.Id == id);
        }
    }
}
