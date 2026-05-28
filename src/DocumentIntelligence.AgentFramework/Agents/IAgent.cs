using System.Threading;
using System.Threading.Tasks;
using DocumentIntelligence.AgentFramework.Models;

namespace DocumentIntelligence.AgentFramework.Agents;

public interface IAgent
{
    string Name { get; }

    Task<AgentResult> ExecuteAsync(string input, CancellationToken cancellationToken = default);
}
