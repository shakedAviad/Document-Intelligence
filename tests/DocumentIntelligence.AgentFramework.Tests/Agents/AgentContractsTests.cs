using System;
using DocumentIntelligence.AgentFramework.Models;
using FluentAssertions;
using Xunit;

namespace DocumentIntelligence.AgentFramework.Tests.Agents;

public class AgentContractsTests
{
    [Fact]
    public void AgentResult_HasProperties()
    {
        AgentResult result = new AgentResult("TestAgent", "Hello", Array.Empty<AgentExecutionStep>());

        result.AgentName.Should().Be("TestAgent");
        result.Response.Should().Be("Hello");
    }
}
