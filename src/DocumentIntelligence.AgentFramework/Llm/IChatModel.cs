namespace DocumentIntelligence.AgentFramework.Llm;

public interface IChatModel
{
    Task<ChatModelResponse> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default);

    Task<TResponse?> CompleteStructuredAsync<TResponse>(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
        where TResponse : class;
}
