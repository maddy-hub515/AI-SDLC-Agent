using SmartAIAgent.Domain.Enums;

namespace SmartAIAgent.Domain.Entities;

public class WorkflowEvent
{
    public Guid Id { get; set; }
    public Guid AgentRunId { get; set; }
    public AgentStage FromStage { get; set; }
    public AgentStage ToStage { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }

    public AgentRun? AgentRun { get; set; }
}
