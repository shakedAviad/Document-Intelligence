using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using DocumentIntelligence.Rag.Indexing;
using DocumentIntelligence.Rag.Mapping;
using DocumentIntelligence.Rag.Search;
using DocumentIntelligence.Rag.Text;

namespace DocumentIntelligence.Rag.DependencyInjection;

public static class RagExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDocumentIntelligenceRag()
        {
            services.AddSingleton<VectorStore, InMemoryVectorStore>();
            services.AddSingleton<IRagTextBuilder, ReflectionRagTextBuilder>();
            services.AddSingleton<IRagDocumentMapper, RagDocumentMapper>();
            services.AddTransient<IRagIndexingService, RagIndexingService>();
            services.AddTransient<IRagSearchService, RagSearchService>();
            return services;
        }
    }
}
