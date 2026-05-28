using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DocumentIntelligence.AgentFramework.Llm;

public interface IChatModel
{
    Task<ChatModelResponse> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default);
}
