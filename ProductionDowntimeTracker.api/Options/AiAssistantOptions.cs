namespace ProductionDowntimeTracker.api.Options
{
    public class AiAssistantOptions
    {
        public const string SectionName = "AiAssistant";

        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gemini-3.5-flash-lite";
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxUserMessageLength { get; set; } = 2000;
        public int MaxContextExchanges { get; set; } = 10;
    }
}
