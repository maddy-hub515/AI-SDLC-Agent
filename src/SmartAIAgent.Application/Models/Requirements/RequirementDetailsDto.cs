using SmartAIAgent.Application.Models.AgentRuns;
using SmartAIAgent.Application.Models.UserStories;
using SmartAIAgent.Domain.Enums;

namespace SmartAIAgent.Application.Models.Requirements;

public sealed class RequirementDetailsDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public RequirementStatus Status { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
    public IReadOnlyCollection<UserStoryDto> UserStories { get; init; } = Array.Empty<UserStoryDto>();
    public IReadOnlyCollection<AgentRunSummaryDto> AgentRuns { get; init; } = Array.Empty<AgentRunSummaryDto>();
}
