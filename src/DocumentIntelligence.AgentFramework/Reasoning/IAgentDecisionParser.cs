namespace DocumentIntelligence.AgentFramework.Reasoning;

using DocumentIntelligence.AgentFramework.Models;

public interface IAgentDecisionParser
{
    AgentDecision Parse(string content);
}
