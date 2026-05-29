using DocumentIntelligence.MiniRag.Models;

namespace DocumentIntelligence.MiniRag.Indexing;

public interface IDocumentIndex
{
    Task AddAsync(EmbeddedDocumentChunk chunk, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IReadOnlyList<EmbeddedDocumentChunk> chunks, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmbeddedDocumentChunk>> GetAllAsync(CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
