using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Sessions;
using DocumentIntelligence.Core.Agents;
using DocumentIntelligence.Core.Models;
using FluentAssertions;

namespace DocumentIntelligence.Core.Tests.Agents;

public class OrchestratorTests
{
    [Fact]
    public async Task ExecutorAgent_RoutesCommunicationStep_ToCommunicationAgent()
    {
        FakeBusinessAgent commAgent = new("CommunicationAgent", "comm response");
        ExecutorAgent executor = BuildExecutor(communicationAgent: commAgent);

        CasePlan plan = new(
            "Test goal",
            [new CaseStep("step-1", AgentDomain.Communication, "Who attended?")]);

        CaseContext context = await executor.ExecuteAsync("original question", plan);

        context.Findings[0].Domain.Should().Be(AgentDomain.Communication);
        context.Findings[0].Evidence.Should().Be("comm response");
    }

    [Fact]
    public async Task ExecutorAgent_RoutesSalesStep_ToSalesAgent()
    {
        FakeBusinessAgent salesAgent = new("SalesAgent", "sales response");
        ExecutorAgent executor = BuildExecutor(salesAgent: salesAgent);

        CasePlan plan = new(
            "Test goal",
            [new CaseStep("step-1", AgentDomain.Sales, "What is Q1 revenue?")]);

        CaseContext context = await executor.ExecuteAsync("original question", plan);

        context.Findings[0].Domain.Should().Be(AgentDomain.Sales);
        context.Findings[0].Evidence.Should().Be("sales response");
    }

    [Fact]
    public async Task ExecutorAgent_BuildsCaseContext_WithOriginalQuestion()
    {
        ExecutorAgent executor = BuildExecutor();

        CasePlan plan = new("goal", [new CaseStep("s1", AgentDomain.OutOfScope, "q")]);

        CaseContext context = await executor.ExecuteAsync("the original question", plan);

        context.OriginalQuestion.Should().Be("the original question");
    }

    [Fact]
    public async Task PlannerAgent_PlanAsync_ReturnsCasePlan()
    {
        CasePlan expectedPlan = new(
            "Investigate Q1 sales",
            [new CaseStep("step-1", AgentDomain.Sales, "What is Q1 revenue?")]);

        SequencedStructuredModel model = new([expectedPlan]);
        InMemorySessionStore store = new();
        PlannerAgent planner = new(model, store);

        CasePlan plan = await planner.PlanAsync("What are the Q1 sales figures?");

        plan.Goal.Should().NotBeEmpty();
        plan.Steps.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PlannerAgent_EvaluateAsync_ReturnsCaseResult()
    {
        CasePlan expectedPlan = new(
            "Investigate",
            [new CaseStep("step-1", AgentDomain.Sales, "Revenue?")]);
        CaseResult expectedResult = new(true, "answer", null);

        SequencedStructuredModel model = new([expectedPlan, expectedResult]);
        InMemorySessionStore store = new();
        PlannerAgent planner = new(model, store);

        CasePlan plan = await planner.PlanAsync("What is the revenue?");

        CaseContext context = new(
            "What is the revenue?",
            plan,
            [new CaseFinding("step-1", AgentDomain.Sales, "Revenue was $1M", [])]);

        CaseResult result = await planner.ReviewAsync(context);

        result.IsComplete.Should().BeTrue();
        result.FinalAnswer.Should().Be("answer");
    }

    [Fact]
    public async Task MainOrchestratorAgent_ExecuteAsync_ReturnsAnswerWhenComplete()
    {
        CasePlan plan = new(
            "Investigate communication",
            [new CaseStep("step-1", AgentDomain.Communication, "Who attended?")]);
        CaseResult completeResult = new(true, "Final answer", null);

        SequencedStructuredModel model = new([plan, completeResult]);
        InMemorySessionStore store = new();
        PlannerAgent planner = new(model, store);

        FakeBusinessAgent commAgent = new("CommunicationAgent", "evidence");
        ExecutorAgent executor = BuildExecutor(communicationAgent: commAgent);

        MainOrchestratorAgent orchestrator = new(planner, executor, store);

        AgentResult agentResult = await orchestrator.ExecuteAsync("Who attended the meeting?");

        agentResult.Response.Should().Be("Final answer");
        agentResult.AgentName.Should().Be("MainOrchestratorAgent");
    }

    private static ExecutorAgent BuildExecutor(
        IAgent? communicationAgent = null,
        IAgent? salesAgent = null,
        IAgent? technicalAgent = null,
        IAgent? outOfScopeAgent = null)
    {
        return new ExecutorAgent(
            communicationAgent ?? new FakeBusinessAgent("CommunicationAgent", "default"),
            salesAgent ?? new FakeBusinessAgent("SalesAgent", "default"),
            technicalAgent ?? new FakeBusinessAgent("TechnicalAgent", "default"),
            outOfScopeAgent ?? new FakeBusinessAgent("OutOfScopeAgent", "default"));
    }

    private sealed class FakeBusinessAgent(string name, string response) : IAgent
    {
        public string Name => name;

        public Task<AgentResult> ExecuteAsync(
            string input,
            CancellationToken ct = default)
            => Task.FromResult(new AgentResult(name, response, []));
    }

    private sealed class SequencedStructuredModel(IReadOnlyList<object> responses) : IChatModel
    {
        private int _index;

        public Task<ChatModelResponse> CompleteAsync(
            IReadOnlyList<ChatMessage> messages,
            CancellationToken ct = default)
            => Task.FromResult(new ChatModelResponse(string.Empty));

        public Task<TResponse?> CompleteStructuredAsync<TResponse>(
            IReadOnlyList<ChatMessage> messages,
            CancellationToken ct = default)
            where TResponse : class
        {
            object response = responses[Math.Min(_index, responses.Count - 1)];
            _index++;
            return Task.FromResult(response as TResponse);
        }
    }
}
