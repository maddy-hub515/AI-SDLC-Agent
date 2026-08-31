using SmartAIAgent.Domain.Enums;

namespace SmartAIAgent.Application.Models.Approvals;

public sealed class ApprovalDto
{
    public Guid Id { get; init; }
    public ApprovalType Type { get; init; }
    public ApprovalStatus Status { get; init; }
    public string? Comment { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? DecidedAtUtc { get; init; }
}
