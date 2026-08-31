using SmartAIAgent.Domain.Enums;

namespace SmartAIAgent.Application.Models.Workflow;

public sealed class WorkflowEventDto
{
    public Guid Id { get; init; }
    public AgentStage FromStage { get; init; }
    public AgentStage ToStage { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
}
