using DocumentIntelligence.MiniRag.Chunking;
using DocumentIntelligence.MiniRag.Embeddings;
using DocumentIntelligence.MiniRag.Models;

namespace DocumentIntelligence.MiniRag.Indexing;

public sealed class DocumentIndexingService(
    ITextEmbeddingGenerator embeddingGenerator,
    IDocumentIndex documentIndex) : IDocumentIndexingService
{
    public async Task IndexAsync(
        IReadOnlyList<DocumentToIndex> documents,
        IDocumentChunker chunker,
        CancellationToken cancellationToken = default)
    {
        foreach (DocumentToIndex document in documents)
        {
            var request = new DocumentChunkingRequest(document.SourceFileName, document.DocumentType, document.Content);
            IReadOnlyList<DocumentChunk> chunks = chunker.Chunk(request);

            foreach (DocumentChunk chunk in chunks)
            {
                IReadOnlyList<float> embedding = await embeddingGenerator.GenerateEmbeddingAsync(chunk.Content, cancellationToken);
                var embeddedChunk = new EmbeddedDocumentChunk(chunk, embedding);
                await documentIndex.AddAsync(embeddedChunk, cancellationToken);
            }
        }
    }
}
