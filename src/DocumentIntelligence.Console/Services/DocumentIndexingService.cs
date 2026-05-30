using DocumentIntelligence.Core.Extractors;
using DocumentIntelligence.Rag.Indexing;
using DocumentIntelligence.Rag.Models;

namespace DocumentIntelligence.Console;

internal sealed record DocumentsPath(string Value);

internal sealed class DocumentIndexingService(
    IRagIndexingService indexing,
    MeetingExtractor meetingExtractor,
    EmailExtractor emailExtractor,
    SalesExtractor salesExtractor,
    LogExtractor logExtractor,
    ConfigExtractor configExtractor,
    DocumentsPath documentsPath)
{
    public async Task IndexAllAsync(
        Action<string> onFileIndexed,
        CancellationToken cancellationToken)
    {
        await IndexAsync(meetingExtractor, "meetings.md", cancellationToken).ConfigureAwait(false);
        onFileIndexed("meetings.md");

        await IndexAsync(emailExtractor, "emails.txt", cancellationToken).ConfigureAwait(false);
        onFileIndexed("emails.txt");

        await IndexAsync(salesExtractor, "sales-q1.csv", cancellationToken).ConfigureAwait(false);
        onFileIndexed("sales-q1.csv");

        await IndexAsync(logExtractor, "server-log.txt", cancellationToken).ConfigureAwait(false);
        onFileIndexed("server-log.txt");

        await IndexAsync(configExtractor, "config.json", cancellationToken).ConfigureAwait(false);
        onFileIndexed("config.json");
    }

    private async Task IndexAsync<T>(
        LlmDocumentExtractor<T> extractor,
        string fileName,
        CancellationToken cancellationToken)
        where T : class
    {
        string filePath = Path.Combine(documentsPath.Value, fileName);
        string content = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
        IReadOnlyList<RagDocument<T>> documents = await extractor.ExtractAsync(content, cancellationToken).ConfigureAwait(false);
        await indexing.IndexAsync(documents, cancellationToken).ConfigureAwait(false);
    }
}
