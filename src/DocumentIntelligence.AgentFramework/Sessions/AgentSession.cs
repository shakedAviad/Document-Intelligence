namespace DocumentIntelligence.AgentFramework.Sessions;

using System.Collections.Generic;

public sealed class AgentSession
{
    public required string AgentName { get; init; }

    public List<string> Messages { get; init; } = new();
}
