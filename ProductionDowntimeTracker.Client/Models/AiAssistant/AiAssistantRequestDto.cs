namespace ProductionDowntimeTracker.Client.Models.AiAssistant
{
    public class AiAssistantRequestDto
    {
        public List<AiChatMessageDto> Messages { get; set; }
            = new List<AiChatMessageDto>();
    }
}
