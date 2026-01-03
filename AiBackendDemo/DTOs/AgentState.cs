namespace AiBackendDemo.DTOs;
public sealed class AgentState
{
    public int RetrievedActionsCount { get; init; }
    public int OverdueActionsCount { get; init; }
    public bool HasSummary { get; init; }
    public bool HasSuggestions { get; init; }
}


public enum AgentAction
{
    SEARCH_ACTIONS,
    SUMMARIZE_ACTIONS,
    SUGGEST_RESOLUTION,
    STOP
}
    
public sealed class AgentDecision
{
    public AgentAction Action { get; init; }
}
