using DocumentIntelligence.Rag.Models;

namespace DocumentIntelligence.Rag.Mapping;

public interface IRagDocumentMapper
{
    RagVectorRecord Map<T>(
        RagDocument<T> document,
        string searchableText,
        ReadOnlyMemory<float> embedding);

    RagSearchResult Map(RagVectorRecord record, double score);
}
