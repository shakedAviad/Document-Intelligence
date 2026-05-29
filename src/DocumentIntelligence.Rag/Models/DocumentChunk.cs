namespace DocumentIntelligence.Rag.Models;

public sealed record DocumentChunk
{
    public string Id { get; init; }
    public string Content { get; init; }
    public IReadOnlyDictionary<string, string> Metadata { get; init; }

    public DocumentChunk(string id, string content, IReadOnlyDictionary<string, string>? metadata = null)
    {
        Id = id;
        Content = content;
        Metadata = metadata ?? new Dictionary<string, string>();
    }
}
