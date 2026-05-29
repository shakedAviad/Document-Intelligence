using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Tools;
using DocumentIntelligence.Core.Tools;

namespace DocumentIntelligence.Core.Agents;

public sealed class LogAgent(
    IChatModel chatModel,
    ReadFileTool readFileTool,
    SearchDocumentsTool searchDocumentsTool) : BaseAgent(chatModel)
{
    public override string Name => "LogAgent";

    protected override IReadOnlyList<ITool> Tools => [readFileTool, searchDocumentsTool];

    protected override string Instructions =>
        """
        You are a log analyst. You investigate server logs in server-log.txt.
        Focus on: errors, warnings, timestamps, patterns, root causes.
        Use SearchDocuments to find relevant entries, then ReadFile to read server-log.txt.
        Return findings only — do NOT produce a final answer for the user.
        """;
}
