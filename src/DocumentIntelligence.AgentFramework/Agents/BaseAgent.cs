using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Tools;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace DocumentIntelligence.AgentFramework.Agents;

public abstract class BaseAgent(IChatModel chatModel) : IAgent
{
    public abstract string Name { get; }

    // Expose tools to derived agents. Default is empty.
    protected virtual IReadOnlyList<ITool> Tools => Array.Empty<ITool>();

    // Reasoning/instructions configuration
    protected virtual string Instructions => "You are a helpful generic agent.";

    protected virtual string ResponseFormatInstructions =>
        "The model must return ONLY valid JSON matching the AgentDecision schema:\n" +
        "{\n  \"action\": \"FinalAnswer\" | \"Tool\",\n  \"toolName\": \"tool name or null\",\n  \"toolInput\": \"tool input or null\",\n  \"finalAnswer\": \"final answer or null\"\n}";

    protected virtual AgentExecutionOptions ExecutionOptions => new();

    protected virtual string BuildSystemPrompt()
    {
        List<string> lines = [];
        lines.Add($"Agent: {Name}");
        lines.Add(string.Empty);
        lines.Add(Instructions);
        lines.Add(string.Empty);
        lines.Add("Available tools:");

        if (Tools is null || Tools.Count == 0)
        {
            lines.Add("No tools are available.");
        }
        else
        {
            foreach (ITool tool in Tools)
            {
                string desc = tool.Description ?? string.Empty;
                if (string.IsNullOrWhiteSpace(desc))
                {
                    lines.Add($"- {tool.Name}");
                }
                else
                {
                    lines.Add($"- {tool.Name}: {desc}");
                }
            }
        }

        lines.Add(string.Empty);
        lines.Add("Response format instructions:");
        lines.Add(ResponseFormatInstructions);

        return string.Join('\n', lines);
    }

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

            List<ChatMessage> messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, BuildSystemPrompt()),
                new ChatMessage(ChatRole.User, input)
            };

            for (int i = 0; i < ExecutionOptions.MaxReasoningIterations; i++)
            {
                AgentDecision? decision = await chatModel.CompleteStructuredAsync<AgentDecision>(messages, cancellationToken).ConfigureAwait(false);

                if (decision is null)
                {
                    throw new InvalidOperationException("Chat model returned null decision");
                }

                if (decision.Action == AgentAction.FinalAnswer)
                {
                    string final = decision.FinalAnswer ?? string.Empty;
                    steps.Add(new AgentExecutionStep("Execution completed"));
                    ReadOnlyCollection<AgentExecutionStep> readonlySteps = steps.AsReadOnly();
                    return new AgentResult(Name, final, readonlySteps);
                }

                if (decision.Action == AgentAction.Tool)
                {
                    // append assistant decision content (structured)
                    string serialized = JsonSerializer.Serialize(decision);
                    messages.Add(new ChatMessage(ChatRole.Assistant, serialized));

                    ToolExecutionRequest toolRequest = new ToolExecutionRequest(decision.ToolName ?? string.Empty, decision.ToolInput ?? string.Empty);
                    ToolResult toolResult = await ExecuteToolAsync(toolRequest, cancellationToken).ConfigureAwait(false);

                    // append tool observation as user message
                    messages.Add(new ChatMessage(ChatRole.User, toolResult.Output));
                }
            }

            // Max steps reached
            steps.Add(new AgentExecutionStep("Execution completed"));
            ReadOnlyCollection<AgentExecutionStep> finalSteps = steps.AsReadOnly();
            return new AgentResult(Name, "Max steps reached", finalSteps);
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
