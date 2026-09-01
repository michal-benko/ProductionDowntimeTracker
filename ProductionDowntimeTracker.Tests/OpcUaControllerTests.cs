using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using ProductionDowntimeTracker.api.Controllers;
using ProductionDowntimeTracker.api.DTOs;
using ProductionDowntimeTracker.api.Services;
using Xunit;

namespace ProductionDowntimeTracker.Tests
{
    public class OpcUaControllerTests
    {
        private static OpcUaController CreateController(
            IOpcUaService opcUaService)
        {
            return new OpcUaController(
                opcUaService,
                NullLogger<OpcUaController>.Instance);
        }

        [Fact]
        public async Task GetStatus_ServiceReturnsStatus_ReturnsOkWithStatus()
        {
            var expectedStatus = new OpcUaStatusDto
            {
                MachineRunning = true,
                FaultActive = false,
                ProducedCycles = 125
            };

            var opcUaService = new FakeOpcUaService(
                () => Task.FromResult(expectedStatus));

            var controller = CreateController(opcUaService);

            ActionResult<OpcUaStatusDto> result =
                await controller.GetStatus();

            var okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            var returnedStatus =
                Assert.IsType<OpcUaStatusDto>(okResult.Value);

            Assert.Equal(
                StatusCodes.Status200OK,
                okResult.StatusCode);

            Assert.Equal(
                expectedStatus.MachineRunning,
                returnedStatus.MachineRunning);

            Assert.Equal(
                expectedStatus.FaultActive,
                returnedStatus.FaultActive);

            Assert.Equal(
                expectedStatus.ProducedCycles,
                returnedStatus.ProducedCycles);
        }

        [Fact]
        public async Task GetStatus_ServiceThrowsException_ReturnsServiceUnavailable()
        {
            var opcUaService = new FakeOpcUaService(
                () => Task.FromException<OpcUaStatusDto>(
                    new InvalidOperationException(
                        "OPC UA server není dostupný.")));

            var controller = CreateController(opcUaService);

            ActionResult<OpcUaStatusDto> result =
                await controller.GetStatus();

            var objectResult =
                Assert.IsType<ObjectResult>(result.Result);

            var problemDetails =
                Assert.IsType<ProblemDetails>(objectResult.Value);

            Assert.Equal(
                StatusCodes.Status503ServiceUnavailable,
                objectResult.StatusCode);

            Assert.Equal(
                StatusCodes.Status503ServiceUnavailable,
                problemDetails.Status);

            Assert.Equal(
                "OPC UA server není dostupný.",
                problemDetails.Title);
        }

        private sealed class FakeOpcUaService : IOpcUaService
        {
            private readonly Func<Task<OpcUaStatusDto>> _readStatus;

            public FakeOpcUaService(
                Func<Task<OpcUaStatusDto>> readStatus)
            {
                _readStatus = readStatus;
            }

            public Task<OpcUaStatusDto> ReadStatusAsync()
            {
                return _readStatus();
            }
        }
    }
}