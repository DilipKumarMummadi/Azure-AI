using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AiBackendDemo.Models;
using Action = AiBackendDemo.Models.Action;

namespace AiBackendDemo.Repositories
{
    public class ActionsRepository : IActionsRepository
    {
        private readonly AiBackendDbContext _context;
        public ActionsRepository(AiBackendDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Action>> GetAllActionsAsync()
        {
            return await _context.Actions.ToListAsync();
        }

        public async Task<IEnumerable<Action>> SearchActionsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await _context.Actions.ToListAsync();

            return await _context.Actions
                .Where(a => a.Title.Contains(searchText) || (a.Description != null && a.Description.Contains(searchText)))
                .ToListAsync();
        }
    }
}
