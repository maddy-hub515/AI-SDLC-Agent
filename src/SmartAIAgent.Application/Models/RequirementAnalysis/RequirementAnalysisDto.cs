using SmartAIAgent.Application.Models.AgentRuns;

namespace SmartAIAgent.Application.Models.RequirementAnalysis;

public sealed class RequirementAnalysisDto
{
    public Guid RequirementId { get; init; }
    public RequirementAnalysisResultDto? Analysis { get; init; }
    public AgentRunDetailsDto? LatestRun { get; init; }
}

public sealed class RequirementAnalysisResultDto
{
    public Guid UserStoryId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IReadOnlyCollection<string> AcceptanceCriteria { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> TechnicalAreas { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> DevelopmentTasks { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Assumptions { get; init; } = Array.Empty<string>();
    public DateTime CreatedAtUtc { get; init; }
}
