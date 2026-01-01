namespace AiBackendDemo.Services;
public interface IAiTextClassifier
{
    Task<ClassificationResult> ClassifyAsync(
        string text,
        IReadOnlyCollection<string> allowedLabels,
        CancellationToken ct);
}
