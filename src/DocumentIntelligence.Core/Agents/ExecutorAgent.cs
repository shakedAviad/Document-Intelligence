using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.Core.Models;

namespace DocumentIntelligence.Core.Agents;

public sealed class ExecutorAgent(
    IAgent communicationAgent,
    IAgent salesAgent,
    IAgent technicalAgent,
    IAgent outOfScopeAgent)
{
    public async Task<CaseContext> ExecuteAsync(
        string originalQuestion,
        CasePlan plan,
        CancellationToken cancellationToken = default)
    {
        List<CaseFinding> findings = [];

        foreach (CaseStep step in plan.Steps)
        {
            IAgent agent = step.Domain switch
            {
                AgentDomain.Communication => communicationAgent,
                AgentDomain.Sales => salesAgent,
                AgentDomain.Technical => technicalAgent,
                AgentDomain.OutOfScope => outOfScopeAgent,
                _ => outOfScopeAgent
            };

            AgentResult result = await agent
                .ExecuteAsync(step.Question, cancellationToken)
                .ConfigureAwait(false);

            CaseFinding finding = new(
                step.StepId,
                step.Domain,
                result.Response,
                result.Steps);

            findings.Add(finding);
        }

        return new CaseContext(originalQuestion, plan, findings);
    }
}
