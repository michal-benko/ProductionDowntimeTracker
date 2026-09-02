using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductionDowntimeTracker.api.DTOs.AiAssistant;
using ProductionDowntimeTracker.api.Options;
using ProductionDowntimeTracker.api.Services;

namespace ProductionDowntimeTracker.api.Controllers
{
    [ApiController]
    [Route("api/assistant")]
    public class AiAssistantController : ControllerBase
    {
        private readonly IAiAssistantService _assistantService;
        private readonly AiAssistantOptions _options;
        private readonly ILogger<AiAssistantController> _logger;

        public AiAssistantController(
            IAiAssistantService assistantService,
            IOptions<AiAssistantOptions> options,
            ILogger<AiAssistantController> logger)
        {
            _assistantService = assistantService;
            _options = options.Value;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<AiAssistantResponseDto>> AskAsync(
            [FromBody] AiAssistantRequestDto? request,
            CancellationToken cancellationToken)
        {
            if (request?.Messages is null ||
                request.Messages.Count == 0)
            {
                return BadRequest(new
                {
                    message = "Konverzace musí obsahovat alespoň jednu zprávu."
                });
            }

            if (request.Messages.Any(message =>
                    message is null ||
                    string.IsNullOrWhiteSpace(message.Content)))
            {
                return BadRequest(new
                {
                    message = "Každá zpráva musí obsahovat text."
                });
            }

            if (request.Messages.Any(message =>
                    !string.Equals(
                        message.Role,
                        "user",
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(
                        message.Role,
                        "assistant",
                        StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new
                {
                    message = "Povolené role jsou pouze user a assistant."
                });
            }

            AiChatMessageDto currentMessage =
                request.Messages[^1];

            if (!string.Equals(
                    currentMessage.Role,
                    "user",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Poslední zpráva musí pocházet od uživatele."
                });
            }

            if (currentMessage.Content.Length >
                _options.MaxUserMessageLength)
            {
                return BadRequest(new
                {
                    message =
                        $"Dotaz může mít maximálně " +
                        $"{_options.MaxUserMessageLength} znaků."
                });
            }

            if (!_assistantService.IsConfigured)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new
                    {
                        message = "AI služba není nakonfigurovaná."
                    });
            }

            try
            {
                string reply =
                    await _assistantService.GetReplyAsync(
                        request.Messages,
                        cancellationToken);

                if (string.IsNullOrWhiteSpace(reply))
                {
                    return StatusCode(
                        StatusCodes.Status502BadGateway,
                        new
                        {
                            message =
                                "AI služba nevrátila žádnou odpověď."
                        });
                }

                return Ok(new AiAssistantResponseDto
                {
                    Reply = reply
                });
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                return StatusCode(
                    StatusCodes.Status504GatewayTimeout,
                    new
                    {
                        message =
                            "AI služba neodpověděla v časovém limitu."
                    });
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Volání AI providera selhalo.");

                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        message =
                            "AI služba je momentálně nedostupná " +
                            "nebo byl vyčerpán její limit."
                    });
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Při zpracování AI dotazu došlo k chybě.");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message =
                            "Při zpracování dotazu došlo k chybě."
                    });
            }
        }
    }
}