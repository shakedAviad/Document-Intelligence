using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Models;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using AgentChatMessage = DocumentIntelligence.AgentFramework.Llm.ChatMessage;

namespace DocumentIntelligence.Console;

public sealed class OpenAIChatModel(ChatClient client) : IChatModel
{
    public async Task<ChatModelResponse> CompleteAsync(
        IReadOnlyList<AgentChatMessage> messages,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<OpenAI.Chat.ChatMessage> mapped = MapMessages(messages);

        ClientResult<ChatCompletion> result = await client
            .CompleteChatAsync(mapped, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return new ChatModelResponse(result.Value.Content[0].Text);
    }

    public async Task<TResponse?> CompleteStructuredAsync<TResponse>(
        IReadOnlyList<AgentChatMessage> messages,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        IReadOnlyList<OpenAI.Chat.ChatMessage> mapped = MapMessages(messages);

        ChatCompletionOptions options = new()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
        };

        ClientResult<ChatCompletion> result = await client
            .CompleteChatAsync(mapped, options, cancellationToken)
            .ConfigureAwait(false);

        string json = result.Value.Content[0].Text;
        return JsonSerializer.Deserialize<TResponse>(json);
    }

    private static IReadOnlyList<OpenAI.Chat.ChatMessage> MapMessages(
        IReadOnlyList<AgentChatMessage> messages)
    {
        List<OpenAI.Chat.ChatMessage> mapped = [];

        foreach (AgentChatMessage message in messages)
        {
            OpenAI.Chat.ChatMessage openAiMessage = message.Role switch
            {
                ChatRole.System    => new SystemChatMessage(message.Content),
                ChatRole.Assistant => new AssistantChatMessage(message.Content),
                _                  => new UserChatMessage(message.Content)
            };

            mapped.Add(openAiMessage);
        }

        return mapped;
    }
}
