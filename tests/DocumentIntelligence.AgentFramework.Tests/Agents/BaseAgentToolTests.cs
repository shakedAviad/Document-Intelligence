using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Tools;
// using DocumentIntelligence.AgentFramework.Reasoning;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DocumentIntelligence.AgentFramework.Tests.Agents;

public class BaseAgentToolTests
{
    [Fact]
    public async Task Agent_CanExecuteTool_And_ReturnsResult()
    {
        AgentWithTool agent = new AgentWithTool();

        AgentResult result = await agent.ExecuteAsync("input");

        result.AgentName.Should().Be("AgentWithTool");
        result.Response.Should().Contain("ToolOutput: OK");
        result.Steps.Should().NotBeNull();
        result.Steps.Should().Contain(s => s.Description == "Tool executed: MyTool");
    }

    [Fact]
    public async Task ToolNameMatching_IsCaseInsensitive()
    {
        AgentWithToolCase agent = new AgentWithToolCase();

        AgentResult result = await agent.ExecuteAsync("input");

        result.Response.Should().Contain("ToolOutput: OK");
        result.Steps.Should().Contain(s => s.Description == "Tool executed: MyTool");
    }

    [Fact]
    public async Task UnknownTool_ReturnsToolNotFound()
    {
        AgentWithMissingTool agent = new AgentWithMissingTool();

        AgentResult result = await agent.ExecuteAsync("input");

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
            })
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
            })
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
            })
            {
            }

        public override string Name => "AgentWithMissingTool";

        protected override IReadOnlyList<ITool> Tools { get; } = new List<ITool>();
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

        public Task<TResponse?> CompleteStructuredAsync<TResponse>(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
            where TResponse : class
        {
            string lastUser = messages[^1].Content;
            string template = _templates[Math.Min(_index - 1, _templates.Length - 1)];
            string escaped = lastUser.Replace("\"", "\\\"");
            string content = template.Replace("{0}", escaped);
            // determine decision from content
            if (content.Contains("\"Action\":\"Tool\""))
            {
                AgentDecision decision = new DocumentIntelligence.AgentFramework.Models.AgentDecision(DocumentIntelligence.AgentFramework.Models.AgentAction.Tool, ExtractValue(content, "ToolName"), ExtractValue(content, "ToolInput"), null);
                return Task.FromResult(decision as TResponse);
            }

            if (content.Contains("\"Action\":\"FinalAnswer\""))
            {
                string? final = ExtractValue(content, "FinalAnswer");
                AgentDecision decision = new DocumentIntelligence.AgentFramework.Models.AgentDecision(DocumentIntelligence.AgentFramework.Models.AgentAction.FinalAnswer, null, null, final);
                return Task.FromResult(decision as TResponse);
            }

            return Task.FromResult<TResponse?>(null);
        }

        private static string? ExtractValue(string json, string key)
        {
            // very small extractor for test templates: find "key":"value"
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
