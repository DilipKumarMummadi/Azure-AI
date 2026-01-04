using AiBackendDemo.DTOs;

namespace AiBackendDemo.Queries;

public interface IActionAgentQueryService
{
    Task<ActionEmbedding> SemanticSearchAsync(string searchText);
    Task<string> GetActionSummaryAsync(int actionId);
    Task<string> RagSearchAsync(string searchText);
    Task SaveMemoryAsync(string title, string summary,string resolution);
}
