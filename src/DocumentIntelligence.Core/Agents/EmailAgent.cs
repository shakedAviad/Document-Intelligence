using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Tools;
using DocumentIntelligence.Core.Tools;

namespace DocumentIntelligence.Core.Agents;

public sealed class EmailAgent(
    IChatModel chatModel,
    ReadFileTool readFileTool,
    SearchDocumentsTool searchDocumentsTool) : BaseAgent(chatModel)
{
    public override string Name => "EmailAgent";

    protected override IReadOnlyList<ITool> Tools => [readFileTool, searchDocumentsTool];

    protected override string Instructions =>
        """
        You are an email analyst. You investigate email threads in emails.txt.
        Focus on: who said what, follow-ups, decisions referenced, participants.
        Use SearchDocuments to find relevant sections, then ReadFile to read emails.txt.
        Return findings only — do NOT produce a final answer for the user.
        """;
}
