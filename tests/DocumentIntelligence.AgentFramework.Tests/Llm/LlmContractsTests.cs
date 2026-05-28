using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Models;
using FluentAssertions;

namespace DocumentIntelligence.AgentFramework.Tests.Llm;

public class LlmContractsTests
{
    [Fact]
    public void ChatModelResponse_CanBeCreated()
    {
        ChatModelResponse resp = new("hello world");

        resp.Content.Should().Be("hello world");
    }

    [Fact]
    public async Task FakeChatModel_ReturnsExpectedResponse()
    {
        IChatModel model = new FakeChatModel();

        IReadOnlyList<ChatMessage> messages = [new ChatMessage(ChatRole.User, "hi")];

        ChatModelResponse resp = await model.CompleteAsync(messages, CancellationToken.None);

        resp.Content.Should().Be("echo: hi");
    }



    private class FakeChatModel : IChatModel
    {
        public Task<ChatModelResponse> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
            string content = $"echo: {messages[0].Content}";
            ChatModelResponse resp = new(content);
            return Task.FromResult(resp);
        }

        public Task<TResponse?> CompleteStructuredAsync<TResponse>(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
            where TResponse : class
        {
            // Return AgentDecision when requested, otherwise null
            if (typeof(TResponse) == typeof(AgentDecision))
            {
                string content = messages[0].Content;
                AgentDecision decision = new(AgentAction.FinalAnswer, null, null, $"echo: {content}");
                return Task.FromResult(decision as TResponse);
            }

            return Task.FromResult<TResponse?>(null);
        }
    }


}
