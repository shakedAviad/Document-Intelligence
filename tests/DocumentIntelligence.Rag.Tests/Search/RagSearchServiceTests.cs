using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using DocumentIntelligence.Rag.DependencyInjection;
using DocumentIntelligence.Rag.Indexing;
using DocumentIntelligence.Rag.Models;
using DocumentIntelligence.Rag.Search;
using FluentAssertions;

namespace DocumentIntelligence.Rag.Tests.Search;

public class RagSearchServiceTests
{
    private static ServiceProvider CreateProvider()
    {
        ServiceCollection services = [];
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, FakeEmbeddingGenerator>();
        services.AddDocumentIntelligenceRag();
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task SearchAsync_Should_Return_Relevant_Result()
    {
        using ServiceProvider provider = CreateProvider();
        IRagIndexingService indexing = provider.GetRequiredService<IRagIndexingService>();
        IRagSearchService search = provider.GetRequiredService<IRagSearchService>();

        await indexing.IndexAsync<SalesInfo>(
        [
            new RagDocument<SalesInfo>(
                "sales-1",
                new SalesInfo("Q1 Report", "Q1 sales revenue summary"),
                new RagMetadata("reports.json", "sales"))
        ]);

        await indexing.IndexAsync<LogInfo>(
        [
            new RagDocument<LogInfo>(
                "log-1",
                new LogInfo("Server", "server error log entry"),
                new RagMetadata("logs.json", "log"))
        ]);

        IReadOnlyList<RagSearchResult> results = await search.SearchAsync("sales numbers", top: 1);

        results.Should().HaveCount(1);
        results[0].DocumentId.Should().Be("sales-1");
        results[0].Score.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task SearchAsync_Should_Return_Empty_For_Whitespace_Query()
    {
        using ServiceProvider provider = CreateProvider();
        IRagSearchService search = provider.GetRequiredService<IRagSearchService>();

        IReadOnlyList<RagSearchResult> results = await search.SearchAsync("   ");

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_Should_Return_Empty_When_Top_Is_Zero_Or_Negative()
    {
        using ServiceProvider provider = CreateProvider();
        IRagSearchService search = provider.GetRequiredService<IRagSearchService>();

        IReadOnlyList<RagSearchResult> zeroResults = await search.SearchAsync("sales", top: 0);
        IReadOnlyList<RagSearchResult> negativeResults = await search.SearchAsync("sales", top: -1);

        zeroResults.Should().BeEmpty();
        negativeResults.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_Should_Return_Empty_When_Collection_Is_Empty()
    {
        using ServiceProvider provider = CreateProvider();
        IRagSearchService search = provider.GetRequiredService<IRagSearchService>();

        IReadOnlyList<RagSearchResult> results = await search.SearchAsync("sales numbers", top: 5);

        results.Should().BeEmpty();
    }

    private sealed record SalesInfo(string Title, string Description);
    private sealed record LogInfo(string Title, string Message);

    private sealed class FakeEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
    {
        public EmbeddingGeneratorMetadata Metadata { get; } = new EmbeddingGeneratorMetadata();

        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            List<Embedding<float>> results = [];

            foreach (string value in values)
            {
                float[] vector = new float[1536];

                if (value.Contains("sales") || value.Contains("revenue"))
                {
                    vector[0] = 1f;
                }
                else if (value.Contains("log") || value.Contains("error"))
                {
                    vector[1] = 1f;
                }
                else
                {
                    vector[2] = 1f;
                }

                results.Add(new Embedding<float>(vector));
            }

            return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(results));
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }
}
