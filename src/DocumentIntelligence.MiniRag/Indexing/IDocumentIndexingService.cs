using DocumentIntelligence.MiniRag.Chunking;

namespace DocumentIntelligence.MiniRag.Indexing;

public interface IDocumentIndexingService
{
    Task IndexAsync(
        IReadOnlyList<DocumentToIndex> documents,
        IDocumentChunker chunker,
        CancellationToken cancellationToken = default);
}
