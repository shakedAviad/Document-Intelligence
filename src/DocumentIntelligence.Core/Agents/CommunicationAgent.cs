using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Tools;
using DocumentIntelligence.Core.Tools;

namespace DocumentIntelligence.Core.Agents;

public sealed class CommunicationAgent(
    IChatModel chatModel,
    ReadFileTool readFileTool,
    SearchDocumentsTool searchDocumentsTool) : BaseAgent(chatModel)
{
    public override string Name => "CommunicationAgent";

    protected override IReadOnlyList<ITool> Tools =>
    [
        readFileTool,
        searchDocumentsTool
    ];

    protected override string Instructions =>
        """
        You are a communication analyst. You investigate meeting notes and email threads.
        Available documents: meetings.md (meeting notes, decisions, attendees, action items),
        emails.txt (email conversations, discussions, follow-ups).
        Your job: find relevant information, extract evidence, return findings.
        Do NOT produce a final answer for the user. Return only what you found.
        Use SearchDocuments to find relevant sections, then ReadFile to get full content.
        """;
}
