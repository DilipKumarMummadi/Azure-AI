namespace AiBackendDemo.Queries;

public interface IMultiModelService
{
    Task<string> AnalyzeScreenshotAsync(byte[] imageBytes, CancellationToken ct);
    Task<string> ExtractTextFromImageAsync(byte[] imageBytes, CancellationToken ct);
    //Image → Reasoning
    Task<string> AnalyzeImageForIssueAsync(byte[] imageBytes, CancellationToken ct);
    // Audio Transcription (Azure OpenAI / Whisper-style)
    Task<string> TranscribeAudioAsync(Stream audioStream, string fileName, CancellationToken ct);
}