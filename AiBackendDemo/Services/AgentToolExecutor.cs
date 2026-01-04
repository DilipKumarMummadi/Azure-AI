using AiBackendDemo.DTOs;
using AiBackendDemo.Extensions;

namespace AiBackendDemo.Services;

public sealed class AgentToolExecutor : IAgentToolExecutor
{
    public async Task<ToolResult> ExecuteAsync(AgentAction action, AgentState state, CancellationToken ct)
    {
        return action switch
        {
            AgentAction.SEARCH_ACTIONS => await SearchActionsAsync(ct),
            AgentAction.SUMMARIZE_ACTIONS => await SummarizeActionsAsync(state, ct),
            AgentAction.SUGGEST_RESOLUTION => await SuggestResolutionAsync(state, ct),
            AgentAction.STOP => state.ToToolResult(),
            _ => throw new InvalidOperationException("Unsupported agent action")
        };
    }

    private async Task<ToolResult> SearchActionsAsync(CancellationToken ct)
    {
        // Call your semantic search / DB logic here
        var retrieved = 5;
        var overdue = 2;

        return new ToolResult
        {
            RetrievedActionsCount = retrieved,
            OverdueActionsCount = overdue,
            HasSummary = false,
            HasSuggestions = false
        };
    }

    private async Task<ToolResult> SummarizeActionsAsync(
    AgentState state,
    CancellationToken ct)
    {
        if (state.RetrievedActionsCount == 0)
            return state.ToToolResult();

        // Call your summarization AI here

        return new ToolResult
        {
            RetrievedActionsCount = state.RetrievedActionsCount,
            OverdueActionsCount = state.OverdueActionsCount,
            HasSummary = true,
            HasSuggestions = state.HasSuggestions
        };
    }

    private async Task<ToolResult> SuggestResolutionAsync(
        AgentState state,
        CancellationToken ct)
    {
        if (!state.HasSummary)
            return state.ToToolResult();

        // Call suggestion AI here

        return new ToolResult
        {
            RetrievedActionsCount = state.RetrievedActionsCount,
            OverdueActionsCount = state.OverdueActionsCount,
            HasSummary = true,
            HasSuggestions = true
        };
    }
}

