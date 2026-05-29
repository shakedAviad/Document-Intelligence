using DocumentIntelligence.AgentFramework.Agents;
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
    private static (ReadFileTool, SearchDocumentsTool) BuildTools()
    {
        ReadFileTool readFileTool = new(Path.GetTempPath());
        SearchDocumentsTool searchTool = new(new FakeRagSearchService());
        return (readFileTool, searchTool);
    }

    [Fact]
    public async Task AgentAsTool_DelegatesCallToInnerAgent()
    {
        FakeAgent fakeAgent = new("MyAgent", "inner result");
        AgentAsTool tool = new(fakeAgent);

        ToolResult result = await tool.ExecuteAsync("question");

        result.Output.Should().Be("inner result");
        result.ToolName.Should().Be(fakeAgent.Name);
    }

    [Fact]
    public void AgentAsTool_ExposesAgentNameAndDescription()
    {
        FakeAgent fakeAgent = new("MyAgent", "response");
        AgentAsTool tool = new(fakeAgent);

        tool.Name.Should().Be(fakeAgent.Name);
        tool.Description.Should().NotBeEmpty();
    }

    [Fact]
    public void MeetingAgent_HasCorrectName()
    {
        (ReadFileTool readFileTool, SearchDocumentsTool searchTool) = BuildTools();
        MeetingAgent agent = new(new FinalAnswerChatModel("answer"), readFileTool, searchTool);

        agent.Name.Should().Be("MeetingAgent");
    }

    [Fact]
    public void EmailAgent_HasCorrectName()
    {
        (ReadFileTool readFileTool, SearchDocumentsTool searchTool) = BuildTools();
        EmailAgent agent = new(new FinalAnswerChatModel("answer"), readFileTool, searchTool);

        agent.Name.Should().Be("EmailAgent");
    }

    [Fact]
    public void LogAgent_HasCorrectName()
    {
        (ReadFileTool readFileTool, SearchDocumentsTool searchTool) = BuildTools();
        LogAgent agent = new(new FinalAnswerChatModel("answer"), readFileTool, searchTool);

        agent.Name.Should().Be("LogAgent");
    }

    [Fact]
    public void ConfigAgent_HasCorrectName()
    {
        (ReadFileTool readFileTool, SearchDocumentsTool searchTool) = BuildTools();
        ConfigAgent agent = new(new FinalAnswerChatModel("answer"), readFileTool, searchTool);

        agent.Name.Should().Be("ConfigAgent");
    }

    [Fact]
    public void CommunicationAgent_HasCorrectName()
    {
        (ReadFileTool readFileTool, SearchDocumentsTool searchTool) = BuildTools();
        FinalAnswerChatModel chatModel = new("answer");
        MeetingAgent meetingAgent = new(chatModel, readFileTool, searchTool);
        EmailAgent emailAgent = new(chatModel, readFileTool, searchTool);
        CommunicationAgent agent = new(chatModel, meetingAgent, emailAgent);

        agent.Name.Should().Be("CommunicationAgent");
    }

    [Fact]
    public void CommunicationAgent_HasMeetingAndEmailAgentsAsTools()
    {
        (ReadFileTool readFileTool, SearchDocumentsTool searchTool) = BuildTools();
        FinalAnswerChatModel chatModel = new("answer");
        MeetingAgent meetingAgent = new(chatModel, readFileTool, searchTool);
        EmailAgent emailAgent = new(chatModel, readFileTool, searchTool);
        TestableCommunicationAgent agent = new(chatModel, meetingAgent, emailAgent);

        agent.ExposedTools.Should().HaveCount(2);
        agent.ExposedTools.Select(t => t.Name).Should().Contain("MeetingAgent");
        agent.ExposedTools.Select(t => t.Name).Should().Contain("EmailAgent");
    }

    [Fact]
    public void TechnicalAgent_HasLogAndConfigAgentsAsTools()
    {
        (ReadFileTool readFileTool, SearchDocumentsTool searchTool) = BuildTools();
        FinalAnswerChatModel chatModel = new("answer");
        LogAgent logAgent = new(chatModel, readFileTool, searchTool);
        ConfigAgent configAgent = new(chatModel, readFileTool, searchTool);
        TestableTechnicalAgent agent = new(chatModel, logAgent, configAgent);

        agent.ExposedTools.Should().HaveCount(2);
        agent.ExposedTools.Select(t => t.Name).Should().Contain("LogAgent");
        agent.ExposedTools.Select(t => t.Name).Should().Contain("ConfigAgent");
    }

    [Fact]
    public async Task OutOfScopeAgent_ReturnsOutOfScopeResponse()
    {
        OutOfScopeAgent agent = new(
            new FinalAnswerChatModel("outside the scope"));

        AgentResult result = await agent.ExecuteAsync("What is the weather?");

        result.Response.Should().Contain("scope");
    }

    private sealed class FinalAnswerChatModel(string answer) : IChatModel
    {
        public Task<ChatModelResponse> CompleteAsync(
            IReadOnlyList<ChatMessage> messages,
            CancellationToken ct = default)
            => Task.FromResult(new ChatModelResponse(string.Empty));

        public Task<TResponse?> CompleteStructuredAsync<TResponse>(
            IReadOnlyList<ChatMessage> messages,
            CancellationToken ct = default)
            where TResponse : class
        {
            AgentDecision decision = new(
                AgentAction.FinalAnswer, null, null, answer);
            return Task.FromResult(decision as TResponse);
        }
    }

    private sealed class FakeRagSearchService : IRagSearchService
    {
        public Task<IReadOnlyList<RagSearchResult>> SearchAsync(
            string query,
            int top = 5,
            CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<RagSearchResult>>([]);
    }

    private sealed class FakeAgent(string name, string response) : IAgent
    {
        public string Name => name;

        public Task<AgentResult> ExecuteAsync(
            string input,
            CancellationToken ct = default)
            => Task.FromResult(new AgentResult(name, response, []));
    }

    private sealed class TestableCommunicationAgent(
        IChatModel chatModel,
        MeetingAgent meetingAgent,
        EmailAgent emailAgent)
        : CommunicationAgent(chatModel, meetingAgent, emailAgent)
    {
        public IReadOnlyList<ITool> ExposedTools => Tools;
    }

    private sealed class TestableTechnicalAgent(
        IChatModel chatModel,
        LogAgent logAgent,
        ConfigAgent configAgent)
        : TechnicalAgent(chatModel, logAgent, configAgent)
    {
        public IReadOnlyList<ITool> ExposedTools => Tools;
    }
}
