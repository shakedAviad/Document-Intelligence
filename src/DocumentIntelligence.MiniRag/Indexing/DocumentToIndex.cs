namespace DocumentIntelligence.MiniRag.Indexing;

public sealed record DocumentToIndex(string SourceFileName, string DocumentType, string Content);
