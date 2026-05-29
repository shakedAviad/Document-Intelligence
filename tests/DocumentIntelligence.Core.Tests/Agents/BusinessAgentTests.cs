using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Tools;
using DocumentIntelligence.Core.Agents;
using DocumentIntelligence.Core.Tools;
using DocumentIntelligence.Rag.Models;
using DocumentIntelligence.Rag.Search;
using FluentAssertions;

namespace DocumentIntelligence.Core.Tests.Agents;

public class BusinessAgentTests
{
    private static ReadFileTool MakeReadFileTool() => new(Path.GetTempPath());
    private static SearchDocumentsTool MakeSearchTool() => new(new FakeRagSearchService());

    [Fact]
    public void CommunicationAgent_HasCorrectName()
    {
        CommunicationAgent agent = new(
            new FinalAnswerChatModel("answer"),
            MakeReadFileTool(),
            MakeSearchTool());

        agent.Name.Should().Be("CommunicationAgent");
    }

    [Fact]
    public async Task CommunicationAgent_ReturnsFindings()
    {
        CommunicationAgent agent = new(
            new FinalAnswerChatModel("Meeting on March 12 decided X"),
            MakeReadFileTool(),
            MakeSearchTool());

        AgentResult result = await agent.ExecuteAsync("What was decided?");

        result.Response.Should().Be("Meeting on March 12 decided X");
        result.AgentName.Should().Be("CommunicationAgent");
    }

    [Fact]
    public void SalesAgent_HasCorrectName()
    {
        SalesAgent agent = new(
            new FinalAnswerChatModel("answer"),
            MakeReadFileTool(),
            MakeSearchTool());

        agent.Name.Should().Be("SalesAgent");
    }

    [Fact]
    public void TechnicalAgent_HasCorrectName()
    {
        TechnicalAgent agent = new(
            new FinalAnswerChatModel("answer"),
            MakeReadFileTool(),
            MakeSearchTool());

        agent.Name.Should().Be("TechnicalAgent");
    }

    [Fact]
    public void OutOfScopeAgent_HasCorrectName()
    {
        OutOfScopeAgent agent = new(new FinalAnswerChatModel("answer"));

        agent.Name.Should().Be("OutOfScopeAgent");
    }

    [Fact]
    public async Task OutOfScopeAgent_ReturnsOutOfScopeResponse()
    {
        OutOfScopeAgent agent = new(
            new FinalAnswerChatModel("This question is outside the scope of available documents."));

        AgentResult result = await agent.ExecuteAsync("What is the weather?");

        result.Response.Should().Contain("scope");
    }

    private sealed class FinalAnswerChatModel(string answer) : IChatModel
    {
        public Task<ChatModelResponse> CompleteAsync(
            IReadOnlyList<ChatMessage> messages,
            CancellationToken ct = default) =>
            Task.FromResult(new ChatModelResponse(string.Empty));

        public Task<TResponse?> CompleteStructuredAsync<TResponse>(
            IReadOnlyList<ChatMessage> messages,
            CancellationToken ct = default)
            where TResponse : class
        {
            AgentDecision decision = new(AgentAction.FinalAnswer, null, null, answer);
            return Task.FromResult(decision as TResponse);
        }
    }

    private sealed class FakeRagSearchService : IRagSearchService
    {
        public Task<IReadOnlyList<RagSearchResult>> SearchAsync(
            string query,
            int top = 5,
            CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<RagSearchResult>>([]);
    }
}
