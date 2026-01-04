
using System.Text.Json;
using AiBackendDemo.Clients;

namespace AiBackendDemo.Queries;

public class MultiModelService : IMultiModelService
{
    private readonly HttpClient _httpClient;
    private readonly IOpenAiClient _openAiClient;
    public MultiModelService(HttpClient httpClient, IConfiguration config, IOpenAiClient openAiClient)
    {
        var apiKey = config["AzureOpenAI:ApiKey"]
                     ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

        var endpoint = config["AzureOpenAI:Endpoint"]; // https://vecna.openai.azure.com/

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(endpoint))
            throw new InvalidOperationException("Azure OpenAI config missing");
        _httpClient = httpClient;
        _openAiClient = openAiClient;
        _httpClient.BaseAddress = new Uri(endpoint);
        _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
    }
    public async Task<string> AnalyzeScreenshotAsync(byte[] imageBytes, CancellationToken ct)
    {
        var base64Image = Convert.ToBase64String(imageBytes);
        var payload = new
        {
            messages = new object[]
        {
            new
            {
                role = "system",
                content = "You are a technical assistant. Describe exactly what is visible in the image."
            },
            new
            {
                role = "user",
                content = new object[]
                {
                    new { type = "text", text = "Explain what this screenshot shows." },
                    new
                    {
                        type = "image_url",
                        image_url = new
                        {
                            url = $"data:image/png;base64,{base64Image}"
                        }
                    }
                }
            }
        },
            temperature = 0.2
        };

        var response = await _httpClient.PostAsJsonAsync(
        $"openai/deployments/gpt-4o/chat/completions?api-version=2024-02-15-preview",
        payload,
        ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        return json
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()!;
    }

    public async Task<string> ExtractTextFromImageAsync(byte[] imageBytes, CancellationToken ct)
    {
        var base64Image = Convert.ToBase64String(imageBytes);

        var payload = new
        {
            messages = new object[]
            {
            new
            {
                role = "system",
                content = "You are an OCR engine. Extract only visible text. Do not explain or summarize."
            },
            new
            {
                role = "user",
                content = new object[]
                {
                    new { type = "text", text = "Extract all visible text exactly as shown in the image." },
                    new
                    {
                        type = "image_url",
                        image_url = new
                        {
                            url = $"data:image/png;base64,{base64Image}"
                        }
                    }
                }
            }
            },
            temperature = 0.0
        };
        var response = await _httpClient.PostAsJsonAsync(
            $"openai/deployments/gpt-4o/chat/completions?api-version=2024-02-15-preview",
            payload,
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        return json
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()!;
    }

    public async Task<string> AnalyzeImageForIssueAsync(
    byte[] imageBytes,
    CancellationToken ct)
    {
        var base64Image = Convert.ToBase64String(imageBytes);

        var payload = new
        {
            messages = new object[]
            {
            new
            {
                role = "system",
                content = """
You are a technical reasoning assistant.
Reason only from visible content.
Do not guess causes that are not shown.
"""
            },
            new
            {
                role = "user",
                content = new object[]
                {
                    new
                    {
                        type = "text",
                        text = "Based on this image, what issue or problem does it indicate?"
                    },
                    new
                    {
                        type = "image_url",
                        image_url = new
                        {
                            url = $"data:image/png;base64,{base64Image}"
                        }
                    }
                }
            }
            },
            temperature = 0.2
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"openai/deployments/gpt-4o/chat/completions?api-version=2024-02-15-preview",
            payload,
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        return json
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()!;
    }

    public async Task<string> TranscribeAudioAsync(Stream audioStream, string fileName, CancellationToken ct)
    {
        using var content = new MultipartFormDataContent();

        content.Add(
            new StreamContent(audioStream),
            "file",
            fileName);
            
        var url = "openai/deployments/whisper/audio/translations?api-version=2024-06-01";

        var response = await _httpClient.PostAsync(url, content, ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        var text = json
            .GetProperty("text")
            .GetString()!;
        
        var result = await SummarizeTranscriptAsync(text, ct);
        return result;
    }

    public async Task<string> SummarizeTranscriptAsync(string transcript, CancellationToken ct)
    {
        var prompt = $"""
Summarize the following conversation in 3–5 bullet points.

Rules:
- Be factual
- Do not add assumptions
- Capture decisions and actions

Transcript:
{transcript}
""";

        return await _openAiClient.SummarizeAsync(prompt, ct);
    }

}