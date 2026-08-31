using SmartAIAgent.Application.Models.Prompts;

namespace SmartAIAgent.Application.Interfaces;

public interface IPromptService
{
    Task<PromptTemplate> GetRequirementAnalysisPromptAsync(string requirementTitle, string requirementDescription, CancellationToken cancellationToken = default);
}
