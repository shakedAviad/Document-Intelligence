using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Tools;

namespace DocumentIntelligence.Core.Agents;

public class TechnicalAgent(
    IChatModel chatModel,
    LogAgent logAgent,
    ConfigAgent configAgent) : BaseAgent(chatModel)
{
    public override string Name => "TechnicalAgent";

    protected override IReadOnlyList<ITool> Tools =>
    [
        new AgentAsTool(logAgent),
        new AgentAsTool(configAgent)
    ];

    protected override string Instructions =>
        """
        You are a technical analyst coordinating log and configuration investigations.
        Use LogAgent to investigate server logs, errors, and events.
        Use ConfigAgent to investigate configuration settings and changes.
        You may call both agents to correlate errors with configuration.
        Return combined findings — do NOT produce a final answer for the user.
        """;
}
