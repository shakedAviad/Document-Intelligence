namespace DocumentIntelligence.Core.Models;

public sealed record CaseContext(
    string OriginalQuestion,
    CasePlan Plan,
    IReadOnlyList<CaseFinding> Findings);
