using DocumentIntelligence.AgentFramework.Models;
using FluentAssertions;

namespace DocumentIntelligence.AgentFramework.Tests.Agents;

public class AgentContractsTests
{
    [Fact]
    public void AgentResult_HasProperties()
    {
        AgentResult result = new AgentResult("TestAgent", "Hello");

        result.AgentName.Should().Be("TestAgent");
        result.Response.Should().Be("Hello");
    }
}
