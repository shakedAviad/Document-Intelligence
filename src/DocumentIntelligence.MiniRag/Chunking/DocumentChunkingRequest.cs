namespace DocumentIntelligence.MiniRag.Chunking;

public sealed record DocumentChunkingRequest(string SourceFileName, string DocumentType, string Content);
