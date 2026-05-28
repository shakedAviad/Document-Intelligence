using System;
using DocumentIntelligence.AgentFramework.Reasoning;
using FluentAssertions;
using Xunit;

namespace DocumentIntelligence.AgentFramework.Tests.Reasoning;

public class JsonAgentDecisionParserTests
{
    [Fact]
    public void Parses_FinalAnswer_Decision()
    {
        var parser = new JsonAgentDecisionParser();

        string json = "{\"Action\":\"FinalAnswer\",\"FinalAnswer\":\"Done\"}";

        var decision = parser.Parse(json);

        decision.Action.Should().Be(DocumentIntelligence.AgentFramework.Models.AgentAction.FinalAnswer);
        decision.FinalAnswer.Should().Be("Done");
    }

    [Fact]
    public void Parses_Tool_Decision()
    {
        var parser = new JsonAgentDecisionParser();

        string json = "{\"Action\":\"Tool\",\"ToolName\":\"MyTool\",\"ToolInput\":\"x\"}";

        var decision = parser.Parse(json);

        decision.Action.Should().Be(DocumentIntelligence.AgentFramework.Models.AgentAction.Tool);
        decision.ToolName.Should().Be("MyTool");
        decision.ToolInput.Should().Be("x");
    }

    [Fact]
    public void Throws_For_Invalid_Json()
    {
        var parser = new JsonAgentDecisionParser();

        Action act = () => parser.Parse("not json");

        act.Should().Throw<InvalidOperationException>();
    }
}
