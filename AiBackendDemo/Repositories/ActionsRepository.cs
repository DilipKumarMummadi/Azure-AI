using Microsoft.EntityFrameworkCore;
using AiBackendDemo.DTOs;
using Action = AiBackendDemo.Models.Action;
using AiBackendDemo.Clients;

namespace AiBackendDemo.Repositories
{
    public class ActionsRepository : IActionsRepository
    {
        private readonly AiBackendDbContext _context;
        private readonly IOpenAiClient _openAiClient;
        public ActionsRepository(AiBackendDbContext context, IOpenAiClient openAiClient)
        {
            _context = context;
            _openAiClient = openAiClient;
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

        public static double CosineSimilarity(float[] v1, float[] v2)
        {
            double dot = 0.0;
            double mag1 = 0.0;
            double mag2 = 0.0;

            for (int i = 0; i < v1.Length; i++)
            {
                dot += v1[i] * v2[i];
                mag1 += v1[i] * v1[i];
                mag2 += v2[i] * v2[i];
            }

            return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
        }

        public async Task<ActionEmbedding> SemanticSearchAsync(string searchText)
        {
            var queryEmbedding = await _openAiClient.CreateEmbeddingAsync(searchText, CancellationToken.None);
            var results = await GetAllActionsAsync();
            var searchResult = results.Select(a => new { a.Id, a.Title, Score = CosineSimilarity(queryEmbedding, a.Embedding!) })
               .OrderByDescending(x => x.Score).Max();
            return new ActionEmbedding
            {
                ActionId = searchResult!.Id,
                Title = results.First(a => a.Id == searchResult.Id).Title,
                Description = results.First(a => a.Id == searchResult.Id).Description ?? string.Empty
            };
        }
    }
}
