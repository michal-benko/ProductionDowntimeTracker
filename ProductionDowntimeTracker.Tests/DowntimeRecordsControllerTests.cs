using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionDowntimeTracker.api.Controllers;
using ProductionDowntimeTracker.api.Data;
using ProductionDowntimeTracker.api.DTOs;
using ProductionDowntimeTracker.api.Models;
using Xunit;

namespace ProductionDowntimeTracker.Tests
{
    public class DowntimeRecordsControllerTests
    {
        // Vytvoří pro každý test samostatnou databázi v paměti.
        private static MachineDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<MachineDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new MachineDbContext(options);
        }

        [Fact]
        public async Task StartDowntime_MachineDoesNotExist_ReturnsNotFound()
        {
            // Arrange – databáze neobsahuje stroj s ID 999.
            await using var context = CreateContext();
            var controller = new DowntimeRecordsController(context);

            var request = new StartDowntimeRequest
            {
                MachineId = 999,
                Reason = "Test"
            };

            // Act – pokus o zahájení prostoje.
            var result = await controller.StartDowntime(request);

            // Assert – očekáváme odpověď 404 Not Found.
            var notFoundResult =
                Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal(
                "Stroj s ID 999 neexistuje.",
                notFoundResult.Value);

            // Žádný prostoj se nesměl uložit.
            Assert.Equal(
                0,
                await context.DowntimeRecords.CountAsync());
        }

        [Fact]
        public async Task StartDowntime_ActiveDowntimeAlreadyExists_ReturnsConflict()
        {
            // Arrange – stroj existuje a už má aktivní prostoj.
            await using var context = CreateContext();

            context.Machines.Add(new Machine
            {
                Id = 1,
                Name = "M1"
            });

            context.DowntimeRecords.Add(new DowntimeRecord
            {
                MachineId = 1,
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                EndTime = null,
                Reason = "Probíhající prostoj"
            });

            await context.SaveChangesAsync();

            var controller = new DowntimeRecordsController(context);

            var request = new StartDowntimeRequest
            {
                MachineId = 1,
                Reason = "Nový prostoj"
            };

            // Act – pokus o zahájení druhého prostoje.
            var result = await controller.StartDowntime(request);

            // Assert – očekáváme odpověď 409 Conflict.
            var conflictResult =
                Assert.IsType<ConflictObjectResult>(result);

            Assert.Equal(
                "Stroj s ID 1 už má probíhající prostoj.",
                conflictResult.Value);

            // V databázi musí zůstat pouze původní prostoj.
            Assert.Equal(
                1,
                await context.DowntimeRecords.CountAsync());
        }
    }
}