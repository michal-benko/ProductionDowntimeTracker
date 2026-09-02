using Google.GenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ProductionDowntimeTracker.api.DTOs.AiAssistant;
using ProductionDowntimeTracker.api.Options;

namespace ProductionDowntimeTracker.api.Services
{
    public sealed class GeminiAiAssistantService : IAiAssistantService
    {
        private const string SystemPrompt = """
            Jsi AI asistent aplikace Production Downtime Tracker.

            Aplikace slouží k evidenci výrobních strojů a jejich prostojů.
            Na stránce Stroje lze přidávat nové stroje.
            Na stránce Prostoje lze zahájit prostoj vybraného stroje.
            Při ukončení prostoje uživatel vybere kategorii příčiny
            a zapíše podrobnější detail.
            Data o prostojích lze na stránce Prostoje exportovat do CSV.

            OPC UA část demonstruje komunikaci nadřazeného systému,
            například MES, s PLC na výrobní lince.
            ASP.NET Core backend zde funguje jako OPC UA klient,
            který ze serveru čte stav stroje, aktivní poruchu
            a počet vyrobených cyklů.

            V tomto výukovém projektu poskytuje data simulovaný
            OPC UA server spuštěný v Docker kontejneru.
            Před použitím OPC UA části proto musí být spuštěný Docker
            a příslušný kontejner se serverem.
            Pokud server neběží, aplikace se k němu nemůže připojit
            a zobrazí informaci o nedostupném spojení.
            Ve skutečné výrobě může OPC UA server poskytovat přímo PLC,
            průmyslová brána nebo jiný řídicí systém.

            Jde o rozvíjený výukový projekt vytvořený v C# a .NET.

            Nemáš přístup k aktuálním datům v databázi
            ani možnost aplikaci ovládat.
            Pokud se uživatel ptá na konkrétní aktuální data,
            jasně mu řekni, že k nim zatím nemáš přístup.

            Nevymýšlej si neznámé funkce ani data.
            Odpovídej stručně, srozumitelně a česky.

            Odpovídej pouze čistým prostým textem. Nepoužívej Markdown 
            ani formátovací znaky, například hvězdičky pro tučný text 
            nebo seznamy a znak # pro nadpisy.

            Informaci, že jde o výukový projekt, používej
            pouze jako interní kontext a uživatelům ji neuváděj.
            """;

        private readonly AiAssistantOptions _options;
        private readonly IChatClient? _chatClient;

        public bool IsConfigured => _chatClient is not null;

        public GeminiAiAssistantService(
            IOptions<AiAssistantOptions> options)
        {
            _options = options.Value;

            if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                _chatClient = new Client(apiKey: _options.ApiKey)
                    .AsIChatClient(_options.Model);
            }
        }

        public async Task<string> GetReplyAsync(
            IReadOnlyList<AiChatMessageDto> messages,
            CancellationToken cancellationToken = default)
        {
            if (_chatClient is null)
            {
                throw new InvalidOperationException(
                    "AI služba není nakonfigurovaná.");
            }

            var chatMessages = new List<ChatMessage>
            {
                new(ChatRole.System, SystemPrompt)
            };

            int maxMessageCount =
                (_options.MaxContextExchanges * 2) + 1;

            foreach (var message in messages.TakeLast(maxMessageCount))
            {
                ChatRole role = message.Role.Equals(
                    "assistant",
                    StringComparison.OrdinalIgnoreCase)
                    ? ChatRole.Assistant
                    : ChatRole.User;

                chatMessages.Add(
                    new ChatMessage(role, message.Content));
            }

            using var timeoutSource =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            timeoutSource.CancelAfter(
                TimeSpan.FromSeconds(_options.TimeoutSeconds));

            ChatResponse response =
                await _chatClient.GetResponseAsync(
                    chatMessages,
                    cancellationToken: timeoutSource.Token);

            return response.Text ?? string.Empty;
        }
    }
}