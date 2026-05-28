namespace DocumentIntelligence.AgentFramework.Models;

public sealed record ToolExecutionRequest(string ToolName, string Input);
