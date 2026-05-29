using DocumentIntelligence.Rag.Models;
using DocumentIntelligence.Rag.Text;
using FluentAssertions;

namespace DocumentIntelligence.Rag.Tests.Text;

public class ReflectionRagTextBuilderTests
{
    private readonly IRagTextBuilder _builder = new ReflectionRagTextBuilder();

    [Fact]
    public void BuildText_Should_Include_Public_Properties()
    {
        var document = new RagDocument<PersonWithId>(
            "person-1",
            new PersonWithId(1, "Shaked"),
            new RagMetadata("people.json", "person"));

        string result = _builder.BuildText(document);

        result.Should().Contain("Id: 1");
        result.Should().Contain("Name: Shaked");
    }

    [Fact]
    public void BuildText_Should_Ignore_Null_Values()
    {
        var document = new RagDocument<PersonWithEmail>(
            "person-1",
            new PersonWithEmail("Shaked", null),
            new RagMetadata("people.json", "person"));

        string result = _builder.BuildText(document);

        result.Should().Contain("Name: Shaked");
        result.Should().NotContain("Email:");
    }

    [Fact]
    public void BuildText_Should_Join_String_Collections()
    {
        var document = new RagDocument<Meeting>(
            "meeting-1",
            new Meeting("March 12", ["Sarah", "David", "Noa"]),
            new RagMetadata("meetings.json", "meeting"));

        string result = _builder.BuildText(document);

        result.Should().Contain("Attendees: Sarah, David, Noa");
    }

    [Fact]
    public void BuildText_Should_Return_Empty_When_Value_Is_Null()
    {
        var document = new RagDocument<object?>(
            "null-1",
            null,
            new RagMetadata("source.json", "unknown"));

        string result = _builder.BuildText(document);

        result.Should().BeEmpty();
    }

    [Fact]
    public void BuildText_Should_Not_Include_Metadata()
    {
        var document = new RagDocument<PersonWithId>(
            "person-1",
            new PersonWithId(1, "Shaked"),
            new RagMetadata("people.json", "person"));

        string result = _builder.BuildText(document);

        result.Should().NotContain("people.json");
        result.Should().NotContain("DocumentType");
    }

    private sealed record PersonWithId(int Id, string Name);
    private sealed record PersonWithEmail(string Name, string? Email);
    private sealed record Meeting(string Date, IReadOnlyList<string> Attendees);
}
