using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.Core.Documents;

namespace DocumentIntelligence.Core.Extractors;

public sealed class LogExtractor(IChatModel chatModel) : LlmDocumentExtractor<LogEntry>(chatModel)
{
    public override string DocumentName => "server-log.txt";
}
