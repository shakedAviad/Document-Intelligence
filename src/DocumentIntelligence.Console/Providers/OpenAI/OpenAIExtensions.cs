using DocumentIntelligence.AgentFramework.Llm;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

namespace DocumentIntelligence.Console;

internal static class OpenAIExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddOpenAI(IConfiguration configuration)
        {
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                ?? throw new InvalidOperationException(
                    "OPENAI_API_KEY environment variable is not set.");

            string chatModel = configuration["OpenAI:ChatModel"]
                ?? throw new InvalidOperationException("OpenAI:ChatModel is not configured.");

            string embeddingModel = configuration["OpenAI:EmbeddingModel"]
                ?? throw new InvalidOperationException("OpenAI:EmbeddingModel is not configured.");

            OpenAIClient openAiClient = new(apiKey);

            services.AddSingleton<IChatModel>(
                new OpenAIChatModel(openAiClient.GetChatClient(chatModel)));

            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
                new OpenAIEmbeddingGeneratorAdapter(openAiClient.GetEmbeddingClient(embeddingModel)));

            return services;
        }
    }
}
