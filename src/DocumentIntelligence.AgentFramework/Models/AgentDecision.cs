namespace DocumentIntelligence.AgentFramework.Models;

public sealed record AgentDecision(
    AgentAction Action,
    string? ToolName,
    string? ToolInput,
    string? FinalAnswer) : IDecision
{
    public bool IsComplete => Action == AgentAction.FinalAnswer;

    public string Answer => FinalAnswer ?? string.Empty;

    public ToolExecutionRequest? ToolRequest =>
        Action == AgentAction.Tool
            ? new(ToolName ?? string.Empty, ToolInput ?? string.Empty)
            : null;
}
