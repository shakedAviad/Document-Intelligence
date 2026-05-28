using DocumentIntelligence.AgentFramework.Sessions;
using FluentAssertions;

namespace DocumentIntelligence.AgentFramework.Tests.Sessions;

public class InMemorySessionStoreTests
{
    [Fact]
    public async Task SaveAndRetrieveSession_Succeeds()
    {
        InMemorySessionStore store = new InMemorySessionStore();

        AgentSession session = new AgentSession { AgentName = "TestAgent" };
        session.Messages.Add("hello");

        await store.SaveAsync(session);

        AgentSession? fetched = await store.GetAsync("TestAgent");

        fetched.Should().NotBeNull();
        fetched!.AgentName.Should().Be("TestAgent");
        fetched.Messages.Should().ContainSingle().Which.Should().Be("hello");
    }

    [Fact]
    public async Task UnknownSession_ReturnsNull()
    {
        InMemorySessionStore store = new InMemorySessionStore();

        AgentSession? fetched = await store.GetAsync("DoesNotExist");

        fetched.Should().BeNull();
    }

    [Fact]
    public async Task SessionLookup_IsCaseInsensitive()
    {
        InMemorySessionStore store = new InMemorySessionStore();

        AgentSession session = new AgentSession { AgentName = "CaseAgent" };
        session.Messages.Add("m");

        await store.SaveAsync(session);

        AgentSession? fetched = await store.GetAsync("caseagent");

        fetched.Should().NotBeNull();
        fetched!.AgentName.Should().Be("CaseAgent");
    }

    [Fact]
    public async Task StoredMessages_PersistCorrectly()
    {
        InMemorySessionStore store = new InMemorySessionStore();

        AgentSession session = new AgentSession { AgentName = "PersistAgent" };
        session.Messages.Add("one");
        session.Messages.Add("two");

        await store.SaveAsync(session);

        AgentSession? fetched = await store.GetAsync("PersistAgent");

        fetched.Should().NotBeNull();
        fetched!.Messages.Should().HaveCount(2);
        fetched.Messages.Should().ContainInOrder(new[] { "one", "two" });
    }
}
