using System.Text.Json.Serialization;

namespace SmartAIAgent.Infrastructure.Options;

internal sealed class OllamaGenerateResponse
{
    [JsonPropertyName("response")]
    public string Response { get; init; } = string.Empty;
}
