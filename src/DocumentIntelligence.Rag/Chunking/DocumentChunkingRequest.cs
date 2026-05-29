namespace DocumentIntelligence.Rag.Chunking;

public sealed record DocumentChunkingRequest(string SourceFileName, string DocumentType, string Content);
