using System.Text.Json;
using DocumentIntelligence.AgentFramework.Models;

namespace DocumentIntelligence.Core.Models;

public sealed record CaseResult(
    bool IsComplete,
    string? FinalAnswer,
    IReadOnlyList<CaseStep>? AdditionalSteps) : IDecision
{
    bool IDecision.IsComplete => true;
    string IDecision.Answer => JsonSerializer.Serialize(this);
    ToolExecutionRequest? IDecision.ToolRequest => null;
}
