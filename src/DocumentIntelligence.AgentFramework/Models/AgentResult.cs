namespace DocumentIntelligence.AgentFramework.Models;

using System.Collections.Generic;

public sealed record AgentResult(string AgentName, string Response, IReadOnlyList<AgentExecutionStep> Steps);
