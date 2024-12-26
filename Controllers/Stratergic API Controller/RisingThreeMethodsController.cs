using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models.Hammer;
using StockLogger.Models.Stratergic_Models.Rising_Three_Methods;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class RisingThreeMethodsController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public RisingThreeMethodsController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/RisingThreeMethods
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RisingThreeMethodsDb>>> GetFromDb()
        {
            return await _context.RisingThreeMethodsDb
                .Include(t => t.RisingThreeMethodsCandels) // Eager load the navigation property
                .ToListAsync();
        }

        // POST: https://localhost:44364/api/RisingThreeMethods
        [HttpPost]
        public async Task<ActionResult<RisingThreeMethodsDb>> PostToDb(RisingThreeMethodsDb payload)
        {
            _context.RisingThreeMethodsDb.Add(payload);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
