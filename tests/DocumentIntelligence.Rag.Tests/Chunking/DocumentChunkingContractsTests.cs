using DocumentIntelligence.Rag.Chunking;
using DocumentIntelligence.Rag.Models;
using FluentAssertions;

namespace DocumentIntelligence.Rag.Tests.Chunking;

public class DocumentChunkingContractsTests
{
    [Fact]
    public void DocumentChunkingRequest_Should_Create_Successfully()
    {
        var request = new DocumentChunkingRequest("meetings.md", "meeting", "content");

        request.SourceFileName.Should().Be("meetings.md");
        request.DocumentType.Should().Be("meeting");
        request.Content.Should().Be("content");
    }

    [Fact]
    public void IDocumentChunker_Should_Return_Chunks()
    {
        var request = new DocumentChunkingRequest("meetings.md", "meeting", "content");
        IDocumentChunker chunker = new FakeDocumentChunker();

        IReadOnlyList<DocumentChunk> result = chunker.Chunk(request);

        result.Should().HaveCount(1);
        result[0].Content.Should().Be(request.Content);
        result[0].Metadata["SourceFileName"].Should().Be(request.SourceFileName);
        result[0].Metadata["DocumentType"].Should().Be(request.DocumentType);
    }

    private sealed class FakeDocumentChunker : IDocumentChunker
    {
        public IReadOnlyList<DocumentChunk> Chunk(DocumentChunkingRequest request)
        {
            return
            [
                new DocumentChunk("chunk-1", request.Content, new Dictionary<string, string>
                {
                    ["SourceFileName"] = request.SourceFileName,
                    ["DocumentType"] = request.DocumentType,
                    ["SectionName"] = null!
                })
            ];
        }
    }
}
