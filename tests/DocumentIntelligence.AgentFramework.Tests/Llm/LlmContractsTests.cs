using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocumentIntelligence.AgentFramework.Llm;
using FluentAssertions;
using Xunit;

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

        IReadOnlyList<string> messages = new List<string> { "hi" };

        ChatModelResponse resp = await model.CompleteAsync(messages, CancellationToken.None);

        resp.Content.Should().Be("echo: hi");
    }

    [Fact]
    public async Task FakeEmbeddingModel_ReturnsExpectedVector()
    {
        IEmbeddingModel embedder = new FakeEmbeddingModel();

        IReadOnlyList<float> vec = await embedder.EmbedAsync("x", CancellationToken.None);

        vec.Should().NotBeNull();
        vec.Should().HaveCount(2);
        vec[0].Should().Be(1f);
        vec[1].Should().Be(2f);
    }

    private class FakeChatModel : IChatModel
    {
        public Task<ChatModelResponse> CompleteAsync(IReadOnlyList<string> messages, CancellationToken cancellationToken = default)
        {
            var content = $"echo: {messages[0]}";
            ChatModelResponse resp = new(content);
            return Task.FromResult(resp);
        }
    }

    private class FakeEmbeddingModel : IEmbeddingModel
    {
        public Task<IReadOnlyList<float>> EmbedAsync(string input, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<float> vec = new float[] { 1f, 2f };
            return Task.FromResult(vec);
        }
    }
}
