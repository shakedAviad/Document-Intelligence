using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Sessions;
using DocumentIntelligence.AgentFramework.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentIntelligence.AgentFramework.DependencyInjection;

public static class AgentFrameworkExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAgentFramework()
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddSingleton<ISessionStore, InMemorySessionStore>();

            return services;
        }

        public IServiceCollection AddAgent<TAgent>()
            where TAgent : class, IAgent
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddTransient<TAgent>();
            services.AddTransient<IAgent, TAgent>(sp => sp.GetRequiredService<TAgent>());

            return services;
        }

        public IServiceCollection AddTool<TTool>()
            where TTool : class, ITool
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddTransient<TTool>();
            services.AddTransient<ITool, TTool>(sp => sp.GetRequiredService<TTool>());

            return services;
        }
    }


}
