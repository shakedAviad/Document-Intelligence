using System.Reflection;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.Rag.Models;

namespace DocumentIntelligence.Core.Extractors;

public abstract class LlmDocumentExtractor<T>(IChatModel chatModel) where T : class
{
    public abstract string DocumentName { get; }

    public virtual async Task<IReadOnlyList<RagDocument<T>>> ExtractAsync(
        string content,
        CancellationToken ct = default)
    {
        PropertyInfo[] properties = typeof(T).GetProperties();
        List<string> schemaLines = [];

        foreach (PropertyInfo property in properties)
        {
            schemaLines.Add($"{property.Name}: {property.PropertyType.Name}");
        }

        string schema = string.Join('\n', schemaLines);

        string systemPrompt =
            $"""
            You are a document extraction assistant.
            Extract all {typeof(T).Name} records from the provided document.
            Return ONLY a valid JSON array matching the schema below.
            Normalize values: remove currency symbols, parse numbers cleanly.
            Return [] if no records found.

            Schema for {typeof(T).Name}:
            {schema}
            """;

        IReadOnlyList<ChatMessage> messages =
        [
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, content)
        ];

        List<T>? extracted = await chatModel.CompleteStructuredAsync<List<T>>(messages, ct);

        if (extracted is not { Count: > 0 })
        {
            return [];
        }

        List<RagDocument<T>> documents = [];

        for (int i = 0; i < extracted.Count; i++)
        {
            documents.Add(new RagDocument<T>(
                $"{DocumentName}-{i}",
                extracted[i],
                new RagMetadata(DocumentName, typeof(T).Name)));
        }

        return documents;
    }
}
