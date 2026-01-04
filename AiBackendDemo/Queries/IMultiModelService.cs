namespace AiBackendDemo.Queries;

public interface IMultiModelService
{
    Task<string> AnalyzeScreenshotAsync(byte[] imageBytes, CancellationToken ct);
}