using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Tools;
using FluentAssertions;

namespace DocumentIntelligence.AgentFramework.Tests.Agents;

public class BaseAgentToolTests
{
    [Fact]
    public async Task Agent_CanExecuteTool_And_ReturnsResult()
    {
        var agent = new AgentWithTool();

        AgentResult result = await agent.ExecuteAsync("input");

        result.AgentName.Should().Be("AgentWithTool");
        result.Response.Should().Contain("ToolOutput: OK");
        result.Steps.Should().NotBeNull();
        result.Steps.Should().Contain(s => s.Description == "Tool executed: MyTool");
    }

    [Fact]
    public async Task ToolNameMatching_IsCaseInsensitive()
    {
        var agent = new AgentWithToolCase();

        AgentResult result = await agent.ExecuteAsync("input");

        result.Response.Should().Contain("ToolOutput: OK");
        result.Steps.Should().Contain(s => s.Description == "Tool executed: MyTool");
    }

    [Fact]
    public async Task UnknownTool_ReturnsToolNotFound()
    {
        var agent = new AgentWithMissingTool();

        AgentResult result = await agent.ExecuteAsync("input");

        result.Response.Should().Contain("Tool not found: MissingTool");
        result.Steps.Should().Contain(s => s.Description == "Tool not found: MissingTool");
    }

    private class AgentWithTool : BaseAgent
    {
        public AgentWithTool()
            : base(new SequencedChatModel(
            [
                "{{\"Action\":\"Tool\",\"ToolName\":\"MyTool\",\"ToolInput\":\"{0}\"}}",
                "{{\"Action\":\"FinalAnswer\",\"FinalAnswer\":\"Processed: {0} | ToolOutput: OK\"}}"
            ]))
        {
        }

        public override string Name => "AgentWithTool";

        protected override IReadOnlyList<ITool> Tools { get; } = [new FakeTool("MyTool")];
    }

    private class AgentWithToolCase : BaseAgent
    {
        public AgentWithToolCase()
            : base(new SequencedChatModel(
            [
                "{\"Action\":\"Tool\",\"ToolName\":\"mytool\",\"ToolInput\":\"{0}\"}",
                "{\"Action\":\"FinalAnswer\",\"FinalAnswer\":\"Processed: {0} | ToolOutput: OK\"}"
            ]))
        {
        }

        public override string Name => "AgentWithToolCase";

        protected override IReadOnlyList<ITool> Tools { get; } = [new FakeTool("MyTool")];
    }

    private class AgentWithMissingTool : BaseAgent
    {
        public AgentWithMissingTool()
            : base(new SequencedChatModel(
            [
                "{\"Action\":\"Tool\",\"ToolName\":\"MissingTool\",\"ToolInput\":\"{0}\"}",
                "{\"Action\":\"FinalAnswer\",\"FinalAnswer\":\"Tool not found: MissingTool\"}"
            ]))
        {
        }

        public override string Name => "AgentWithMissingTool";

        protected override IReadOnlyList<ITool> Tools { get; } = [];
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
}
