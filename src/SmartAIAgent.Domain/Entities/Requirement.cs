using SmartAIAgent.Domain.Enums;

namespace SmartAIAgent.Domain.Entities;

public class Requirement
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RequirementStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<UserStory> UserStories { get; set; } = new List<UserStory>();
    public ICollection<AgentRun> AgentRuns { get; set; } = new List<AgentRun>();
}
