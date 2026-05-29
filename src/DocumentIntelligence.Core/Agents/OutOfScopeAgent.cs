using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;

namespace DocumentIntelligence.Core.Agents;

public sealed class OutOfScopeAgent(IChatModel chatModel) : BaseAgent(chatModel)
{
    public override string Name => "OutOfScopeAgent";

    protected override string Instructions =>
        """
        You handle questions that are outside the scope of the available documents.
        Always respond with a polite message explaining that the question cannot be
        answered from the available documents (meetings, emails, sales data,
        server logs, configuration).
        """;
}
