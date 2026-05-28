namespace DocumentIntelligence.AgentFramework.Llm;

public interface IEmbeddingModel
{
    Task<IReadOnlyList<float>> EmbedAsync(string input, CancellationToken cancellationToken = default);
}
