using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Tools;
using DocumentIntelligence.Core.Tools;

namespace DocumentIntelligence.Core.Agents;

public sealed class MeetingAgent(
    IChatModel chatModel,
    ReadFileTool readFileTool,
    SearchDocumentsTool searchDocumentsTool) : BaseAgent(chatModel)
{
    public override string Name => "MeetingAgent";

    protected override IReadOnlyList<ITool> Tools => [readFileTool, searchDocumentsTool];

    protected override string Instructions =>
        """
        You are a meeting analyst. You investigate meeting notes in meetings.md.
        Focus on: decisions made, attendees present, action items assigned, dates.
        Use SearchDocuments to find relevant sections, then ReadFile to read meetings.md.
        Return findings only — do NOT produce a final answer for the user.
        """;
}
