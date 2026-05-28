using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Reasoning;
using DocumentIntelligence.AgentFramework.Tools;
using FluentAssertions;

namespace DocumentIntelligence.AgentFramework.Tests.Agents;

public class BaseAgentReasoningTests
{
    [Fact]
    public async Task ChatModelFinalAnswer_ReturnsFinal()
    {
        FinalAnswerChatModel chat = new FinalAnswerChatModel("The answer");
        JsonAgentDecisionParser parser = new JsonAgentDecisionParser();

        ReasoningAgent agent = new ReasoningAgent(chat, parser);

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

        JsonAgentDecisionParser parser = new JsonAgentDecisionParser();

        ReasoningAgent agent = new ReasoningAgent(chat, parser, new List<ITool> { new FakeTool("MyTool") });

        AgentResult result = await agent.ExecuteAsync("input");

        result.Response.Should().Be("Final after tool");
        result.Steps.Should().Contain(s => s.Description == "Tool executed: MyTool");
    }

    [Fact]
    public async Task MaxSteps_Protection_ReturnsMaxReached()
    {
        AlwaysToolChatModel chat = new AlwaysToolChatModel("LoopTool");
        JsonAgentDecisionParser parser = new JsonAgentDecisionParser();

        LimitedStepsAgent agent = new LimitedStepsAgent(chat, parser, new List<ITool> { new FakeTool("LoopTool") });

        AgentResult result = await agent.ExecuteAsync("input");

        result.Response.Should().Be("Max steps reached");
    }

    private class ReasoningAgent : BaseAgent
    {
        private readonly IReadOnlyList<ITool> _tools;

        public ReasoningAgent(IChatModel chat, IAgentDecisionParser parser, IReadOnlyList<ITool>? tools = null)
            : base(chat, parser)
        {
            _tools = tools ?? new List<ITool>();
        }

        public override string Name => "ReasoningAgent";

        protected override IReadOnlyList<ITool> Tools => _tools;
    }

    private class LimitedStepsAgent : ReasoningAgent
    {
        public LimitedStepsAgent(IChatModel chat, IAgentDecisionParser parser, IReadOnlyList<ITool>? tools = null)
            : base(chat, parser, tools)
        {
        }
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
    }

    private class SequencedChatModel : IChatModel
    {
        private readonly string[] _templates;
        private int _index;

        public SequencedChatModel(string[] templates)
        {
            _templates = templates ?? new string[0];
            _index = 0;
        }

        public Task<ChatModelResponse> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
            string lastUser = messages[^1].Content;
            string template = _templates[Math.Min(_index, _templates.Length - 1)];
            string escaped = lastUser.Replace("\"", "\\\"");
            string content = template.Replace("{0}", escaped);
            _index++;
            return Task.FromResult(new ChatModelResponse(content));
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
    }

    private class FakeTool : ITool
    {
        public FakeTool(string name) => Name = name;

        public string Name { get; }

        public Task<ToolResult> ExecuteAsync(string input, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ToolResult(Name, "OK"));
        }
    }
}
