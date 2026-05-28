namespace DocumentIntelligence.AgentFramework.Sessions;

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public sealed class InMemorySessionStore : ISessionStore
{
    private readonly ConcurrentDictionary<string, AgentSession> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task SaveAsync(AgentSession session, CancellationToken cancellationToken = default)
    {
        if (session is null)
        {
            throw new System.ArgumentNullException(nameof(session));
        }

        _store[session.AgentName] = session;
        return Task.CompletedTask;
    }

    public Task<AgentSession?> GetAsync(string agentName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(agentName))
        {
            return Task.FromResult<AgentSession?>(null);
        }

        if (_store.TryGetValue(agentName, out AgentSession? session))
        {
            return Task.FromResult<AgentSession?>(session);
        }

        return Task.FromResult<AgentSession?>(null);
    }
}
