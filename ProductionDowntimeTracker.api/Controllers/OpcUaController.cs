using Microsoft.AspNetCore.Mvc;
using ProductionDowntimeTracker.api.DTOs;
using ProductionDowntimeTracker.api.Services;

namespace ProductionDowntimeTracker.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpcUaController : ControllerBase
    {
        private readonly IOpcUaService _opcUaService;
        private readonly ILogger<OpcUaController> _logger;

        public OpcUaController(
            IOpcUaService opcUaService,
            ILogger<OpcUaController> logger)
        {
            _opcUaService = opcUaService;
            _logger = logger;
        }

        [HttpGet("status")]
        public async Task<ActionResult<OpcUaStatusDto>> GetStatus()
        {
            try
            {
                OpcUaStatusDto status =
                    await _opcUaService.ReadStatusAsync();

                return Ok(status);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Nepodařilo se načíst stav z OPC UA serveru.");

                return Problem(
                    title: "OPC UA server není dostupný.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}