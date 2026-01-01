public interface IOpenAiClient
{
    Task<string> CompleteAsync(string prompt, double temperature, CancellationToken ct);
    Task<float[]> CreateEmbeddingAsync(string text, CancellationToken ct);
}