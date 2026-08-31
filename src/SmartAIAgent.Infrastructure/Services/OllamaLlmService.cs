using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartAIAgent.Application.Common;
using SmartAIAgent.Application.Interfaces;
using SmartAIAgent.Application.Options;
using SmartAIAgent.Infrastructure.Options;

namespace SmartAIAgent.Infrastructure.Services;

public sealed class OllamaLlmService : ILlmService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly IOptions<AiOptions> _options;
    private readonly ILogger<OllamaLlmService> _logger;

    public OllamaLlmService(HttpClient httpClient, IOptions<AiOptions> options, ILogger<OllamaLlmService> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<T> GenerateStructuredAsync<T>(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var settings = _options.Value;
        if (!string.Equals(settings.Provider, "Ollama", StringComparison.OrdinalIgnoreCase))
        {
            throw new LlmException($"Unsupported AI provider '{settings.Provider}'.");
        }

        if (string.IsNullOrWhiteSpace(settings.Model))
        {
            throw new LlmException("AI model configuration is required.");
        }

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(TimeSpan.FromSeconds(settings.TimeoutSeconds));

        var request = new OllamaGenerateRequest
        {
            Model = settings.Model,
            Prompt = userPrompt,
            Stream = false,
            Format = "json",
            System = systemPrompt,
            Options = new OllamaRequestOptions
            {
                Temperature = settings.Temperature
            }
        };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("/api/generate", request, SerializerOptions, timeoutSource.Token);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new LlmException("The AI request timed out.", exception);
        }
        catch (HttpRequestException exception)
        {
            throw new LlmException("The AI provider could not be reached.", exception);
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Ollama returned HTTP {StatusCode} for model {Model}", (int)response.StatusCode, settings.Model);
            throw new LlmException("The AI provider returned an unsuccessful response.");
        }

        var payload = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(SerializerOptions, timeoutSource.Token);
        if (payload is null || string.IsNullOrWhiteSpace(payload.Response))
        {
            throw new LlmException("The AI provider returned an empty response.");
        }

        try
        {
            var result = JsonSerializer.Deserialize<T>(payload.Response, SerializerOptions);
            if (result is null)
            {
                throw new LlmException("The AI provider returned an empty structured payload.");
            }

            _logger.LogInformation("Structured AI response received from provider {Provider} using model {Model}", settings.Provider, settings.Model);
            return result;
        }
        catch (JsonException exception)
        {
            throw new LlmException("The AI provider returned malformed structured JSON.", exception);
        }
    }
}
