namespace AiBackendDemo.Clients;

public interface IOpenAiClient
{
    Task<string> CompleteAsync(string prompt, CancellationToken ct);
    Task<float[]> CreateEmbeddingAsync(string text, CancellationToken ct);
    Task<string> SummarizeAsync(string prompt, CancellationToken ct);
}