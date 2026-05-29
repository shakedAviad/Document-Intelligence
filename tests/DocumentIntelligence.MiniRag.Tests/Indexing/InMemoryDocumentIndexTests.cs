using DocumentIntelligence.MiniRag.Indexing;
using DocumentIntelligence.MiniRag.Models;
using FluentAssertions;

namespace DocumentIntelligence.MiniRag.Tests.Indexing;

public class InMemoryDocumentIndexTests
{
    private static EmbeddedDocumentChunk CreateChunk(string id) =>
        new(new DocumentChunk(id, $"Content of {id}"), [1f, 2f, 3f]);

    [Fact]
    public async Task AddAsync_Should_Store_Chunk()
    {
        var index = new InMemoryDocumentIndex();

        await index.AddAsync(CreateChunk("chunk-1"));

        IReadOnlyList<EmbeddedDocumentChunk> result = await index.GetAllAsync();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddRangeAsync_Should_Store_All_Chunks()
    {
        var index = new InMemoryDocumentIndex();
        EmbeddedDocumentChunk first = CreateChunk("chunk-1");
        EmbeddedDocumentChunk second = CreateChunk("chunk-2");

        await index.AddRangeAsync([first, second]);

        IReadOnlyList<EmbeddedDocumentChunk> result = await index.GetAllAsync();
        result.Should().HaveCount(2);
        result[0].Should().Be(first);
        result[1].Should().Be(second);
    }

    [Fact]
    public async Task ClearAsync_Should_Remove_All_Chunks()
    {
        var index = new InMemoryDocumentIndex();
        await index.AddRangeAsync([CreateChunk("chunk-1"), CreateChunk("chunk-2")]);

        await index.ClearAsync();

        IReadOnlyList<EmbeddedDocumentChunk> result = await index.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Snapshot()
    {
        var index = new InMemoryDocumentIndex();
        await index.AddAsync(CreateChunk("chunk-1"));

        IReadOnlyList<EmbeddedDocumentChunk> firstSnapshot = await index.GetAllAsync();

        await index.AddAsync(CreateChunk("chunk-2"));

        firstSnapshot.Should().HaveCount(1);
        IReadOnlyList<EmbeddedDocumentChunk> secondSnapshot = await index.GetAllAsync();
        secondSnapshot.Should().HaveCount(2);
    }
}
