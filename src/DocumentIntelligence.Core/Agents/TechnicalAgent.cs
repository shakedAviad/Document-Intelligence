using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Tools;
using DocumentIntelligence.Core.Tools;

namespace DocumentIntelligence.Core.Agents;

public sealed class TechnicalAgent(
    IChatModel chatModel,
    ReadFileTool readFileTool,
    SearchDocumentsTool searchDocumentsTool) : BaseAgent(chatModel)
{
    public override string Name => "TechnicalAgent";

    protected override IReadOnlyList<ITool> Tools =>
    [
        readFileTool,
        searchDocumentsTool
    ];

    protected override string Instructions =>
        """
        You are a technical analyst. You investigate server logs and configuration.
        Available documents: server-log.txt (timestamped log entries with INFO/WARN/ERROR levels),
        config.json (application configuration settings).
        Your job: identify errors, trace issues, analyze configuration, return findings.
        Do NOT produce a final answer for the user. Return only what you found.
        Use SearchDocuments to find relevant sections, then ReadFile to get full content.
        """;
}
