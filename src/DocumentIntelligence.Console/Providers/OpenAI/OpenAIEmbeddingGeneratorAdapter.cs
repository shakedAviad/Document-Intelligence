using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI.Embeddings;
using MeaiEmbeddingOptions = Microsoft.Extensions.AI.EmbeddingGenerationOptions;

namespace DocumentIntelligence.Console;

public sealed class OpenAIEmbeddingGeneratorAdapter(EmbeddingClient client)
    : IEmbeddingGenerator<string, Embedding<float>>
{
    public EmbeddingGeneratorMetadata Metadata { get; } = new EmbeddingGeneratorMetadata();

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        MeaiEmbeddingOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        List<Embedding<float>> embeddings = [];

        foreach (string value in values)
        {
            ClientResult<OpenAIEmbedding> result = await client
                .GenerateEmbeddingAsync(value, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            embeddings.Add(new Embedding<float>(result.Value.ToFloats()));
        }

        return [.. embeddings];
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose() { }
}
