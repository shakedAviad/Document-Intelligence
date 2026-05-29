using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Tools;

namespace DocumentIntelligence.Core.Tools;

public sealed class ReadFileTool(string documentsPath) : ITool
{
    public string Name => "ReadFile";
    public string Description => "Reads the full content of a document file. Input: filename.";

    public async Task<ToolResult> ExecuteAsync(string input, CancellationToken cancellationToken = default)
    {
        string sanitized = Path.GetFileName(input);
        string fullPath = Path.Combine(documentsPath, sanitized);

        if (!File.Exists(fullPath))
        {
            return new ToolResult("ReadFile", $"File not found: {sanitized}");
        }

        string content = await File.ReadAllTextAsync(fullPath, cancellationToken);
        return new ToolResult("ReadFile", content);
    }
}
