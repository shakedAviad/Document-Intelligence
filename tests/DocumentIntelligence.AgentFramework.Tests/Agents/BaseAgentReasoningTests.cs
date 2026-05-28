using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Tools;
using FluentAssertions;

namespace DocumentIntelligence.AgentFramework.Tests.Agents;

public class BaseAgentReasoningTests
{
    [Fact]
    public async Task ChatModelFinalAnswer_ReturnsFinal()
    {
        FinalAnswerChatModel chat = new FinalAnswerChatModel("The answer");

        ReasoningAgent agent = new ReasoningAgent(chat);

        AgentResult result = await agent.ExecuteAsync("input");

        result.Response.Should().Be("The answer");
    }

    [Fact]
    public async Task ToolRequested_ExecutesTool_ThenReturnsFinal()
    {
        SequencedChatModel chat = new SequencedChatModel(new[]
        {
            "{\"Action\":\"Tool\",\"ToolName\":\"MyTool\",\"ToolInput\":\"{0}\"}",
            "{\"Action\":\"FinalAnswer\",\"FinalAnswer\":\"Final after tool\"}"
        });

        ReasoningAgent agent = new(chat, [new FakeTool("MyTool")]);

        AgentResult result = await agent.ExecuteAsync("input");

        result.Response.Should().Be("Final after tool");
        result.Steps.Should().Contain(s => s.Description == "Tool executed: MyTool");
    }

    [Fact]
    public async Task MaxSteps_Protection_ReturnsMaxReached()
    {
        AlwaysToolChatModel chat = new AlwaysToolChatModel("LoopTool");
        LimitedStepsAgent agent = new(chat, [new FakeTool("LoopTool")]);

        AgentResult result = await agent.ExecuteAsync("input");

        result.Response.Should().Be("Max steps reached");
    }

    private class ReasoningAgent : BaseAgent
    {
        private readonly IReadOnlyList<ITool> _tools;

        public ReasoningAgent(IChatModel chat, IReadOnlyList<ITool>? tools = null) : base(chat)
        {
            _tools = tools ?? new List<ITool>();
        }

        public override string Name => "ReasoningAgent";

        protected override IReadOnlyList<ITool> Tools => _tools;
    }

    private class LimitedStepsAgent(IChatModel chat, IReadOnlyList<ITool>? tools = null) : ReasoningAgent(chat, tools)
    {
        protected override AgentExecutionOptions ExecutionOptions => new(2);
    }

    private class FinalAnswerChatModel : IChatModel
    {
        private readonly string _answer;

        public FinalAnswerChatModel(string answer)
        {
            _answer = answer;
        }

        public Task<ChatModelResponse> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
            string json = $"{{\"Action\":\"FinalAnswer\",\"FinalAnswer\":\"{_answer}\"}}";
            return Task.FromResult(new ChatModelResponse(json));
        }

        public Task<TResponse?> CompleteStructuredAsync<TResponse>(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
            where TResponse : class
        {
            AgentDecision decision = new DocumentIntelligence.AgentFramework.Models.AgentDecision(DocumentIntelligence.AgentFramework.Models.AgentAction.FinalAnswer, null, null, _answer);
            return Task.FromResult(decision as TResponse);
        }
    }

    private class SequencedChatModel : IChatModel
    {
        private readonly string[] _templates;
        private int _index;

        public SequencedChatModel(string[] templates)
        {
            _templates = templates ?? [];
            _index = 0;
        }

        public Task<ChatModelResponse> CompleteAsync(
            IReadOnlyList<ChatMessage> messages,
            CancellationToken cancellationToken = default)
        {
            string content = BuildNextContent(messages);

            return Task.FromResult(new ChatModelResponse(content));
        }

        public Task<TResponse?> CompleteStructuredAsync<TResponse>(
            IReadOnlyList<ChatMessage> messages,
            CancellationToken cancellationToken = default)
            where TResponse : class
        {
            string content = BuildNextContent(messages);

            if (content.Contains("\"Action\":\"Tool\""))
            {
                var decision = new AgentDecision(
                    AgentAction.Tool,
                    ExtractValue(content, "ToolName"),
                    ExtractValue(content, "ToolInput"),
                    null);

                return Task.FromResult(decision as TResponse);
            }

            if (content.Contains("\"Action\":\"FinalAnswer\""))
            {
                var decision = new AgentDecision(
                    AgentAction.FinalAnswer,
                    null,
                    null,
                    ExtractValue(content, "FinalAnswer"));

                return Task.FromResult(decision as TResponse);
            }

            return Task.FromResult<TResponse?>(null);
        }

        private string BuildNextContent(IReadOnlyList<ChatMessage> messages)
        {
            if (_templates.Length == 0)
            {
                return string.Empty;
            }

            string lastUser = messages[^1].Content;
            string template = _templates[Math.Min(_index, _templates.Length - 1)];
            string escaped = lastUser.Replace("\"", "\\\"");
            string content = template.Replace("{0}", escaped);

            _index++;

            return content;
        }
    }

    private class AlwaysToolChatModel : IChatModel
    {
        private readonly string _toolName;

        public AlwaysToolChatModel(string toolName)
        {
            _toolName = toolName;
        }

        public Task<ChatModelResponse> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
            string json = $"{{\"Action\":\"Tool\",\"ToolName\":\"{_toolName}\",\"ToolInput\":\"{messages[^1].Content}\"}}";
            return Task.FromResult(new ChatModelResponse(json));
        }

        public Task<TResponse?> CompleteStructuredAsync<TResponse>(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
            where TResponse : class
        {
            string input = messages[^1].Content;
            AgentDecision decision = new DocumentIntelligence.AgentFramework.Models.AgentDecision(DocumentIntelligence.AgentFramework.Models.AgentAction.Tool, _toolName, input, null);
            return Task.FromResult(decision as TResponse);
        }
    }

    private class FakeTool : ITool
    {
        public FakeTool(string name, string description = "")
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }

        public string Description { get; }

        public Task<ToolResult> ExecuteAsync(string input, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ToolResult(Name, "OK"));
        }
    }

    private static string? ExtractValue(string json, string key)
    {
        string marker = $"\"{key}\":\"";
        int idx = json.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return null;
        }

        int start = idx + marker.Length;
        int end = json.IndexOf('"', start);
        if (end < 0)
        {
            return null;
        }

        return json.Substring(start, end - start);
    }
}
