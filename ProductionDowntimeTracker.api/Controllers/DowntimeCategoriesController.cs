using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionDowntimeTracker.api.Data;
using ProductionDowntimeTracker.api.Models;

namespace ProductionDowntimeTracker.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
        
    public class DowntimeCategoriesController : ControllerBase
    {
        private readonly MachineDbContext _context;

        public DowntimeCategoriesController(MachineDbContext dbContext)
        {
            _context = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DowntimeCategory>>> GetDowntimeCategories()
        {
            var downtimeCategories = await _context.DowntimeCategories
                .OrderBy(x => x.Id)
                .ToListAsync();

            return Ok(downtimeCategories);
        }

    }
}
