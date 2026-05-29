using DocumentIntelligence.Rag.Models;
using FluentAssertions;

namespace DocumentIntelligence.Rag.Tests.Models;

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


}
