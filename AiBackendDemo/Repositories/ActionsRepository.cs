using Microsoft.EntityFrameworkCore;
using AiBackendDemo.DTOs;
using Action = AiBackendDemo.Models.Action;
using AiBackendDemo.Clients;
using System.Text;
using AiBackendDemo.Services;

namespace AiBackendDemo.Repositories
{
    public class ActionsRepository : IActionsRepository
    {
        private readonly AiBackendDbContext _context;
        private readonly IOpenAiClient _openAiClient;
        private readonly IAgentPlanner _agentPlanner;
        public ActionsRepository(AiBackendDbContext context, IOpenAiClient openAiClient, IAgentPlanner agentPlanner)
        {
            _context = context;
            _openAiClient = openAiClient;
            _agentPlanner = agentPlanner;
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

        public Task<string> GetActionSummaryAsync(int actionId)
        {
            var action = _context.Actions.FirstOrDefault(a => a.Id == actionId);
            if (action == null)
                throw new ArgumentException($"Action with ID {actionId} not found.");

            var prompt = BuildActionSummaryPrompt(
                action.Title,
                action.Description ?? "No description provided.",
                action.ActionStatus ?? "No status provided.");

            return _openAiClient.SummarizeAsync(prompt, CancellationToken.None);
        }

        private static string BuildActionSummaryPrompt(string title, string description, string status)
        {
            return $"""
You are a backend summarization engine.

Task:
- First, provide a concise factual summary of the action.
- Then, provide up to 3 practical suggestions.

Rules:
- Base the summary only on the given details.
- Suggestions can be general best practices.
- Do not invent facts.
- Keep everything concise.

Action details:
Title: {title}
Description: {description}
Status: {status}

Output format:
Summary:
<summary text>

Suggestions:
- <suggestion 1>
- <suggestion 2>
- <suggestion 3>
""";
        }

        public async Task<string> RagSearchAsync(string searchText)
        {
            var actions = await GetAllOpenActionsAsync();
            var questionEmbedding = await _openAiClient.CreateEmbeddingAsync(searchText, CancellationToken.None);
            var topActions = actions.Select(a => new { a.Title, a.Description, Score = CosineSimilarity(questionEmbedding, a.Embedding!) }).Where(x => x.Score >= 0.65).OrderByDescending(x => x.Score).Take(3).ToList();
            if (!topActions.Any())
            {
                return "- No relevant open actions found for the given question.";
            }
            var promot = BuildRagPrompt(
                searchText,
                BuildContext(topActions));
            return await _openAiClient.SummarizeAsync(promot, CancellationToken.None);
        }

        private static string BuildContext(IEnumerable<dynamic> actions)
        {
            var sb = new StringBuilder();

            int i = 1;
            foreach (var a in actions)
            {
                sb.AppendLine($"{i}. {a.Title}: {a.Description}");
                i++;
            }

            return sb.ToString();
        }

        private static string BuildRagPrompt(string question, string context)
        {
            return $"""
You are an assistant answering questions using ONLY the provided context.

Rules:
- Use only the context below.
- Do not add information that is not present.
- If the context does not contain the answer, say "No relevant open actions found."
- Respond in clear bullet points.

Context:
{context}

Question:
{question}

Answer:
""";
        }

        public async Task<Action?> GetActionByIdAsync()
        {
            // You may want to pass an actionId parameter to this method for real use
            AgentState state = new AgentState();
            const int MAX_STEPS = 5;

            for (int step = 0; step < MAX_STEPS; step++)
            {
                var decision = await _agentPlanner.DecideAsync(state, CancellationToken.None);

                switch (decision.Action)
                {
                    case AgentAction.SEARCH_ACTIONS:
                        // Example: update state based on search, but do not return a new Action
                        // state = await SearchActionsAsync("Test");
                        break;

                    case AgentAction.SUMMARIZE_ACTIONS:
                        // Example: update state based on summary, but do not return a new Action
                        // state = await GetActionSummaryAsync(state);
                        break;

                    case AgentAction.SUGGEST_RESOLUTION:
                        // Example: update state based on RAG, but do not return a new Action
                        // state = await RagSearchAsync(state);
                        break;

                    case AgentAction.STOP:
                        // Try to get the action from the database if you have an ID in state
                        // if (state.ActionId != null)
                        // {
                        //     return await _context.Actions.FindAsync(state.ActionId);
                        // }
                        return null;

                    default:
                        throw new InvalidOperationException("Unknown agent action");
                }
            }
            return null;
        }

    }
}
