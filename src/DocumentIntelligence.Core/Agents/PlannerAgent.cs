using System.Text.Json;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Sessions;
using DocumentIntelligence.Core.Models;

namespace DocumentIntelligence.Core.Agents;

public sealed class PlannerAgent(IChatModel chatModel, ISessionStore sessionStore)
{
    private const string SessionKey = "PlannerAgent";

    private const string SystemPrompt =
        """
        You are an investigation planner for a document intelligence system.
        Available domains:
        - Communication: meetings (meetings.md) and email threads (emails.txt)
        - Sales: Q1 sales data (sales-q1.csv)
        - Technical: server logs (server-log.txt) and configuration (config.json)
        - OutOfScope: questions that cannot be answered from the available documents

        Given a user question, return an investigation plan as JSON.
        Plan must include a goal and a list of steps.
        Each step must specify: stepId (string), domain (Communication/Sales/Technical/OutOfScope),
        question (focused question for that domain).
        Keep the plan minimal — only include steps that are truly needed.
        """;

    public async Task<CasePlan> PlanAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        List<ChatMessage> messages = await LoadMessagesAsync(cancellationToken)
            .ConfigureAwait(false);

        if (messages.Count == 0)
        {
            messages.Add(new ChatMessage(ChatRole.System, SystemPrompt));
        }

        messages.Add(new ChatMessage(ChatRole.User, question));

        CasePlan? result = await chatModel
            .CompleteStructuredAsync<CasePlan>(messages, cancellationToken)
            .ConfigureAwait(false) ??
            throw new InvalidOperationException("PlannerAgent returned null plan");

        messages.Add(new ChatMessage(ChatRole.Assistant, JsonSerializer.Serialize(result)));

        await SaveMessagesAsync(messages, cancellationToken).ConfigureAwait(false);

        return result;
    }

    public async Task<CaseResult> ReviewAsync(
        CaseContext context,
        CancellationToken cancellationToken = default)
    {
        List<ChatMessage> messages = await LoadMessagesAsync(cancellationToken)
            .ConfigureAwait(false);

        string evaluationPrompt =
            "Findings collected:\n" + JsonSerializer.Serialize(context) +
            "\n\nBased on these findings, is the investigation complete?" +
            "\nIf yes: set IsComplete=true and provide the FinalAnswer." +
            "\nIf no: set IsComplete=false and provide AdditionalSteps.";

        messages.Add(new ChatMessage(ChatRole.User, evaluationPrompt));

        CaseResult? result = await chatModel
            .CompleteStructuredAsync<CaseResult>(messages, cancellationToken)
            .ConfigureAwait(false);

        if (result is null)
        {
            throw new InvalidOperationException("PlannerAgent returned null result");
        }

        messages.Add(new ChatMessage(ChatRole.Assistant, JsonSerializer.Serialize(result)));

        await SaveMessagesAsync(messages, cancellationToken).ConfigureAwait(false);

        return result;
    }

    private async Task<List<ChatMessage>> LoadMessagesAsync(CancellationToken ct)
    {
        AgentSession? session = await sessionStore
            .GetAsync(SessionKey, ct)
            .ConfigureAwait(false);

        if (session is null)
        {
            return [];
        }

        List<ChatMessage> messages = [];
        foreach (string raw in session.Messages)
        {
            if (JsonSerializer.Deserialize<ChatMessage>(raw) is { } msg)
            {
                messages.Add(msg);
            }
        }
        return messages;
    }

    private async Task SaveMessagesAsync(
        List<ChatMessage> messages,
        CancellationToken ct)
    {
        List<string> serialized = [];
        foreach (ChatMessage msg in messages)
        {
            serialized.Add(JsonSerializer.Serialize(msg));
        }

        AgentSession session = new()
        {
            AgentName = SessionKey,
            Messages = serialized
        };

        await sessionStore.SaveAsync(session, ct).ConfigureAwait(false);
    }
}
