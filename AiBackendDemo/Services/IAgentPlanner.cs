
using AiBackendDemo.DTOs;

namespace AiBackendDemo.Services;

public interface IAgentPlanner
{
    Task<AgentDecision> DecideAsync(AgentState state, CancellationToken ct);
}