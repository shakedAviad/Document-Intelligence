using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Models;
using DocumentIntelligence.AgentFramework.Sessions;
using DocumentIntelligence.Core.Models;

namespace DocumentIntelligence.Core.Agents;

public sealed class MainOrchestratorAgent(
    PlannerAgent plannerAgent,
    ExecutorAgent executorAgent,
    ISessionStore sessionStore) : IAgent
{
    public string Name => "MainOrchestratorAgent";

    public async Task<AgentResult> ExecuteAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        AgentSession session = await sessionStore
            .GetAsync("MainOrchestratorAgent", cancellationToken)
            .ConfigureAwait(false)
            ?? new AgentSession { AgentName = "MainOrchestratorAgent" };

        string contextualInput = session.Messages.Count > 0
            ? $"Previous conversation:\n{string.Join('\n', session.Messages)}\n\nCurrent question: {input}"
            : input;

        const int maxIterations = 3;
        CasePlan? plan = null;
        CaseContext? context = null;
        CaseResult? result = null;

        plan = await plannerAgent
            .PlanAsync(contextualInput, cancellationToken)
            .ConfigureAwait(false);

        for (int i = 0; i < maxIterations; i++)
        {
            context = await executorAgent
                .ExecuteAsync(input, plan, cancellationToken)
                .ConfigureAwait(false);

            result = await plannerAgent
                .ReviewAsync(context, cancellationToken)
                .ConfigureAwait(false);

            if (result.IsComplete)
            {
                break;
            }

            if (result.AdditionalSteps is { Count: > 0 } additionalSteps)
            {
                IReadOnlyList<CaseStep> allSteps = [..plan.Steps, ..additionalSteps];
                plan = new CasePlan(plan.Goal, allSteps);
            }
        }

        string finalAnswer = result?.FinalAnswer ?? "Investigation could not be completed.";

        session.Messages.Add($"User: {input}");
        session.Messages.Add($"Assistant: {finalAnswer}");
        await sessionStore.SaveAsync(session, cancellationToken).ConfigureAwait(false);

        return new AgentResult(Name, finalAnswer, []);
    }
}
