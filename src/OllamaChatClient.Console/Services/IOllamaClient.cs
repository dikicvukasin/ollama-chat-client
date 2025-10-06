using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using OllamaChatClient.Console.Models;

namespace OllamaChatClient.Console.Services;

public interface IOllamaClient
{
    /// <summary>
    /// Gets the list of available Ollama models.
    /// </summary>
    Task<List<OllamaModel>> GetModelsAsync();

    /// <summary>
    /// Streams the response from Ollama line by line.
    /// </summary>
    IAsyncEnumerable<(string text, bool isThinking)> StreamMessageAsync(
        string model,
        string message,
        CancellationToken cancellationToken = default);
}

public class OllamaClient : IOllamaClient
{
    private readonly HttpClient _http;

    public OllamaClient(HttpClient http) => _http = http;

    public async Task<List<OllamaModel>> GetModelsAsync()
    {
        var response = await _http.GetFromJsonAsync<Dictionary<string, List<OllamaModel>>>("tags");
        return response?["models"] ?? new List<OllamaModel>();
    }

    /// <summary>
    /// Streams the response from Ollama line by line.
    /// </summary>
    public async IAsyncEnumerable<(string text, bool isThinking)> StreamMessageAsync(
        string model,
        string message,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model,
            prompt = message,
            stream = true
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "generate")
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        };

        using var response =
            await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Ollama returned {response.StatusCode}: {errorText}");
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var isInThinkingBlock = false;

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Remove "data:" prefix if it exists (SSE mode)
            if (line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                line = line["data:".Length..].Trim();

            if (line == "[DONE]") yield break;

            OllamaResponse? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<OllamaResponse>(line);
            }
            catch
            {
                continue;
            }

            if (string.IsNullOrEmpty(parsed?.Response))
                continue;

            var text = parsed.Response;

            // Handle <think> ... </think> sections
            int startIdx, endIdx;

            // Case 1: starts a new thinking block
            if ((startIdx = text.IndexOf("<think>", StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                isInThinkingBlock = true;
                text = text[(startIdx + "<think>".Length)..];
            }

            // Case 2: ends a thinking block
            if ((endIdx = text.IndexOf("</think>", StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                isInThinkingBlock = false;
                text = text[..endIdx];
            }

            // Strip any remaining tags just in case
            text = text
                .Replace("<think>", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("</think>", string.Empty, StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(text))
                yield return (text, isInThinkingBlock);
        }
    }

}

public class OllamaSimClient : IOllamaClient
{
    private static readonly Random Random = new();

    private readonly HttpClient _http;

    public OllamaSimClient(HttpClient http) => _http = http;
    
    public Task<List<OllamaModel>> GetModelsAsync()
    {
        // Return some dummy models
        var models = new List<OllamaModel>
        {
            new OllamaModel { Name = "SimModel-1" },
            new OllamaModel { Name = "SimModel-2" }
        };

        return Task.FromResult(models);
    }

    /// <summary>
    /// Simulates streaming messages from Ollama with dummy data.
    /// </summary>
    public async IAsyncEnumerable<(string text, bool isThinking)> StreamMessageAsync(
        string model,
        string message,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Thinking pool
        var thinkingPool = new[]
        {
            "Processing your request",
            "...almost there",
            "Just a bit more",
            "Analyzing data",
            "Crunching numbers",
            "Formulating response",
            "Checking details",
            "Loading context",
            "Verifying input",
            "Thinking..."
        };

        // Pick random subset of thinking chunks (1..half of pool)
        int thinkingSubsetSize = Random.Next(1, thinkingPool.Length / 2 + 1);
        var thinkingChunks = Shuffle(thinkingPool).Take(thinkingSubsetSize).ToArray();

        foreach (var chunk in thinkingChunks)
        {
            await Task.Delay(Random.Next(300, 700), cancellationToken);
            yield return (chunk + " ", false); // false = thinking
        }

        // Final pool: 3x bigger than thinking pool
        var finalPool = new[]
        {
            $"Here is the final response based on your input: \"{message}\".",
            $"Done! Your input \"{message}\" has been processed successfully!",
            $"All set! Result for \"{message}\": success.",
            $"Your request \"{message}\" is now complete.",
            $"Finished processing \"{message}\"!",
            $"Result ready: \"{message}\" has been handled.",
            $"Successfully generated response for \"{message}\".",
            $"Output ready for \"{message}\"!",
            $"Completed: \"{message}\" was processed correctly.",
            $"Final response for \"{message}\" is now available."
        };

        // Pick random subset of final chunks (1..half of pool)
        int finalSubsetSize = Random.Next(1, finalPool.Length / 2 + 1);
        var finalChunks = Shuffle(finalPool).Take(finalSubsetSize).ToArray();

        foreach (var chunk in finalChunks)
        {
            await Task.Delay(Random.Next(300, 600), cancellationToken);
            yield return (chunk + " ", true); // true = final
        }
    }

    // Helper to shuffle an array
    private static T[] Shuffle<T>(T[] array)
    {
        var arr = (T[])array.Clone();
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Next(i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
        return arr;
    }
}