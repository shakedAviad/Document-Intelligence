using System;
using System.Text.Json;
using DocumentIntelligence.AgentFramework.Models;

namespace DocumentIntelligence.AgentFramework.Reasoning;

public sealed class JsonAgentDecisionParser : IAgentDecisionParser
{
    public AgentDecision Parse(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Content is empty");
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var dto = JsonSerializer.Deserialize<DecisionDto>(content, options);

            if (dto is null)
            {
                throw new InvalidOperationException("Failed to parse decision");
            }

            if (string.IsNullOrWhiteSpace(dto.Action))
            {
                throw new InvalidOperationException("Action is required");
            }

            if (!Enum.TryParse<AgentAction>(dto.Action, true, out var action))
            {
                throw new InvalidOperationException($"Unknown action: {dto.Action}");
            }

            return new AgentDecision(action, dto.ToolName, dto.ToolInput, dto.FinalAnswer);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse decision", ex);
        }
    }

    private sealed class DecisionDto
    {
        public string? Action { get; set; }

        public string? ToolName { get; set; }

        public string? ToolInput { get; set; }

        public string? FinalAnswer { get; set; }
    }
}
