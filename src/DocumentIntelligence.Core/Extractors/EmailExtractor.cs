using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.Core.Documents;

namespace DocumentIntelligence.Core.Extractors;

public sealed class EmailExtractor(IChatModel chatModel) : LlmDocumentExtractor<EmailMessage>(chatModel)
{
    public override string DocumentName => "emails.txt";
}
