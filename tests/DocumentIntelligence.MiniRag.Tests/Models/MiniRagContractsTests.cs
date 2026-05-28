using DocumentIntelligence.MiniRag.Models;
using FluentAssertions;

namespace DocumentIntelligence.MiniRag.Tests.Models;

public class MiniRagContractsTests
{
    [Fact]
    public void DocumentChunk_Should_Create_Successfully()
    {
        var chunk = new DocumentChunk("chunk-1", "Decision was made.");

        chunk.Id.Should().Be("chunk-1");
        chunk.Content.Should().Be("Decision was made.");
        chunk.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void DocumentChunk_Should_Create_With_Metadata()
    {
        var metadata = new Dictionary<string, string>
        {
            ["source"] = "meeting-notes",
            ["date"] = "2024-01-15"
        };

        var chunk = new DocumentChunk("chunk-1", "Decision was made.", metadata);

        chunk.Id.Should().Be("chunk-1");
        chunk.Content.Should().Be("Decision was made.");
        chunk.Metadata.Should().BeEquivalentTo(metadata);
    }

    [Fact]
    public void EmbeddedDocumentChunk_Should_Create_Successfully()
    {
        var chunk = new DocumentChunk("chunk-1", "Decision was made.");
        IReadOnlyList<float> embedding = [1f, 2f, 3f];

        var embeddedChunk = new EmbeddedDocumentChunk(chunk, embedding);

        embeddedChunk.Chunk.Should().Be(chunk);
        embeddedChunk.Embedding.Should().BeEquivalentTo(embedding);
    }

    [Fact]
    public void SearchResult_Should_Create_Successfully()
    {
        var chunk = new DocumentChunk("chunk-1", "Decision was made.");

        var result = new SearchResult(chunk, 0.87);

        result.Chunk.Should().Be(chunk);
        result.Score.Should().Be(0.87);
    }
}
