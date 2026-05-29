namespace DocumentIntelligence.Core.Documents;

public sealed record Meeting(
    string Date,
    string[] Attendees,
    string[] Decisions,
    string[] ActionItems);
