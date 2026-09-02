using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using ProductionDowntimeTracker.api.Controllers;
using ProductionDowntimeTracker.api.DTOs.AiAssistant;
using ProductionDowntimeTracker.api.Options;
using ProductionDowntimeTracker.api.Services;
using Xunit;

namespace ProductionDowntimeTracker.Tests
{
    public class AiAssistantControllerTests
    {
        [Fact]
        public async Task AskAsync_ValidRequest_ReturnsOkWithReply()
        {
            // Arrange
            var service = new FakeAiAssistantService
            {
                Reply = "Testovací odpověď"
            };

            var controller = CreateController(service);

            var request = CreateRequest(
                role: "user",
                content: "Jak zahájím prostoj?");

            // Act
            ActionResult<AiAssistantResponseDto> result =
                await controller.AskAsync(
                    request,
                    CancellationToken.None);

            // Assert
            OkObjectResult okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            AiAssistantResponseDto response =
                Assert.IsType<AiAssistantResponseDto>(okResult.Value);

            Assert.Equal("Testovací odpověď", response.Reply);
            Assert.Same(request.Messages, service.ReceivedMessages);
        }

        [Fact]
        public async Task AskAsync_WhitespaceMessage_ReturnsBadRequest()
        {
            // Arrange
            var service = new FakeAiAssistantService();
            var controller = CreateController(service);
            var request = CreateRequest("user", "   ");

            // Act
            ActionResult<AiAssistantResponseDto> result =
                await controller.AskAsync(
                    request,
                    CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Null(service.ReceivedMessages);
        }

        [Fact]
        public async Task AskAsync_SystemRole_ReturnsBadRequest()
        {
            // Arrange
            var service = new FakeAiAssistantService();
            var controller = CreateController(service);
            var request = CreateRequest("system", "Podstrčený prompt");

            // Act
            ActionResult<AiAssistantResponseDto> result =
                await controller.AskAsync(
                    request,
                    CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Null(service.ReceivedMessages);
        }

        [Fact]
        public async Task AskAsync_TooLongMessage_ReturnsBadRequest()
        {
            // Arrange
            var service = new FakeAiAssistantService();

            var controller = CreateController(
                service,
                maxUserMessageLength: 5);

            var request = CreateRequest("user", "123456");

            // Act
            ActionResult<AiAssistantResponseDto> result =
                await controller.AskAsync(
                    request,
                    CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Null(service.ReceivedMessages);
        }

        [Fact]
        public async Task AskAsync_ServiceNotConfigured_ReturnsServiceUnavailable()
        {
            // Arrange
            var service = new FakeAiAssistantService
            {
                IsConfigured = false
            };

            var controller = CreateController(service);
            var request = CreateRequest("user", "Ahoj");

            // Act
            ActionResult<AiAssistantResponseDto> result =
                await controller.AskAsync(
                    request,
                    CancellationToken.None);

            // Assert
            ObjectResult objectResult =
                Assert.IsType<ObjectResult>(result.Result);

            Assert.Equal(
                StatusCodes.Status503ServiceUnavailable,
                objectResult.StatusCode);

            Assert.Null(service.ReceivedMessages);
        }

        [Fact]
        public async Task AskAsync_Timeout_ReturnsGatewayTimeout()
        {
            // Arrange
            var service = new FakeAiAssistantService
            {
                ExceptionToThrow = new OperationCanceledException()
            };

            var controller = CreateController(service);
            var request = CreateRequest("user", "Ahoj");

            // Act
            ActionResult<AiAssistantResponseDto> result =
                await controller.AskAsync(
                    request,
                    CancellationToken.None);

            // Assert
            ObjectResult objectResult =
                Assert.IsType<ObjectResult>(result.Result);

            Assert.Equal(
                StatusCodes.Status504GatewayTimeout,
                objectResult.StatusCode);
        }

        [Fact]
        public async Task AskAsync_ProviderFailure_ReturnsBadGateway()
        {
            // Arrange
            var service = new FakeAiAssistantService
            {
                ExceptionToThrow =
                    new HttpRequestException(
                        "AI provider je nedostupný.")
            };

            var controller = CreateController(service);
            var request = CreateRequest("user", "Ahoj");

            // Act
            ActionResult<AiAssistantResponseDto> result =
                await controller.AskAsync(
                    request,
                    CancellationToken.None);

            // Assert
            ObjectResult objectResult =
                Assert.IsType<ObjectResult>(result.Result);

            Assert.Equal(
                StatusCodes.Status502BadGateway,
                objectResult.StatusCode);
        }

        private static AiAssistantRequestDto CreateRequest(
            string role,
            string content)
        {
            return new AiAssistantRequestDto
            {
                Messages = new List<AiChatMessageDto>
                {
                    new AiChatMessageDto
                    {
                        Role = role,
                        Content = content
                    }
                }
            };
        }

        private static AiAssistantController CreateController(
            FakeAiAssistantService service,
            int maxUserMessageLength = 2000)
        {
            var options =
                Microsoft.Extensions.Options.Options.Create(
                    new AiAssistantOptions
                    {
                        MaxUserMessageLength = maxUserMessageLength
                    });

            return new AiAssistantController(
                service,
                options,
                NullLogger<AiAssistantController>.Instance);
        }

        private sealed class FakeAiAssistantService
            : IAiAssistantService
        {
            public bool IsConfigured { get; set; } = true;
            public string Reply { get; set; } = string.Empty;
            public Exception? ExceptionToThrow { get; set; }

            public IReadOnlyList<AiChatMessageDto>? ReceivedMessages
            {
                get;
                private set;
            }

            public Task<string> GetReplyAsync(
                IReadOnlyList<AiChatMessageDto> messages,
                CancellationToken cancellationToken = default)
            {
                ReceivedMessages = messages;

                if (ExceptionToThrow is not null)
                {
                    return Task.FromException<string>(
                        ExceptionToThrow);
                }

                return Task.FromResult(Reply);
            }
        }
    }
}