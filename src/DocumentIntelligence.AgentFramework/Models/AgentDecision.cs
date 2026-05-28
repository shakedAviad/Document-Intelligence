namespace DocumentIntelligence.AgentFramework.Models;

public sealed record AgentDecision(AgentAction Action, string? ToolName, string? ToolInput, string? FinalAnswer);
