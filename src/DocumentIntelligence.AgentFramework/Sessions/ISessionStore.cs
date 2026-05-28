namespace DocumentIntelligence.AgentFramework.Sessions;

using System.Threading;
using System.Threading.Tasks;

public interface ISessionStore
{
    Task<AgentSession?> GetAsync(string agentName, CancellationToken cancellationToken = default);

    Task SaveAsync(AgentSession session, CancellationToken cancellationToken = default);
}
