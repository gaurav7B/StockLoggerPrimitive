using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models.Bullish_Engulfing;
using StockLogger.Models.Stratergic_Models.Dragonfly_Doji;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DragonflyDojiController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public DragonflyDojiController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/DragonflyDoji
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DragonflyDojiDb>>> GetFromDb()
        {
            return await _context.DragonflyDojiDb
                .Include(t => t.DragonflyDojiCandels) // Eager load the navigation property
                .ToListAsync();
        }

        // POST: https://localhost:44364/api/DragonflyDoji
        [HttpPost]
        public async Task<ActionResult<DragonflyDojiDb>> PostToDb(DragonflyDojiDb payload)
        {
            _context.DragonflyDojiDb.Add(payload);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
