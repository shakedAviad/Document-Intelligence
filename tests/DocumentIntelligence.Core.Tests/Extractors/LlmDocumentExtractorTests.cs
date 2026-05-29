using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.Core.Documents;
using DocumentIntelligence.Core.Extractors;
using DocumentIntelligence.Rag.Models;
using FluentAssertions;

namespace DocumentIntelligence.Core.Tests.Extractors;

public class LlmDocumentExtractorTests
{
    [Fact]
    public async Task ExtractAsync_Meeting_ReturnsRagDocuments()
    {
        List<Meeting> meetings =
        [
            new("March 12", ["Sarah", "David"], ["Use Adyen"], ["Finish webhooks"])
        ];
        MeetingExtractor extractor = new(new FakeChatModel(meetings));

        IReadOnlyList<RagDocument<Meeting>> result = await extractor.ExtractAsync("content");

        result.Count.Should().Be(1);
        result[0].Value.Date.Should().Be("March 12");
        result[0].Metadata.SourceName.Should().Be("meetings.md");
        result[0].Metadata.DocumentType.Should().Be("Meeting");
    }

    [Fact]
    public async Task ExtractAsync_SalesRecord_NormalizesData()
    {
        List<SalesRecord> records =
        [
            new("2026-01-05", "North America", "Globe Pro", 12, 99.00m, "USD", "Mike", "completed")
        ];
        SalesExtractor extractor = new(new FakeChatModel(records));

        IReadOnlyList<RagDocument<SalesRecord>> result = await extractor.ExtractAsync("content");

        result[0].Value.UnitPrice.Should().Be(99.00m);
        result[0].Value.Product.Should().Be("Globe Pro");
    }

    [Fact]
    public async Task ExtractAsync_WhenLlmReturnsNull_ReturnsEmpty()
    {
        MeetingExtractor extractor = new(new FakeChatModel(null!));

        IReadOnlyList<RagDocument<Meeting>> result = await extractor.ExtractAsync("content");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExtractAsync_AssignsCorrectIds()
    {
        List<LogEntry> entries =
        [
            new("2026-01-01T00:00:00", "INFO", "Started"),
            new("2026-01-01T00:01:00", "ERROR", "Failed")
        ];
        LogExtractor extractor = new(new FakeChatModel(entries));

        IReadOnlyList<RagDocument<LogEntry>> result = await extractor.ExtractAsync("content");

        result[0].Id.Should().Be("server-log.txt-0");
        result[1].Id.Should().Be("server-log.txt-1");
    }

    [Fact]
    public async Task ExtractAsync_BuildsSystemPromptWithSchemaHint()
    {
        CapturingChatModel capturing = new();
        MeetingExtractor extractor = new(capturing);

        await extractor.ExtractAsync("content");

        ChatMessage systemMessage = capturing.CapturedMessages![0];
        systemMessage.Content.Should().Contain("Meeting");
        systemMessage.Content.Should().Contain("Date");
        systemMessage.Content.Should().Contain("Attendees");
    }

    private sealed class FakeChatModel(object response) : IChatModel
    {
        public Task<ChatModelResponse> CompleteAsync(
            IReadOnlyList<ChatMessage> messages,
            CancellationToken ct = default) =>
            Task.FromResult(new ChatModelResponse(string.Empty));

        public Task<TResponse?> CompleteStructuredAsync<TResponse>(
            IReadOnlyList<ChatMessage> messages,
            CancellationToken ct = default)
            where TResponse : class =>
            Task.FromResult(response as TResponse);
    }

    private sealed class CapturingChatModel : IChatModel
    {
        public IReadOnlyList<ChatMessage>? CapturedMessages { get; private set; }

        public Task<ChatModelResponse> CompleteAsync(
            IReadOnlyList<ChatMessage> messages,
            CancellationToken ct = default) =>
            Task.FromResult(new ChatModelResponse(string.Empty));

        public Task<TResponse?> CompleteStructuredAsync<TResponse>(
            IReadOnlyList<ChatMessage> messages,
            CancellationToken ct = default)
            where TResponse : class
        {
            CapturedMessages = messages;
            return Task.FromResult<TResponse?>(null);
        }
    }
}
