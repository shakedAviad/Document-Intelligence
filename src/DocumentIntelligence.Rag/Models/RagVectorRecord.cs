using Microsoft.Extensions.VectorData;

namespace DocumentIntelligence.Rag.Models;

public sealed class RagVectorRecord
{
    [VectorStoreKey]
    public string Id { get; init; } = string.Empty;

    [VectorStoreData(IsIndexed = true)]
    public string SearchableText { get; init; } = string.Empty;

    [VectorStoreData(IsIndexed = true)]
    public string SourceName { get; init; } = string.Empty;

    [VectorStoreData(IsIndexed = true)]
    public string DocumentType { get; init; } = string.Empty;

    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> Embedding { get; init; }
}
