using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Tools;

namespace DocumentIntelligence.Core.Agents;

public class CommunicationAgent(
    IChatModel chatModel,
    MeetingAgent meetingAgent,
    EmailAgent emailAgent) : BaseAgent(chatModel)
{
    public override string Name => "CommunicationAgent";

    protected override IReadOnlyList<ITool> Tools =>
    [
        new AgentAsTool(meetingAgent),
        new AgentAsTool(emailAgent)
    ];

    protected override string Instructions =>
        """
        You are a communication analyst coordinating meeting and email investigations.
        Use MeetingAgent to investigate meeting notes and decisions.
        Use EmailAgent to investigate email threads and follow-ups.
        You may call both agents to cross-reference information.
        Return combined findings — do NOT produce a final answer for the user.
        """;
}
