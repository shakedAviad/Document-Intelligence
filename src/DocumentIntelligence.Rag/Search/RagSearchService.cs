using DocumentIntelligence.Rag.Mapping;
using DocumentIntelligence.Rag.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace DocumentIntelligence.Rag.Search;

public sealed class RagSearchService(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore,
    IRagDocumentMapper mapper) : IRagSearchService
{
    private const string CollectionName = "rag_documents";

    public async Task<IReadOnlyList<RagSearchResult>> SearchAsync(
        string query,
        int top = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || top <= 0)
        {
            return [];
        }

        GeneratedEmbeddings<Embedding<float>> embeddings = await embeddingGenerator.GenerateAsync(
            [query], cancellationToken: cancellationToken);

        ReadOnlyMemory<float> queryVector = embeddings[0].Vector;

        VectorStoreCollection<string, RagVectorRecord> collection =
            vectorStore.GetCollection<string, RagVectorRecord>(CollectionName);

        await collection.EnsureCollectionExistsAsync(cancellationToken);

        List<RagSearchResult> results = [];

        await foreach (VectorSearchResult<RagVectorRecord> result in
            collection.SearchAsync(queryVector, top, cancellationToken: cancellationToken))
        {
            results.Add(mapper.Map(result.Record, result.Score ?? 0));
        }

        return [.. results.OrderByDescending(r => r.Score)];
    }
}
