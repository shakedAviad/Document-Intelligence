using DocumentIntelligence.AgentFramework.Models;

namespace DocumentIntelligence.Core.Models;

public sealed record CaseFinding(
    string StepId,
    AgentDomain Domain,
    string Evidence,
    IReadOnlyList<AgentExecutionStep> ReasoningSteps);
