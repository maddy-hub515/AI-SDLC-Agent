using SmartAIAgent.Domain.Enums;

namespace SmartAIAgent.Application.Models.Requirements;

public sealed class RequirementSummaryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public RequirementStatus Status { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}
