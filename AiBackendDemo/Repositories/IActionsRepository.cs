using System.Collections.Generic;
using System.Threading.Tasks;
using AiBackendDemo.DTOs;
using AiBackendDemo.Models;
using Action = AiBackendDemo.Models.Action;

namespace AiBackendDemo.Repositories
{
    public interface IActionsRepository
    {
        Task<IEnumerable<Action>> GetAllActionsAsync();
        Task<Action> GetActionByIdAsync(int actionId);
        Task UpdateActionAsync(Action action);
        Task AddActionAsync(Action action);
        Task<IEnumerable<Action>> GetAllOpenActionsAsync();
        Task AddActionMemoryAsync(ActionMemory memory);
    }
}
