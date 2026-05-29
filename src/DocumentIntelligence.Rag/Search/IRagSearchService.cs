using DocumentIntelligence.Rag.Models;

namespace DocumentIntelligence.Rag.Search;

public interface IRagSearchService
{
    Task<IReadOnlyList<RagSearchResult>> SearchAsync(
        string query,
        int top = 5,
        CancellationToken cancellationToken = default);
}
