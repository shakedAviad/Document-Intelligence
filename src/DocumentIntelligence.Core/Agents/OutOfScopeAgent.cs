using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;

namespace DocumentIntelligence.Core.Agents;

public sealed class OutOfScopeAgent(IChatModel chatModel) : BaseAgent(chatModel)
{
    public override string Name => "OutOfScopeAgent";

    protected override string Instructions =>
        """
        You handle questions outside the scope of the available documents.
        The available documents cover: meeting notes, email threads, sales data,
        server logs, and configuration settings.
        Always respond with a polite explanation that the question cannot be answered
        from the available documents.
        """;
}
