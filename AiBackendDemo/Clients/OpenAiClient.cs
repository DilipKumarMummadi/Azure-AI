using System.Text;
using System.Text.Json;

public sealed class OpenAiClient : IOpenAiClient
{
    private readonly HttpClient _httpClient;
    private const string ApiVersion = "2024-02-15-preview";
    private const string DeploymentName = "gpt-5.1-chat"; // 👈 change this

    public OpenAiClient(HttpClient httpClient, IConfiguration config)
    {
        var apiKey = config["AzureOpenAI:ApiKey"]
                     ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

        var endpoint = config["AzureOpenAI:Endpoint"]; // https://vecna.openai.azure.com/

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(endpoint))
            throw new InvalidOperationException("Azure OpenAI config missing");

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(endpoint);
        _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
    }

    public async Task<string> CompleteAsync(string prompt, double temperature, CancellationToken ct)
    {
        var payload = new
        {
            messages = new[]
            {
                new { role = "system", content = "You are a strict JSON classification engine. Return ONLY valid JSON. Do NOT wrap the JSON in quotes. Do NOT escape characters." },
                new { role = "user", content = prompt }
            },
            temperature = 1,
            max_completion_tokens = 100
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url =
            $"openai/deployments/{DeploymentName}/chat/completions?api-version={ApiVersion}";

        var response = await _httpClient.PostAsync(url, content, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Azure OpenAI request failed: {response.StatusCode} {response.ReasonPhrase} - {body}");
        }

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        return doc.RootElement
                  .GetProperty("choices")[0]
                  .GetProperty("message")
                  .GetProperty("content")
                  .GetString()!;
    }

    public async Task<float[]> CreateEmbeddingAsync(string text, CancellationToken ct)
    {
        var payload = new
        {
            input = text
        };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var url =
        "openai/deployments/text-embedding-3-small/embeddings?api-version=2024-02-15-preview";
        var response = await _httpClient.PostAsync(url, content, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Embedding request failed: {response.StatusCode} - {body}");
        }
        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        return doc.RootElement
                  .GetProperty("data")[0]
                  .GetProperty("embedding")
                  .EnumerateArray()
                  .Select(x => x.GetSingle())
                  .ToArray();
    }
}
