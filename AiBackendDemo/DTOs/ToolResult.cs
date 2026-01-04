namespace AiBackendDemo.DTOs;

public class ToolResult
{
    public int RetrievedActionsCount { get; init; }
    public int OverdueActionsCount { get; init; }
    public bool HasSummary { get; init; }
    public bool HasSuggestions { get; init; }
}
