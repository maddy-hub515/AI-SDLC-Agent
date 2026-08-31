using SmartAIAgent.Application.Models.Approvals;
using SmartAIAgent.Application.Models.Workflow;
using SmartAIAgent.Domain.Enums;

namespace SmartAIAgent.Application.Models.AgentRuns;

public sealed class AgentRunDetailsDto
{
    public Guid Id { get; init; }
    public Guid RequirementId { get; init; }
    public AgentRunStatus Status { get; init; }
    public AgentStage CurrentStage { get; init; }
    public string? Provider { get; init; }
    public string? Model { get; init; }
    public string? PromptVersion { get; init; }
    public int RetryCount { get; init; }
    public DateTime StartedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyCollection<ApprovalDto> Approvals { get; init; } = Array.Empty<ApprovalDto>();
    public IReadOnlyCollection<WorkflowEventDto> WorkflowEvents { get; init; } = Array.Empty<WorkflowEventDto>();
}
