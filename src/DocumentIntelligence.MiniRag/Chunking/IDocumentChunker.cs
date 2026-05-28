using DocumentIntelligence.MiniRag.Models;

namespace DocumentIntelligence.MiniRag.Chunking;

public interface IDocumentChunker
{
    IReadOnlyList<DocumentChunk> Chunk(DocumentChunkingRequest request);
}
