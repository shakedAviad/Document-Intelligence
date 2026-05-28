using System.Threading;
using System.Threading.Tasks;
using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Reasoning;
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
        public TestAgent()
            : base(new FinalAnswerChatModel(), new JsonAgentDecisionParser())
        {
        }

        public override string Name => "TestAgent";

        private class FinalAnswerChatModel : IChatModel
        {
            public Task<ChatModelResponse> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
            {
                // Return a JSON decision with final answer based on the last user message
                string user = messages[^1].Content;
                string json = $"{{\"Action\":\"FinalAnswer\",\"FinalAnswer\":\"Processed: {user}\"}}";
                return Task.FromResult(new ChatModelResponse(json));
            }
        }
    }
}
