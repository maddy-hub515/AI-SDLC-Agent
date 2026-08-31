namespace SmartAIAgent.Application.Options;

public sealed class AiOptions
{
    public const string SectionName = "AI";

    public string Provider { get; init; } = "Ollama";
    public string BaseUrl { get; init; } = "http://localhost:11434";
    public string Model { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 120;
    public decimal Temperature { get; init; } = 0.2m;
}
