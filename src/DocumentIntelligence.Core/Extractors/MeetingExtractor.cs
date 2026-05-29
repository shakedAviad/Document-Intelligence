using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.Core.Documents;

namespace DocumentIntelligence.Core.Extractors;

public sealed class MeetingExtractor(IChatModel chatModel) : LlmDocumentExtractor<Meeting>(chatModel)
{
    public override string DocumentName => "meetings.md";
}
