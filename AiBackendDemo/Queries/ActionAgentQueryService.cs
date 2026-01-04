using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AiBackendDemo.DTOs;
using AiBackendDemo.Models;
using AiBackendDemo.Clients;
using AiBackendDemo.Services;
using AiBackendDemo.Repositories;
using System.Text;
using AiBackendDemo.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AiBackendDemo.Queries
{
    // This class will handle agent and OpenAI logic, and call only DB methods from the repository
    public class ActionAgentQueryService : IActionAgentQueryService
    {
        private readonly IOpenAiClient _openAiClient;
        private readonly IAgentPlanner _agentPlanner;
        private readonly IActionsRepository _actionsRepository;
        private readonly IAgentToolExecutor _toolExecutor;
        private readonly AiBackendDbContext _aiBackendDbContext;

        public ActionAgentQueryService(IOpenAiClient openAiClient, IAgentPlanner agentPlanner, IActionsRepository actionsRepository, IAgentToolExecutor toolExecutor, AiBackendDbContext aiBackendDbContext)
        {
            _openAiClient = openAiClient;
            _agentPlanner = agentPlanner;
            _actionsRepository = actionsRepository;
            _toolExecutor = toolExecutor;
            _aiBackendDbContext = aiBackendDbContext;
        }

        // Example: Semantic search using OpenAI, then fetch from DB
        public async Task<ActionEmbedding> SemanticSearchAsync(string searchText)
        {
            var queryEmbedding = await _openAiClient.CreateEmbeddingAsync(searchText, CancellationToken.None);
            var results = await _actionsRepository.GetAllActionsAsync();
            var searchResult = results.Select(a => new { a.Id, a.Title, Score = CosineSimilarity(queryEmbedding, a.Embedding!) })
               .OrderByDescending(x => x.Score).MaxBy(x => x.Score);
            return new ActionEmbedding
            {
                ActionId = searchResult!.Id,
                Title = results.First(a => a.Id == searchResult.Id).Title,
                Description = results.First(a => a.Id == searchResult.Id).Description ?? string.Empty
            };
        }

        public async Task<string> GetActionSummaryAsync(int actionId)
        {
            var action = await _actionsRepository.GetActionByIdAsync(actionId);
            if (action == null)
                throw new ArgumentException($"Action with ID {actionId} not found.");

            var prompt = BuildActionSummaryPrompt(
                action.Title,
                action.Description ?? "No description provided.",
                action.ActionStatus ?? "No status provided.");

            return await _openAiClient.SummarizeAsync(prompt, CancellationToken.None);
        }

        // Example: Use agent planner to decide, then fetch from DB
        public async Task<Models.Action?> GetActionByIdAgentAsync(int actionId)
        {
            AgentState state = new AgentState();
            const int MAX_STEPS = 5;
            for (int step = 0; step < MAX_STEPS; step++)
            {
                var decision = await _agentPlanner.DecideAsync(state, CancellationToken.None);
                if (decision.Action == AgentAction.STOP)
                {
                    return (await _actionsRepository.GetAllActionsAsync()).FirstOrDefault(a => a.Id == actionId);
                }
                // Add logic to update state as needed
            }
            return null;
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
            var actions = await _actionsRepository.GetAllOpenActionsAsync();
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

        public async Task ExecuteAgentAsync()
        {
            AgentState state = new AgentState();
            const int MAX_STEPS = 5;

            for (int step = 0; step < MAX_STEPS; step++)
            {
                var decision = await _agentPlanner.DecideAsync(state, CancellationToken.None);

                if (decision.Action == AgentAction.STOP)
                    break;

                var result = await _toolExecutor.ExecuteAsync(decision.Action, state, CancellationToken.None);

                state = new AgentState
                {
                    RetrievedActionsCount = result.RetrievedActionsCount,
                    OverdueActionsCount = result.OverdueActionsCount,
                    HasSummary = result.HasSummary,
                    HasSuggestions = result.HasSuggestions
                };
            }
        }
        public async Task<Models.Action?> GetActionByIdAsync()
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

        public async Task SaveMemoryAsync(string title, string summary, string resolution)
        {
            var text = $"{title}. {summary}. {resolution}";

            var embedding = await _openAiClient.CreateEmbeddingAsync(text, CancellationToken.None);

            var memory = new ActionMemory
            {
                ActionTitle = title,
                ProblemSummary = summary,
                Resolution = resolution,
                Embedding = embedding
            };

            await _actionsRepository.AddActionMemoryAsync(memory);
        }

        public async Task<List<ActionMemory>> SearchMemoryAsync(string query)
        {
            var queryEmbedding = await _openAiClient.CreateEmbeddingAsync(query, CancellationToken.None);

            // Use CosineSimilarity extension for float[]
            return await _aiBackendDbContext.ActionMemories
                .OrderByDescending(m => m.Embedding != null ? m.Embedding.CosineSimilarity(queryEmbedding) : double.NegativeInfinity)
                .Take(3)
                .ToListAsync();
        }

    }
}
