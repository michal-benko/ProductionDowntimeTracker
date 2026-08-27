using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionDowntimeTracker.api.Controllers;
using ProductionDowntimeTracker.api.Data;
using ProductionDowntimeTracker.api.DTOs;
using ProductionDowntimeTracker.api.Models;
using ProductionDowntimeTracker.api.Services;
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

        private static DowntimeRecordsController CreateController(
            MachineDbContext context)
        {
            return new DowntimeRecordsController(
                context,
                new CsvExportService());
        }

        [Fact]
        public async Task StartDowntime_MachineDoesNotExist_ReturnsNotFound()
        {
            // Arrange – databáze neobsahuje stroj s ID 999.
            await using var context = CreateContext();
            var controller = CreateController(context);

            var request = new StartDowntimeRequest
            {
                MachineId = 999,
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
            });

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var request = new StartDowntimeRequest
            {
                MachineId = 1,
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

        [Fact]
        public async Task StopDowntime_ValidRequest_ReturnsOkAndUpdatesRecord()
        {
            // Arrange
            await using var context = CreateContext();

            var machine = new Machine
            {
                Id = 1,
                Name = "M1"
            };

            var category = new DowntimeCategory
            {
                Id = 2,
                Name = "Problém s PLC logikou"
            };

            var downtimeRecord = new DowntimeRecord
            {
                MachineId = machine.Id,
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                EndTime = null
            };

            context.Machines.Add(machine);
            context.DowntimeCategories.Add(category);
            context.DowntimeRecords.Add(downtimeRecord);

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var request = new StopDowntimeRequest
            {
                CategoryId = category.Id,
                Detail = "  PLC program byl opraven.  "
            };

            DateTime beforeStop = DateTime.UtcNow;

            // Act
            var result = await controller.StopDowntime(downtimeRecord.Id, request);

            DateTime afterStop = DateTime.UtcNow;

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedRecord = Assert.IsType<DowntimeRecord>(okResult.Value);

            Assert.Equal(category.Id, returnedRecord.CategoryId);
            Assert.Equal("PLC program byl opraven.", returnedRecord.Detail);
            Assert.NotNull(returnedRecord.EndTime);
            Assert.InRange(returnedRecord.EndTime.Value, beforeStop, afterStop);

            context.ChangeTracker.Clear();

            var savedRecord = await context.DowntimeRecords.FindAsync(downtimeRecord.Id);

            Assert.NotNull(savedRecord);
            Assert.Equal(category.Id, savedRecord.CategoryId);
            Assert.Equal("PLC program byl opraven.", savedRecord.Detail);
            Assert.Equal(returnedRecord.EndTime, savedRecord.EndTime);
        }

        [Fact]
        public async Task StopDowntime_CategoryDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            await using var context = CreateContext();

            var machine = new Machine
            {
                Id = 1,
                Name = "M1"
            };

            var downtimeRecord = new DowntimeRecord
            {
                MachineId = machine.Id,
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                EndTime = null
            };

            context.Machines.Add(machine);
            context.DowntimeRecords.Add(downtimeRecord);

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var request = new StopDowntimeRequest
            {
                CategoryId = 999,
                Detail = "  PLC program byl opraven.  "
            };

            // Act
            var result = await controller.StopDowntime(downtimeRecord.Id, request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal("Kategorie s ID 999 neexistuje.", notFoundResult.Value);

            context.ChangeTracker.Clear();

            var savedRecord = await context.DowntimeRecords.FindAsync(downtimeRecord.Id);

            Assert.NotNull(savedRecord);
            Assert.Null(savedRecord.CategoryId);
            Assert.Null(savedRecord.Detail);
            Assert.Null(savedRecord.EndTime);
        }

        [Fact]
        public async Task StopDowntime_DetailTooShortAfterTrim_ReturnsBadRequest()
        {
            // Arrange
            await using var context = CreateContext();

            var machine = new Machine
            {
                Id = 1,
                Name = "M1"
            };

            var category = new DowntimeCategory
            {
                Id = 2,
                Name = "Problém s PLC logikou"
            };

            var downtimeRecord = new DowntimeRecord
            {
                MachineId = machine.Id,
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                EndTime = null
            };

            context.Machines.Add(machine);
            context.DowntimeCategories.Add(category);
            context.DowntimeRecords.Add(downtimeRecord);

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var request = new StopDowntimeRequest
            {
                CategoryId = category.Id,

                // Před Trim() má text více než 5 znaků,
                // po odstranění mezer zůstane pouze "PLC", tedy 3 znaky.
                Detail = "   PLC   "
            };

            // Act
            var result = await controller.StopDowntime(
                downtimeRecord.Id,
                request);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Detail musí obsahovat alespoň 5 znaků.",
                badRequestResult.Value);

            // Znovu načteme záznam z databáze.
            context.ChangeTracker.Clear();

            var savedRecord =
                await context.DowntimeRecords.FindAsync(downtimeRecord.Id);

            Assert.NotNull(savedRecord);

            // Neplatný požadavek nesmí prostoj nijak změnit.
            Assert.Null(savedRecord.CategoryId);
            Assert.Null(savedRecord.Detail);
            Assert.Null(savedRecord.EndTime);
        }
    }
}