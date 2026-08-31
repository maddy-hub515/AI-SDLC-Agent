using SmartAIAgent.Application.Models.AgentRuns;

namespace SmartAIAgent.Application.Interfaces;

public interface IRequirementAgent
{
    Task<AgentRunDetailsDto> AnalyzeAsync(Guid requirementId, CancellationToken cancellationToken);
}
