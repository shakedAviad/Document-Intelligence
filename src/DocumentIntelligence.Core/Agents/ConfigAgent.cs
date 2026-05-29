using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Tools;
using DocumentIntelligence.Core.Tools;

namespace DocumentIntelligence.Core.Agents;

public sealed class ConfigAgent(
    IChatModel chatModel,
    ReadFileTool readFileTool,
    SearchDocumentsTool searchDocumentsTool) : BaseAgent(chatModel)
{
    public override string Name => "ConfigAgent";

    protected override IReadOnlyList<ITool> Tools => [readFileTool, searchDocumentsTool];

    protected override string Instructions =>
        """
        You are a configuration analyst. You investigate settings in config.json.
        Focus on: configuration values, settings changes, potential misconfigurations.
        Use SearchDocuments to find relevant settings, then ReadFile to read config.json.
        Return findings only — do NOT produce a final answer for the user.
        """;
}
