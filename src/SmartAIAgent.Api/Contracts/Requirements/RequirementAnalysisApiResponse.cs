using SmartAIAgent.Application.Models.AgentRuns;
using SmartAIAgent.Application.Models.RequirementAnalysis;

namespace SmartAIAgent.Api.Contracts.Requirements;

public sealed class RequirementAnalysisApiResponse
{
    public Guid RequirementId { get; init; }
    public RequirementAnalysisResultDto? Analysis { get; init; }
    public AgentRunDetailsDto? LatestRun { get; init; }
}
