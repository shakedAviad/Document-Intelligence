using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Tools;
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
        public override string Name => "AgentWithTool";

        protected override IReadOnlyList<ITool> Tools { get; } = new List<ITool> { new FakeTool("MyTool") };

        protected override async Task<string> ProcessAsync(string input, CancellationToken cancellationToken = default)
        {
            var tr = await ExecuteToolAsync(new ToolExecutionRequest("MyTool", input), cancellationToken);
            return $"Processed: {input} | ToolOutput: {tr.Output}";
        }
    }

    private class AgentWithToolCase : BaseAgent
    {
        public override string Name => "AgentWithToolCase";

        protected override IReadOnlyList<ITool> Tools { get; } = new List<ITool> { new FakeTool("MyTool") };

        protected override async Task<string> ProcessAsync(string input, CancellationToken cancellationToken = default)
        {
            var tr = await ExecuteToolAsync(new ToolExecutionRequest("mytool", input), cancellationToken);
            return $"Processed: {input} | ToolOutput: {tr.Output}";
        }
    }

    private class AgentWithMissingTool : BaseAgent
    {
        public override string Name => "AgentWithMissingTool";

        protected override IReadOnlyList<ITool> Tools { get; } = new List<ITool>();

        protected override async Task<string> ProcessAsync(string input, CancellationToken cancellationToken = default)
        {
            var tr = await ExecuteToolAsync(new ToolExecutionRequest("MissingTool", input), cancellationToken);
            return $"Processed: {input} | {tr.Output}";
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
