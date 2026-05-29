using System.Text.Json;
using System.Text.Json.Serialization;
using DocumentIntelligence.AgentFramework.Models;

namespace DocumentIntelligence.Core.Models;

public sealed record CasePlan(string Goal, IReadOnlyList<CaseStep> Steps) : IDecision
{
    public bool IsComplete => true;

    [JsonIgnore]
    public string Answer => JsonSerializer.Serialize(this);

    public ToolExecutionRequest? ToolRequest => null;
}
