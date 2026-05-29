using DocumentIntelligence.Rag.Mapping;
using DocumentIntelligence.Rag.Models;
using DocumentIntelligence.Rag.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace DocumentIntelligence.Rag.Indexing;

public sealed class RagIndexingService(
    IRagTextBuilder textBuilder,
    IRagDocumentMapper mapper,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore) : IRagIndexingService
{
    private const string CollectionName = "rag_documents";

    public async Task IndexAsync<T>(
        IReadOnlyList<RagDocument<T>> documents,
        CancellationToken cancellationToken = default)
    {
        if (documents.Count > 0)
        {

            VectorStoreCollection<string, RagVectorRecord> collection =
                vectorStore.GetCollection<string, RagVectorRecord>(CollectionName);

            await collection.EnsureCollectionExistsAsync(cancellationToken);

            foreach (RagDocument<T> document in documents)
            {
                string searchableText = textBuilder.BuildText(document);

                if (!string.IsNullOrWhiteSpace(searchableText))
                {
                    GeneratedEmbeddings<Embedding<float>> embeddings = await embeddingGenerator.GenerateAsync(
                        [searchableText], cancellationToken: cancellationToken);

                    RagVectorRecord record = mapper.Map(document, searchableText, embeddings[0].Vector);

                    await collection.UpsertAsync(record, cancellationToken);
                }
            }

        }
    }
}
