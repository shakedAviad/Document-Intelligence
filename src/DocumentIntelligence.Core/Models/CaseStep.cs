namespace DocumentIntelligence.Core.Models;

public sealed record CaseStep(string StepId, AgentDomain Domain, string Question);
