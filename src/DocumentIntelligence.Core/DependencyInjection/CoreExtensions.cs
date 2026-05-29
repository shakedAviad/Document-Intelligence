using DocumentIntelligence.Core.Agents;
using DocumentIntelligence.Core.Extractors;
using DocumentIntelligence.Core.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentIntelligence.Core.DependencyInjection;

public static class CoreExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDocumentIntelligenceCore(string documentsPath)
        {
            ArgumentNullException.ThrowIfNull(documentsPath);

            services.AddSingleton<ReadFileTool>(_ => new ReadFileTool(documentsPath));
            services.AddSingleton<ListFilesTool>(_ => new ListFilesTool(documentsPath));
            services.AddTransient<SearchDocumentsTool>();

            services.AddTransient<MeetingExtractor>();
            services.AddTransient<EmailExtractor>();
            services.AddTransient<SalesExtractor>();
            services.AddTransient<LogExtractor>();
            services.AddTransient<ConfigExtractor>();

            services.AddTransient<MeetingAgent>();
            services.AddTransient<EmailAgent>();
            services.AddTransient<LogAgent>();
            services.AddTransient<ConfigAgent>();
            services.AddTransient<SalesAgent>();
            services.AddTransient<OutOfScopeAgent>();

            services.AddTransient<CommunicationAgent>();
            services.AddTransient<TechnicalAgent>();

            services.AddTransient<PlannerAgent>();
            services.AddTransient<ExecutorAgent>(sp => new ExecutorAgent(
                sp.GetRequiredService<CommunicationAgent>(),
                sp.GetRequiredService<SalesAgent>(),
                sp.GetRequiredService<TechnicalAgent>(),
                sp.GetRequiredService<OutOfScopeAgent>()));
            services.AddTransient<MainOrchestratorAgent>();

            return services;
        }
    }
}
