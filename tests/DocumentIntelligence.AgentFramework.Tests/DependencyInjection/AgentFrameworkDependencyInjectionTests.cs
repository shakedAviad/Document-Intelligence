using Microsoft.Extensions.DependencyInjection;
using DocumentIntelligence.AgentFramework.DependencyInjection;
using DocumentIntelligence.AgentFramework.Sessions;
using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Tools;
using DocumentIntelligence.AgentFramework.Models;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace DocumentIntelligence.AgentFramework.Tests.DependencyInjection;

public class AgentFrameworkDependencyInjectionTests
{
    [Fact]
    public void AddAgentFramework_RegistersSessionStore()
    {
        var services = new ServiceCollection();

        services.AddAgentFramework();

        var provider = services.BuildServiceProvider();

        var store = provider.GetRequiredService<ISessionStore>();

        store.Should().NotBeNull();
        store.Should().BeOfType<InMemorySessionStore>();
    }

    [Fact]
    public void AddAgent_RegistersAgent()
    {
        var services = new ServiceCollection();

        services.AddAgentFramework();
        services.AddAgent<FakeAgent>();

        var provider = services.BuildServiceProvider();

        var agent = provider.GetRequiredService<IAgent>();

        agent.Should().NotBeNull();
        agent.Should().BeOfType<FakeAgent>();
    }

    [Fact]
    public void AddTool_RegistersTool()
    {
        var services = new ServiceCollection();

        services.AddAgentFramework();
        services.AddTool<FakeTool>();

        var provider = services.BuildServiceProvider();

        var tool = provider.GetRequiredService<ITool>();

        tool.Should().NotBeNull();
        tool.Should().BeOfType<FakeTool>();
    }

    private class FakeAgent : IAgent
    {
        public string Name => "Fake";

        public Task<AgentResult> ExecuteAsync(string input, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }

    private class FakeTool : ITool
    {
        public string Name => "FakeTool";

        public string Description => "desc";

        public Task<ToolResult> ExecuteAsync(string input, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
