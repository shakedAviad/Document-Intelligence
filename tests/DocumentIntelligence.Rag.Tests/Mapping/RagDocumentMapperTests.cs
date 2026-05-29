using DocumentIntelligence.Rag.Mapping;
using DocumentIntelligence.Rag.Models;
using FluentAssertions;

namespace DocumentIntelligence.Rag.Tests.Mapping;

public class RagDocumentMapperTests
{
    private readonly IRagDocumentMapper _mapper = new RagDocumentMapper();

    [Fact]
    public void Map_Should_Create_Vector_Record()
    {
        var document = new RagDocument<Person>(
            "person-1",
            new Person(1, "Shaked"),
            new RagMetadata("people.json", "person"));
        ReadOnlyMemory<float> embedding = new float[1536];

        RagVectorRecord record = _mapper.Map(document, "Id: 1\nName: Shaked", embedding);

        record.Id.Should().Be("person-1");
        record.SourceName.Should().Be("people.json");
        record.DocumentType.Should().Be("person");
        record.SearchableText.Should().Be("Id: 1\nName: Shaked");
        record.Embedding.Length.Should().Be(1536);
    }

    [Fact]
    public void Map_Should_Create_Search_Result()
    {
        var record = new RagVectorRecord
        {
            Id = "person-1",
            SearchableText = "Id: 1\nName: Shaked",
            SourceName = "people.json",
            DocumentType = "person",
            Embedding = new float[1536]
        };

        RagSearchResult result = _mapper.Map(record, 0.92);

        result.DocumentId.Should().Be("person-1");
        result.Score.Should().Be(0.92);
    }

    private sealed record Person(int Id, string Name);
}
