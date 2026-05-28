using DocumentIntelligence.AgentFramework.Agents;
using DocumentIntelligence.AgentFramework.Llm;
using DocumentIntelligence.AgentFramework.Models;
// using DocumentIntelligence.AgentFramework.Reasoning;
using DocumentIntelligence.AgentFramework.Tools;
using FluentAssertions;

namespace DocumentIntelligence.AgentFramework.Tests.Agents;

public class BaseAgentPromptTests
{
    [Fact]
    public async Task BuildSystemPrompt_IncludesAgentInfo_Tools_And_FormatInstructions()
    {
        TestTool tool = new TestTool("FakeTool", "Does X");
        CapturingChatModel chat = new CapturingChatModel("Final");

        PromptAgent agent = new PromptAgent(chat, new List<ITool> { tool });

        AgentResult result = await agent.ExecuteAsync("input");

        // ensure chat captured the system prompt
        chat.CapturedSystemPrompt.Should().NotBeNullOrEmpty();
        chat.CapturedSystemPrompt.Should().Contain("Agent: PromptAgent");
        chat.CapturedSystemPrompt.Should().Contain("Custom instructions");
        chat.CapturedSystemPrompt.Should().Contain("FakeTool");
        chat.CapturedSystemPrompt.Should().Contain("Does X");
        chat.CapturedSystemPrompt.Should().Contain("\"action\"");
        chat.CapturedSystemPrompt.Should().Contain("\"toolName\"");
    }

    private class PromptAgent : BaseAgent
    {
        private readonly IReadOnlyList<ITool> _tools;

        public PromptAgent(IChatModel chat, IReadOnlyList<ITool>? tools = null)
            : base(chat)
        {
            _tools = tools ?? new List<ITool>();
        }

        public override string Name => "PromptAgent";

        protected override string Instructions => "Custom instructions";

        protected override IReadOnlyList<ITool> Tools => _tools;
    }

    private class CapturingChatModel : IChatModel
    {
        private readonly string _final;

        public string CapturedSystemPrompt { get; private set; } = string.Empty;

        public CapturingChatModel(string final)
        {
            _final = final;
        }

        public Task<ChatModelResponse> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
            CapturedSystemPrompt = messages[0].Content;
            string json = $"{{\"Action\":\"FinalAnswer\",\"FinalAnswer\":\"{_final}\"}}";
            return Task.FromResult(new ChatModelResponse(json));
        }

        public Task<TResponse?> CompleteStructuredAsync<TResponse>(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
            where TResponse : class
        {
            CapturedSystemPrompt = messages[0].Content;
            AgentDecision decision = new DocumentIntelligence.AgentFramework.Models.AgentDecision(DocumentIntelligence.AgentFramework.Models.AgentAction.FinalAnswer, null, null, _final);
            return Task.FromResult(decision as TResponse);
        }
    }

    private class TestTool : ITool
    {
        public TestTool(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }

        public string Description { get; }

        public Task<ToolResult> ExecuteAsync(string input, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ToolResult(Name, "OK"));
        }
    }
}
