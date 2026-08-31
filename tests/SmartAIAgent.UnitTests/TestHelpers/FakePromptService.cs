using SmartAIAgent.Application.Interfaces;
using SmartAIAgent.Application.Models.Prompts;

namespace SmartAIAgent.UnitTests.TestHelpers;

internal sealed class FakePromptService : IPromptService
{
    private readonly PromptTemplate _template;

    public FakePromptService(string version = "Requirement.UserStory.v1")
    {
        _template = new PromptTemplate
        {
            Version = version,
            SystemPrompt = "system",
            UserPrompt = "user"
        };
    }

    public Task<PromptTemplate> GetRequirementAnalysisPromptAsync(string requirementTitle, string requirementDescription, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_template);
    }
}
