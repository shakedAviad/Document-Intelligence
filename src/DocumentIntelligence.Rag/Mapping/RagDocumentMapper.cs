using DocumentIntelligence.Rag.Models;

namespace DocumentIntelligence.Rag.Mapping;

public sealed class RagDocumentMapper : IRagDocumentMapper
{
    public RagVectorRecord Map<T>(
        RagDocument<T> document,
        string searchableText,
        ReadOnlyMemory<float> embedding) =>
        new()
        {
            Id = document.Id,
            SearchableText = searchableText,
            SourceName = document.Metadata.SourceName,
            DocumentType = document.Metadata.DocumentType,
            Embedding = embedding
        };

    public RagSearchResult Map(RagVectorRecord record, double score) =>
        new(record.Id, score);
}
