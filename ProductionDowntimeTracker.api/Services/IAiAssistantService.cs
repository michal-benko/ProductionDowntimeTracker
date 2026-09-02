using ProductionDowntimeTracker.api.DTOs.AiAssistant;

namespace ProductionDowntimeTracker.api.Services
{
    public interface IAiAssistantService
    {
        Task<string> GetReplyAsync(
            IReadOnlyList<AiChatMessageDto> messages,
            CancellationToken cancellationToken = default);

        bool IsConfigured { get; }
    }
}
