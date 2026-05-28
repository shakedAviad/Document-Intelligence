namespace DocumentIntelligence.MiniRag.Models;

public sealed record EmbeddedDocumentChunk(DocumentChunk Chunk, IReadOnlyList<float> Embedding);
