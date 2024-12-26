using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models;
using StockLogger.Models.Stratergic_Models.Inverted_Hammer;
using StockLogger.Models.Stratergic_Models.Morning_Star;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class MorningStarDbController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public MorningStarDbController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/MorningStarDb
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MorningStarDb>>> GetThreeWhiteSoilderDbs()
        {
            return await _context.MorningStarDb
                .Include(t => t.MorningStarCandels) // Eager load the navigation property
                .ToListAsync();
        }

        // POST: https://localhost:44364/api/MorningStarDb
        [HttpPost]
        public async Task<ActionResult<MorningStarDb>> PostThreeWhiteSoilderDb(MorningStarDb payload)
        {
            _context.MorningStarDb.Add(payload);
            await _context.SaveChangesAsync();

            return Ok();
        }


    }
}
