using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Tools;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Reasoning;
using FluentAssertions;
using Xunit;

namespace DocumentIntelligence.AgentFramework.Tests.Agents;

public class BaseAgentToolTests
{
    [Fact]
    public async Task Agent_CanExecuteTool_And_ReturnsResult()
    {
        var agent = new AgentWithTool();

        var result = await agent.ExecuteAsync("input");

        result.AgentName.Should().Be("AgentWithTool");
        result.Response.Should().Contain("ToolOutput: OK");
        result.Steps.Should().NotBeNull();
        result.Steps.Should().Contain(s => s.Description == "Tool executed: MyTool");
    }

    [Fact]
    public async Task ToolNameMatching_IsCaseInsensitive()
    {
        var agent = new AgentWithToolCase();

        var result = await agent.ExecuteAsync("input");

        result.Response.Should().Contain("ToolOutput: OK");
        result.Steps.Should().Contain(s => s.Description == "Tool executed: MyTool");
    }

    [Fact]
    public async Task UnknownTool_ReturnsToolNotFound()
    {
        var agent = new AgentWithMissingTool();

        var result = await agent.ExecuteAsync("input");

        result.Response.Should().Contain("Tool not found: MissingTool");
        result.Steps.Should().Contain(s => s.Description == "Tool not found: MissingTool");
    }

    private class AgentWithTool : BaseAgent
    {
        public AgentWithTool()
            : base(new SequencedChatModel(new[]
            {
                // First: ask to run tool
                "{\"Action\":\"Tool\",\"ToolName\":\"MyTool\",\"ToolInput\":\"{0}\"}",
                // Second: final answer
                "{\"Action\":\"FinalAnswer\",\"FinalAnswer\":\"Processed: {0} | ToolOutput: OK\"}"
            }), new JsonAgentDecisionParser())
        {
        }

        public override string Name => "AgentWithTool";

        protected override IReadOnlyList<ITool> Tools { get; } = new List<ITool> { new FakeTool("MyTool") };
    }

    private class AgentWithToolCase : BaseAgent
    {
        public AgentWithToolCase()
            : base(new SequencedChatModel(new[]
            {
                "{\"Action\":\"Tool\",\"ToolName\":\"mytool\",\"ToolInput\":\"{0}\"}",
                "{\"Action\":\"FinalAnswer\",\"FinalAnswer\":\"Processed: {0} | ToolOutput: OK\"}"
            }), new JsonAgentDecisionParser())
        {
        }

        public override string Name => "AgentWithToolCase";

        protected override IReadOnlyList<ITool> Tools { get; } = new List<ITool> { new FakeTool("MyTool") };
    }

    private class AgentWithMissingTool : BaseAgent
    {
        public AgentWithMissingTool()
            : base(new SequencedChatModel(new[]
            {
                "{\"Action\":\"Tool\",\"ToolName\":\"MissingTool\",\"ToolInput\":\"{0}\"}",
                "{\"Action\":\"FinalAnswer\",\"FinalAnswer\":\"Tool not found: MissingTool\"}"
            }), new JsonAgentDecisionParser())
        {
        }

        public override string Name => "AgentWithMissingTool";

        protected override IReadOnlyList<ITool> Tools { get; } = new List<ITool>();
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
}
