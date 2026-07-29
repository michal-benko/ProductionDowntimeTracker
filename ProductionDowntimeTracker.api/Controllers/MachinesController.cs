using Microsoft.AspNetCore.Mvc;
using ProductionDowntimeTracker.api.Models;
using ProductionDowntimeTracker.api.Data;
using Microsoft.EntityFrameworkCore;

namespace ProductionDowntimeTracker.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class MachinesController : ControllerBase
    {

        private readonly MachineDbContext _dbContext;

        public MachinesController(MachineDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        [HttpGet]
        public async Task<ActionResult<List<Machine>>> GetAll()
        {
            List<Machine> machines = await _dbContext.Machines.ToListAsync();

            return Ok(machines);
        }


        [HttpGet("{id:int}")]
        
        public async Task<ActionResult<Machine>> GetById(int id)
        {
            Machine? machine = await _dbContext.Machines.FindAsync(id);

            if (machine is null)
            {
                return NotFound();
            }

            return Ok(machine);
        }

        [HttpPost]

        public async Task <ActionResult<Machine>> Create(CreateMachineRequest request)
        {
            Machine newMachine = new Machine
            {
                Name = request.Name,
                IsRunning = request.IsRunning
            };

            _dbContext.Machines.Add(newMachine);

            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = newMachine.Id },
                newMachine);
        }

        [HttpPut("{id:int}")]

        public async Task<ActionResult<Machine>> Update(int id, UpdateMachineRequest request)
        {
            Machine? machine = await _dbContext.Machines.FindAsync(id);

            if (machine is null)
            {
                return NotFound();
            }

            machine.Name = request.Name;
            machine.IsRunning = request.IsRunning;

            await _dbContext.SaveChangesAsync();


            return Ok(machine);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            Machine? machine = await _dbContext.Machines.FindAsync(id);

            if (machine is null)
            {
                return NotFound();
            }

            _dbContext.Machines.Remove(machine);

            await _dbContext.SaveChangesAsync();

            return NoContent();

        }

    }
}
