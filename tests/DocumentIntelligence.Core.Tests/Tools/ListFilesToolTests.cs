using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.Core.Tools;
using FluentAssertions;

namespace DocumentIntelligence.Core.Tests.Tools;

public sealed class ListFilesToolTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public ListFilesToolTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task ExecuteAsync_WithFiles_ReturnsFileList()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "meetings.md"), "content");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "sales-q1.csv"), "content");
        ListFilesTool tool = new(_tempDir);

        ToolResult result = await tool.ExecuteAsync(string.Empty);

        result.Output.Should().Contain("meetings.md");
        result.Output.Should().Contain("sales-q1.csv");
    }

    [Fact]
    public async Task ExecuteAsync_EmptyDirectory_ReturnsNoDocumentsMessage()
    {
        ListFilesTool tool = new(_tempDir);

        ToolResult result = await tool.ExecuteAsync(string.Empty);

        result.Output.Should().Be("No documents available.");
    }

    [Fact]
    public void Tool_HasCorrectNameAndDescription()
    {
        ListFilesTool tool = new(_tempDir);

        tool.Name.Should().Be("ListFiles");
        tool.Description.Should().NotBeEmpty();
    }
}
