using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models.Bullish_Harami;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BullishHaramiController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public BullishHaramiController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/BullishHarami
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BullishHaramiDb>>> GetFromDb()
        {
            return await _context.BullishHaramiDb
                .Include(t => t.BullishHaramiCandels) // Eager load the navigation property
                .ToListAsync();
        }

        // POST: https://localhost:44364/api/BullishHarami
        [HttpPost]
        public async Task<ActionResult<BullishHaramiDb>> PostToDb(BullishHaramiDb payload)
        {
            _context.BullishHaramiDb.Add(payload);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
