using System.Net.Http.Json;
using System.Text.Json;
using ProductionDowntimeTracker.Client.Models.AiAssistant;

namespace ProductionDowntimeTracker.Client.Services
{
    public sealed class AiAssistantChatService
    {
        public const int MaxUserMessageLength = 2000;

        public const string WelcomeMessage =
            "Dobrý den, jsem AI asistent aplikace Production Downtime Tracker. " +
            "S čím vám mohu poradit?";

        private readonly HttpClient _httpClient;

        private readonly List<AiChatMessageDto> _messages =
            new List<AiChatMessageDto>();

        public IReadOnlyList<AiChatMessageDto> Messages => _messages;

        public bool IsLoading { get; private set; }

        public string? ErrorMessage { get; private set; }

        public AiAssistantChatService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendMessageAsync(
            string message,
            CancellationToken cancellationToken = default)
        {
            if (IsLoading)
            {
                return;
            }

            string trimmedMessage = message.Trim();

            if (string.IsNullOrWhiteSpace(trimmedMessage))
            {
                ErrorMessage = "Napište zprávu.";
                return;
            }

            if (trimmedMessage.Length > MaxUserMessageLength)
            {
                ErrorMessage =
                    $"Zpráva může mít maximálně " +
                    $"{MaxUserMessageLength} znaků.";

                return;
            }

            IsLoading = true;
            ErrorMessage = null;

            _messages.Add(new AiChatMessageDto
            {
                Role = "user",
                Content = trimmedMessage
            });

            var request = new AiAssistantRequestDto
            {
                Messages = new List<AiChatMessageDto>(_messages)
            };

            try
            {
                using HttpResponseMessage response =
                    await _httpClient.PostAsJsonAsync(
                        "api/assistant",
                        request,
                        cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = await ReadErrorMessageAsync(
                        response,
                        cancellationToken);

                    return;
                }

                AiAssistantResponseDto? result =
                    await response.Content
                        .ReadFromJsonAsync<AiAssistantResponseDto>(
                            cancellationToken);

                if (result is null ||
                    string.IsNullOrWhiteSpace(result.Reply))
                {
                    ErrorMessage =
                        "AI služba nevrátila žádnou odpověď.";

                    return;
                }

                _messages.Add(new AiChatMessageDto
                {
                    Role = "assistant",
                    Content = result.Reply
                });
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                ErrorMessage =
                    "Požadavek na AI službu byl ukončen.";
            }
            catch (HttpRequestException)
            {
                ErrorMessage =
                    "Nepodařilo se spojit s AI službou.";
            }
            catch (JsonException)
            {
                ErrorMessage =
                    "AI služba vrátila neplatnou odpověď.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private static async Task<string> ReadErrorMessageAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            try
            {
                Dictionary<string, string>? error =
                    await response.Content
                        .ReadFromJsonAsync<Dictionary<string, string>>(
                            cancellationToken);

                if (error is not null &&
                    error.TryGetValue("message", out string? message) &&
                    !string.IsNullOrWhiteSpace(message))
                {
                    return message;
                }
            }
            catch (JsonException)
            {
                // Pokud odpověď není JSON, použije se obecná zpráva.
            }

            return "AI služba je momentálně nedostupná.";
        }
    }
}