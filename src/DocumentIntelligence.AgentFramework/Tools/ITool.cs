using System.Threading;
using System.Threading.Tasks;
using DocumentIntelligence.AgentFramework.Models;

namespace DocumentIntelligence.AgentFramework.Tools;

public interface ITool
{
    string Name { get; }

    Task<ToolResult> ExecuteAsync(string input, CancellationToken cancellationToken = default);
}
