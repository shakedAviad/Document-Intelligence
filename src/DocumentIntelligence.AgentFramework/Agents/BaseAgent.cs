using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Tools;

namespace DocumentIntelligence.AgentFramework.Agents;

public abstract class BaseAgent : IAgent
{
    public abstract string Name { get; }

    // Expose tools to derived agents. Default is empty.
    protected virtual IReadOnlyList<ITool> Tools => Array.Empty<ITool>();

    // Internal holder for the current execution steps so helpers can add steps.
    private List<AgentExecutionStep>? _currentSteps;

    // Helper for derived code to add an execution step to the current trace.
    protected void AddStep(string description)
    {
        if (_currentSteps is not null)
        {
            _currentSteps.Add(new AgentExecutionStep(description));
        }
    }

    public async Task<AgentResult> ExecuteAsync(string input, CancellationToken cancellationToken = default)
    {
        List<AgentExecutionStep> steps = new();
        try
        {
            _currentSteps = steps;
            steps.Add(new AgentExecutionStep("Execution started"));

            string response = await ProcessAsync(input, cancellationToken).ConfigureAwait(false);

            steps.Add(new AgentExecutionStep("Execution completed"));

            ReadOnlyCollection<AgentExecutionStep> readonlySteps = steps.AsReadOnly();

            return new AgentResult(Name, response, readonlySteps);
        }
        finally
        {
            _currentSteps = null;
        }
    }

    protected virtual Task<string> ProcessAsync(string input, CancellationToken cancellationToken = default)
        => Task.FromResult(input);

    protected async Task<ToolResult> ExecuteToolAsync(ToolExecutionRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ITool? tool = Tools is null ? null : Tools.FirstOrDefault(t => string.Equals(t.Name, request.ToolName, StringComparison.OrdinalIgnoreCase));

        if (tool is null)
        {
            ToolResult notFound = new ToolResult(request.ToolName, $"Tool not found: {request.ToolName}");
            AddStep($"Tool not found: {request.ToolName}");
            return notFound;
        }

        ToolResult result = await tool.ExecuteAsync(request.Input, cancellationToken).ConfigureAwait(false);
        AddStep($"Tool executed: {tool.Name}");
        return result;
    }
}
