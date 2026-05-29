namespace DocumentIntelligence.Core.Models;

public sealed record CasePlan(string Goal, IReadOnlyList<CaseStep> Steps);
