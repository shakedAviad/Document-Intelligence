namespace DocumentIntelligence.AgentFramework.Models;

public interface IDecision
{
    bool IsComplete { get; }
    string Answer { get; }
    ToolExecutionRequest? ToolRequest { get; }
}
