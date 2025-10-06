using System.Text.Json.Serialization;

namespace OllamaChatClient.Console.Models;

public class OllamaModel
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
}