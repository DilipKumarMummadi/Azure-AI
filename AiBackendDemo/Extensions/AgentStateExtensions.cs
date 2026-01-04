using AiBackendDemo.DTOs;

namespace AiBackendDemo.Extensions;

public static class AgentStateExtensions
{
    public static ToolResult ToToolResult(this AgentState state)
        => new()
        {
            RetrievedActionsCount = state.RetrievedActionsCount,
            OverdueActionsCount = state.OverdueActionsCount,
            HasSummary = state.HasSummary,
            HasSuggestions = state.HasSuggestions
        };
}
