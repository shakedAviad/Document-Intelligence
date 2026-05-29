using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.Core.Tools;
using FluentAssertions;

namespace DocumentIntelligence.Core.Tests.Tools;

public sealed class ReadFileToolTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public ReadFileToolTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task ExecuteAsync_ExistingFile_ReturnsContent()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "notes.txt"), "hello world");
        ReadFileTool tool = new(_tempDir);

        ToolResult result = await tool.ExecuteAsync("notes.txt");

        result.Output.Should().Be("hello world");
    }

    [Fact]
    public async Task ExecuteAsync_NonExistentFile_ReturnsNotFoundMessage()
    {
        ReadFileTool tool = new(_tempDir);

        ToolResult result = await tool.ExecuteAsync("missing.txt");

        result.Output.Should().StartWith("File not found:");
    }

    [Fact]
    public async Task ExecuteAsync_FilenameWithPathSeparator_IsSanitized()
    {
        ReadFileTool tool = new(_tempDir);

        ToolResult result = await tool.ExecuteAsync("../secret.txt");

        result.Output.Should().Be("File not found: secret.txt");
    }

    [Fact]
    public void Tool_HasCorrectNameAndDescription()
    {
        ReadFileTool tool = new(_tempDir);

        tool.Name.Should().Be("ReadFile");
        tool.Description.Should().NotBeEmpty();
    }
}
