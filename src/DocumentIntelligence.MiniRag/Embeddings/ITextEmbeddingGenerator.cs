namespace DocumentIntelligence.MiniRag.Embeddings;

public interface ITextEmbeddingGenerator
{
    Task<IReadOnlyList<float>> GenerateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default);
}
