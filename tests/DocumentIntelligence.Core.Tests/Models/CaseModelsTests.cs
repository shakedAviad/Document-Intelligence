using System.Text.Json;
using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.Core.Models;
using FluentAssertions;

namespace DocumentIntelligence.Core.Tests.Models;

public class CaseModelsTests
{
    [Fact]
    public void CaseStep_HasCorrectProperties()
    {
        CaseStep step = new("step-1", AgentDomain.Sales, "What is Q1 revenue?");

        step.StepId.Should().Be("step-1");
        step.Domain.Should().Be(AgentDomain.Sales);
        step.Question.Should().Be("What is Q1 revenue?");
    }

    [Fact]
    public void CasePlan_HasGoalAndSteps()
    {
        IReadOnlyList<CaseStep> steps =
        [
            new("step-1", AgentDomain.Sales, "What is Q1 revenue?"),
            new("step-2", AgentDomain.Communication, "Who is the account manager?")
        ];
        CasePlan plan = new("Analyze account performance", steps);

        plan.Goal.Should().Be("Analyze account performance");
        plan.Steps.Count.Should().Be(2);
    }

    [Fact]
    public void CaseFinding_HasEvidenceAndReasoningSteps()
    {
        IReadOnlyList<AgentExecutionStep> reasoningSteps = [new("Checked revenue data")];
        CaseFinding finding = new("step-1", AgentDomain.Sales, "Q1 revenue was $1M", reasoningSteps);

        finding.Evidence.Should().Be("Q1 revenue was $1M");
        finding.ReasoningSteps.Count.Should().Be(1);
    }

    [Fact]
    public void CaseContext_TracksOriginalQuestion()
    {
        CasePlan plan = new("Analyze account", []);
        CaseContext context = new("How is the account doing?", plan, []);

        context.OriginalQuestion.Should().Be("How is the account doing?");
    }

    [Fact]
    public void CaseResult_WhenComplete_HasFinalAnswer()
    {
        CaseResult result = new(true, "The account is performing well.", null);

        result.IsComplete.Should().BeTrue();
        result.FinalAnswer.Should().Be("The account is performing well.");
        result.AdditionalSteps.Should().BeNull();
    }

    [Fact]
    public void CaseResult_WhenIncomplete_HasAdditionalSteps()
    {
        IReadOnlyList<CaseStep> additionalSteps = [new("step-2", AgentDomain.Technical, "Check production data")];
        CaseResult result = new(false, null, additionalSteps);

        result.IsComplete.Should().BeFalse();
        result.FinalAnswer.Should().BeNull();
        result.AdditionalSteps.Should().HaveCount(1);
    }

    [Fact]
    public void CasePlan_SerializesAndDeserializesToJson()
    {
        IReadOnlyList<CaseStep> steps =
        [
            new("step-1", AgentDomain.Sales, "What is Q1 revenue?"),
            new("step-2", AgentDomain.Communication, "Who is the account manager?")
        ];
        CasePlan plan = new("Analyze account performance", steps);

        string json = JsonSerializer.Serialize(plan);
        CasePlan? deserialized = JsonSerializer.Deserialize<CasePlan>(json);

        deserialized.Should().NotBeNull();
        deserialized!.Goal.Should().Be(plan.Goal);
        deserialized.Steps.Count.Should().Be(plan.Steps.Count);
    }

    [Fact]
    public void CaseResult_SerializesAndDeserializesToJson()
    {
        CaseResult result = new(true, "The account is performing well.", null);

        string json = JsonSerializer.Serialize(result);
        CaseResult? deserialized = JsonSerializer.Deserialize<CaseResult>(json);

        deserialized.Should().NotBeNull();
        deserialized!.IsComplete.Should().Be(result.IsComplete);
        deserialized.FinalAnswer.Should().Be(result.FinalAnswer);
    }
}
