namespace ProductionDowntimeTracker.api.DTOs.AiAssistant
{
    public class AiAssistantRequestDto
    {
        public List<AiChatMessageDto> Messages { get; set; }
            = new List<AiChatMessageDto>();
    }
}
