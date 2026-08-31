using SmartAIAgent.Application.Interfaces;

namespace SmartAIAgent.UnitTests.TestHelpers;

internal sealed class FakeLlmService : ILlmService
{
    private readonly Func<string, string, CancellationToken, Task<object>> _handler;

    public FakeLlmService(Func<string, string, CancellationToken, Task<object>> handler)
    {
        _handler = handler;
    }

    public async Task<T> GenerateStructuredAsync<T>(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var result = await _handler(systemPrompt, userPrompt, cancellationToken);
        return (T)result;
    }
}
