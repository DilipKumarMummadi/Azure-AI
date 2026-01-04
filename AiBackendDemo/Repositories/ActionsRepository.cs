using Microsoft.EntityFrameworkCore;
using AiBackendDemo.DTOs;
using Action = AiBackendDemo.Models.Action;
using AiBackendDemo.Clients;
using System.Text;
using AiBackendDemo.Services;
using AiBackendDemo.Models;

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

        public async Task<IEnumerable<Action>> GetAllOpenActionsAsync()
        {
            return await _context.Actions.Where(a => a.ActionStatus == "Open").ToListAsync();
        }

        public async Task<IEnumerable<Action>> SearchActionsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await _context.Actions.ToListAsync();

            return await _context.Actions
                .Where(a => a.Title.Contains(searchText) || (a.Description != null && a.Description.Contains(searchText)))
                .ToListAsync();
        }

        public async Task UpdateActionAsync(Action action)
        {
            _context.Actions.Update(action);
            await SaveChangesAsync();
        }

        public async Task AddActionAsync(Action action)
        {
            await _context.Actions.AddAsync(action);
            await SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<Action> GetActionByIdAsync(int actionId)
        {
            var action = await _context.Actions.FirstOrDefaultAsync(a => a.Id == actionId);
            if (action == null)
                throw new ArgumentException($"Action with ID {actionId} not found.");
            return action;
        }
        public async Task AddActionMemoryAsync(ActionMemory memory)
        {
            await _context.ActionMemories.AddAsync(memory);
            await SaveChangesAsync();
        }
    }
}
