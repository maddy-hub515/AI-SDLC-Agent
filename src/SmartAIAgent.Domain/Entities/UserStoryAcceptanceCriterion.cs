namespace SmartAIAgent.Domain.Entities;

public class UserStoryAcceptanceCriterion
{
    public Guid Id { get; set; }
    public Guid UserStoryId { get; set; }
    public string Value { get; set; } = string.Empty;

    public UserStory? UserStory { get; set; }
}
