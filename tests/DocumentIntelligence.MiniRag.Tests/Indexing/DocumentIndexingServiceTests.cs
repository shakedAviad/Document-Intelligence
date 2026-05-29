using DocumentIntelligence.MiniRag.Chunking;
using DocumentIntelligence.MiniRag.Embeddings;
using DocumentIntelligence.MiniRag.Indexing;
using DocumentIntelligence.MiniRag.Models;
using FluentAssertions;

namespace DocumentIntelligence.MiniRag.Tests.Indexing;

public class DocumentIndexingServiceTests
{
    [Fact]
    public async Task IndexAsync_Should_Index_Chunks_With_Embeddings()
    {
        var index = new InMemoryDocumentIndex();
        var service = new DocumentIndexingService(new FakeTextEmbeddingGenerator(), index);
        var chunker = new FakeDocumentChunker([
            new DocumentChunk("chunk-1", "First sentence."),
            new DocumentChunk("chunk-2", "Second sentence.")
        ]);

        await service.IndexAsync(
            [new DocumentToIndex("doc.md", "meeting", "First sentence. Second sentence.")],
            chunker);

        IReadOnlyList<EmbeddedDocumentChunk> result = await index.GetAllAsync();
        result.Should().HaveCount(2);
        result[0].Embedding.Should().BeEquivalentTo([1f, 2f, 3f]);
        result[1].Embedding.Should().BeEquivalentTo([1f, 2f, 3f]);
        result[0].Chunk.Content.Should().Be("First sentence.");
        result[1].Chunk.Content.Should().Be("Second sentence.");
    }

    [Fact]
    public async Task IndexAsync_Should_Do_Nothing_When_Documents_Empty()
    {
        var index = new InMemoryDocumentIndex();
        var service = new DocumentIndexingService(new FakeTextEmbeddingGenerator(), index);

        await service.IndexAsync([], new FakeDocumentChunker([]));

        IReadOnlyList<EmbeddedDocumentChunk> result = await index.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task IndexAsync_Should_Skip_Document_When_Chunker_Returns_No_Chunks()
    {
        var index = new InMemoryDocumentIndex();
        var service = new DocumentIndexingService(new FakeTextEmbeddingGenerator(), index);

        await service.IndexAsync(
            [new DocumentToIndex("doc.md", "meeting", "content")],
            new FakeDocumentChunker([]));

        IReadOnlyList<EmbeddedDocumentChunk> result = await index.GetAllAsync();
        result.Should().BeEmpty();
    }

    private sealed class FakeTextEmbeddingGenerator : ITextEmbeddingGenerator
    {
        public Task<IReadOnlyList<float>> GenerateEmbeddingAsync(
            string input,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<float> vector = [1f, 2f, 3f];
            return Task.FromResult(vector);
        }
    }

    private sealed class FakeDocumentChunker(IReadOnlyList<DocumentChunk> chunks) : IDocumentChunker
    {
        public IReadOnlyList<DocumentChunk> Chunk(DocumentChunkingRequest request) => chunks;
    }
}
