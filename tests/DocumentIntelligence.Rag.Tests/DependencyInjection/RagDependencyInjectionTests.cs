using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using DocumentIntelligence.Rag.DependencyInjection;
using DocumentIntelligence.Rag.Indexing;
using DocumentIntelligence.Rag.Mapping;
using DocumentIntelligence.Rag.Search;
using DocumentIntelligence.Rag.Text;
using FluentAssertions;

namespace DocumentIntelligence.Rag.Tests.DependencyInjection;

public class RagDependencyInjectionTests
{
    [Fact]
    public void AddDocumentIntelligenceRag_Should_Register_VectorStore()
    {
        ServiceCollection services = [];
        services.AddDocumentIntelligenceRag();

        ServiceProvider provider = services.BuildServiceProvider();
        VectorStore? store = provider.GetService<VectorStore>();

        store.Should().NotBeNull();
        store.Should().BeAssignableTo<VectorStore>();
    }

    [Fact]
    public void AddDocumentIntelligenceRag_Should_Register_TextBuilder_And_Mapper()
    {
        ServiceCollection services = [];
        services.AddDocumentIntelligenceRag();

        ServiceProvider provider = services.BuildServiceProvider();

        IRagTextBuilder? textBuilder = provider.GetService<IRagTextBuilder>();
        IRagDocumentMapper? mapper = provider.GetService<IRagDocumentMapper>();

        textBuilder.Should().BeOfType<ReflectionRagTextBuilder>();
        mapper.Should().BeOfType<RagDocumentMapper>();
    }

    [Fact]
    public void AddDocumentIntelligenceRag_Should_Register_Indexing_And_Search_Services_When_EmbeddingGenerator_Is_Registered()
    {
        ServiceCollection services = [];
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, FakeEmbeddingGenerator>();
        services.AddDocumentIntelligenceRag();

        ServiceProvider provider = services.BuildServiceProvider();

        IRagIndexingService? indexingService = provider.GetService<IRagIndexingService>();
        IRagSearchService? searchService = provider.GetService<IRagSearchService>();

        indexingService.Should().NotBeNull();
        searchService.Should().NotBeNull();
    }

    private sealed class FakeEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
    {
        public EmbeddingGeneratorMetadata Metadata { get; } = new EmbeddingGeneratorMetadata();

        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            float[] vector = new float[1536];
            vector[0] = 1f;
            return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>([new Embedding<float>(vector)]));
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }
}
