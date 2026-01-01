using System.Collections.Generic;
using System.Threading.Tasks;
using AiBackendDemo.Models;
using Action = AiBackendDemo.Models.Action;

namespace AiBackendDemo.Repositories
{
    public interface IActionsRepository
    {
        Task<IEnumerable<Action>> GetAllActionsAsync();
        Task<IEnumerable<Action>> SearchActionsAsync(string searchText);
        Task UpdateActionAsync(Action action);
        Task AddActionAsync(Action action);
    }
}
