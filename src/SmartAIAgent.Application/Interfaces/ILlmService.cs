namespace SmartAIAgent.Application.Interfaces;

public interface ILlmService
{
    Task<T> GenerateStructuredAsync<T>(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);
}
