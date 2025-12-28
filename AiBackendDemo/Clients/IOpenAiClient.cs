public interface IOpenAiClient
{
    Task<string> CompleteAsync(string prompt, double temperature, CancellationToken ct);
}