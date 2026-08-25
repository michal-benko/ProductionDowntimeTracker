using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionDowntimeTracker.api.Data;
using ProductionDowntimeTracker.api.DTOs;
using ProductionDowntimeTracker.api.Models;
using System.Reflection.PortableExecutable;


namespace ProductionDowntimeTracker.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DowntimeRecordsController : ControllerBase
    {
        private readonly MachineDbContext _context;

        public DowntimeRecordsController(MachineDbContext context)
        {
            _context = context;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartDowntime(
            [FromBody] StartDowntimeRequest request)
        {
            // Ověření, že požadovaný stroj existuje
            bool machineExists = await _context.Machines
                .AnyAsync(machine => machine.Id == request.MachineId);

            if (!machineExists)
            {
                return NotFound(
                    $"Stroj s ID {request.MachineId} neexistuje.");
            }

            // Jeden stroj nesmí mít současně dva probíhající prostoje
            bool activeDowntimeExists = await _context.DowntimeRecords
                .AnyAsync(downtime =>
                    downtime.MachineId == request.MachineId &&
                    downtime.EndTime == null);

            if (activeDowntimeExists)
            {
                return Conflict(
                    $"Stroj s ID {request.MachineId} už má probíhající prostoj.");
            }

            var downtimeRecord = new DowntimeRecord
            {
                MachineId = request.MachineId,
                StartTime = DateTime.UtcNow,
                EndTime = null,
            };

            _context.DowntimeRecords.Add(downtimeRecord);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, downtimeRecord);
        }

        [HttpPut("{id}/stop")]
        public async Task<IActionResult> StopDowntime(
            int id,
            [FromBody] StopDowntimeRequest request)
        {
            // Vyhledání prostoje podle jeho ID
            var downtimeRecord =
                await _context.DowntimeRecords.FindAsync(id);

            if (downtimeRecord == null)
            {
                return NotFound(
                    $"Prostoj s ID {id} neexistuje.");
            }

            // Již ukončený prostoj nelze ukončit znovu
            if (downtimeRecord.EndTime != null)
            {
                return Conflict(
                    $"Prostoj s ID {id} už byl ukončen.");
            }

            // Kontrola existence kategorie
            bool categoryExists = await _context.DowntimeCategories
                .AnyAsync(category => category.Id == request.CategoryId);

            if (!categoryExists)
            {
                return NotFound($"Kategorie s ID {request.CategoryId} neexistuje.");
            }

            string trimmedDetail = request.Detail.Trim();

            if (trimmedDetail.Length < 5)
            {
                return BadRequest("Detail musí obsahovat alespoň 5 znaků.");
            }

            downtimeRecord.CategoryId = request.CategoryId;
            downtimeRecord.Detail = trimmedDetail;
            downtimeRecord.EndTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(downtimeRecord);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DowntimeRecord>>> GetDowntimeRecords()
        {
            var downtimeRecords = await _context.DowntimeRecords
                .OrderBy(record => record.StartTime)
                .ToListAsync();

            return Ok(downtimeRecords);
        }
    }
}
