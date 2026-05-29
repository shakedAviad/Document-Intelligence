namespace DocumentIntelligence.Core.Models;

public sealed record CaseResult(
    bool IsComplete,
    string? FinalAnswer,
    IReadOnlyList<CaseStep>? AdditionalSteps);
