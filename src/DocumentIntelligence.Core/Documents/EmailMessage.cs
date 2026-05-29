namespace DocumentIntelligence.Core.Documents;

public sealed record EmailMessage(
    string From,
    string To,
    string Date,
    string Subject,
    string Body);
