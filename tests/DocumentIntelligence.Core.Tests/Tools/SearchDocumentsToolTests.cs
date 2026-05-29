using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.Core.Tools;
using DocumentIntelligence.Rag.Models;
using DocumentIntelligence.Rag.Search;
using FluentAssertions;

namespace DocumentIntelligence.Core.Tests.Tools;

public class SearchDocumentsToolTests
{
    [Fact]
    public async Task ExecuteAsync_WithResults_ReturnsFormattedList()
    {
        IReadOnlyList<RagSearchResult> fakeResults =
        [
            new("meetings.md", 0.95),
            new("sales-q1.csv", 0.82)
        ];
        SearchDocumentsTool tool = new(new FakeRagSearchService(fakeResults));

        ToolResult result = await tool.ExecuteAsync("quarterly review");

        result.Output.Should().Contain("meetings.md");
        result.Output.Should().Contain("sales-q1.csv");
    }

    [Fact]
    public async Task ExecuteAsync_NoResults_ReturnsNoResultsMessage()
    {
        SearchDocumentsTool tool = new(new FakeRagSearchService([]));

        ToolResult result = await tool.ExecuteAsync("unknown topic");

        result.Output.Should().Contain("No results found");
    }

    [Fact]
    public void Tool_HasCorrectNameAndDescription()
    {
        SearchDocumentsTool tool = new(new FakeRagSearchService([]));

        tool.Name.Should().Be("SearchDocuments");
        tool.Description.Should().NotBeEmpty();
    }

    private sealed class FakeRagSearchService(IReadOnlyList<RagSearchResult> results) : IRagSearchService
    {
        public Task<IReadOnlyList<RagSearchResult>> SearchAsync(
            string query,
            int top = 5,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(results);
    }
}
