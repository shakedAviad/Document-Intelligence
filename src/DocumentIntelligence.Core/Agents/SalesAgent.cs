using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Tools;
using DocumentIntelligence.Core.Tools;

namespace DocumentIntelligence.Core.Agents;

public sealed class SalesAgent(
    IChatModel chatModel,
    ReadFileTool readFileTool,
    SearchDocumentsTool searchDocumentsTool) : BaseAgent(chatModel)
{
    public override string Name => "SalesAgent";

    protected override IReadOnlyList<ITool> Tools =>
    [
        readFileTool,
        searchDocumentsTool
    ];

    protected override string Instructions =>
        """
        You are a sales analyst. You investigate sales data.
        Available documents: sales-q1.csv (Q1 sales records: date, region, product,
        units sold, unit price, currency, sales rep, status).
        Your job: analyze sales figures, identify trends, compare data, return findings.
        Do NOT produce a final answer for the user. Return only what you found.
        Note: some prices may have currency symbols (e.g. $49.00) — treat them as numbers.
        Use SearchDocuments to find relevant sections, then ReadFile to get full content.
        """;
}
