using DocumentIntelligence.AgentFramework.DependencyInjection;
using DocumentIntelligence.Core.DependencyInjection;
using DocumentIntelligence.Rag.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DocumentIntelligence.Console;

public static class ConsoleExtensions
{
    extension(HostApplicationBuilder builder)
    {
        public HostApplicationBuilder ConfigureServices()
        {
            builder.Services.AddConsoleServices(builder.Configuration);
            return builder;
        }

        public IHost BuildApplication() => builder.Build();
    }

    extension(IServiceCollection services)
    {
        internal IServiceCollection AddConsoleServices(IConfiguration configuration)
        {
            string documentsPath = configuration["DocumentsPath"]
                ?? throw new InvalidOperationException("DocumentsPath is not configured.");

            string resolvedPath = Path.Combine(AppContext.BaseDirectory, documentsPath);

            services.AddOpenAI(configuration);
            services.AddAgentFramework();
            services.AddDocumentIntelligenceRag();
            services.AddDocumentIntelligenceCore(resolvedPath);

            services.AddSingleton(new DocumentsPath(resolvedPath));
            services.AddSingleton<DocumentIndexingService>();
            services.AddHostedService<DocumentIntelligenceApp>();

            return services;
        }
    }
}
