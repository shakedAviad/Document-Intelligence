using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Models;
using FluentAssertions;

namespace DocumentIntelligence.AgentFramework.Tests.Agents;

public class BaseAgentTests
{
    [Fact]
    public async Task BaseAgent_Process_ReturnsProcessedResponseAndSteps()
    {
        TestAgent agent = new TestAgent();

        AgentResult result = await agent.ExecuteAsync("input");

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
            : base(new FinalAnswerChatModel())
        {
        }

        public override string Name => "TestAgent";

        private class FinalAnswerChatModel : IChatModel
        {
            public Task<ChatModelResponse> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
            {
                string user = messages[^1].Content;
                string json = $"{{\"Action\":\"FinalAnswer\",\"FinalAnswer\":\"Processed: {user}\"}}";
                return Task.FromResult(new ChatModelResponse(json));
            }

            public Task<AgentDecision?> CompleteStructuredAsync<AgentDecision>(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
                where AgentDecision : class
            {
                string user = messages[^1].Content;
                Models.AgentDecision decision = new DocumentIntelligence.AgentFramework.Models.AgentDecision(DocumentIntelligence.AgentFramework.Models.AgentAction.FinalAnswer, null, null, $"Processed: {user}");
                return Task.FromResult(decision as AgentDecision);
            }
        }
    }
}
