using System.Text.Json;
using AiBackendDemo.Clients;
using AiBackendDemo.DTOs;

namespace AiBackendDemo.Services;

public class AgentPlanner : IAgentPlanner
{
    private readonly IOpenAiClient _openAiClient;

    public AgentPlanner(IOpenAiClient openAiClient)
    {
        _openAiClient = openAiClient;
    }

    public async Task<AgentDecision> DecideAsync(AgentState state, CancellationToken ct)
    {
        var prompt = BuildPlannerPrompt(state);

        var response = await _openAiClient
            .CompleteAsync(prompt, ct);
        var decision = JsonSerializer.Deserialize<AgentDecision>(response);

        if (decision == null)
            throw new InvalidOperationException("Invalid agent decision");

        return decision;
    }
    private static string BuildPlannerPrompt(AgentState state)
    {
        return $$"""
You are an AI agent planner.

Rules:
- Choose ONLY one action
- Use ONLY the allowed actions
- Return ONLY valid JSON
- Do NOT explain your choice

Current state:
- RetrievedActionsCount: {state.RetrievedActionsCount}
- OverdueActionsCount: {state.OverdueActionsCount}
- HasSummary: {state.HasSummary}
- HasSuggestions: {state.HasSuggestions}

Allowed actions:
- SEARCH_ACTIONS
- SUMMARIZE_ACTIONS
- SUGGEST_RESOLUTION
- STOP

Decision format:
{{"action"}}
""";
    }

}
