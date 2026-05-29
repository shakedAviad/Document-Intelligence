using DocumentIntelligence.MiniRag.Embeddings;
using FluentAssertions;

namespace DocumentIntelligence.MiniRag.Tests.Embeddings;

public class TextEmbeddingGeneratorContractTests
{
    [Fact]
    public async Task ITextEmbeddingGenerator_Should_Return_Embedding()
    {
        ITextEmbeddingGenerator generator = new FakeTextEmbeddingGenerator();

        IReadOnlyList<float> result = await generator.GenerateEmbeddingAsync("hello");

        result.Should().BeEquivalentTo([1f, 2f, 3f]);
    }

    private sealed class FakeTextEmbeddingGenerator : ITextEmbeddingGenerator
    {
        public Task<IReadOnlyList<float>> GenerateEmbeddingAsync(
            string input,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<float> vector = [1f, 2f, 3f];
            return Task.FromResult(vector);
        }
    }
}
