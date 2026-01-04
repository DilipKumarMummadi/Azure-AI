
using System.Text.Json;

namespace AiBackendDemo.Queries;

public class MultiModelService : IMultiModelService
{
    private readonly HttpClient _httpClient;
    public MultiModelService(HttpClient httpClient, IConfiguration config)
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
}