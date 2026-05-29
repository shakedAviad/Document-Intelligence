using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Tools;

namespace DocumentIntelligence.Core.Tools;

public sealed class ListFilesTool(string documentsPath) : ITool
{
    public string Name => "ListFiles";
    public string Description => "Lists all available document files.";

    public Task<ToolResult> ExecuteAsync(string input, CancellationToken cancellationToken = default)
    {
        string[] files = Directory.GetFiles(documentsPath);

        if (files.Length == 0)
        {
            return Task.FromResult(new ToolResult("ListFiles", "No documents available."));
        }

        List<string> lines = ["Available files:"];

        foreach (string file in files)
        {
            lines.Add($"- {Path.GetFileName(file)}");
        }

        return Task.FromResult(new ToolResult("ListFiles", string.Join('\n', lines)));
    }
}
