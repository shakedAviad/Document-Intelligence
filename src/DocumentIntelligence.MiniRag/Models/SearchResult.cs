namespace DocumentIntelligence.MiniRag.Models;

public sealed record SearchResult(DocumentChunk Chunk, double Score);
