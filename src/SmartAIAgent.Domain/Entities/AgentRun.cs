using SmartAIAgent.Domain.Enums;

namespace SmartAIAgent.Domain.Entities;

public class AgentRun
{
    public Guid Id { get; set; }
    public Guid RequirementId { get; set; }
    public AgentRunStatus Status { get; set; }
    public AgentStage CurrentStage { get; set; }
    public string? Provider { get; set; }
    public string? Model { get; set; }
    public string? PromptVersion { get; set; }
    public int RetryCount { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string? ErrorMessage { get; set; }

    public Requirement? Requirement { get; set; }
    public ICollection<UserStory> UserStories { get; set; } = new List<UserStory>();
    public ICollection<Approval> Approvals { get; set; } = new List<Approval>();
    public ICollection<WorkflowEvent> WorkflowEvents { get; set; } = new List<WorkflowEvent>();
}
