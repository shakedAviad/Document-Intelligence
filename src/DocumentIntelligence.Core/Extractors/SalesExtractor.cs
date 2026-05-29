using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.Core.Documents;

namespace DocumentIntelligence.Core.Extractors;

public sealed class SalesExtractor(IChatModel chatModel) : LlmDocumentExtractor<SalesRecord>(chatModel)
{
    public override string DocumentName => "sales-q1.csv";
}
