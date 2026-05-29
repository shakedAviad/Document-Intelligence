using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Models;

namespace DocumentIntelligence.AgentFramework.Tools;

public sealed class AgentAsTool(IAgent agent) : ITool
{
    public string Name => agent.Name;

    public string Description => $"Delegates investigation to {agent.Name}.";

    public async Task<ToolResult> ExecuteAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        AgentResult result = await agent
            .ExecuteAsync(input, cancellationToken)
            .ConfigureAwait(false);

        return new ToolResult(agent.Name, result.Response);
    }
}
