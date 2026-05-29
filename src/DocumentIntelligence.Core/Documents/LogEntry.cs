namespace DocumentIntelligence.Core.Documents;

public sealed record LogEntry(
    string Timestamp,
    string Level,
    string Message);
