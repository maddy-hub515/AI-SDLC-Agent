namespace SmartAIAgent.Domain.Entities;

public class UserStory
{
    public Guid Id { get; set; }
    public Guid RequirementId { get; set; }
    public Guid? AgentRunId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TechnicalAreasJson { get; set; } = "[]";
    public string DevelopmentTasksJson { get; set; } = "[]";
    public string AssumptionsJson { get; set; } = "[]";
    public ICollection<UserStoryAcceptanceCriterion> AcceptanceCriteria { get; set; } = new List<UserStoryAcceptanceCriterion>();
    public DateTime CreatedAtUtc { get; set; }

    public Requirement? Requirement { get; set; }
    public AgentRun? AgentRun { get; set; }
}
