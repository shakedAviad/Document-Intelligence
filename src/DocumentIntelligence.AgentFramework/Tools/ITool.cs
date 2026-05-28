using DocumentIntelligence.AgentFramework.Models;

namespace DocumentIntelligence.AgentFramework.Tools;

public interface ITool
{
    string Name { get; }
    string Description { get; }

    Task<ToolResult> ExecuteAsync(string input, CancellationToken cancellationToken = default);
}
