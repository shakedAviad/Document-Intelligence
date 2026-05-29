using DocumentIntelligence.Rag.Models;

namespace DocumentIntelligence.Rag.Text;

public interface IRagTextBuilder
{
    string BuildText<T>(RagDocument<T> document);
}
