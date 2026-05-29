using DocumentIntelligence.Rag.Models;
using FluentAssertions;

namespace DocumentIntelligence.Rag.Tests.Models;

public class RagModelsTests
{
    [Fact]
    public void RagMetadata_Should_Create_Successfully()
    {
        var metadata = new RagMetadata("people.json", "person");

        metadata.SourceName.Should().Be("people.json");
        metadata.DocumentType.Should().Be("person");
    }

    [Fact]
    public void RagDocument_Should_Create_Successfully()
    {
        var metadata = new RagMetadata("people.json", "person");
        var person = new Person(1, "Shaked");

        var document = new RagDocument<Person>("doc-1", person, metadata);

        document.Id.Should().Be("doc-1");
        document.Value.Should().Be(person);
        document.Metadata.Should().Be(metadata);
    }

    [Fact]
    public void RagSearchResult_Should_Create_Successfully()
    {
        var result = new RagSearchResult("doc-1", 0.92);

        result.DocumentId.Should().Be("doc-1");
        result.Score.Should().Be(0.92);
    }

    [Fact]
    public void RagVectorRecord_Should_Create_Successfully()
    {
        var record = new RagVectorRecord
        {
            Id = "1",
            SearchableText = "Name: Shaked",
            SourceName = "people.json",
            DocumentType = "person",
            Embedding = new float[1536]
        };

        record.Id.Should().Be("1");
        record.SearchableText.Should().Be("Name: Shaked");
        record.SourceName.Should().Be("people.json");
        record.DocumentType.Should().Be("person");
        record.Embedding.Length.Should().Be(1536);
    }

    private sealed record Person(int Id, string Name);
}
