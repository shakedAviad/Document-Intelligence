namespace DocumentIntelligence.Rag.Models;

public sealed record RagDocument<T>(string Id, T Value, RagMetadata Metadata);
