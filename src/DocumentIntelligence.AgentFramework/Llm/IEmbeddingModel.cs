using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DocumentIntelligence.AgentFramework.Llm;

public interface IEmbeddingModel
{
    Task<IReadOnlyList<float>> EmbedAsync(string input, CancellationToken cancellationToken = default);
}
