using System.Text.Json.Serialization;

namespace SmartAIAgent.Infrastructure.Options;

internal sealed class OllamaGenerateRequest
{
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; init; } = string.Empty;

    [JsonPropertyName("stream")]
    public bool Stream { get; init; }

    [JsonPropertyName("format")]
    public string Format { get; init; } = "json";

    [JsonPropertyName("system")]
    public string System { get; init; } = string.Empty;

    [JsonPropertyName("options")]
    public OllamaRequestOptions Options { get; init; } = new();
}

internal sealed class OllamaRequestOptions
{
    [JsonPropertyName("temperature")]
    public decimal Temperature { get; init; }
}
