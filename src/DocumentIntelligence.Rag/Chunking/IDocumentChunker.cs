using DocumentIntelligence.Rag.Models;

namespace DocumentIntelligence.Rag.Chunking;

public interface IDocumentChunker
{
    IReadOnlyList<DocumentChunk> Chunk(DocumentChunkingRequest request);
}
