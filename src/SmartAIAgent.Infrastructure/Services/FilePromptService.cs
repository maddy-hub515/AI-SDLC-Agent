using Microsoft.Extensions.Hosting;
using SmartAIAgent.Application.Interfaces;
using SmartAIAgent.Application.Models.Prompts;

namespace SmartAIAgent.Infrastructure.Services;

public sealed class FilePromptService : IPromptService
{
    private const string RequirementPromptVersion = "Requirement.UserStory.v1";
    private readonly IHostEnvironment _hostEnvironment;

    public FilePromptService(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public async Task<PromptTemplate> GetRequirementAnalysisPromptAsync(string requirementTitle, string requirementDescription, CancellationToken cancellationToken = default)
    {
        var promptsRoot = Path.Combine(_hostEnvironment.ContentRootPath, "Prompts", "Requirement");
        var systemPromptPath = Path.Combine(promptsRoot, "SystemPrompt.txt");
        var userPromptPath = Path.Combine(promptsRoot, "UserStoryPrompt.txt");

        var systemPrompt = await File.ReadAllTextAsync(systemPromptPath, cancellationToken);
        var userPromptTemplate = await File.ReadAllTextAsync(userPromptPath, cancellationToken);
        var userPrompt = userPromptTemplate
            .Replace("{requirementTitle}", requirementTitle.Trim(), StringComparison.Ordinal)
            .Replace("{requirementDescription}", requirementDescription.Trim(), StringComparison.Ordinal);

        return new PromptTemplate
        {
            Version = RequirementPromptVersion,
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt
        };
    }
}
