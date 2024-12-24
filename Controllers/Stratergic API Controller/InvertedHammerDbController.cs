using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockLogger.Data;
using StockLogger.Models.Stratergic_Models;
using StockLogger.Models.Stratergic_Models.Inverted_Hammer;

namespace StockLogger.Controllers.Stratergic_API_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvertedHammerDbController : ControllerBase
    {
        private readonly StockLoggerDbContext _context;

        public InvertedHammerDbController(StockLoggerDbContext context)
        {
            _context = context;
        }

        // GET: https://localhost:44364/api/InvertedHammerDb
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvertedHammerDb>>> GetThreeWhiteSoilderDbs()
        {
            return await _context.InvertedHammerDb
                .Include(t => t.InvertedHammerCandels) // Eager load the navigation property
                .ToListAsync();
        }

        // POST: https://localhost:44364/api/InvertedHammerDb
        [HttpPost]
        public async Task<ActionResult<InvertedHammerDb>> PostThreeWhiteSoilderDb(InvertedHammerDb invertedHammerDb)
        {
            _context.InvertedHammerDb.Add(invertedHammerDb);
            await _context.SaveChangesAsync();

            return Ok();
        }


    }
}
