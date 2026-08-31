using SmartAIAgent.Domain.Enums;

namespace SmartAIAgent.Domain.Entities;

public class Approval
{
    public Guid Id { get; set; }
    public Guid AgentRunId { get; set; }
    public ApprovalType Type { get; set; }
    public ApprovalStatus Status { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? DecidedAtUtc { get; set; }

    public AgentRun? AgentRun { get; set; }
}
