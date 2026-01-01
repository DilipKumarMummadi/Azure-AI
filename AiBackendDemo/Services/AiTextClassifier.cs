using System.Linq;
using System.Text.Json;
using AiBackendDemo.Clients;
using AiBackendDemo.Models;
using Microsoft.Extensions.Logging;

namespace AiBackendDemo.Services;

public sealed class AiTextClassifier : IAiTextClassifier
{
    private readonly IOpenAiClient _client;
    private readonly ILogger<AiTextClassifier> _logger;

    public AiTextClassifier(IOpenAiClient client, ILogger<AiTextClassifier> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<ClassificationResult> ClassifyAsync(
        string text,
        IReadOnlyCollection<string> allowedLabels,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Fallback();

        try
        {
            var prompt = BuildPrompt(text, allowedLabels);
            var raw = await _client.CompleteAsync(prompt, ct);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            if (TryParseClassification(raw, allowedLabels, options, out var result))
                return result!;

            return Fallback();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Classification failed");
            return Fallback();
        }
    }

    private static bool TryParseClassification(string raw, IReadOnlyCollection<string> allowedLabels, JsonSerializerOptions options, out ClassificationResult? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        // 1) Try direct deserialize
        try
        {
            result = JsonSerializer.Deserialize<ClassificationResult>(raw, options);
            if (result != null && allowedLabels.Contains(result.Label))
                return true;
        }
        catch { }

        // 2) If the AI returned a quoted JSON string like "{...}", unescape it
        try
        {
            if (raw.StartsWith("\"") && raw.EndsWith("\""))
            {
                // Deserialize to string to unescape JSON content
                var unquoted = JsonSerializer.Deserialize<string>(raw);
                if (!string.IsNullOrWhiteSpace(unquoted))
                {
                    result = JsonSerializer.Deserialize<ClassificationResult>(unquoted, options);
                    if (result != null && allowedLabels.Contains(result.Label))
                        return true;
                }
            }
        }
        catch { }

        // 3) Extract first JSON object {...} from the text
        try
        {
            var first = raw.IndexOf('{');
            var last = raw.LastIndexOf('}');
            if (first >= 0 && last > first)
            {
                var snippet = raw.Substring(first, last - first + 1);
                result = JsonSerializer.Deserialize<ClassificationResult>(snippet, options);
                if (result != null && allowedLabels.Contains(result.Label))
                    return true;
            }
        }
        catch { }

        return false;
    }

    private static string BuildPrompt(string text, IEnumerable<string> labels)
    {
        var labelBlock = string.Join(Environment.NewLine, labels);

        return
            "Classify the following text into ONE label from the allowed list.\n" +
            "Return ONLY valid JSON in this format:\n" +
            "{ \"label\": \"<LABEL>\", \"confidence\": <0.0-1.0> }\n\n" +
            "Allowed labels:\n" +
            labelBlock + "\n\n" +
            "Text:\n" +
            $"\"{text}\"";
    }


    private static ClassificationResult Fallback()
        => new() { Label = "OTHER", Confidence = 0.0 };
}
