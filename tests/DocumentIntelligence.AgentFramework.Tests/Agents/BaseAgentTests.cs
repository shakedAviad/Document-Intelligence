using System.Threading;
using System.Threading.Tasks;
using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Models;
using FluentAssertions;
using Xunit;

namespace DocumentIntelligence.AgentFramework.Tests.Agents;

public class BaseAgentTests
{
    [Fact]
    public async Task BaseAgent_Process_ReturnsProcessedResponseAndSteps()
    {
        var agent = new TestAgent();

        var result = await agent.ExecuteAsync("input");

        result.AgentName.Should().Be("TestAgent");
        result.Response.Should().Be("Processed: input");
        result.Steps.Should().NotBeNull();
        result.Steps.Count.Should().BeGreaterThan(0);
        result.Steps[0].Description.Should().Be("Execution started");
        result.Steps[result.Steps.Count - 1].Description.Should().Be("Execution completed");
    }

    private class TestAgent : BaseAgent
    {
        public override string Name => "TestAgent";

        protected override Task<string> ProcessAsync(string input, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("Processed: " + input);
        }
    }
}
