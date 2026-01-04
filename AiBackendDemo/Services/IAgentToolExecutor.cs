
using AiBackendDemo.DTOs;

namespace AiBackendDemo.Services;
public interface IAgentToolExecutor
{
    Task<ToolResult> ExecuteAsync(AgentAction action, AgentState state, CancellationToken ct);
}
