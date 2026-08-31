namespace SmartAIAgent.Application.Models.Prompts;

public sealed class PromptTemplate
{
    public string Version { get; init; } = string.Empty;
    public string SystemPrompt { get; init; } = string.Empty;
    public string UserPrompt { get; init; } = string.Empty;
}
