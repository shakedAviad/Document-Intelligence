using DocumentIntelligence.MiniRag.Models;

namespace DocumentIntelligence.MiniRag.Indexing;

public sealed class InMemoryDocumentIndex : IDocumentIndex
{
    private readonly List<EmbeddedDocumentChunk> _chunks = [];

    public Task AddAsync(EmbeddedDocumentChunk chunk, CancellationToken cancellationToken = default)
    {
        _chunks.Add(chunk);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IReadOnlyList<EmbeddedDocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        _chunks.AddRange(chunks);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<EmbeddedDocumentChunk>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmbeddedDocumentChunk> snapshot = [.. _chunks];
        return Task.FromResult(snapshot);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _chunks.Clear();
        return Task.CompletedTask;
    }
}
