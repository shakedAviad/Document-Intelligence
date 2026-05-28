using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocumentIntelligence.AgentFramework.Models;

namespace DocumentIntelligence.AgentFramework.Agents;

public abstract class BaseAgent : IAgent
{
    public abstract string Name { get; }

    public async Task<AgentResult> ExecuteAsync(string input, CancellationToken cancellationToken = default)
    {
        var steps = new List<AgentExecutionStep>();
        steps.Add(new AgentExecutionStep("Execution started"));

        var response = await ProcessAsync(input, cancellationToken).ConfigureAwait(false);

        steps.Add(new AgentExecutionStep("Execution completed"));

        var readonlySteps = steps.AsReadOnly();

        return new AgentResult(Name, response, readonlySteps);
    }

    protected virtual Task<string> ProcessAsync(string input, CancellationToken cancellationToken = default)
        => Task.FromResult(input);
}
