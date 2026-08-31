namespace SmartAIAgent.Application.Models.UserStories;

public sealed class UserStoryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IReadOnlyCollection<string> AcceptanceCriteria { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> TechnicalAreas { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> DevelopmentTasks { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Assumptions { get; init; } = Array.Empty<string>();
    public DateTime CreatedAtUtc { get; init; }
}
