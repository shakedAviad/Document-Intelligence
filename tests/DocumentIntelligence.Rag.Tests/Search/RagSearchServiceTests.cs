using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using DocumentIntelligence.Rag.Indexing;
using DocumentIntelligence.Rag.Mapping;
using DocumentIntelligence.Rag.Models;
using DocumentIntelligence.Rag.Search;
using DocumentIntelligence.Rag.Text;
using FluentAssertions;

namespace DocumentIntelligence.Rag.Tests.Search;

public class RagSearchServiceTests
{
    private static (RagIndexingService indexing, RagSearchService search, InMemoryVectorStore store) CreateServices()
    {
        var store = new InMemoryVectorStore();
        var generator = new FakeEmbeddingGenerator();
        var mapper = new RagDocumentMapper();

        var indexing = new RagIndexingService(new ReflectionRagTextBuilder(), mapper, generator, store);
        var search = new RagSearchService(generator, store, mapper);

        return (indexing, search, store);
    }

    [Fact]
    public async Task SearchAsync_Should_Return_Relevant_Result()
    {
        var (indexing, search, _) = CreateServices();

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
        var (_, search, _) = CreateServices();

        IReadOnlyList<RagSearchResult> results = await search.SearchAsync("   ");

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_Should_Return_Empty_When_Top_Is_Zero_Or_Negative()
    {
        var (_, search, _) = CreateServices();

        IReadOnlyList<RagSearchResult> zeroResults = await search.SearchAsync("sales", top: 0);
        IReadOnlyList<RagSearchResult> negativeResults = await search.SearchAsync("sales", top: -1);

        zeroResults.Should().BeEmpty();
        negativeResults.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_Should_Return_Empty_When_Collection_Is_Empty()
    {
        var (_, search, _) = CreateServices();

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
            var results = new List<Embedding<float>>();

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
