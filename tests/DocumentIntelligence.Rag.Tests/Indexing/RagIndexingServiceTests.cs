using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using DocumentIntelligence.Rag.Indexing;
using DocumentIntelligence.Rag.Mapping;
using DocumentIntelligence.Rag.Models;
using DocumentIntelligence.Rag.Text;
using FluentAssertions;

namespace DocumentIntelligence.Rag.Tests.Indexing;

public class RagIndexingServiceTests
{
    private static RagIndexingService CreateService(VectorStore vectorStore) =>
        new(
            new ReflectionRagTextBuilder(),
            new RagDocumentMapper(),
            new FakeEmbeddingGenerator(),
            vectorStore);

    [Fact]
    public async Task IndexAsync_Should_Index_Generic_Rag_Documents()
    {
        var store = new InMemoryVectorStore();
        RagIndexingService service = CreateService(store);

        IReadOnlyList<RagDocument<Person>> documents =
        [
            new RagDocument<Person>("person-1", new Person(1, "Shaked"), new RagMetadata("people.json", "person")),
            new RagDocument<Person>("person-2", new Person(2, "Noa"), new RagMetadata("people.json", "person"))
        ];

        await service.IndexAsync(documents);

        VectorStoreCollection<string, RagVectorRecord> collection =
            store.GetCollection<string, RagVectorRecord>("rag_documents");

        RagVectorRecord? first = await collection.GetAsync("person-1");
        RagVectorRecord? second = await collection.GetAsync("person-2");

        first.Should().NotBeNull();
        first!.SearchableText.Should().Contain("Shaked");
        first.SourceName.Should().Be("people.json");
        first.DocumentType.Should().Be("person");

        second.Should().NotBeNull();
        second!.SearchableText.Should().Contain("Noa");
    }

    [Fact]
    public async Task IndexAsync_Should_Do_Nothing_When_Documents_Are_Empty()
    {
        var store = new InMemoryVectorStore();
        RagIndexingService service = CreateService(store);

        Func<Task> act = async () => await service.IndexAsync<Person>([]);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task IndexAsync_Should_Skip_Documents_With_Empty_Searchable_Text()
    {
        var store = new InMemoryVectorStore();
        RagIndexingService service = CreateService(store);

        IReadOnlyList<RagDocument<object?>> documents =
        [
            new RagDocument<object?>("null-doc", null, new RagMetadata("source.json", "unknown"))
        ];

        await service.IndexAsync(documents);

        VectorStoreCollection<string, RagVectorRecord> collection =
            store.GetCollection<string, RagVectorRecord>("rag_documents");

        RagVectorRecord? record = await collection.GetAsync("null-doc");
        record.Should().BeNull();
    }

    private sealed record Person(int Id, string Name);

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
