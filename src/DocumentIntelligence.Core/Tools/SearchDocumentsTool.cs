using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Tools;
using DocumentIntelligence.Rag.Models;
using DocumentIntelligence.Rag.Search;

namespace DocumentIntelligence.Core.Tools;

public sealed class SearchDocumentsTool(IRagSearchService ragSearchService) : ITool
{
    public string Name => "SearchDocuments";
    public string Description => "Searches documents semantically. Input: search query.";

    public async Task<ToolResult> ExecuteAsync(string input, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RagSearchResult> results = await ragSearchService.SearchAsync(input, top: 5, cancellationToken);

        if (results.Count == 0)
        {
            return new ToolResult("SearchDocuments", $"No results found for '{input}'.");
        }

        List<string> lines = [$"Found {results.Count} result(s) for '{input}':"];

        foreach (RagSearchResult result in results)
        {
            lines.Add($"- {result.DocumentId} (score: {result.Score:F2})");
        }

        return new ToolResult("SearchDocuments", string.Join('\n', lines));
    }
}
