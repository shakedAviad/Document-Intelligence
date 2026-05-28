namespace DocumentIntelligence.AgentFramework.Llm;

public sealed record ChatMessage(ChatRole Role, string Content);
