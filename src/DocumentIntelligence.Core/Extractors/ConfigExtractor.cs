using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.Core.Documents;

namespace DocumentIntelligence.Core.Extractors;

public sealed class ConfigExtractor(IChatModel chatModel) : LlmDocumentExtractor<ConfigEntry>(chatModel)
{
    public override string DocumentName => "config.json";
}
