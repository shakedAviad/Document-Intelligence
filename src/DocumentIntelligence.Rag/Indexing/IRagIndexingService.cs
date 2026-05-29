using DocumentIntelligence.Rag.Models;

namespace DocumentIntelligence.Rag.Indexing;

public interface IRagIndexingService
{
    Task IndexAsync<T>(
        IReadOnlyList<RagDocument<T>> documents,
        CancellationToken cancellationToken = default);
}
