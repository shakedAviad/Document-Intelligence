using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Tools;
using System.Text.Json;

namespace DocumentIntelligence.AgentFramework.Agents;

public abstract class BaseAgent<TDecision>(IChatModel chatModel) : IAgent
    where TDecision : class, IDecision
{
    protected static readonly string AgentDecisionFormatInstructions =
        "The model must return ONLY valid JSON matching the AgentDecision schema:\n" +
        "{\n  \"action\": \"FinalAnswer\" | \"Tool\",\n  \"toolName\": \"tool name or null\"," +
        "\n  \"toolInput\": \"tool input or null\",\n  \"finalAnswer\": \"final answer or null\"\n}";

    public abstract string Name { get; }

    protected virtual IReadOnlyList<ITool> Tools => Array.Empty<ITool>();

    protected virtual string Instructions => "You are a helpful generic agent.";

    protected virtual string ResponseFormatInstructions => string.Empty;

    protected virtual AgentExecutionOptions ExecutionOptions => new();

    protected virtual string BuildSystemPrompt()
    {
        List<string> lines = [];
        lines.Add($"Agent: {Name}");
        lines.Add(string.Empty);
        lines.Add(Instructions);
        lines.Add(string.Empty);
        lines.Add("Available tools:");

        if (Tools.Count == 0)
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

        if (!string.IsNullOrWhiteSpace(ResponseFormatInstructions))
        {
            lines.Add(string.Empty);
            lines.Add("Response format instructions:");
            lines.Add(ResponseFormatInstructions);
        }

        return string.Join('\n', lines);
    }

    private List<AgentExecutionStep>? _currentSteps;

    protected void AddStep(string description)
        => _currentSteps?.Add(new AgentExecutionStep(description));

    public async Task<AgentResult> ExecuteAsync(string input, CancellationToken cancellationToken = default)
    {
        List<AgentExecutionStep> steps = [];
        try
        {
            _currentSteps = steps;
            steps.Add(new AgentExecutionStep("Execution started"));

            List<ChatMessage> messages =
            [
                new(ChatRole.System, BuildSystemPrompt()),
                new(ChatRole.User, input)
            ];

            for (int i = 0; i < ExecutionOptions.MaxReasoningIterations; i++)
            {
                TDecision? decision = await chatModel
                    .CompleteStructuredAsync<TDecision>(messages, cancellationToken)
                    .ConfigureAwait(false);

                if (decision is null)
                {
                    throw new InvalidOperationException("Chat model returned null decision");
                }

                if (decision.IsComplete)
                {
                    steps.Add(new AgentExecutionStep("Execution completed"));
                    return new AgentResult(Name, decision.Answer, steps.AsReadOnly());
                }

                if (decision.ToolRequest is { } toolRequest)
                {
                    string serialized = JsonSerializer.Serialize(decision);
                    messages.Add(new(ChatRole.Assistant, serialized));

                    ToolResult toolResult = await ExecuteToolAsync(toolRequest, cancellationToken)
                        .ConfigureAwait(false);

                    messages.Add(new(ChatRole.User, toolResult.Output));
                }
            }

            steps.Add(new AgentExecutionStep("Execution completed"));
            return new AgentResult(Name, "Max steps reached", steps.AsReadOnly());
        }
        finally
        {
            _currentSteps = null;
        }
    }

    protected async Task<ToolResult> ExecuteToolAsync(
        ToolExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ITool? tool = Tools.FirstOrDefault(
            t => string.Equals(t.Name, request.ToolName, StringComparison.OrdinalIgnoreCase));

        if (tool is null)
        {
            ToolResult notFound = new(request.ToolName, $"Tool not found: {request.ToolName}");
            AddStep($"Tool not found: {request.ToolName}");
            return notFound;
        }

        ToolResult result = await tool.ExecuteAsync(request.Input, cancellationToken).ConfigureAwait(false);
        AddStep($"Tool executed: {tool.Name}");
        return result;
    }
}

public abstract class BaseAgent(IChatModel chatModel) : BaseAgent<AgentDecision>(chatModel)
{
    protected override string ResponseFormatInstructions => AgentDecisionFormatInstructions;
}
